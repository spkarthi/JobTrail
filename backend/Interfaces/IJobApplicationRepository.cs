using JobTrail.DTOs;
using JobTrail.Models;

namespace JobTrail.Interfaces;

public interface IJobApplicationRepository
{
    Task <IEnumerable<JobApplication>> GetAllAsync();
    Task<JobApplication?> GetByIdAsync(int id);
    Task<JobApplication> CreateAsync(JobApplication jobApplication);
    Task <JobApplication?> UpdateAsync(int id, JobApplicationRequestDto dto);
    Task<bool> DeleteAsync(int id);
}