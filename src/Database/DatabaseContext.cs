using Microsoft.EntityFrameworkCore;
using Database.Entities;

namespace Database;

/// <summary>
/// An DbContext implementation for the management of concerns.
/// </summary>
/// <param name="options">Options for configuring the initialized context.</param>
public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options: options)
{
    /// <summary>
    /// Defines a Jobs table.
    /// </summary>
    public DbSet<JobEntity> Jobs { get; set; }

    /// <summary>
    /// Defines a JobInfos table.
    /// </summary>
    public DbSet<JobInfoEntity> JobInfos { get; set; }

    /// <summary>
    /// Defines a Workers table.
    /// </summary>
    public DbSet<WorkerEntity> Workers { get; set; }

    /// <summary>
    /// Defines a WorkerInfos table.
    /// </summary>
    public DbSet<WorkerInfoEntity> WorkerInfos { get; set; }

    /// <summary>
    /// Modify the schema of the database on entities that are being created.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance used to modify the schema.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /// 1. Set the primary key for the Jobs table.
        /// 2. Add a 1-to-1 relationship between Jobs and JobInfos.
        /// 3. Add a 1-to-many relationship between Jobs and Workers.
        modelBuilder.Entity<JobEntity>(jobEntity =>
        {
            jobEntity.HasKey(job => job.Id);

            jobEntity.Property(job => job.Name);

            jobEntity
                .HasOne(job => job.JobInfo)
                .WithOne(jobInfo => jobInfo.Job)
                .HasForeignKey<JobInfoEntity>(jobInfo => jobInfo.JobId);

            jobEntity
                .HasOne(job => job.Worker)
                .WithMany(worker => worker.Jobs)
                .HasForeignKey(job => job.WorkerId);
        });

        // Set the primary key for the JobInfos table.
        modelBuilder.Entity<JobInfoEntity>(jobInfoEntity =>
        {
            jobInfoEntity.HasKey(jobInfo => jobInfo.JobId);
        });

        /// 1. Set the primary key for the Workers table.
        /// 2. Add a 1-to-1 relationship between Workers and WorkerInfos.
        modelBuilder.Entity<WorkerEntity>(workerEntity =>
        {
            workerEntity.HasKey(worker => worker.Id);

            workerEntity.Property(worker => worker.Name);

            workerEntity
                .HasOne(worker => worker.WorkerInfo)
                .WithOne(workerInfo => workerInfo.Worker)
                .HasForeignKey<WorkerInfoEntity>(workerInfo => workerInfo.WorkerId);
        });

        // Set the primary key for the WorkerInfos table.
        modelBuilder.Entity<WorkerInfoEntity>(workerInfoEntity =>
        {
            workerInfoEntity.HasKey(workerInfo => workerInfo.WorkerId);
        });
    }
}
