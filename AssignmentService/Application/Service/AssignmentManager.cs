using AssignmentService.Application.DTO;
using AssignmentService.Application.Interface;
using ShareLibrary;
using ShareLibrary.Event;

namespace AssignmentService.Application.Service
{
    public class AssignmentManager
    {
        private readonly IAssignmentRepository _assignmentRepo;
        private readonly IEventBus _eventBus;
        public AssignmentManager(IAssignmentRepository control,IEventBus eventBus)
        {
            _assignmentRepo = control;
            _eventBus = eventBus;
        }

        public async Task<bool> AddAssignment(AssignmentRequest request)
        {
            if (request == null) { return false; }
            var check = await _assignmentRepo.AddAssignment(request);
            if (check == null) return false;
            var publishMessToNoti = new DeadlineNotification
            {
                Message = "Bạn có bài tập mới",
                Deadline = check.Deadline,
            };
            _eventBus.Publish(publishMessToNoti);
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
        public async Task<List<AssignmentDTO>> GetAssignmentForStudent()
        {
            return await _assignmentRepo.GetAssignmentForStudent();
        }

        public async Task<bool> UpdateIsHidden(AssignmentRequest a)
        {
            bool isSuccess = await _assignmentRepo.UpdateIsHidden(a.AssignmentId,a.IsHidden);
            return isSuccess;
        }
    }
}
