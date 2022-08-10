using System.ComponentModel.DataAnnotations;

namespace LFG.Dtos.User
{
    public class UpdateUserDto  
    { 
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string ProfilePicture { get; set; }
        [Required]
        public string Bio { get; set; }
    }
}
