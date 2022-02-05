using System.ComponentModel.DataAnnotations;
using lfg.Models;

namespace lfg 
{
    public class AddGameDto 
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string? Thumbnail { get; set; }
    }
}