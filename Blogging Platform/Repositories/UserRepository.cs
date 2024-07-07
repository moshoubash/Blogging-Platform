using Blogging_Platform.Models;
using Blogging_Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace Blogging_Platform.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MyDbContext dbContext;

        public UserRepository(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        void IUserRepository.UpdateUser(string id, AppUser appUser)
        {
            var targetUser = (from u in dbContext.Users 
                              where u.Id.Contains(id) 
                              select u).FirstOrDefault();
            
            targetUser.Age = appUser.Age;
            targetUser.Bio = appUser.Bio;
            targetUser.PhoneNumber = appUser.PhoneNumber;
            targetUser.Email = appUser.Email;

            var targetCountry = (from c in dbContext.Countries where c.Id.ToString() == appUser.Country select c).FirstOrDefault();
            targetUser.Country = targetCountry.Name;

            targetUser.FirstName = appUser.FirstName;
            targetUser.LastName = appUser.LastName;
            targetUser.FullName = appUser.FullName;
            targetUser.ProfilePicture = appUser.ProfilePicture;
            targetUser.Gender = appUser.Gender;

            dbContext.SaveChanges();
        }
    }
}
