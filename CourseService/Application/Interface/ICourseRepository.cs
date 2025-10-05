using CourseService.Domain;

namespace CourseService.Application.Interface;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Course>> GetAllAsync();
    Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null);
    Task AddAsync(Course entity);
    Task UpdateAsync(Course entity);
    Task DeleteAsync(Course entity);
    Task<int> SaveChangesAsync();
}
