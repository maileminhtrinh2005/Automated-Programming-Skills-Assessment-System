using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;

namespace AssignmentService.Application
{
    public class AssignmentControl
    {
        private readonly ICrudAssignment _control;
        public AssignmentControl(ICrudAssignment control)
        {
            _control = control;
        }



        public async Task<bool> AddAssignment(RequestAssignment request)
        {
            if (request == null) { return false; }

            bool check=await  _control.AddAssignment(request);

            if (!check) return false;
            return true;
        }

        public async Task<AssignmentDTO> GetAssignmentById(RequestAssignment request)
        {
            if (request == null)
            {
                return null; 
            }
            return await _control.GetAssignmentById(request);
        }
    }
}
