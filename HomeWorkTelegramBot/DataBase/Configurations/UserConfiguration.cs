using HomeWorkTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeWorkTelegramBot.DataBase.Configurations
{
  /// <summary>
  /// Настройки таблицы пользователей.
  /// </summary>
  public class UserConfiguration : IEntityTypeConfiguration<Models.User>
  {
    /// <summary>
    /// Конфигурация сущности User для Entity Framework Core.
    /// </summary>
    /// <param name="builder">Строитель конфигурации типа сущности.</param>
    void IEntityTypeConfiguration<User>.Configure(EntityTypeBuilder<User> builder)
    {
      builder
        .HasKey(u => u.Id);

      builder
        .HasOne(u => u.UserRegistration)
        .WithOne(ur => ur.User)
        .HasForeignKey<Models.User>(u => u.ChatId)
        .OnDelete(DeleteBehavior.Cascade);

      builder
        .HasOne(u => u.CourseEnrollment)
        .WithOne(ce => ce.User);

      builder
        .HasMany(u => u.TeachingCourses)
        .WithOne(c => c.Teacher)
        .HasForeignKey(c => c.TeacherId)
        .HasPrincipalKey(u => u.ChatId);
    }
  }
}