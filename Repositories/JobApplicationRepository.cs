using JobTrail.Data;
using JobTrail.DTOs;
using JobTrail.Interfaces;
using JobTrail.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTrail.Repositories;

public class JobApplicationRepository(AppDbContext dbContext) : IJobApplicationRepository
{
    public async Task<IEnumerable<JobApplication>> GetAllAsync()
    {
        return await dbContext.JobApplications.ToListAsync();
    }

    public async Task<JobApplication?> GetByIdAsync(int id)
    {
        return await dbContext.JobApplications.FirstOrDefaultAsync(jobApplication => jobApplication.Id == id); 
    }

    public async Task<JobApplication> CreateAsync(JobApplication jobApplication)
    {
        jobApplication.CreatedOn = DateTime.UtcNow;
        dbContext.JobApplications.Add(jobApplication);
        await dbContext.SaveChangesAsync();
        return jobApplication;
    }

    public async Task<JobApplication?> UpdateAsync(int id, JobApplicationRequestDto dto)
    {
        var application = await dbContext.JobApplications.FindAsync(id);
        if (application == null) return null;
        application.Name = dto.Name;
        application.Description = dto.Description ?? string.Empty;
        application.Url = dto.Url ?? string.Empty;
        application.Notes = dto.Notes ?? string.Empty;
        application.Status = dto.Status ?? ApplicationStatus.Applied;
        application.AppliedOn = dto.AppliedOn;
        application.UpdatedOn = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return application;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var application = await dbContext.JobApplications.FindAsync(id);
        if (application is null) return false;
        
        dbContext.JobApplications.Remove(application);
        await dbContext.SaveChangesAsync();
        return true;
    }
}