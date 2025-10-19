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

        public async Task<IEnumerable<APIConfig>> GetAllAPIs()
        {
            return await _context.APIConfig.ToListAsync();
        }

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
    }
}
