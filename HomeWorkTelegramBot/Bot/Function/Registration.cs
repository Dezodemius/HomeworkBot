using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function
{
  internal class Registration
  {
    private readonly Dictionary<long, User> _registrationData = new Dictionary<long, User>();
    private readonly Dictionary<long, RegistrationStep> _userSteps = new Dictionary<long, RegistrationStep>();

    private enum RegistrationStep
    {
      Name,
      Surname,
      Lastname,
      BirthDate,
      Email,
      Completed
    }

    /// <summary>
    /// Обрабатывает шаг регистрации для пользователя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="input">Входные данные от пользователя.</param>
    public void ProcessRegistrationStep(long chatId, string input)
    {
      if (!_userSteps.ContainsKey(chatId))
      {
        InitializeRegistration(chatId);
      }

      var currentStep = _userSteps[chatId];
      var user = _registrationData[chatId];

      switch (currentStep)
      {
        case RegistrationStep.Name:
          ProcessNameStep(chatId, input, user);
          break;

        case RegistrationStep.Surname:
          ProcessSurnameStep(chatId, input, user);
          break;

        case RegistrationStep.Lastname:
          ProcessLastnameStep(chatId, input, user);
          break;

        case RegistrationStep.BirthDate:
          ProcessBirthDateStep(chatId, input, user);
          break;

        case RegistrationStep.Email:
          ProcessEmailStep(chatId, input, user);
          break;

        case RegistrationStep.Completed:
          LogInformation($"Регистрация пользователя {chatId} уже завершена.");
          break;
      }
    }

    /// <summary>
    /// Инициализирует процесс регистрации для нового пользователя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    private void InitializeRegistration(long chatId)
    {
      _userSteps[chatId] = RegistrationStep.Name;
      _registrationData[chatId] = new User { ChatId = chatId };
      LogInformation($"Начало регистрации для пользователя с ChatId {chatId}");
    }

    /// <summary>
    /// Обрабатывает ввод имени пользователя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="input">Имя пользователя.</param>
    /// <param name="user">Объект пользователя.</param>
    private void ProcessNameStep(long chatId, string input, User user)
    {
      user.Name = input;
      _userSteps[chatId] = RegistrationStep.Surname;
      LogInformation($"Имя пользователя {chatId} установлено: {input}");
    }

    /// <summary>
    /// Обрабатывает ввод фамилии пользователя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="input">Фамилия пользователя.</param>
    /// <param name="user">Объект пользователя.</param>
    private void ProcessSurnameStep(long chatId, string input, User user)
    {
      user.Surname = input;
      _userSteps[chatId] = RegistrationStep.Lastname;
      LogInformation($"Фамилия пользователя {chatId} установлена: {input}");
    }

    /// <summary>
    /// Обрабатывает ввод отчества пользователя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="input">Отчество пользователя.</param>
    /// <param name="user">Объект пользователя.</param>
    private void ProcessLastnameStep(long chatId, string input, User user)
    {
      user.Lastname = input;
      _userSteps[chatId] = RegistrationStep.BirthDate;
      LogInformation($"Отчество пользователя {chatId} установлено: {input}");
    }

    /// <summary>
    /// Обрабатывает ввод даты рождения пользователя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="input">Дата рождения пользователя.</param>
    /// <param name="user">Объект пользователя.</param>
    private void ProcessBirthDateStep(long chatId, string input, User user)
    {
      if (DateOnly.TryParse(input, out var birthDate))
      {
        user.BirthDate = birthDate;
        _userSteps[chatId] = RegistrationStep.Email;
        LogInformation($"Дата рождения пользователя {chatId} установлена: {input}");
      }
      else
      {
        LogWarning($"Некорректная дата рождения от пользователя {chatId}: {input}");
      }
    }

    /// <summary>
    /// Обрабатывает ввод электронной почты пользователя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="input">Электронная почта пользователя.</param>
    /// <param name="user">Объект пользователя.</param>
    private void ProcessEmailStep(long chatId, string input, User user)
    {
      user.Email = input;
      _userSteps[chatId] = RegistrationStep.Completed;
      LogInformation($"Почта пользователя {chatId} установлена: {input}");
      CompleteRegistration(chatId);
    }

    /// <summary>
    /// Завершает регистрацию пользователя.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    private void CompleteRegistration(long chatId)
    {
      if (_registrationData.TryGetValue(chatId, out var user))
      {
        UserService.AddUser(user);
        LogInformation($"Регистрация завершена для пользователя с ChatId {chatId}");
        _registrationData.Remove(chatId);
        _userSteps.Remove(chatId);
      }
    }
  }
}