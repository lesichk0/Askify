using Askify.DataAccessLayer.Entities;
using Askify.DataAccessLayer.Interfaces.Repositories;

namespace Askify.DataAccessLayer.Data.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await Task.FromResult<User?>(null); // заміни на реальну логіку
        }

        public async Task<IEnumerable<User>> GetExpertsAsync()
        {
            return await Task.FromResult(new List<User>()); // фільтрація по ролі
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string query)
        {
            return await Task.FromResult(new List<User>()); // пошук по імені, email тощо
        }
    }

}
