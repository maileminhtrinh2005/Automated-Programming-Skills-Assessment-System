using AssignmentService.Application.DTO;

namespace AssignmentService.Application.Interface
{
    public interface ICrudAssignment
    {
        public Task<AssignmentDTO> GetAssignmentById(RequestAssignment request);
        public Task<bool> AddAssignment(RequestAssignment request);
        public Task<bool> UpdateAssignment(RequestAssignment request); 
        public Task<bool > DeleteAssignment(RequestAssignment request);
    }
}
