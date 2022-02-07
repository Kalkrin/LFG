using System.ComponentModel.DataAnnotations;

namespace lfg.Models
{
    public class User
    {
        public int Id { get; set; }
        [MaxLength(30)]
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public List<Game> Games { get; set; }
        public List<Request> Requests { get; set; }
        public DateTime CreatedAt {get; set;}
        public DateTime LastUpdated { get; set; }
    }
}