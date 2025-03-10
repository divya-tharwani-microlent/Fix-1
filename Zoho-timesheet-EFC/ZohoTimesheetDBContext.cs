using Microsoft.EntityFrameworkCore;
using Zoho_timesheet_Core.Entities;

namespace Zoho_timesheet_EFC
{
    public partial class ZohoTimesheetDBContext : DbContext
    {
        public ZohoTimesheetDBContext() { }

        public ZohoTimesheetDBContext(DbContextOptions<ZohoTimesheetDBContext> options) : base(options)
        { }

        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<TaskDetail> TaskDetails { get; set; }
        public virtual DbSet<Timesheet> Timesheets { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectTask> ProjectTasks { get; set; }
        public virtual DbSet<Approver> Approvers { get; set; }
        public virtual DbSet<Source> Sources { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=DefaultConnection");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Users>(entity =>
            {
                entity.Property(e => e.Email).IsUnicode(false);
            });

            modelBuilder.Entity<ProjectTask>(task =>
            {
                task.HasOne(d => d.project)
                    .WithMany(p => p.TaskForProject)
                    .HasForeignKey(d => d.ProjectId);
            });

            modelBuilder.Entity<Timesheet>(sheet =>
            {
                sheet.HasOne(d => d.source)
                    .WithMany(p => p.SourceForTimesheet)
                    .HasForeignKey(d => d.SourceId);

                sheet.HasOne(d => d.task)
                  .WithMany(p => p.TaskForTimesheet)
                  .HasForeignKey(d => d.InternalTaskId);
            });

            modelBuilder.Entity<TaskDetail>(detail =>
            {
                detail.HasOne(d => d.project)
                    .WithMany(p => p.InternalTaskforProject)
                    .HasForeignKey(d => d.ProjectId);

                detail.HasOne(d => d.projecttask)
                  .WithMany(p => p.InternalTaskforProjectTask)
                  .HasForeignKey(d => d.ProjectTaskId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        public void ApplyMigrations()
        {
            try
            {
                if (this.Database.IsSqlServer())
                {
                    if (this.Database.GetPendingMigrations().Any())
                    {
                        this.Database.Migrate();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying migrations: {ex.Message}");
                throw;
            }
        }
    }
}
