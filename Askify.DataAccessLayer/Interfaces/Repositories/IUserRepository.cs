using Askify.DataAccessLayer.Entities;

namespace Askify.DataAccessLayer.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetExpertsAsync();
        Task<IEnumerable<User>> SearchUsersAsync(string query);
    }

}
