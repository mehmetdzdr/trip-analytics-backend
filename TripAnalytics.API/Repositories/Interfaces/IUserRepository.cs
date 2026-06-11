using TripAnalytics.API.Domain.Entities;

namespace TripAnalytics.API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);

        Task<User?> GetByEmailAsync(string email);

        Task<User> CreateAsync(User user);
    }
}
