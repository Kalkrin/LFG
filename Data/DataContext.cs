using lfg.Models;
using Microsoft.EntityFrameworkCore;

namespace lfg.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games {get; set;}
        public DbSet<Request> Requests {get; set;}
    }
}