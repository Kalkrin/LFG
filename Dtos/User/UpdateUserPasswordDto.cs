using System.ComponentModel.DataAnnotations;

namespace LFG.Dtos.User
{
    public class UpdateUserPasswordDto
    {
        [Required]
        public string ExistingPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
