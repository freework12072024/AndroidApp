using Api.Models;

namespace Api.Services
{
    public interface IJwtService
    {
        Task<string> GenerateToken(User user);
    }
}
