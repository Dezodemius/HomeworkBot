using HomeWorkTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeWorkTelegramBot.DataBase.Configurations
{
  /// <summary>
  /// Настройки таблицы записи пользователя на курс.
  /// </summary>
  public class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
  {
    /// <summary>
    /// Конфигурация сущности CourseEnrollment для Entity Framework Core.
    /// </summary>
    /// <param name="builder">Строитель конфигурации типа сущности.</param>
    public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
    {
      builder
        .HasKey(ce => ce.Id);

      builder
        .HasOne(ce => ce.User)
        .WithOne(u => u.CourseEnrollment)
        .HasForeignKey<CourseEnrollment>(ce => ce.UserId)
        .OnDelete(DeleteBehavior.Cascade);

      builder
        .HasOne(ce => ce.Course)
        .WithOne(c => c.CourseEnrollment)
        .HasForeignKey<CourseEnrollment>(ce => ce.CourseId)
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
