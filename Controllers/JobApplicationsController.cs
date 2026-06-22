using JobTrail.DTOs;
using JobTrail.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobTrail.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private static readonly List<JobApplication> _applications = new();
    private static int _nextId = 1;
    
    [HttpGet]
    public ActionResult<IEnumerable<JobApplicationResponseDto>> GetAll()
    {
        var result = _applications.Select(MapToResponse);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public ActionResult<JobApplicationResponseDto> GetById(int id)
    {
        var application = _applications.FirstOrDefault(x => x.Id == id);
        if (application is null) return NotFound();
        return Ok(MapToResponse(application));
    }

    [HttpPost]
    public ActionResult<JobApplicationResponseDto> Create([FromBody] JobApplicationRequestDto request)
    {
        var application = new JobApplication
        {
            Id = _nextId++,
            Name = request.Name,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Url = request.Url ?? string.Empty,
            Notes = request.Notes ?? string.Empty,
            AppliedOn = request.AppliedOn,
            Status = request.Status ?? ApplicationStatus.Saved,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        _applications.Add(application);
        return CreatedAtAction(nameof(GetById), new { id = application.Id }, MapToResponse(application));
    }

    [HttpPut("{id}")]
    public ActionResult<JobApplicationResponseDto> Update(int id, [FromBody] JobApplicationRequestDto request)
    {
        var application = _applications.FirstOrDefault(x => x.Id == id);
        if (application is null) return NotFound();

        application.Name = request.Name;
        application.Title = request.Title;
        application.Description = request.Description ?? string.Empty;
        application.Url = request.Url ?? string.Empty;
        application.Notes = request.Notes ?? string.Empty;
        application.AppliedOn = request.AppliedOn;
        application.Status = request.Status ?? application.Status;
        application.UpdatedOn = DateTime.UtcNow;

        return Ok(MapToResponse(application));
    }
    
    [HttpDelete("{id}")]
    public ActionResult<JobApplicationResponseDto> Delete(int id)
    {
        var application = _applications.FirstOrDefault(x => x.Id == id);
        if (application is null) return NotFound();
        _applications.Remove(application);
        return Ok(MapToResponse(application));
    }

    private static JobApplicationResponseDto MapToResponse(JobApplication application)
        => new()
        {
            Id = application.Id,
            Name = application.Name,
            Title = application.Title,
            Description = application.Description,
            Url = application.Url,
            Status = application.Status.ToString(),
            Notes = string.IsNullOrWhiteSpace(application.Notes) ? null : application.Notes,
            AppliedOn = application.AppliedOn,
            CreatedOn = application.CreatedOn,
            UpdatedOn = application.UpdatedOn,
        };
}