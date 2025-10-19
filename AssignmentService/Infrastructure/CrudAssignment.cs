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
        public async Task<AssignmentDTO?> AddAssignment(AssignmentRequest request)
        {
            if (request == null)
            {
                return null;
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

            return new AssignmentDTO
            {
                AssignmentId = assignment.AssignmentId
            };
        }

        public async Task<bool> DeleteAssignment(AssignmentRequest request)
        {
            Console.WriteLine("checkkk");
            if (request == null) return false;
            var assignment = await _context.assignments.FirstOrDefaultAsync(a=>a.AssignmentId==request.AssignmentId);
            if (assignment == null) { return false; }


            Console.WriteLine("checkkkkk");
             _context.assignments.Remove(assignment); 
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AssignmentDTO?> GetAssignmentById(AssignmentRequest request)
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
                Title= assignment.Title??"",
                Description= assignment.Description ?? "",
                Deadline= assignment.Deadline,
                Difficulty = assignment.Difficulty ?? ""
            };
            return assign;
        }

        public async Task<bool> UpdateAssignment(AssignmentRequest request)
        {
            //Console.WriteLine("check111");
            if (request == null) return false;

            var assignment = await _context.assignments.FirstOrDefaultAsync(a=>a.AssignmentId== request.AssignmentId);
            if (assignment == null) return false;
            assignment.Title = request.Title;
            assignment.Description = request.Description;
            assignment.Deadline = request.Deadline;
            assignment.Difficulty = request.Difficulty;
            assignment.UpdatedAt= DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }





    }
}
