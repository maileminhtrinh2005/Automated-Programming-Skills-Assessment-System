using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;
using AssignmentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentService.Infrastructure
{
    public class ResourceRepository : IResourceRepository
    {

        private readonly AssignmentDbContext _context;
        public ResourceRepository(AssignmentDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddResource(ResourceDTO r)
        {
            if (r == null)
            {
                return false;
            }
            var resource = new Resource
            {
                AssignmentId = r.AssignmentId,
                Title = r.Title,
                Link = r.Link,
                Type = r.Type,
                CreatedAt = DateTime.Now,
            };
            _context.resources.Add(resource);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteResource(int id)
        {
            if (id <= 0)
            {
                return false;
            }
            var resource = await _context.resources.FindAsync(id);
            if (resource == null) { return false; }

            _context.resources.Remove(resource);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ResourceDTO>?> GetResoureById(int assignmentId)
        {
            if(assignmentId <= 0)
            {
                return null;
            }
            var resources = await _context.resources.
                Where(r => r.AssignmentId == assignmentId).
                Select(r=>new ResourceDTO
                {
                    ResourceId=r.ResourceId,
                    Link = r.Link,
                    Title = r.Title,
                    Type = r.Type

                }).ToListAsync();
            if (resources == null) { return null; }

            return resources;
        }

        public async Task<bool> UpdateResource(ResourceDTO r)
        {
            if (r == null) return false;
            var resource= await _context.resources.FindAsync(r.ResourceId);
            if (resource == null) { return false; }
            if (r.Title !=string.Empty) resource.Title = r.Title;
            if (r.Type !=string.Empty) resource.Type = r.Type;
            if (r.Link !=string.Empty) resource.Link = r.Link; 
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
