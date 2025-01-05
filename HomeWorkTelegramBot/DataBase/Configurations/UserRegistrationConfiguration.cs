using HomeWorkTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeWorkTelegramBot.DataBase.Configurations
{
  /// <summary>
  /// Настройки таблицы регистрации пользователей в базе данных.
  /// </summary>
  public class UserRegistrationConfiguration : IEntityTypeConfiguration<UserRegistration>
  {
    /// <summary>
    /// Конфигурация сущности UserRegistration для Entity Framework Core.
    /// </summary>
    /// <param name="builder">Строитель конфигурации типа сущности.</param>
    public void Configure(EntityTypeBuilder<UserRegistration> builder)
    {
      builder.HasKey(x => x.Id);

      builder
        .HasIndex(x => x.ChatId)
        .IsUnique();

      builder
        .HasOne(x => x.Course)
        .WithMany(c => c.Registrations)
        .HasForeignKey(x => x.CourseId)
        .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
