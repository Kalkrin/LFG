using System.ComponentModel.DataAnnotations;
using lfg.Models;

namespace lfg
{
    public class UpdateGameDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Thumbnail { get; set; }
        public List<int> PlayersToAdd { get; set; }
        public List<int> PlayersToRemove {get; set;}
        public DateTime LastEdited { get; set; }
    }
}