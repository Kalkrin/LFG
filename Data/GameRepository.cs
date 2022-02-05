using System.Linq;
using lfg.Models;
using Microsoft.EntityFrameworkCore;

namespace lfg.Data
{
    public class GameRepository
    {
        private readonly DataContext _context;
        private readonly UserRepository _userRepository;
        public GameRepository(DataContext context, UserRepository userRepository, IConfiguration configuration)
        {
            _context = context;
            _userRepository = userRepository;
        }

        public async Task<ServiceResponse<List<PublicGameDto>>> GetAllGames()
        {
            ServiceResponse<List<PublicGameDto>> response = new ServiceResponse<List<PublicGameDto>>();

            try
            {
                List<Game> games = new List<Game>();

                games = await _context.Games.Include(g => g.Players).ToListAsync();    

                response.Data = games.Select(g => MapGameToPublicGame(g)).ToList<PublicGameDto>();
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;                
            }
            
            return response;
        }

        public async Task<ServiceResponse<Game>> GetGameById(int id)
        {
            ServiceResponse<Game> response = new ServiceResponse<Game>();

            try
            {
                response.Data = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);    
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;                
            }
            
            return response;
        }

        public async Task<ServiceResponse<Game>> CeateGame(AddGameDto newGame, int userId)
        {
            ServiceResponse<Game> response = new ServiceResponse<Game>();

            List<User> players = new List<User>();

            players.Add(await _context.Users.FirstOrDefaultAsync(u => u.Id == userId));

            try
            {
                Game gameToAdd = new Game{
                    Creator = userId,
                    Players  = players,
                    Name = newGame.Name,
                    Description = newGame.Description,
                    Thumbnail = newGame.Thumbnail,
                    CreatedAt = DateTime.Now,
                    LastEdited = DateTime.Now
                };

               await _context.AddAsync(gameToAdd);
               await _context.SaveChangesAsync();

                response.Data = gameToAdd;
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;                
            }
            
            return response;
        }

        public async Task<ServiceResponse<Game>> UpdateGame(int id, Game updatedGame)
        {
            ServiceResponse<Game> response = new ServiceResponse<Game>();

            try
            {
                Game gameToUpdate = (Game) _context.Games.Where(g => g.Id == id).SelectMany(g => g.Players);

                gameToUpdate.Description = updatedGame.Description;
                gameToUpdate.Name = updatedGame.Name;
                gameToUpdate.Players = updatedGame.Players;
                gameToUpdate.Thumbnail = updatedGame.Thumbnail;
                gameToUpdate.LastEdited = DateTime.Now;

                _context.Update(gameToUpdate);
                _context.SaveChangesAsync();

                response.Data = gameToUpdate;
            } catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<int>> DeleteGame(int id)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();
            try
            {
                _context.Games.Remove(await _context.Games.FirstOrDefaultAsync(g => g.Id == id));
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }
            return response;
        }

        public PublicGameDto MapGameToPublicGame(Game game)
        {
            PublicGameDto publicGame = new PublicGameDto 
            {
                Id = game.Id,
                Creator = game.Creator,
                Name = game.Name,
                Description = game.Description,
                Players = game.Players.Select(u => _userRepository.MapUserToPublicUser(u)).ToList<PublicUserDto>(),
                Thumbnail = game.Thumbnail,
                CreatedAt = game.CreatedAt,
                LastEdited = game.LastEdited
            };
            return publicGame;
        }
    }
}