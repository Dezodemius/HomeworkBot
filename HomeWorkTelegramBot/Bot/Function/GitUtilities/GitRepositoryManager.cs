using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.Models;
using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HomeWorkTelegramBot.Bot.Function.GitUtilities
{
  /// <summary>
  /// Управляет операциями с репозиториями Git.
  /// </summary>
  public class GitRepositoryManager
  {
    /// <summary>
    /// Клонирует репозиторий и выполняет подготовительные действия.
    /// </summary>
    /// <param name="repositoryUrl">URL репозитория Git.</param>
    /// <returns>Вовзвращает результат проверки.</returns>
    public ErrorCode CloneAndPrepareRepository(string repositoryUrl, CourseEnrollment courseEnrollment, Answer answer)
    {
      ErrorCode errorCode;
      if ((errorCode = new GitLinkValidator().IsValidGitLink(repositoryUrl)) != ErrorCode.None)
      {
        return errorCode;
      }

      string baseRepoUrl = GetBaseRepositoryUrl(repositoryUrl);
      string repoName = GetRepositoryName(baseRepoUrl);
      string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClonedRepo", repoName + "_" + Guid.NewGuid().ToString());

      try
      {
        string branchName = ExtractBranchName(repositoryUrl);
        CloneRepository(baseRepoUrl, localPath, branchName);
        errorCode = PrepareRepository(localPath, courseEnrollment, answer);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Ошибка при клонировании: {ex.Message}");
        return ErrorCode.CloneFailed;
      }
      finally
      {
        TryDeleteDirectory(localPath);
      }

      return errorCode;
    }

    /// <summary>
    /// Извлекает имя репозитория из URL.
    /// </summary>
    /// <param name="repositoryUrl">URL репозитория Git.</param>
    /// <returns>Имя репозитория.</returns>
    private static string GetBaseRepositoryUrl(string repositoryUrl)
    {
      var uri = new Uri(repositoryUrl);
      return $"{uri.Scheme}://{uri.Host}{uri.Segments[0]}{uri.Segments[1]}{uri.Segments[2].TrimEnd('/')}";
    }

    /// <summary>
    /// Извлекает имя репозитория из базового URL.
    /// </summary>
    /// <param name="baseRepoUrl">Базовый URL репозитория Git.</param>
    /// <returns>Имя репозитория.</returns>
    private static string GetRepositoryName(string baseRepoUrl)
    {
      var uri = new Uri(baseRepoUrl);
      return uri.Segments[2].TrimEnd('/');
    }

    /// <summary>
    /// Извлекает имя ветки из URL.
    /// </summary>
    /// <param name="url">URL репозитория Git.</param>
    /// <returns>Имя ветки.</returns>
    private string ExtractBranchName(string url)
    {
      var match = Regex.Match(url, @"/tree/([\w-]+)");
      return match.Success ? match.Groups[1].Value : "main";
    }

    /// <summary>
    /// Клонирует репозиторий на указанную ветку.
    /// </summary>
    /// <param name="repositoryUrl">URL репозитория Git.</param>
    /// <param name="localPath">Локальный путь для клонирования.</param>
    /// <param name="branchName">Имя ветки для клонирования.</param>
    private void CloneRepository(string repositoryUrl, string localPath, string branchName)
    {
      var cloneOptions = new CloneOptions
      {
        BranchName = branchName,
        Checkout = true
      };

      Repository.Clone(repositoryUrl, localPath, cloneOptions);
      Console.WriteLine($"Репозиторий успешно клонирован на ветку {branchName}.");
    }

    /// <summary>
    /// Выполняет подготовительные действия после клонирования репозитория.
    /// </summary>
    /// <param name="localPath">Локальный путь к клонированному репозиторию.</param>
    private ErrorCode PrepareRepository(string localPath, CourseEnrollment courseEnrollment, Answer answer)
    {
      string solutionFilePath = FindSolutionFile(localPath);
      if (!string.IsNullOrEmpty(solutionFilePath))
      {
        ConfigureStyleCop(solutionFilePath);
        RestorePackages(solutionFilePath);
        return BuildSolution(solutionFilePath, courseEnrollment, answer);
      }
      else
      {
        return ErrorCode.SystemError;
      }
    }

    /// <summary>
    /// Находит файл решения в указанной директории.
    /// </summary>
    /// <param name="directory">Директория для поиска.</param>
    /// <returns>Путь к файлу решения или null, если файл не найден.</returns>
    private string FindSolutionFile(string directory)
    {
      var files = Directory.GetFiles(directory, "*.sln", SearchOption.AllDirectories);
      return files.Length > 0 ? files[0] : null;
    }

    /// <summary>
    /// Настраивает StyleCop для указанного решения.
    /// </summary>
    /// <param name="solutionFilePath">Путь к файлу решения.</param>
    private void ConfigureStyleCop(string solutionFilePath)
    {
      AddStyleCopAnalyzersToSolution(solutionFilePath);
      var styleCopConfigManager = new StyleCopConfigManager(solutionFilePath);
      styleCopConfigManager.EnsureConfigExists();
    }

    /// <summary>
    /// Добавляет StyleCop.Analyzers к проекту.
    /// </summary>
    /// <param name="projectFilePath">Путь к файлу проекта.</param>
    private void AddStyleCopAnalyzersToSolution(string projectFilePath)
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

    /// <summary>
    /// Восстанавливает пакеты для указанного решения.
    /// </summary>
    /// <param name="solutionFilePath">Путь к файлу решения.</param>
    private void RestorePackages(string solutionFilePath)
    {
      ExecuteProcess("dotnet", $"restore \"{solutionFilePath}\"");
    }

    /// <summary>
    /// Собирает указанное решение.
    /// </summary>
    /// <param name="solutionFilePath">Путь к файлу решения.</param>
    private ErrorCode BuildSolution(string solutionFilePath, CourseEnrollment courseEnrollment, Answer answerId)
    {
      var output = ExecuteBuildProcess(solutionFilePath);
      if (ProcessAndGroupMessages(output, courseEnrollment, answerId))
      {
        return ErrorCode.CompiletedError;
      }

      return ErrorCode.None;
    }

    /// <summary>
    /// Выполняет процесс сборки.
    /// </summary>
    /// <param name="solutionFilePath">Путь к файлу решения.</param>
    /// <returns>Вывод процесса сборки.</returns>
    private static string ExecuteBuildProcess(string solutionFilePath)
    {
      return ExecuteProcess("dotnet", $"build \"{solutionFilePath}\"");
    }

    /// <summary>
    /// Выполняет процесс и возвращает его вывод.
    /// </summary>
    /// <param name="command">Команда для выполнения.</param>
    /// <param name="arguments">Аргументы команды.</param>
    /// <returns>Вывод процесса.</returns>
    private static string ExecuteProcess(string command, string arguments)
    {
      var startInfo = new ProcessStartInfo(command, arguments)
      {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
      };

      using (var process = Process.Start(startInfo))
      {
        string output = process.StandardOutput.ReadToEnd();
        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
          Console.WriteLine($"Ошибка при выполнении команды {command}.");
          Console.WriteLine("Вывод ошибок:");
          Console.WriteLine(errors);
        }

        return output;
      }
    }

    /// <summary>
    /// Обрабатывает и группирует сообщения из вывода процесса и сохраняет их в текстовый файл.
    /// </summary>
    /// <param name="output">Вывод процесса.</param>
    /// <returns>True, если есть ошибки; иначе False.</returns>
    private static bool ProcessAndGroupMessages(string output, CourseEnrollment courseEnrollment, Answer answerId)
    {
      Console.OutputEncoding = Encoding.UTF8;
      var regex = new Regex(@"^(.*?\.cs)\((\d+),(\d+)\):\s*(.*)$");
      var lines = output.Split('\n');
      var groupedMessages = lines
          .Select(line => regex.Match(line))
          .Where(match => match.Success)
          .GroupBy(match => match.Groups[1].Value.Trim());

      var textBuilder = new StringBuilder();
      bool hasErrors = false;

      foreach (var group in groupedMessages)
      {
        string filePath = group.Key;
        Console.WriteLine($"\n\u001b[32mФАЙЛ: {filePath}\u001b[0m");
        textBuilder.AppendLine($"ФАЙЛ: {filePath}");

        var uniqueMessages = new HashSet<string>();

        foreach (var match in group)
        {
          string lineNumber = match.Groups[2].Value.Trim();
          string columnNumber = match.Groups[3].Value.Trim();
          string message = match.Groups[4].Value.Trim();
          message = Regex.Replace(message, @"\s*\[.*?\]$", "").Trim();
          string messageColor = message.Contains("warning") ? "\u001b[33m" : "\u001b[31m";

          string fullMessage = $"Строка ({lineNumber},{columnNumber}) {messageColor}Сообщение: {message}\u001b[0m";

          if (uniqueMessages.Add(fullMessage))
          {
            Console.WriteLine($"  \u001b[37m{fullMessage}");
            textBuilder.AppendLine($"Строка ({lineNumber},{columnNumber}) Сообщение: {message}");

            if (messageColor.Contains("\u001b[31m"))
            {
              hasErrors = true;
            }
          }
        }
      }

      if (hasErrors)
      {
        var course = CourseService.GetCourseById(courseEnrollment.Id);
        var user = UserService.GetUserByChatId(courseEnrollment.UserId);
        var task = TaskWorkService.GetTaskWorkById(answerId.TaskId);

        string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HomeWorkAnswers", course.Name, $"{user.Name} {user.Surname} {user.Lastname}");
        Directory.CreateDirectory(directoryPath);
        string filePath = Path.Combine(directoryPath, $"{task.Name}.txt");

        using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
          writer.Write(textBuilder.ToString());
        }
      }

      return hasErrors;
    }

    /// <summary>
    /// Удаляет локальную директорию после завершения работы.
    /// </summary>
    /// <param name="path">Путь к директории.</param>
    private void TryDeleteDirectory(string path)
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
          System.Threading.Thread.Sleep(1000);
          TryDeleteDirectory(path);
        }
        catch (UnauthorizedAccessException ex)
        {
          Console.WriteLine($"Не удалось удалить директорию: {ex.Message}");
          var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
          foreach (var file in files)
          {
            File.SetAttributes(file, FileAttributes.Normal);
          }
          TryDeleteDirectory(path);
        }
      }
    }
  }
}