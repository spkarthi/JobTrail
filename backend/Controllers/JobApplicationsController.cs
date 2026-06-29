using JobTrail.DTOs;
using JobTrail.Interfaces;
using JobTrail.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobTrail.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private readonly IJobApplicationRepository _repository;
    
    public JobApplicationsController(IJobApplicationRepository repository)
    {
        _repository = repository;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobApplicationResponseDto>>> GetAll()
    {
        var application = await _repository.GetAllAsync();
        return Ok(application.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplicationResponseDto>> GetById(int id)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application is null) return NotFound();
        return Ok(MapToResponse(application));
    }

    [HttpPost]
    public async Task<ActionResult<JobApplicationResponseDto>> Create([FromBody] JobApplicationRequestDto request)
    {
        var application = new JobApplication
        {
           
            Name = request.Name,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Url = request.Url ?? string.Empty,
            Notes = request.Notes ?? string.Empty,
            AppliedOn = request.AppliedOn,
            Status = request.Status ?? ApplicationStatus.Saved,
        };

        var created = await _repository.CreateAsync(application);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JobApplicationResponseDto>> Update(int id, [FromBody] JobApplicationRequestDto request)
    {
        var updated = await _repository.UpdateAsync(id, request);
        if (updated is null) return NotFound();
        return Ok(MapToResponse(updated));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<JobApplicationResponseDto>> Delete(int id)
    {
        var result = await _repository.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
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