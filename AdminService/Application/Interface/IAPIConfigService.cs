<<<<<<< HEAD
﻿using System.Collections.Generic;
using System.Threading.Tasks;
=======
﻿
>>>>>>> vu
using AdminService.Application.DTO;
using AdminService.Domain;

namespace AdminService.Application.Interface
{
    public interface IAPIConfigService
    {
        Task<bool> AddAPI(APIConfigDTO api);
<<<<<<< HEAD
        Task<IEnumerable<APIConfig>> GetAllAPIs();
        Task<APIConfig> AddAPIConfig(APIConfig config);
=======
        Task<IEnumerable<object>> GetAllAPI();

        Task<APIConfig> AddAPIConfig(APIConfig config);
        Task<bool> UpdateAPI(int id, APIConfigDTO api);

>>>>>>> vu
    }
}
