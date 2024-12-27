using HomeWorkTelegramBot.DataBase;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Config
{
  internal class ApplicationData
  {
    /// <summary>
    /// Получает или устанавливает конфигурацию приложения.
    /// </summary>
    public static Config ConfigApp { get; private set; }

    /// <summary>
    /// Контекст базы данных приложения.
    /// </summary>
    public static ApplicationDbContext DbContext { get; set; }

    /// <summary>
    /// Статический конструктор для инициализации конфигурации приложения.
    /// </summary>
    static ApplicationData()
    {
      const string configFilePath = "config.yaml";

      ConfigApp = new Config(configFilePath);
      LogInformation("Конфигурация загружена из файла.");
    }
  }
}
