using System;
using System.IO;

namespace AI
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
      var editorConfig = @"
      [*.cs]
      
      # SA1200: Using directives should be placed correctly
      dotnet_diagnostic.SA1200.severity = none
      
      # SA1208: System using directives should be placed before other using directives
      dotnet_diagnostic.SA1208.severity = none
      
      # SA1642: Constructor summary documentation should begin with standard text
      dotnet_diagnostic.SA1642.severity = none
      
      # SA1623: Property summary documentation should match accessors
      dotnet_diagnostic.SA1623.severity = none
      
      # SA1210: Using directives should be ordered alphabetically by namespace
      dotnet_diagnostic.SA1210.severity = none
      
      # SA1101: Prefix local calls with this
      dotnet_diagnostic.SA1101.severity = none
      
      # SA1117: Parameters should be on same line or separate lines
      dotnet_diagnostic.SA1117.severity = silent
      
      # SA1309: Field names should not begin with underscore
      dotnet_diagnostic.SA1309.severity = none

      # SA1600: Elements should be documented

      # CS8618 : Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
      dotnet_diagnostic.CS8618.severity = none

      #CS8602 : 
      dotnet_diagnostic.CS8602.severity = none

      dotnet_diagnostic.CS8625.severity = none

      dotnet_diagnostic.CS8603.severity = none

      dotnet_diagnostic.CS8604.severity = none

      dotnet_diagnostic.SA1216.severity = none

      dotnet_diagnostic.SA1206.severity = none

      dotnet_diagnostic.SA1208.severity = none

      dotnet_diagnostic.SA1633.severity = none

      dotnet_diagnostic.SA1202.severity = none

      dotnet_diagnostic.SA1201.severity = none

      dotnet_diagnostic.SA1311.severity = none

      dotnet_diagnostic.SA1312.severity = none

      dotnet_diagnostic.SA1512.severity = none

      dotnet_diagnostic.CS8600.severity = none

      dotnet_diagnostic.CS8601.severity = none

      dotnet_diagnostic.SA1401.severity = none

      dotnet_diagnostic.SA1307.severity = none

      dotnet_diagnostic.SA1400.severity = none

      dotnet_diagnostic.CS1998.severity = error

      dotnet_diagnostic.SA1122.severity = error

      dotnet_diagnostic.SA1009.severity = error

      dotnet_diagnostic.SA1507.severity = error

      dotnet_diagnostic.SA1600.severity = error

      dotnet_diagnostic.SA1413.severity = error

      dotnet_diagnostic.SA1111.severity = error

      dotnet_diagnostic.SA1508.severity = error

      dotnet_diagnostic.SA1505.severity = error

      dotnet_diagnostic.SA1513.severity = error

      dotnet_diagnostic.SA1028.severity = error

      dotnet_diagnostic.CA1600.severity = error
      ";
      File.WriteAllText(configFilePath, editorConfig);
      Console.WriteLine($"Создан файл .editorconfig по адресу: {configFilePath}");
    }

  }
}