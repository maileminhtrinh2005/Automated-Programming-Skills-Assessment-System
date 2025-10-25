using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;

namespace AssignmentService.Application.Service
{
    public class AssignmentManager
    {
        private readonly IAssignmentRepository _assignmentRepo;
        public AssignmentManager(IAssignmentRepository control)
        {
            _assignmentRepo = control;
        }

        public async Task<bool> AddAssignment(AssignmentRequest request)
        {
            if (request == null) { return false; }
            var check = await _assignmentRepo.AddAssignment(request);
            if (check == null) return false;

            return true;
        }

        public async Task<AssignmentDTO?> GetAssignmentById(AssignmentRequest request)
        {
            if (request == null)
            {
                return null;
            }
            return await _assignmentRepo.GetAssignmentById(request);
        }

        public async Task<bool> UpdateAssignment(AssignmentRequest request)
        {
            if (request == null) return false;
            if (!await _assignmentRepo.UpdateAssignment(request)) return false;
            return true;
        }
        public async Task<bool> DeleteAssignment(AssignmentRequest request)
        {
            if (request == null) return false;
            if (!await _assignmentRepo.DeleteAssignment(request)) return false;
            return true;
        }

        public async Task<List<AssignmentDTO>> GetAllAssignment()
        {
            return await _assignmentRepo.GetAllAssigment();
        }
    }
}
