using Database;
using Messaging;
using Microsoft.AspNetCore.Mvc;
using OperationalService.Api.Schemas;
using OperationalService.BusinessLogic.Objects;
using OperationalService.BusinessLogic.Services;
using System.ComponentModel;

namespace OperationalService.Api.Controllers;

/// <summary>
/// Controller for managing workers.
/// </summary>
/// <param name="dbContext">The currently connected database context.</param>
[Route("[controller]")]
[ApiController]
public class WorkersController(DatabaseContext dbContext, IMessageHandler messageHandler) : ControllerBase
{
    /// <summary>
    /// Get a paginated list of worker objects.
    /// </summary>
    /// <param name="page">The page to retrieve.</param>
    /// <param name="pageSize">The number of items to retrieve.</param>
    /// <returns>An action result with the paginated list of worker objects.</returns>
    [HttpGet]
    [Produces<WorkerList>]
    public IActionResult GetAll(int page = 1, int pageSize = 10)
    {
        /// 1. Create a workers service.
        /// 2. Retrieve a paginated collection of worker objects.
        /// 3. Get the total number of worker items in the database.
        var workersService = new JobsService(dbContext: dbContext, messageHandler: messageHandler);
        var jobs = workersService.CollectJobs(page: page, pageSize: pageSize);
        var totalItems = workersService.CountJobs();

        // Construct the pagination object.
        var pagination = new Pagination()
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalItems
        };

        // Construct the response.
        return Ok(
            new WorkerList()
            {
                Data = jobs.Select(t => new Worker()
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
    /// Get a single worker object.
    /// </summary>
    /// <param name="id">The unique ID of the worker to retrieve.</param>
    /// <returns>An action result with the retrieved worker object. If none exists, a NotFound error is returned.</returns>
    [HttpGet("{id}")]
    [Produces<Worker>]
    public IActionResult Get([Description("Unique identifier of the worker.")] string id)
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

        /// 1. Create a workers service.
        /// 2. Retrieve the worker with the specified ID.
        var workersService = new WorkersService(dbContext: dbContext, messageHandler: messageHandler);
        var worker = workersService.FetchWorker(parsedId);

        // If not found, return a NotFound error.
        if (worker is null)
        {
            return NotFound();
        }

        // Construct the response.
        return Ok(
            new Worker()
            {
                Id = worker.Id.ToString(),
                Name = worker.Name,
                Status = worker.Status!
            }
        );
    }

    /// <summary>
    /// Create a new worker object.
    /// </summary>
    /// <param name="workerCreate">The worker object to create.</param>
    /// <returns>An action result with the created worker.</returns>
    [HttpPost]
    [Produces<Job>]
    public IActionResult Create(WorkerCreate workerCreate)
    {
        /// 1. Create a workers service.
        /// 2. Spawn a new worker.
        var workerObject = new WorkerServiceObject() { Name = workerCreate.Name };
        var workersService = new WorkersService(dbContext: dbContext, messageHandler: messageHandler);
        workerObject = workersService.SpawnWorker(workerObject);

        // Construct the response.
        return Created(
            $"[controller]/{workerObject.Id.ToString()}",
            new Job()
            {
                Id = workerObject.Id.ToString(),
                Name = workerObject.Name,
                Status = workerObject.Status!
            }
        );
    }
}
