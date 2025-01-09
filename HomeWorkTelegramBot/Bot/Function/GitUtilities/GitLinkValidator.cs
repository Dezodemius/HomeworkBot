using System;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace HomeWorkTelegramBot.Bot.Function.GitUtilities
{
  /// <summary>
  /// Перечисление кодов ошибок, которые могут возникнуть при работе с репозиториями Git.
  /// </summary>
  public enum ErrorCode
  {
    /// <summary>
    /// Ошибок нет.
    /// </summary>
    None,

    /// <summary>
    /// Неверный формат URL.
    /// </summary>
    InvalidUrlFormat,

    /// <summary>
    /// Не удалось извлечь URL репозитория из ссылки.
    /// </summary>
    UnableToExtractRepoUrl,

    /// <summary>
    /// Не удалось получить доступ к репозиторию для клонирования.
    /// </summary>
    CloneAccessFailed,

    /// <summary>
    /// Ошибка при клонировании репозитория.
    /// </summary>
    CloneFailed,

    /// <summary>
    /// Ошибка при компиляции решения.
    /// </summary>
    CompiletedError,

    /// <summary>
    /// Системная ошибка, не связанная с конкретной операцией.
    /// </summary>
    SystemError
  }

  public class GitLinkValidator
  {
    /// <summary>
    /// Проверяет, является ли ссылка на репозиторий Git валидной.
    /// </summary>
    /// <param name="repositoryUrl">URL репозитория Git.</param>
    /// <returns>Код ошибки, если ссылка не валидна; иначе ErrorCode.None.</returns>
    public ErrorCode IsValidGitLink(string repositoryUrl)
    {
      if (!IsWellFormedUrl(repositoryUrl))
      {
        Console.WriteLine("Неверный формат URL.");
        return ErrorCode.InvalidUrlFormat;
      }

      string baseRepoUrl = ExtractBaseRepoUrl(repositoryUrl);
      if (baseRepoUrl == null)
      {
        Console.WriteLine("Не удалось извлечь URL репозитория из ссылки.");
        return ErrorCode.UnableToExtractRepoUrl;
      }

      Console.WriteLine($"Найденный URL репозитория: {baseRepoUrl}");

      if (!CanCloneRepository(baseRepoUrl))
      {
        Console.WriteLine("Не удалось получить доступ к репозиторию для клонирования.");
        return ErrorCode.CloneAccessFailed;
      }

      Console.WriteLine("Ссылка на репозиторий валидна.");
      return ErrorCode.None;
    }

    /// <summary>
    /// Проверяет, является ли URL корректным.
    /// </summary>
    private bool IsWellFormedUrl(string url)
    {
      return Uri.IsWellFormedUriString(url, UriKind.Absolute);
    }

    /// <summary>
    /// Извлекает базовый URL репозитория из ссылки.
    /// </summary>
    private string ExtractBaseRepoUrl(string url)
    {
      var match = Regex.Match(url, @"^(https:\/\/github\.com\/[\w-]+\/[\w-]+)");
      return match.Success ? match.Value + ".git" : null;
    }

    /// <summary>
    /// Проверяет, можно ли клонировать репозиторий.
    /// </summary>
    private bool CanCloneRepository(string url)
    {
      try
      {
        // Проверяем доступность удаленных ссылок
        var refs = Repository.ListRemoteReferences(url);
        return refs != null && refs.Any();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Ошибка при проверке клонирования: {ex.Message}");
        return false;
      }
    }

    /// <summary>
    /// Возвращает строковое описание для заданного кода ошибки.
    /// </summary>
    /// <param name="errorCode">Код ошибки.</param>
    /// <returns>Описание ошибки.</returns>
    public static string GetErrorMessage(ErrorCode errorCode)
    {
      switch (errorCode)
      {
        case ErrorCode.None:
          return "Ошибок нет.";
        case ErrorCode.InvalidUrlFormat:
          return "Неверный формат URL.";
        case ErrorCode.UnableToExtractRepoUrl:
          return "Не удалось извлечь URL репозитория из ссылки.";
        case ErrorCode.CloneAccessFailed:
          return "Не удалось получить доступ к репозиторию для клонирования.";
        case ErrorCode.CloneFailed:
          return "Ошибка при клонировании репозитория.";
        case ErrorCode.CompiletedError:
          return "Ошибка при компиляции решения.";
        case ErrorCode.SystemError:
          return "Системная ошибка.";
        default:
          return "Неизвестная ошибка.";
      }
    }
  }
}