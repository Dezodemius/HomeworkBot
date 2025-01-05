using HomeWorkTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeWorkTelegramBot.DataBase.Configurations
{
  /// <summary>
  /// Настройки таблицы заданий курса.
  /// </summary>
  public class TaskWorConfiguration : IEntityTypeConfiguration<TaskWork>
  {
    /// <summary>
    /// Конфигурация сущности TaskWork для Entity Framework Core.
    /// </summary>
    /// <param name="builder">Строитель конфигурации типа сущности.</param>
    void IEntityTypeConfiguration<TaskWork>.Configure(EntityTypeBuilder<TaskWork> builder)
    {
      builder.HasKey(tw => tw.Id);

      builder
        .HasOne(tw => tw.Course)
        .WithMany(c => c.TaskWorks)
        .HasForeignKey(tw => tw.CourseId)
        .OnDelete(DeleteBehavior.Cascade);

      builder
        .HasMany(tw => tw.Answers)
        .WithOne(a => a.TaskWork)
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
