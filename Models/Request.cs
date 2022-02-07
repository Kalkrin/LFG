using lfg.Models;

namespace lfg 
{
    public class Request
    {
        public int Id { get; set; }
        public User Requestor { get; set; }
        public Game Game {get; set;}
        public Status Status { get; set; }
        public string? Message { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastEdited { get; set; }

    }
}