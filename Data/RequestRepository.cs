using lfg.Data;
using lfg.Models;
using Microsoft.EntityFrameworkCore;

namespace lfg
{
    public class RequestRepository
    {
        private readonly DataContext _context;
        public RequestRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<Request>>> GetAllRequestsAsync()
        {
            ServiceResponse<List<Request>> response = new ServiceResponse<List<Request>>();
            try
            {
                response.Data = await _context.Requests.ToListAsync();
            }
            catch (Exception e)
            {
                response.Message = e.Message;
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<Request>> CreateRequest(CreateRequestDto request, int userId)
        {
            ServiceResponse<Request> response = new ServiceResponse<Request>();

            try
            {
                Request requestToCreate = new Request {
                    Requestor = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId),
                    Game = await _context.Games.FirstOrDefaultAsync(g => g.Id == request.GameId),
                    Message = request.Message,
                    Status = Status.Undecided,
                    Created = DateTime.Now,
                    LastEdited = DateTime.Now
                };

                await _context.Requests.AddAsync(requestToCreate);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                
            }

            return response;
        }
    }
}