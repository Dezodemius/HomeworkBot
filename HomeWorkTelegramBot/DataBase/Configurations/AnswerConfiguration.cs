using HomeWorkTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeWorkTelegramBot.DataBase.Configurations
{
  /// <summary>
  /// Настройки таблицы ответов.
  /// </summary>
  public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
  {
    /// <summary>
    /// Конфигурация сущности Answer для Entity Framework Core.
    /// </summary>
    /// <param name="builder">Строитель конфигурации типа сущности.</param>
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
      builder.HasKey(a => a.Id);

      builder
        .HasOne(a => a.TaskWork)
        .WithMany(tw => tw.Answers)
        .HasForeignKey(a => a.TaskId)
        .OnDelete(DeleteBehavior.Cascade);

      builder
        .HasOne(a => a.Course)
        .WithMany(c => c.Answers)
        .HasForeignKey(a => a.CourseId)
        .OnDelete(DeleteBehavior.Cascade);

      builder
        .HasOne(a => a.User)
        .WithOne(u => u.Answer)
        .HasForeignKey<Answer>(a => a.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
