using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AdminService.Application.Interface;
using AdminService.Domain;
using AdminService.Application.DTO;

namespace AdminService.Infrastructure
{
    public class APIConfigService : IAPIConfigService
    {
        private readonly AdminAppDbContext _context;

        public APIConfigService(AdminAppDbContext context)
        {
            _context = context;
        }

<<<<<<< HEAD
        public async Task<IEnumerable<APIConfig>> GetAllAPIs()
        {
            return await _context.APIConfig.ToListAsync();
        }

=======
        public async Task<IEnumerable<object>> GetAllAPI()
        {
            var list = await _context.APIConfig
                .Select(a => new
                {
                    apiID = a.APIID,
                    name = a.Name,
                    baseURL = a.BaseURL
                })
                .ToListAsync();

            return list;
        }


>>>>>>> vu
        public async Task<APIConfig> AddAPIConfig(APIConfig config)
        {
            config.UpdatedAt = DateTime.Now;
            _context.APIConfig.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }

        // Trong file Infrastructure/APIConfigService.cs
        public async Task<bool> AddAPI(APIConfigDTO api)
        {
            var newConfig = new APIConfig
            {
                Name = api.Name,
                BaseURL = api.BaseURL
            };
            _context.APIConfig.Add(newConfig);
            await _context.SaveChangesAsync();
            return true; 
            
        }
<<<<<<< HEAD
=======
        public async Task<bool> UpdateAPI(int id, APIConfigDTO api)
        {
            var existing = await _context.APIConfig.FindAsync(id);
            if (existing == null)
                return false;

            existing.Name = api.Name;
            existing.BaseURL = api.BaseURL;
            existing.UpdatedAt = DateTime.Now;

            _context.APIConfig.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

>>>>>>> vu
    }
}
