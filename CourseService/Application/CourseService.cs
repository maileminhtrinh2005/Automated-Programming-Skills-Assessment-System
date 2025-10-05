using CourseService.Application;
using CourseService.Application.Dtos;
using CourseService.Application.Interface;
using CourseService.Domain;

namespace CourseService.Infrastructure;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;

    public CourseService(ICourseRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<CourseReadDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(MapToReadDto).ToList();
    }

    public async Task<CourseReadDto?> GetByIdAsync(Guid id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e is null ? null : MapToReadDto(e);
    }

    public async Task<CourseReadDto> CreateAsync(CourseCreateDto input)
    {
        if (await _repo.ExistsCodeAsync(input.Code))
            throw new InvalidOperationException($"Mã môn '{input.Code}' đã tồn tại.");

        var e = new Course
        {
            Id = Guid.NewGuid(),
            Code = input.Code,
            Name = input.Name,
            Description = input.Description,
            Credits = input.Credits,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(e);
        await _repo.SaveChangesAsync();

        return MapToReadDto(e);
    }

    public async Task<CourseReadDto?> UpdateAsync(Guid id, CourseUpdateDto input)
    {
        var e = await _repo.GetByIdAsync(id);
        if (e is null) return null;

        if (await _repo.ExistsCodeAsync(input.Code, id))
            throw new InvalidOperationException($"Mã môn '{input.Code}' đã tồn tại.");

        e.Code = input.Code;
        e.Name = input.Name;
        e.Description = input.Description;
        e.Credits = input.Credits;

        await _repo.UpdateAsync(e);
        await _repo.SaveChangesAsync();

        return MapToReadDto(e);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var e = await _repo.GetByIdAsync(id);
        if (e is null) return false;

        await _repo.DeleteAsync(e);
        await _repo.SaveChangesAsync();
        return true;
    }

    private static CourseReadDto MapToReadDto(Course e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        Description = e.Description,
        Credits = e.Credits,
        CreatedAt = e.CreatedAt
    };
}
