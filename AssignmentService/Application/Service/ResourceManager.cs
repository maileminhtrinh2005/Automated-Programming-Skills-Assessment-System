using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;

namespace AssignmentService.Application.Service
{
    public class ResourceManager
    {
        private readonly IResourceRepository _resourceRepo;
        public ResourceManager(IResourceRepository resourceRepo)
        {
            _resourceRepo = resourceRepo;
        }

        public async Task<bool> AddResource (ResourceRequest request)
        {
            var resource = new ResourceDTO
            {
                AssignmentId = request.AssignmentId,
                Title = request.ResourceTitle,
                Link = request.ResourceLink,
                Type = request.ResourceType
            };

            var isComplete =await _resourceRepo.AddResource(resource);
            if (!isComplete) return false;

            return true;
        }

        public async Task<List<ResourceDTO>?> GetResourceById (int id)
        {
            if (id < 0) return null;
            var resource = await _resourceRepo.GetResoureById(id);
            if (resource == null) return null;
            return resource;
        }

        public async Task<bool> DeleteResource(int id)
        {
            if (id<0) return false;
            var isSuccess= await _resourceRepo.DeleteResource(id);
            if (!isSuccess) return false;
            return true;
        }

        public async Task<bool> UpdateResource(ResourceRequest request)
        {
            if(request.ResourceId <0) return false;
            var resource = new ResourceDTO
            {
                ResourceId = request.ResourceId,
                Title = request.ResourceTitle,
                Link = request.ResourceLink,
                Type = request.ResourceType
            };
            bool isSuccess= await _resourceRepo.UpdateResource(resource);
            if (!isSuccess) return false;
            return true;
        }
    }
}
