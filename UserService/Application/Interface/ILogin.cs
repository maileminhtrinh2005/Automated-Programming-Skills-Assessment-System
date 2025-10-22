using UserService.Application.DTO;
using UserService.Domain;

namespace UserService.Application.Interface
{
    public interface ILogin
    {
        Task<User?> LoginAsync(LoginDTO loginDto);
    }
}
