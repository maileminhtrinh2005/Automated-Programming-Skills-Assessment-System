namespace CourseService.Application.Dtos;

public class CourseUpdateDto
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int Credits { get; set; }
}
