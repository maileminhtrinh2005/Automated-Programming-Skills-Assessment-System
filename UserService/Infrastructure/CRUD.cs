using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTO;
using UserService.Application.Interface;
using UserService.Domain;

namespace UserService.Infrastructure
{
    public class CRUD : ICRUD
    {
        private readonly UserAppDbContext _dbcontext;
        private readonly PasswordHasher<User> _passwordHasher; // 🔹 thêm hasher

        public CRUD(UserAppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
            _passwordHasher = new PasswordHasher<User>(); // khởi tạo
        }

        public async Task<bool> AddUser(UserDTO user)
        {
            var newUser = new User
            {
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,

                RoleID = 1,

                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };


            // 🔐 Hash mật khẩu trước khi lưu

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, user.PasswordHash);

            _dbcontext.user.Add(newUser);


            return true;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _dbcontext.user.FirstOrDefaultAsync(u => u.Username == username);

        }

        public async Task<IEnumerable<UserDTO>> GetAllUsers()
        {
            var users = await _dbcontext.user


                .Select(u => new UserDTO
                {
                    UserID = u.UserID,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,

                    PasswordHash = u.PasswordHash  

                })
                .ToListAsync();

            return users;
        }




        public async Task<bool> UpdateUser(UserDTO user)
        {
            var existingUser = await _dbcontext.user.FindAsync(user.UserID);
            if (existingUser == null)
                return false;

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.FullName = user.FullName;
            existingUser.UpdatedAt = DateTime.Now;

            // Nếu bạn muốn cập nhật mật khẩu (tùy chọn):
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, user.PasswordHash);
            }

            _dbcontext.user.Update(existingUser);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var user = await _dbcontext.user.FindAsync(userId);
            if (user == null)
                return false;

            _dbcontext.user.Remove(user);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

 


        public Task<User> GetUser(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<User?> LoginAsync(LoginDTO loginDto)
        {
            throw new NotImplementedException();
        }


      

    }
}
