using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using DataContracts.Data;
using DataContracts.Models;

namespace Core
{
  static public class CommonRegistrationRequest
  {
    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    /// <summary>
    /// Получает список всех запросов на регистрацию.
    /// </summary>
    /// <returns>Список объектов <see cref="RegistrationRequest"/>.</returns>
    static public List<RegistrationRequest> GetAllRegistrationRequests() => dbManager.GetAllRegistrationRequests();

    /// <summary>
    /// Получает запрос на регистрацию по Telegram ID.
    /// </summary>
    /// <param name="telegramId">Telegram ID пользователя.</param>
    /// <returns>Объект <see cref="RegistrationRequest"/>, если найден; иначе null.</returns>
    static public RegistrationRequest GetRegistrationRequestByTelegramId(long telegramId)
    {
      var request = dbManager.GetAllRegistrationRequests().FirstOrDefault(x => x.TelegramChatId == telegramId);
      return request;
    }

    /// <summary>
    /// Добавляет новый запрос на регистрацию.
    /// </summary>
    /// <param name="registrationRequest">Объект <see cref="RegistrationRequest"/> для добавления.</param>
    static public void AddRegistrationRequests(RegistrationRequest registrationRequest) => dbManager.CreateRegistrationRequest(registrationRequest);

    /// <summary>
    /// Обновляет статус запроса на регистрацию.
    /// </summary>
    /// <param name="registrationRequest">Объект <see cref="RegistrationRequest"/> для обновления.</param>
    /// <param name="status">Новый статус.</param>
    static public void UpdateRegistrationRequestStatus(RegistrationRequest registrationRequest, string status)
    {
      registrationRequest.Status = status;
      dbManager.UpdateRegistrationRequest(registrationRequest.RequestId, registrationRequest);
    }

    /// <summary>
    /// Удаляет запрос на регистрацию.
    /// </summary>
    /// <param name="registrationRequest">Объект <see cref="RegistrationRequest"/> для удаления.</param>
    static public void DeleteRegistrationRequest(RegistrationRequest registrationRequest) => dbManager.DeleteRegistrationRequest(registrationRequest.RequestId);
  }
}
