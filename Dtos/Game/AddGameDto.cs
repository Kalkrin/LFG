using System.ComponentModel.DataAnnotations;

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