using lfg.Data;
using lfg.Models;
using Microsoft.EntityFrameworkCore;

namespace lfg
{
    public class RequestRepository
    {
        private readonly DataContext _context;
        private readonly UserRepository _userRepository;
        private readonly GameRepository _gameRepository;
        public RequestRepository(DataContext context, UserRepository userRepository, GameRepository gameRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
        }

        public async Task<ServiceResponse<List<PublicRequestDto>>> GetAllRequests()
        {
            ServiceResponse<List<PublicRequestDto>> response = new ServiceResponse<List<PublicRequestDto>>();
            try
            {
                List<Request> requests = new List<Request>();

                requests = await _context.Requests.Include(r => r.Game).Include(r => r.Game.Players).Include(r => r.Requestor).ToListAsync();

                response.Data = requests.Select(r => MapRequestToPublicRequest(r)).ToList<PublicRequestDto>();
            }
            catch (Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<PublicRequestDto>> GetRequestById(int id)
        {
            ServiceResponse<PublicRequestDto> response = new ServiceResponse<PublicRequestDto>();
            try
            {
                Request request = await _context.Requests.Include(r => r.Game).Include(r => r.Game.Players).Include(r => r.Requestor).FirstOrDefaultAsync(r => r.Id == id);

                if(request == null)
                {
                    response.Message = "Request with provided ID does not exist";
                    response.Success = false;
                }

                response.Data = MapRequestToPublicRequest(request);

            } catch (Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
            }
           
            return response;
        }

        public async Task<ServiceResponse<List<PublicRequestDto>>> GetRequestsByUserId(int id)
        {
            ServiceResponse<List<PublicRequestDto>> response = new ServiceResponse<List<PublicRequestDto>>();
            try
            {
                List<Request> requests = await _context.Requests.Include(r => r.Game).Include(r => r.Game.Players).Include(r => r.Requestor)
                .Where(r => r.Requestor.Id == id).ToListAsync();

                response.Data = requests.Select(r => MapRequestToPublicRequest(r)).ToList<PublicRequestDto>();

            } catch (Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
            }
           
            return response;
        }

        public async Task<ServiceResponse<List<PublicRequestDto>>> GetRequestsForCreator(int id)
        {
            ServiceResponse<List<PublicRequestDto>> response = new ServiceResponse<List<PublicRequestDto>>();
            try
            {
                List<Request> requests = await _context.Requests.Include(r => r.Game).Include(r => r.Game.Players).Include(r => r.Requestor)
                .Where(r => r.Game.Creator == id).ToListAsync();

                response.Data = requests.Select(r => MapRequestToPublicRequest(r)).ToList<PublicRequestDto>();

            } catch (Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
            }
           
            return response;
        }

        public async Task<ServiceResponse<PublicRequestDto>> CreateRequest(CreateRequestDto request, int userId)
        {
            ServiceResponse<PublicRequestDto> response = new ServiceResponse<PublicRequestDto>();

            try
            {
                Request requestToCreate = new Request {
                    Requestor = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId),
                    Game = await _context.Games.Include(g => g.Players).FirstOrDefaultAsync(g => g.Id == request.GameId),
                    Message = request.Message,
                    Status = Status.Undecided,
                    Created = DateTime.Now,
                    LastEdited = DateTime.Now
                };

                if(requestToCreate.Game == null)
                {
                    response.Message = "Game rquesting to join could not be found";
                    response.Success = false;

                    return response;
                }
                
                if(requestToCreate.Game.Creator == userId)
                {
                    response.Message = "Cannot request to be in your own game.";
                    response.Success = false;

                    return response;
                }

                if(await _context.Requests.FirstOrDefaultAsync(r => r.Requestor == requestToCreate.Requestor && r.Game == requestToCreate.Game && r.Status == Status.Undecided) != null)
                {
                    response.Message = "Already a request for this user to join this game.";
                    response.Success = false;

                    return response;
                }

                await _context.Requests.AddAsync(requestToCreate);
                await _context.SaveChangesAsync();

                response.Data = MapRequestToPublicRequest(requestToCreate);
            }
            catch (Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<PublicRequestDto>> UpdateRequest(int userId, UpdateRequestDto updatedRequest, int requestId)
        {
            ServiceResponse<PublicRequestDto> response = new ServiceResponse<PublicRequestDto>();
            
            try
            {
                Request requestToUpdate = await _context.Requests.Include(r => r.Game).Include(r => r.Game.Players).Include(r => r.Requestor).
                FirstOrDefaultAsync(r => r.Id == requestId);
                
                if(requestToUpdate == null)
                {
                    response.Message = "Request does not exist";
                    response.Success = false;

                    return response;
                }

                if(requestToUpdate.Requestor.Id != userId)
                {
                    response.Message = "Request does not belong to user";
                    response.Success = false;

                    return response;
                }

                requestToUpdate.Message = updatedRequest.Message;

                _context.Update(requestToUpdate);
                await _context.SaveChangesAsync();

                response.Data = MapRequestToPublicRequest(requestToUpdate);
            }
            catch(Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<PublicRequestDto>> ApproveOrDenyRequest(int userId, ApproveOrDenyRequestDto updatedRequest, int requestId)
        {
            ServiceResponse<PublicRequestDto> response = new ServiceResponse<PublicRequestDto>();
            
            try
            {
                Request requestToUpdate = await _context.Requests.Include(r => r.Game).Include(r => r.Game.Players).Include(r => r.Requestor).
                FirstOrDefaultAsync(r => r.Id == requestId);
                
                if(requestToUpdate == null)
                {
                    response.Message = "Request does not exist";
                    response.Success = false;

                    return response;
                }

                if(requestToUpdate.Game.Creator != userId)
                {
                    response.Message = "Request is not for one of this users games";
                    response.Success = false;

                    return response;
                }

                requestToUpdate.Status = updatedRequest.Status;

                _context.Update(requestToUpdate);
                await _context.SaveChangesAsync();

                response.Data = MapRequestToPublicRequest(requestToUpdate);
            }
            catch(Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
            }
            return response;
        }

        public async Task<ServiceResponse<int>> DeleteRequest(int userId, int requestId)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();
            
            try
            {
                Request requestToDelete = await _context.Requests.Include(r => r.Game).Include(r => r.Game.Players).Include(r => r.Requestor).
                FirstOrDefaultAsync(r => r.Id == requestId);
                
                if(requestToDelete == null)
                {
                    response.Message = "Request does not exist";
                    response.Success = false;

                    return response;
                }

                if(requestToDelete.Requestor.Id != userId)
                {
                    response.Message = "Request does not belong to user";
                    response.Success = false;

                    return response;
                }

                _context.Remove(requestToDelete);
                await _context.SaveChangesAsync();

                response.Data = requestToDelete.Id;
            }
            catch(Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
            }
            return response;
        }

        public PublicRequestDto MapRequestToPublicRequest(Request request)
        {
            return new PublicRequestDto
            {
                Id = request.Id,
                Requestor = _userRepository.MapUserToPublicUser(request.Requestor),
                Game = _gameRepository.MapGameToPublicGame(request.Game),
                Status = request.Status,
                Message = request.Message,
                Created = request.Created,
                LastEdited = request.LastEdited
            };
        }
    }
}