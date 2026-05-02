using Microsoft.AspNetCore.Identity;

namespace Malia.Models
{
    public class User : IdentityUser<int>
    {
       // public string FullName { get; set; }
       
       // public UserRole Role { get; set; } = UserRole.Citizen;
        public int Id { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string PasswordHash { get; set; }

        public UserRole Role { get; set; } = UserRole.Citizen; 

        public bool IsDeleted { get; set; } = false;
    }
}
