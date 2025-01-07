using HomeWorkTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeWorkTelegramBot.DataBase.Configurations
{
  public class CoursesConfiguration : IEntityTypeConfiguration<Courses>
  {
    public void Configure(EntityTypeBuilder<Courses> builder)
    {
      builder.HasKey(c => c.Id);

      builder
        .HasMany(c => c.Registrations)
        .WithOne(r => r.Course);

      builder
        .HasOne(c => c.CourseEnrollment)
        .WithOne(ce => ce.Course);

      builder
        .HasOne(c => c.Teacher)
        .WithMany(u => u.TeachingCourses)
        .HasForeignKey(c => c.TeacherId);
    }
  }
}
