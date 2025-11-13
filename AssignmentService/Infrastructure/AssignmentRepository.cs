using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;
using AssignmentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentService.Infrastructure
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly AssignmentDbContext _context;
        public AssignmentRepository(AssignmentDbContext context)
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
                Title = request.Title,
                Description = request.Description,
                SampleTestCases = request.SampleTestCase,
                Deadline = request.Deadline ?? DateTime.Now,
                Difficulty = request.Difficulty,
                CreatedBy = request.UserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsHidden = true
            };
            _context.assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return new AssignmentDTO 
            {
                AssignmentId = assignment.AssignmentId, 
                Deadline = assignment.Deadline
            };
        }

        public async Task<bool> DeleteAssignment(AssignmentRequest request)
        {
            Console.WriteLine("checkkk");
            if (request == null) return false;
            var assignment = await _context.assignments.FindAsync(request.AssignmentId);
            if (assignment == null) { return false; }


            Console.WriteLine("checkkkkk");
             _context.assignments.Remove(assignment); 
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<AssignmentDTO>> GetAllAssigment()
        {
            var assigment = await _context.assignments.ToListAsync();

            var assignmentDTOs = assigment.Select(a => new AssignmentDTO
            {
                AssignmentId= a.AssignmentId,
                Title= a.Title,
                Description= a.Description,
                SampleTestCase= a.SampleTestCases,
                Deadline= a.Deadline,
                Difficulty= a.Difficulty,
                IsHidden= a.IsHidden
            }).ToList();
            return assignmentDTOs;
        }

        public async Task<AssignmentDTO?> GetAssignmentById(AssignmentRequest request)
        {
            if (request == null)
            {
                return null;
            }
            Console.WriteLine($"{request.AssignmentId}");
            var assignment =  await _context.assignments.FirstOrDefaultAsync(a=> a.AssignmentId== request.AssignmentId);
            if (assignment == null) return null;
            Console.WriteLine("dấdadasdasdasdasdas: " + assignment.IsHidden);

            AssignmentDTO assign = new AssignmentDTO
            {
                AssignmentId= assignment.AssignmentId,
                Title= assignment.Title,
                Description= assignment.Description,
                SampleTestCase= assignment.SampleTestCases,
                Deadline= assignment.Deadline,
                Difficulty = assignment.Difficulty,
                IsHidden=assignment.IsHidden
            };
            return assign;
        }

        public async Task<bool> UpdateAssignment(AssignmentRequest request)
        {
            //Console.WriteLine("check111");
            if (request == null) return false;

            var assignment = await _context.assignments.FirstOrDefaultAsync(a=>a.AssignmentId== request.AssignmentId);
            if (assignment == null) return false;
            if(request.Title!=string.Empty) assignment.Title = request.Title;
            if (request.Description!=string.Empty)assignment.Description = request.Description;
            if (request.SampleTestCase!=string.Empty) assignment.SampleTestCases=request.SampleTestCase;
            if (request.Difficulty!=string.Empty) assignment.Difficulty = request.Difficulty;
            if (request.Deadline != null) assignment.Deadline = request.Deadline??DateTime.Now;
            if (request.UserId!=assignment.CreatedBy) assignment.CreatedBy=request.UserId;
            assignment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AssignmentDTO>> GetAssignmentForStudent()
        {
            var assignment = await _context.assignments.
                Where(a=>a.IsHidden==false && a.Deadline>DateTime.Now).
                Select(a=>new AssignmentDTO
                {
                    AssignmentId = a.AssignmentId,
                    Title = a.Title,
                    Description = a.Description,
                    SampleTestCase = a.SampleTestCases,
                    Deadline = a.Deadline,
                    Difficulty = a.Difficulty
                }).ToListAsync();

            return assignment;
        }

        public async Task<bool> UpdateIsHidden(int id,bool isHidden)
        {
            var assignment = await _context.assignments.FindAsync(id);
            if (assignment== null) return false;    
            assignment.IsHidden = isHidden;
            await _context.SaveChangesAsync();
            return true;

        }
    }
}
