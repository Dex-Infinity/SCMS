namespace SCMS.API.DTOs;

// Data transfer object representing department information
public class DepartmentResponseDto
public sealed class DepartmentOptionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
}
