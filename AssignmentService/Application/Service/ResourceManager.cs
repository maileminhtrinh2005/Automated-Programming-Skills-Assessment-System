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

        public async Task<ResourceDTO?> GetResourceById (int id)
        {
            if (id < 0) return null;
            var resource = await _resourceRepo.GetResoureById(id);
            if (resource == null) return null;
            return resource;
        }

    }
}
