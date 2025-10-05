using CourseService.Application.Interface;
using CourseService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Infrastructure;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _db;
    public CourseRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Course?> GetByIdAsync(Guid id)
        => _db.Courses.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<IReadOnlyList<Course>> GetAllAsync()
        => await _db.Courses.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null)
        => _db.Courses.AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId));

    public async Task AddAsync(Course entity)
        => await _db.Courses.AddAsync(entity);

    public Task UpdateAsync(Course entity)
    {
        _db.Courses.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Course entity)
    {
        _db.Courses.Remove(entity);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync()
        => _db.SaveChangesAsync();
}
