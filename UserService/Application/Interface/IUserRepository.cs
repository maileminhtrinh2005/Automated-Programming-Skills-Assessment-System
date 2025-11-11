using UserService.Application.DTO;
using UserService.Domain;
namespace UserService.Application.Interface
{
    public interface IUserRepository
    {
        Task<bool> AddUser(UserDTO user); // Tạo mới User

        Task<User> GetUser(int userId); // Lấy 1 User theo ID

        Task<IEnumerable<UserDTO>> GetAllUsers(); // Lấy danh sách User

        Task<bool> UpdateUser(UserDTO user); // Cập nhật User

        Task<bool> DeleteUser(int userId);
        Task<User> GetUserByUsername(string username);
        Task<bool> ChangePassword(ChangePasswordDTO changeDto);
        Task<IEnumerable<UserDTO>> GetAllStudents(string rolename);
        Task<Role> GetRoleById(int roleId);
    }
}
