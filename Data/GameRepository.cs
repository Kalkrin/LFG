using System.Linq;
using lfg.Models;
using Microsoft.EntityFrameworkCore;

namespace lfg.Data
{
    public class GameRepository
    {
        private readonly DataContext _context;
        private readonly UserRepository _userRepository;
        public GameRepository(DataContext context, UserRepository userRepository)
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

        public async Task<ServiceResponse<PublicGameDto>> CeateGame(AddGameDto newGame, int userId)
        {
            ServiceResponse<PublicGameDto> response = new ServiceResponse<PublicGameDto>();

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

                response.Data = MapGameToPublicGame(gameToAdd);
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;                
            }
            
            return response;
        }

        public async Task<ServiceResponse<PublicGameDto>> UpdateGame(UpdateGameDto updatedGame, int currentUserId, int gameId)
        {
            ServiceResponse<PublicGameDto> response = new ServiceResponse<PublicGameDto>();

            try
            {
                Game gameToUpdate = await _context.Games.Include(g => g.Players).FirstOrDefaultAsync(g => g.Id == gameId);

                if(gameToUpdate == null)
                {
                    response.Success = false;
                    response.Message = "Game with the provided ID doesn't exist";
                    return response;
                }

                if(currentUserId != gameToUpdate.Creator){
                    response.Success = false;
                    response.Message = "Game does not belong to the user currently signed in.";
                    return response;
                }

                foreach(int userIdToRemove in updatedGame.PlayersToRemove)
                {
                    User userToRemove = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdToRemove);

                    if(userToRemove != null && userToRemove.Id != gameToUpdate.Creator && gameToUpdate.Players.Contains(userToRemove))
                        gameToUpdate.Players.Remove(userToRemove);
                    else if(userToRemove == null || !gameToUpdate.Players.Contains(userToRemove))
                        response.Message += "Attempted to remove user that doesn't belong to game: " + userIdToRemove + " ";
                    else 
                        response.Message += "Cannot remove creator from game. Creator id: " + gameToUpdate.Creator + " ";
                }

                foreach(int userIdToAdd in updatedGame.PlayersToAdd)
                {
                    User userToAdd = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdToAdd);

                    if(userToAdd != null && !gameToUpdate.Players.Contains(userToAdd))
                        gameToUpdate.Players.Add(userToAdd);
                    else if(userToAdd != null)
                        response.Message += "Cannot add players already existing in game, id: " + userToAdd.Id + " ";
                    else
                        response.Message += "Attempted to add user to game that doesn't exist: " + userIdToAdd + " ";
                }

                gameToUpdate.Description = updatedGame.Description;
                gameToUpdate.Name = updatedGame.Name;
                gameToUpdate.Thumbnail = updatedGame.Thumbnail;
                gameToUpdate.LastEdited = DateTime.Now;

                _context.Update(gameToUpdate);
                await _context.SaveChangesAsync();

                response.Data = MapGameToPublicGame(gameToUpdate);
            } catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<int>> DeleteGame(int gameId, int userId)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();
            Game gameToRemove = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);
            try
            {
                if(gameToRemove == null)
                {
                    response.Success = false;
                    response.Message = "No game found with provided ID";

                    return response;
                }

                if(gameToRemove.Creator != userId)
                {
                    response.Success = false;
                    response.Message = "Game does not belong to user";

                    return response;
                }

                _context.Games.Remove(gameToRemove);

                await _context.SaveChangesAsync();

                response.Data = gameToRemove.Id;
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