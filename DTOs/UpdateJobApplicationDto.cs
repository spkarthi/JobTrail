using JobTrail.Models;

namespace JobTrail.DTOs;

public class UpdateJobApplicationDto
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedOn { get; set; }
}