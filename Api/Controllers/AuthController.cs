using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IJwtService _jwt;
        private readonly IEmailService _emailService;
        private readonly EmailQueue _emailQueue;
        public AuthController(AppDbContext db, IJwtService jwt, IEmailService emailService, EmailQueue emailQueue)
        {
            _db = db; _jwt = jwt;
            _emailService = emailService;
            _emailQueue = emailQueue;
        }


        public record SignupReq(
            [Required, EmailAddress] string Email,
            [Required, MinLength(6)] string Password,
            [Required] string FirstName,
            [Required] string LastName,
            [Required] string Address,
            [Required] string City,
            [Required] string Pincode,
            [Required] string State,
            [Required, Phone] string Mobile
        );
        public record LoginReq([Required, EmailAddress] string Email, [Required] string Password);
        public record ForgotReq([Required, EmailAddress] string Email);
        public record ResetReq([Required] string Token, [Required, MinLength(6)] string NewPassword);
        public record ActivateReq([Required, EmailAddress] string Email, [Required] string Token);


        [HttpGet("states")]
        public async Task<IActionResult> States()
        {
            var states = await _db.States.OrderBy(s => s.Name).ToListAsync();
            return Ok(states);
        }

        [HttpGet("cities")]
        public async Task<IActionResult> Cities([FromQuery] int stateId)
        {
            var cities = await _db.Cities.Where(c => c.StateId == stateId).OrderBy(c => c.Name).ToListAsync();
            return Ok(cities);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupReq req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _db.Users.AnyAsync(u => u.Email == req.Email);
            if (exists) return Conflict(new { message = "Email already registered" });

            // create user as inactive and generate activation token
            int code = RandomNumberGenerator.GetInt32(0, 1_000_000);
            var activationToken = code.ToString("D6");

            var user = new User
            {
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                FirstName = req.FirstName,
                LastName = req.LastName,
                Address = req.Address,
                City = req.City,
                Pincode = req.Pincode,
                State = req.State,
                Mobile = req.Mobile,
                IsActive = false,
                ActivationToken = activationToken,
                ActivationTokenExpiry = DateTimeOffset.UtcNow.AddMinutes(30)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // enqueue activation email
            if (_emailQueue != null)
            {
                _emailQueue.Enqueue(new EmailItem { To = user.Email, Subject = "Account Activation", Body = $"Your activation code is: {activationToken}" });
            }

            return Ok(new { message = "Account created. Activation code sent to email.", email = user.Email });
        }

        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] ActivateReq req)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return BadRequest(new { message = "Invalid email or token" });

            if (user.IsActive) return BadRequest(new { message = "Account already active" });

            if (user.ActivationToken == null || user.ActivationTokenExpiry == null || user.ActivationTokenExpiry < DateTimeOffset.UtcNow)
                return BadRequest(new { message = "Invalid or expired token" });

            if (user.ActivationToken != req.Token)
                return BadRequest(new { message = "Invalid activation token" });

            user.IsActive = true;
            user.ActivationToken = null;
            user.ActivationTokenExpiry = null;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Account activated" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReq req)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password" });

            if (!user.IsActive) return Unauthorized(new { message = "Account not activated" });

            var token = await _jwt.GenerateToken(user);
            return Ok(new { token, user = new { user.Id, user.Email } });
        }

        [HttpPost("forgot")]
        public async Task<IActionResult> Forgot([FromBody] ForgotReq req)
        {
            try
            {
                var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);
                if (user == null) return Ok(new { message = "If the email exists, a reset token was generated." });


                // generate secure 6-digit numeric OTP
                int code = RandomNumberGenerator.GetInt32(0, 1_000_000);
                user.ResetToken = code.ToString("D6");
                user.ResetTokenExpiry = DateTimeOffset.UtcNow.AddMinutes(30);
                await _db.SaveChangesAsync();

                // Send only token in email (enqueue to background worker)
                if (_emailQueue != null)
                {
                    _emailQueue.Enqueue(new EmailItem { To = user.Email, Subject = "Password Reset Token", Body = $"Your reset token is: {user.ResetToken}" });
                }

                return Ok(new { message = "Reset token generated", token = user.ResetToken });

            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }

        }

        [HttpPost("reset")]
        public async Task<IActionResult> Reset([FromBody] ResetReq req)
        {
            try
            {
                var user = await _db.Users.SingleOrDefaultAsync(u => u.ResetToken == req.Token);
                if (user == null || user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTimeOffset.UtcNow)
                    return BadRequest(new { message = "Invalid or expired token" });


                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
                user.ResetToken = null;
                user.ResetTokenExpiry = null;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Password updated" });
            }
            catch(Exception ex)
            {
                return Ok(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var sub = User.Claims.First(c => c.Type.EndsWith("/sub") || c.Type == "sub").Value;
            var id = int.Parse(sub);
            var user = await _db.Users.FindAsync(id);
            return Ok(new { user = new { user!.Id, user.Email } });
        }
    }
}
