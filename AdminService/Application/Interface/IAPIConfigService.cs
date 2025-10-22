
using AdminService.Application.DTO;
using AdminService.Domain;

namespace AdminService.Application.Interface
{
    public interface IAPIConfigService
    {
        Task<bool> AddAPI(APIConfigDTO api);
        Task<IEnumerable<object>> GetAllAPI();

        Task<APIConfig> AddAPIConfig(APIConfig config);
        Task<bool> UpdateAPI(int id, APIConfigDTO api);

    }
}
