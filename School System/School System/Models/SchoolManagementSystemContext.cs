using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace School_System.Models;

public partial class SchoolManagementSystemContext : DbContext
{
    public SchoolManagementSystemContext()
    {
    }

    public SchoolManagementSystemContext(DbContextOptions<SchoolManagementSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

   

    public virtual DbSet<DailyAnalytic> DailyAnalytics { get; set; }

    public virtual DbSet<Fee> Fees { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notice> Notices { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<HeadTeacherClasses> HeadTeacherClasses { get; set; }
    
    public DbSet<TeacherAssignedSubject> TeacherAssignedSubjects { get; set; }

    public DbSet<TeacherAttendance> TeacherAttendances { get; set; }

    public DbSet<FeeGenerationRecord> FeeGenerationRecord  { get; set; }
    public DbSet<Certificate> Certificates { get; set; }

    public DbSet<BoardAdmission> BoardAdmissions { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=SchoolManagementSystem;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69263C1160A8A7");

            entity.ToTable("Attendance");

            entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.Status).HasMaxLength(10);
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Class).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Attendanc__Class__4D94879B");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Attendanc__Stude__4E88ABD4");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__Attendanc__Teach__4F7CD00D");
        });
        modelBuilder.Entity<BoardAdmission>(entity =>
        {
            entity.HasKey(e => e.BoardAdmissionId);

            entity.Property(e => e.StudentId).IsRequired();

         
            entity.HasOne(e => e.Student)
                  .WithMany(s => s.BoardAdmissions) // <-- add this nav property in Student
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927A048D09219");

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(50);
            entity.Property(e => e.RoomNumber).HasMaxLength(20);
            entity.Property(e => e.HeadTeacherID);
            entity.Property(e => e.TeacherName).HasMaxLength(100);

            // Add this to configure the one-to-many with Subject:
            entity.HasMany(c => c.Subjects)
                  .WithOne(s => s.Class)
                  .HasForeignKey(s => s.ClassId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(s => s.Students)
                .WithOne(c => c.Class)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<DailyAnalytic>(entity =>
        {
            entity.HasKey(e => e.AnalyticsId).HasName("PK__DailyAna__506974C39BDA104F");

            entity.Property(e => e.AnalyticsId).HasColumnName("AnalyticsID");
            entity.Property(e => e.FeeCollected).HasColumnType("money");
        });

        modelBuilder.Entity<Fee>(entity =>
        {
            entity.HasKey(e => e.FeeId).HasName("PK__Fees__B387B2092AA8A595");

            entity.Property(e => e.FeeId).HasColumnName("FeeID");
            entity.Property(e => e.FeeAmount).HasColumnType("money");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Student).WithMany(p => p.Fees)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Fees__StudentID__571DF1D5");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__Grades__54F87A3760397DD4");

            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            entity.Property(e => e.ExamType).HasMaxLength(50);
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Grades__StudentI__52593CB8");

            entity.HasOne(d => d.Subject).WithMany(p => p.Grades)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK__Grades__SubjectI__534D60F1");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Grades)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__Grades__TeacherI__5441852A");
        });

        modelBuilder.Entity<HeadTeacherClasses>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ClassName)
                  .HasMaxLength(100);

            entity.HasOne(e => e.Teacher)
                  .WithMany(t => t.HeadTeacherClasses)  
                  .HasForeignKey(e => e.TeacherId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C037CC5E14E38");

            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.Attachment).HasMaxLength(255);
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.SenderId).HasColumnName("SenderID");
            entity.Property(e => e.Timestamp).HasColumnType("datetime");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK__Messages__Receiv__5DCAEF64");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK__Messages__Sender__5CD6CB2B");
        });

        modelBuilder.Entity<Notice>(entity =>
        {
            entity.HasKey(e => e.NoticeId).HasName("PK__Notices__CE83CB85D043B93D");

            entity.Property(e => e.NoticeId).HasColumnName("NoticeID");
            entity.Property(e => e.Audience).HasMaxLength(50);
            entity.Property(e => e.AudienceValue).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.PostedByNavigation).WithMany(p => p.Notices)
                .HasForeignKey(d => d.PostedBy)
                .HasConstraintName("FK__Notices__PostedB__59FA5E80");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A41566346");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52A795B7B1E9C");

            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.GuardianInfo).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            // Student -> Class (many-to-one)
            entity.HasOne(s => s.Class)
                  .WithMany(c => c.Students)
                  .HasForeignKey(s => s.ClassId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Student -> Certificate (one-to-many)
            entity.HasMany(s => s.Certificates)
        .WithOne(c => c.Student)
        .HasForeignKey(c => c.StudentId)
        .OnDelete(DeleteBehavior.Cascade);
            // Relationship: Student → BoardAdmissions
            entity.HasMany(s => s.BoardAdmissions)
                  .WithOne(b => b.Student)
                  .HasForeignKey(b => b.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId);

            entity.Property(e => e.SubjectId)
                  .ValueGeneratedOnAdd();  // Make EF know it is DB-generated identity

            entity.Property(e => e.SubjectName)
                  .HasMaxLength(100);

            entity.Property(e => e.IsAssigned).HasDefaultValue(false);

            entity.HasMany(e => e.TeacherAssignedSubjects)
                  .WithOne(e => e.Subject)
                  .HasForeignKey(e => e.SubjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Class)
                  .WithMany(c => c.Subjects)
                  .HasForeignKey(s => s.ClassId)
                  .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId);

            entity.Property(e => e.UserId)
                .HasMaxLength(450);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Qualification)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ContactInfo)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.HireDate)
                .IsRequired();
            entity.Property(e => e.IsHeadTeacher);
            entity.Property(e => e.ClassName)
                  .HasMaxLength(100)  
                  .IsRequired(false); 


            entity.Property(e => e.Salary)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.IsPaid)
                .HasDefaultValue(false);

            // ✅ Configure relationship with TeacherAssignedSubjects
            entity.HasMany(e => e.TeacherAssignedSubjects)
                  .WithOne(e => e.Teacher)
                  .HasForeignKey(e => e.TeacherId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.TeacherAttendances)
     .WithOne(e => e.Teacher)
     .HasForeignKey(e => e.TeacherId)
     .OnDelete(DeleteBehavior.Cascade);

        });
        modelBuilder.Entity<TeacherAttendance>(entity =>
        {
            entity.ToTable("TeacherAttendance");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.TeacherId)
                .IsRequired();

            entity.Property(e => e.Date)
                .IsRequired();

            entity.Property(e => e.IsPresent);

            entity.Property(e => e.IsMarked)
                .IsRequired();


            entity.Property(e => e.Remarks)
                .HasMaxLength(500); // optional length limit

            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.TeacherAttendances) // <-- You’ll need to add `ICollection<TeacherAttendance>` to `Teacher` model
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TeacherAssignedSubject>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SubjectName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.ClassName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.IsAssigned)
                  .HasDefaultValue(true);

            entity.HasOne(e => e.Teacher)
                  .WithMany(t => t.TeacherAssignedSubjects)
                  .HasForeignKey(e => e.TeacherId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Subject)
                  .WithMany(t => t.TeacherAssignedSubjects)
                  .HasForeignKey(e => e.SubjectId)
                  .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<FeeGenerationRecord>(entity =>
        {
            entity.HasKey(f => f.Id); // Primary key

            // Unique index so only one record per month/year
            entity.HasIndex(f => new { f.Year, f.Month })
                  .IsUnique();

            entity.Property(f => f.Year)
                  .IsRequired();

            entity.Property(f => f.Month)
                  .IsRequired();

           
               
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
