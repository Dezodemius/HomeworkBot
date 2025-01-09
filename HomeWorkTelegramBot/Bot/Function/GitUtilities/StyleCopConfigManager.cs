using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.Bot.Function.GitUtilities
{
  internal class StyleCopConfigManager
  {
    private readonly string configFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="StyleCopConfigManager"/> class.
    /// </summary>
    /// <param name="solutionFilePath">The path to the solution file where the configuration file will be located.</param>
    public StyleCopConfigManager(string solutionFilePath)
    {
      var solutionDirectory = Path.GetDirectoryName(solutionFilePath);
      configFilePath = Path.Combine(solutionDirectory, ".editorconfig");
    }

    /// <summary>
    /// Ensures that the .editorconfig file exists.
    /// </summary>
    public void EnsureConfigExists()
    {
      if (File.Exists(configFilePath))
      {
        File.Delete(configFilePath);
        Console.WriteLine($"Существующий файл .editorconfig удален: {configFilePath}");
      }

      CreateEditorConfig();
    }

    private void CreateEditorConfig()
    {
      var editorConfig = new StringBuilder();
      editorConfig.AppendLine("[*.cs]");
      editorConfig.AppendLine();
      AppendIgnoredWarnings(editorConfig);
      AppendErrorConfigurations(editorConfig);

      File.WriteAllText(configFilePath, editorConfig.ToString());
      Console.WriteLine($"Создан файл .editorconfig по адресу: {configFilePath}");
    }

    private void AppendIgnoredWarnings(StringBuilder editorConfig)
    {
      editorConfig.AppendLine("dotnet_diagnostic.SA1003.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1101.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1117.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1124.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1127.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1200.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1201.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1202.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1204.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1206.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1208.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1210.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1216.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1307.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1309.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1311.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1312.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1400.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1401.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1500.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1501.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1509.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1512.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1515.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1623.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1633.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.SA1642.severity = none");

      editorConfig.AppendLine("dotnet_diagnostic.CS0618.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.CS8600.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.CS8601.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.CS8602.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.CS8603.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.CS8604.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.CS8618.severity = none");
      editorConfig.AppendLine("dotnet_diagnostic.CS8625.severity = none");
    }

    private void AppendErrorConfigurations(StringBuilder editorConfig)
    {
      editorConfig.AppendLine("dotnet_diagnostic.SA1000.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1009.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1013.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1025.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1028.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1107.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1111.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1122.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1137.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1413.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1505.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1507.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1508.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1513.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1516.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.SA1600.severity = error");

      editorConfig.AppendLine("dotnet_diagnostic.CA1600.severity = error");

      editorConfig.AppendLine("dotnet_diagnostic.CS1998.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.CS4014.severity = error");
      editorConfig.AppendLine("dotnet_diagnostic.CS8073.severity = error");
    }
  }
}
