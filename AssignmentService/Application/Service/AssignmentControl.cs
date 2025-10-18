using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;

namespace AssignmentService.Application.Service
{
    public class AssignmentControl
    {
        private readonly ICrudAssignment _control;
        private readonly ITestCaseFunc _testCaseFunc;
        public AssignmentControl(ICrudAssignment control, ITestCaseFunc testCaseFunc)
        {
            _control = control;
            _testCaseFunc = testCaseFunc;
        }

        public async Task<bool> AddAssignment(AssignmentRequest request)
        {
            if (request == null) { return false; }
            //if (!await _testCaseFunc.SaveTestCase(request)) return false;
            var check = await _control.AddAssignment(request);
            if (check == null) return false;
                
            return true;
        }

        public async Task<AssignmentDTO> GetAssignmentById(AssignmentRequest request)
        {
            if (request == null)
            {
                return null;
            }
            return await _control.GetAssignmentById(request);
        }

        public async Task<bool> UpdateAssignment(AssignmentRequest request)
        {
            if (request == null) return false;
            if (!await _control.UpdateAssignment(request)) return false;
            return true;
        }
        public async Task<bool> DeleteAssignment(AssignmentRequest request)
        {
            if (request == null) return false;
            if (!await _control.DeleteAssignment(request)) return false;
            return true;
        }

        public async Task<List<AssignmentDTO>> GetAllAssignment()
        {
            return await _control.GetAllAssigment();
        }
    }
}
