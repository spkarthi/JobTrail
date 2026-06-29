namespace JobTrail.Models;

public class JobApplication
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedOn { get; set; }
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

public enum ApplicationStatus
{
    Saved,
    Applied,
    Rejected,
    Offer,
    Withdrawn,
    Pending
}