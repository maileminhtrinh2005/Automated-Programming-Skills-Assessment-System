using System.Collections.Generic;
using System.Threading.Tasks;
using AdminService.Application.DTO;
using AdminService.Domain;

namespace AdminService.Application.Interface
{
    public interface IAPIConfigService
    {
        Task<bool> AddAPI(APIConfigDTO api);
        Task<IEnumerable<APIConfig>> GetAllAPIs();
        Task<APIConfig> AddAPIConfig(APIConfig config);
    }
}
