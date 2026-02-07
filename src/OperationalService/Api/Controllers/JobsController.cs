using Microsoft.AspNetCore.Mvc;
using OperationalService.Api.Schemas;
using Database;
using OperationalService.BusinessLogic.Objects;
using OperationalService.BusinessLogic.Services;
using System.ComponentModel;
using Messaging;

namespace OperationalService.Api.Controllers;

/// <summary>
/// Controller for managing jobs.
/// </summary>
/// <param name="dbContext">The currently connected database context.</param>
[Route("[controller]")]
[ApiController]
public class JobsController(DatabaseContext dbContext, IMessageHandler messageHandler) : ControllerBase
{
    /// <summary>
    /// Get a paginated list of job objects.
    /// </summary>
    /// <param name="page">The page to retrieve.</param>
    /// <param name="pageSize">The number of items to retrieve.</param>
    /// <returns>An action result with the paginated list of job objects.</returns>
    [HttpGet]
    [Produces<JobList>]
    public IActionResult GetAll(int page = 1, int pageSize = 10)
    {
        /// 1. Create a jobs service.
        /// 2. Retrieve a paginated collection of job objects.
        /// 3. Get the total number of job items in the database.
        var jobsService = new JobsService(dbContext: dbContext, messageHandler: messageHandler);
        var jobs = jobsService.CollectJobs(page: page, pageSize: pageSize);
        var totalItems = jobsService.CountJobs();

        // Construct the pagination object.
        var pagination = new Pagination()
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalItems
        };

        // Construct the response.
        return Ok(
            new JobList() { 
                Data = jobs.Select(t => new Job() 
                { 
                    Id = t.Id.ToString(), 
                    Name = t.Name,
                    Status = t.Status!
                }), 
                Pagination = pagination 
            }
        );
    }

    /// <summary>
    /// Get a single job object.
    /// </summary>
    /// <param name="id">The unique ID of the job to retrieve.</param>
    /// <returns>An action result with the retrieved job object. If none exists, a NotFound error is returned.</returns>
    [HttpGet("{id}")]
    [Produces<Job>]
    public IActionResult Get([Description("Unique identifier of the job.")] string id)
    {
        Guid parsedId;
        
        /// 1. Parse the ID from the request.
        /// 2. If invalid, return a NotFound error.
        try
        {
            parsedId = Guid.Parse(id);
        }
        catch (Exception)
        {
            return NotFound();
        }

        /// 1. Create a jobs service.
        /// 2. Retrieve the job with the specified ID.
        var jobsService = new JobsService(dbContext: dbContext, messageHandler: messageHandler);
        var job = jobsService.FetchJob(parsedId);

        // If not found, return a NotFound error.
        if (job is null)
        {
            return NotFound();
        }

        // Construct the response.
        return Ok(
            new Job()
            {
                Id = job.Id.ToString(),
                Name = job.Name,
                Status = job.Status!
            }
        );
    }

    /// <summary>
    /// Create a new job object.
    /// </summary>
    /// <param name="jobCreate">The job object to create.</param>
    /// <returns>An action result with the created job.</returns>
    [HttpPost]
    [Produces<Job>]
    public IActionResult Create(JobCreate jobCreate)
    {
        /// 1. Create a jobs service.
        /// 2. Start a new job.
        var jobObject = new JobServiceObject() { Name = jobCreate.Name };
        var jobsService = new JobsService(dbContext: dbContext, messageHandler: messageHandler);
        jobObject = jobsService.StartJob(jobObject);

        // Construct the response.
        return Created(
            $"[controller]/{jobObject.Id.ToString()}",
            new Job() { 
                Id = jobObject.Id.ToString(),
                Name = jobObject.Name,
                Status = jobObject.Status!
            }
        );
    }
}
