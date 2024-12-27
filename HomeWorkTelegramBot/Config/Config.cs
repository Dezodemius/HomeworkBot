using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HomeWorkTelegramBot.Config
{
  /// <summary>
  /// Параметры запуска.
  /// </summary>
  internal class Config
  {
    /// <summary>
    /// Получает или устанавливает строку подключения к базе данных.
    /// </summary>
    public string DataPath { get; set; }

    /// <summary>
    /// Получает или устанавливает токен бота Telegram.
    /// </summary>
    public string BotToken { get; set; }

    /// <summary>
    /// Получает или устанавливает идентификатор администратора бота.
    /// </summary>
    public long AdminId { get; set; }

    /// <summary>
    /// Загружает конфигурацию из указанного YAML файла.
    /// </summary>
    /// <param name="path">Путь к файлу конфигурации YAML.</param>
    public Config(string path)
    {
      var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

      using var reader = new StreamReader(path);
      var data = deserializer.Deserialize<Config>(reader);

      this.DataPath = data.DataPath;
      this.BotToken = data.BotToken;
      this.AdminId = data.AdminId;
    }

    /// <summary>
    /// Загружает конфигурацию из указанного YAML файла.
    /// </summary>
    public Config()
    {
    }
  }
}