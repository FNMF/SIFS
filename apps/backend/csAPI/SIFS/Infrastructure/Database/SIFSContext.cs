using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using SIFS.Infrastructure.Persistence.Models;

namespace SIFS.Infrastructure.Database;

public partial class SIFSContext : DbContext
{
    public SIFSContext()
    {
    }

    public SIFSContext(DbContextOptions<SIFSContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AlgoTask> AlgoTasks { get; set; }

    public virtual DbSet<AlgoType> AlgoTypes { get; set; }

    public virtual DbSet<Localfile> Localfiles { get; set; }

    public virtual DbSet<ResultFile> ResultFiles { get; set; }

    public virtual DbSet<TaskList> TaskLists { get; set; }

    public virtual DbSet<TaskTypeMap> TaskTypeMaps { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=sifs;user id=root;password=123456", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AlgoTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).IsFixedLength();
            entity.Property(e => e.TaskId).IsFixedLength();
        });

        modelBuilder.Entity<AlgoType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Localfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).IsFixedLength();
            entity.Property(e => e.AlgoTaskId).IsFixedLength();
        });

        modelBuilder.Entity<ResultFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).IsFixedLength();
            entity.Property(e => e.AlgoTaskId).IsFixedLength();
        });

        modelBuilder.Entity<TaskList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).IsFixedLength();
            entity.Property(e => e.UserId).IsFixedLength();
        });

        modelBuilder.Entity<TaskTypeMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).IsFixedLength();
            entity.Property(e => e.TaskId).IsFixedLength();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
