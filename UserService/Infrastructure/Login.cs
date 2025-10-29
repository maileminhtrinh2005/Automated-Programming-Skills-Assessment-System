using Microsoft.AspNetCore.Identity;
using UserService.Application.DTO;
using UserService.Application.Interface;
using UserService.Domain;

namespace UserService.Infrastructure
{
    public class Login : ILogin
    {
        private readonly UserAppDbContext _dbcontext;
        private readonly ICRUD _crud;
        private readonly PasswordHasher<User> _passwordHasher;
        public Login(UserAppDbContext dbcontext, ICRUD cRUD)
        {
            _dbcontext = dbcontext;
            _crud = cRUD;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User?> LoginAsync(LoginDTO loginDto)
        {

            var user = await _crud.GetUserByUsername(loginDto.Username);
            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (result == PasswordVerificationResult.Success)
            {
                return user;
            }
            return null;

        }
    }
}
