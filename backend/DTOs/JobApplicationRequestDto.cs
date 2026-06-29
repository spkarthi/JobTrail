using JobTrail.Models;

namespace JobTrail.DTOs;

public class JobApplicationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public ApplicationStatus? Status { get; set; }
    public DateTime AppliedOn { get; set; }
}
