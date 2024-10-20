using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DataContracts.Data
{
  /// <summary>
  /// Представляет конфигурацию приложения, загружаемую из YAML файла.
  /// </summary>
  public class Config
  {
    /// <summary>
    /// Получает или устанавливает строку подключения к базе данных.
    /// </summary>
    public string DatabaseConnectionString { get; private set; }

    /// <summary>
    /// Получает или устанавливает токен бота Telegram.
    /// </summary>
    public string BotToken { get; private set; }

    /// <summary>
    /// Получает или устанавливает идентификатор администратора бота.
    /// </summary>
    public long AdminId { get; set; }

    // Параметрный конструктор без параметров
    public Config() { }

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
      
      this.DatabaseConnectionString = data.DatabaseConnectionString;
      this.BotToken = data.BotToken;
      this.AdminId = data.AdminId;
    }
  }
}
