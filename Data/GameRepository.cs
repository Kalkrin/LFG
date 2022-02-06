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
                if(response.Data == null)
                {
                    response.Success = false;
                    response.Message = "Game with the provided ID doesn't exist";

                    return response;
                }
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
                Game gameToUpdate = await _context.Games.Include(g => g.Players).FirstOrDefaultAsync(g => g.Id == id);

                if(gameToUpdate == null)
                {
                    response.Success = false;
                    response.Message = "Game with the provided ID doesn't exist";
                    return response;
                }

                gameToUpdate.Description = updatedGame.Description;
                gameToUpdate.Name = updatedGame.Name;
                gameToUpdate.Thumbnail = updatedGame.Thumbnail;
                gameToUpdate.LastEdited = DateTime.Now;

                _context.Update(gameToUpdate);
                await _context.SaveChangesAsync();

                response.Data = gameToUpdate;
            } catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<PublicGameDto>> RemovePlayerFromGame(int playerId, int gameId)
        {
            ServiceResponse<PublicGameDto> response = new ServiceResponse<PublicGameDto>();

            try
            {
                Game gameToUpdate = await _context.Games.Include(g => g.Players).FirstOrDefaultAsync(g => g.Id == gameId);

                User playerToRemove = gameToUpdate.Players.FirstOrDefault(p => p.Id == playerId);

                if(gameToUpdate == null || playerToRemove == null)
                {   
                    response.Success = false;

                    if(gameToUpdate == null)
                        response.Message = "Game with provided id not found";
                    if(playerToRemove == null)
                        response.Message = "Player with the provided id not found";

                    return response;
                }

                gameToUpdate.Players.Remove(playerToRemove);

                _context.Games.Update(gameToUpdate);
                await _context.SaveChangesAsync();

                response.Data = MapGameToPublicGame(gameToUpdate);
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<PublicGameDto>> AddPlayerToGame(int playerId, int gameId)
        {
            ServiceResponse<PublicGameDto> response = new ServiceResponse<PublicGameDto>();

            try
            {
                Game gameToAddPlayerTo = await _context.Games.Include(g => g.Players).FirstOrDefaultAsync(g => g.Id == gameId);

                User playerToBeAddedToGame = await _context.Users.FirstOrDefaultAsync(u => u.Id == playerId);

                if(gameToAddPlayerTo == null || playerToBeAddedToGame == null)
                {   
                    response.Success = false;

                    if(gameToAddPlayerTo == null)
                        response.Message = "Game with provided id not found";
                    if(playerToBeAddedToGame == null)
                        response.Message = "Player with the provided id not found";

                    return response;
                }
                
                gameToAddPlayerTo.Players.Add(playerToBeAddedToGame);
                _context.Games.Update(gameToAddPlayerTo);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
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
                if(await _context.Games.FirstOrDefaultAsync(g => g.Id == id) == null)
                {

                }
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