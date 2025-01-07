using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

internal class Program
{
  static async Task Main(string[] args)
  {
    Console.WriteLine("Введите ссылку на репозиторий GitHub:");
    string repoUrl = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(repoUrl))
    {
      Console.WriteLine("Ссылка не может быть пустой.");
      return;
    }

    string repoName = GetRepoNameFromUrl(repoUrl);
    string clonePath = Path.Combine(Path.GetTempPath(), repoName);

    try
    {
      // Клонируем репозиторий
      await CloneRepository(repoUrl, clonePath);

      // Компилируем проект
      bool buildSuccess = BuildProject(clonePath);

      if (buildSuccess)
      {
        Console.WriteLine("Проект успешно скомпилирован.");
        // Здесь можно добавить анализ кода и другие проверки
      }
      else
      {
        Console.WriteLine("Ошибка компиляции проекта.");
      }
    }
    finally
    {
      // Удаляем временные файлы
      try
      {
        if (Directory.Exists(clonePath))
        {
          Directory.Delete(clonePath, true);
        }
      }
      catch (UnauthorizedAccessException ex)
      {
        Console.WriteLine($"Ошибка доступа при удалении временных файлов: {ex.Message}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Не удалось удалить временные файлы: {ex.Message}");
      }
    }
  }

  static string GetRepoNameFromUrl(string url)
  {
    return new Uri(url).Segments[^1].TrimEnd('/').Replace(".git", "");
  }

  static async Task CloneRepository(string repoUrl, string clonePath)
  {
    Console.WriteLine($"Клонирование репозитория {repoUrl}...");
    var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "git",
        Arguments = $"clone {repoUrl} {clonePath}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      }
    };

    process.Start();
    string output = await process.StandardOutput.ReadToEndAsync();
    string error = await process.StandardError.ReadToEndAsync();
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
      throw new Exception($"Ошибка клонирования репозитория: {error}");
    }

    Console.WriteLine(output);
  }

  static bool BuildProject(string projectPath)
  {
    Console.WriteLine($"Компиляция проекта в {projectPath}...");
    var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "dotnet",
        Arguments = $"build {projectPath}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      }
    };

    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();
    process.WaitForExit();

    Console.WriteLine(output);
    if (process.ExitCode != 0)
    {
      Console.WriteLine($"Ошибка компиляции: {error}");
      return false;
    }

    return true;
  }
}