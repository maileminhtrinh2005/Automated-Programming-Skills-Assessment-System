using AssignmentService.Application.DTO;

namespace AssignmentService.Application.Interface
{
    public interface IResourceRepository
    {
        public Task<bool> AddResource(ResourceDTO r);

        public Task<bool> UpdateResource(ResourceDTO r);

        public Task<bool> DeleteResource(int assignmentId);

        public Task<ResourceDTO?> GetResoureById(int assignmentId);

    }
}
