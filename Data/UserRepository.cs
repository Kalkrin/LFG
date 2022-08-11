using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using lfg.Data;
using lfg.Models;
using LFG.Dtos.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace lfg
{
    public class UserRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public UserRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();
            
            try
            {
                User user = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(username.ToLower()));

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                }
                else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                {
                    response.Success = false;
                    response.Message = "Wrong password";
                }
                else
                {
                    response.Data = CreateToken(user);
                }

            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }
           
            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();
            try
            {
                if (await UserExists(user.Username))
                {
                    response.Success = false;
                    response.Message = "User Already Exists";
                    return response;
                }
                CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.CreatedAt = DateTime.Now;
                user.LastUpdated = DateTime.Now;

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                response.Data = user.Id;
            }
            catch (Exception e) 
            {
                response.Success = false;
                response.Message = e.Message;
            }
            
            return response;
        }

        public async Task<ServiceResponse<List<PublicUserDto>>> GetAllUsers()
        {
            ServiceResponse<List<PublicUserDto>> response = new ServiceResponse<List<PublicUserDto>>();
            try
            {
                List<User> users = await _context.Users.ToListAsync();

                response.Data = users.Select(u => MapUserToPublicUser(u)).ToList<PublicUserDto>();

            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }
            
            return response;
        }

        public async Task<ServiceResponse<PublicUserDto>> GetUserById(int id)
        {
            ServiceResponse<PublicUserDto> response = new ServiceResponse<PublicUserDto>();
            try
            {
                User user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

                response.Data = MapUserToPublicUser(user);
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            
           
            return response;
        }

        public async Task<ServiceResponse<PublicUserDto>> UpdateUser(int id, UpdateUserDto updatedUser)
        {
            ServiceResponse<PublicUserDto> response = new ServiceResponse<PublicUserDto>();
            try
            {
              User userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if(userToUpdate == null)
                {
                    response.Message = "User not found";
                    response.Success = false;
                    return response;
                }
                if (await UserExists(updatedUser.Username))
                {
                    response.Success = false;
                    response.Message = "User Already Exists";
                    return response;
                }

                userToUpdate.Username = updatedUser.Username;
                userToUpdate.Email = updatedUser.Email;
                userToUpdate.FirstName = updatedUser.FirstName;
                userToUpdate.LastName = updatedUser.LastName;
                userToUpdate.Bio = updatedUser.Bio;
                userToUpdate.ProfilePicture = updatedUser.ProfilePicture;
                userToUpdate.LastUpdated = DateTime.Now;

                _context.Users.Update(userToUpdate);
                await _context.SaveChangesAsync();

                response.Data = MapUserToPublicUser(userToUpdate);   
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<PublicUserDto>> UpdateUserPassword(UpdateUserPasswordDto updateUserPasswordDto, int id)
        {
            byte[] newHash = new byte[] { };
            byte[] newSalt = new byte[] { };
            ServiceResponse<PublicUserDto> response = new ServiceResponse<PublicUserDto>();
            try
            {
                User userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (userToUpdate == null)
                {
                    response.Message = "User not found";
                    response.Success = false;
                    return response;
                }
                else if (!VerifyPasswordHash(updateUserPasswordDto.ExistingPassword, userToUpdate.PasswordHash, userToUpdate.PasswordSalt))
                {
                    response.Success = false;
                    response.Message = "Wrong password";
                    return response;
                }

                CreatePasswordHash(updateUserPasswordDto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                newHash = passwordHash;
                newSalt = passwordSalt;
                

                if (newHash.Length != 0)
                    userToUpdate.PasswordHash = newHash;

                if (newSalt.Length != 0)
                    userToUpdate.PasswordSalt = newSalt;

                userToUpdate.LastUpdated = DateTime.Now;

                _context.Users.Update(userToUpdate);
                await _context.SaveChangesAsync();

                response.Data = MapUserToPublicUser(userToUpdate);
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }
        public async Task<ServiceResponse<int>> DeleteUser(int id)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();
            try
            {
                _context.Remove(await _context.Users.FirstOrDefaultAsync(u => u.Id == id));
                await _context.SaveChangesAsync();
                response.Data = id;
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(x => x.Username.ToLower() == username.ToLower()))
                return true;

            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                passwordSalt = hmac.Key;
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                        return false;
                }
            }
            return true;
        }

        private string CreateToken(User user)
        {
            string envToken = Environment.GetEnvironmentVariable("Token");
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(envToken is null ? _configuration.GetSection("AppSettings:Token").Value : envToken)
            );

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public PublicUserDto MapUserToPublicUser(User user){
            PublicUserDto publicUser = new PublicUserDto{
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                LastUpdated = user.LastUpdated
            };

            return publicUser;
        }
    }
}