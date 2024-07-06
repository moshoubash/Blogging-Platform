using Blogging_Platform.Models;

namespace Blogging_Platform.Repositories
{
    public interface IUserRepository
    {
        public void UpdateUser(string id, AppUser appUser);
    }
}
