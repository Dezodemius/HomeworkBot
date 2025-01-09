using AI;
using HomeWorkTelegramBot.Bot.Function.GitUtilities;
using LibGit2Sharp;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions; // Добавьте пространство имен для использования StyleCopConfigManager

class Program
{
  static void Main(string[] args)
  {
    // Установка кодировки UTF-8 для консоли
    Console.OutputEncoding = Encoding.UTF8;
    string repositoryUrl = "https://github.com/Dezodemius/HomeworkBot/tree/debug_AI"; // Console.ReadLine();
    // var result = GitLinkValidator.GetErrorMessage(new GitRepositoryManager().CloneAndPrepareRepository(repositoryUrl, 0, 0, 0));
    // Console.WriteLine(result);
    return;

    // Извлечение имени репозитория из URL
    string repoName = GetRepositoryName(repositoryUrl);
    string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClonedRepo", repoName + "_" + Guid.NewGuid().ToString());

    try
    {
      CloneRepository(repositoryUrl, localPath);
      if (Directory.Exists(localPath))
      {
        string solutionFilePath = FindSolutionFile(localPath);
        if (!string.IsNullOrEmpty(solutionFilePath))
        {
          // Установка StyleCop.Analyzers
          AddStyleCopAnalyzersToSolution(solutionFilePath);

          var styleCopConfigManager = new StyleCopConfigManager(solutionFilePath);
          styleCopConfigManager.EnsureConfigExists();

          // Проверка, что файл был создан
          string configFilePath = Path.Combine(Path.GetDirectoryName(solutionFilePath), ".editorconfig");
          if (File.Exists(configFilePath))
          {
            Console.WriteLine($"Файл .editorconfig успешно создан в {configFilePath}");
          }
          else
          {
            Console.WriteLine("Ошибка: файл .editorconfig не был создан.");
          }

          RestorePackages(solutionFilePath);
          BuildSolution(solutionFilePath);
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Ошибка: {ex.Message}");
    }
    finally
    {
      TryDeleteDirectory(localPath);
    }
  }

  static void BuildSolution(string solutionFilePath)
  {
    var output = ExecuteBuildProcess(solutionFilePath);
    ProcessAndGroupMessages(output);
  }

  static string ExecuteBuildProcess(string solutionFilePath)
  {
    var startInfo = new ProcessStartInfo("dotnet", $"build \"{solutionFilePath}\"")
    {
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    using (var process = Process.Start(startInfo))
    {
      string output = process.StandardOutput.ReadToEnd();
      string errors = process.StandardError.ReadToEnd();
      process.WaitForExit();

      // Вывод результата компиляции
      if (process.ExitCode == 0)
      {
        Console.WriteLine("Компиляция завершена успешно.");
      }
      else
      {
        Console.WriteLine("Компиляция завершена с ошибками.");
      }

      return output;
    }
  }


  static void ProcessAndGroupMessages(string output)
  {
    // Регулярное выражение для извлечения пути, строки и столбца
    var regex = new Regex(@"^(.*?\.cs)\((\d+),(\d+)\):\s*(.*)$");

    // Разделение строк по новой строке
    var lines = output.Split('\n');

    // Группировка сообщений по файлам
    var groupedMessages = lines
        .Select(line => regex.Match(line))
        .Where(match => match.Success)
        .GroupBy(match => match.Groups[1].Value.Trim());

    foreach (var group in groupedMessages)
    {
      string filePath = group.Key;
      Console.WriteLine($"\n\u001b[32mФАЙЛ: {filePath}\u001b[0m"); // Зеленый цвет

      foreach (var match in group)
      {
        string lineNumber = match.Groups[2].Value.Trim();
        string columnNumber = match.Groups[3].Value.Trim();
        string message = match.Groups[4].Value.Trim();

        // Удаление части сообщения в квадратных скобках
        message = Regex.Replace(message, @"\s*\[.*?\]$", "").Trim();

        // Определение цвета для сообщения
        string messageColor = message.Contains("warning") ? "\u001b[33m" : "\u001b[31m"; // Желтый для warning, красный для error

        Console.WriteLine($"  \u001b[37mСтрока ({lineNumber},{columnNumber})\u001b[0m {messageColor}Сообщение: {message}\u001b[0m");
      }
    }
  }

  static string GetRepositoryName(string repositoryUrl)
  {
    // Извлечение имени репозитория из URL
    return new Uri(repositoryUrl).Segments[^1].TrimEnd('/');
  }

  static void TryDeleteDirectory(string path)
  {
    if (Directory.Exists(path))
    {
      try
      {
        Directory.Delete(path, true);
        Console.WriteLine("Директория успешно удалена.");
      }
      catch (IOException ex)
      {
        Console.WriteLine($"Не удалось удалить директорию: {ex.Message}");
        // Попробуйте повторно удалить через некоторое время
        System.Threading.Thread.Sleep(1000);
        TryDeleteDirectory(path);
      }
      catch (UnauthorizedAccessException ex)
      {
        Console.WriteLine($"Не удалось удалить директорию: {ex.Message}");
        // Попробуйте изменить атрибуты и повторно удалить
        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
          File.SetAttributes(file, FileAttributes.Normal);
        }
        TryDeleteDirectory(path);
      }
    }
  }

  static void CloneRepository(string repositoryUrl, string localPath)
  {
    Console.WriteLine("Клонирование репозитория...");
    try
    {
      Repository.Clone(repositoryUrl, localPath);
      Console.WriteLine("Репозиторий успешно клонирован.");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Ошибка при клонировании: {ex.Message}");
    }
  }

  static void AddStyleCopAnalyzersToSolution(string projectFilePath)
  {
    var startInfo = new ProcessStartInfo("dotnet", $"add \"{projectFilePath}\" package StyleCop.Analyzers")
    {
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    using (var process = Process.Start(startInfo))
    {
      string output = process.StandardOutput.ReadToEnd();
      string errors = process.StandardError.ReadToEnd();
      process.WaitForExit();

      // Запись вывода в файл для диагностики
      File.WriteAllText("output.log", output);
      File.WriteAllText("errors.log", errors);

      if (process.ExitCode != 0)
      {
        Console.WriteLine("Ошибка при установке StyleCop.Analyzers.");
        Console.WriteLine("Вывод ошибок:");
        Console.WriteLine(errors);
      }
      else
      {
        Console.WriteLine("StyleCop.Analyzers установлен.");
      }
    }
  }

  static void RestorePackages(string solutionFilePath)
  {
    var startInfo = new ProcessStartInfo("dotnet", $"restore \"{solutionFilePath}\" --verbosity minimal")
    {
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    using (var process = Process.Start(startInfo))
    {
      string output = process.StandardOutput.ReadToEnd();
      string errors = process.StandardError.ReadToEnd();
      process.WaitForExit();

      // Фильтрация и вывод только ошибок и предупреждений
      var errorLines = output.Split('\n').Where(line => line.Contains("error") || line.Contains("warning"));
      foreach (var line in errorLines)
      {
        Console.WriteLine(line);
      }

      if (process.ExitCode != 0)
      {
        Console.WriteLine("Ошибка при восстановлении пакетов.");
      }
    }
  }

  static string FindSolutionFile(string directory)
  {
    var files = Directory.GetFiles(directory, "*.sln", SearchOption.AllDirectories);
    if (files.Length == 0)
    {
      return null;
    }
    else if (files.Length == 1)
    {
      return files[0];
    }
    else
    {
      Console.WriteLine("Найдено несколько файлов решений. Выберите один:");
      for (int i = 0; i < files.Length; i++)
      {
        Console.WriteLine($"{i + 1}: {files[i]}");
      }

      int choice;
      do
      {
        Console.Write("Введите номер файла решения: ");
      } while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > files.Length);

      return files[choice - 1];
    }
  }
}