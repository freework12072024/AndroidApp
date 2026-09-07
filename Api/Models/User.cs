using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class User
    {
        public int Id { get; set; }


        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Profile fields
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public string Pincode { get; set; } = string.Empty;
        [Required]
        public string State { get; set; } = string.Empty;
        [Required, Phone]
        public string Mobile { get; set; } = string.Empty;

        // Activation fields
        public bool IsActive { get; set; } = false;
        public string? ActivationToken { get; set; }
        public DateTimeOffset? ActivationTokenExpiry { get; set; }

        public string? ResetToken { get; set; }
        public DateTimeOffset? ResetTokenExpiry { get; set; }


        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
