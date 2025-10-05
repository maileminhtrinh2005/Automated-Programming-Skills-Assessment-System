using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;
using AssignmentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentService.Infrastructure
{
    public class CrudAssignment : ICrudAssignment
    {
        private readonly AssignmentDbContext _context;
        public CrudAssignment(AssignmentDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AddAssignment(RequestAssignment request)
        {
            if (request == null)
            {
                return false;
            }
            Assignment assignment = new Assignment
            {
                Title= request.Title,
                Description= request.Description,
                Deadline = request.Deadline,
                Difficulty = request.Difficulty,
                CreatedBy = 1, // tamj thoiw chua co
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _context.assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> DeleteAssignment(RequestAssignment request)
        {
            throw new NotImplementedException();
        }

        public async Task<AssignmentDTO> GetAssignmentById(RequestAssignment request)
        {
            if (request == null)
            {
                return null;
            }

            var assignment =  await _context.assignments.FirstOrDefaultAsync(a=> a.AssignmentId== request.AssignmentId);
            if (assignment == null) return null;

            AssignmentDTO assign = new AssignmentDTO
            {
                AssignmentId= assignment.AssignmentId,
                Title= assignment.Title,
                Description= assignment.Description,
                Deadline= assignment.Deadline,
                Difficulty = assignment.Difficulty
            };
            return assign;
        }

        public Task<bool> UpdateAssignment(RequestAssignment request)
        {
            throw new NotImplementedException();
        }
    }
}
