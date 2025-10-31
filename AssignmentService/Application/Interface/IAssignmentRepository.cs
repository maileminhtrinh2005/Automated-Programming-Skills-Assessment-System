using AssignmentService.Application.DTO;

namespace AssignmentService.Application.Interface
{
    public interface IAssignmentRepository
    {
        public Task<AssignmentDTO?> GetAssignmentById(AssignmentRequest request);
        public Task<AssignmentDTO?> AddAssignment(AssignmentRequest request);
        public Task<bool> UpdateAssignment(AssignmentRequest request); 
        public Task<bool > DeleteAssignment(AssignmentRequest request);

        public Task<List<AssignmentDTO>> GetAllAssigment();

        public Task<List<AssignmentDTO>> GetAssignmentForStudent();
        public Task<bool> UpdateIsHidden(int id,bool isHidden);
    }
}
