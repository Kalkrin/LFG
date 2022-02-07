using System.ComponentModel.DataAnnotations.Schema;

namespace lfg.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Thumbnail { get; set; }
        public int Creator { get; set; } 
        public List<User> Players { get; set; }
        public List<Request> Requests { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastEdited { get; set; }
    }

}