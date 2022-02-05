namespace lfg
{
    public class PublicGameDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Thumbnail { get; set; }
        public int Creator { get; set; } 
        public List<PublicUserDto> Players { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastEdited { get; set; }
    }

}