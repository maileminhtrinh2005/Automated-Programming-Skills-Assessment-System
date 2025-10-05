using CourseService.Application.Dtos;

namespace CourseService.Application.Interface;


public interface ICourseService
{
    Task<IReadOnlyList<CourseReadDto>> GetAllAsync();
    Task<CourseReadDto?> GetByIdAsync(Guid id);
    Task<CourseReadDto> CreateAsync(CourseCreateDto input);
    Task<CourseReadDto?> UpdateAsync(Guid id, CourseUpdateDto input);
    Task<bool> DeleteAsync(Guid id);
}
