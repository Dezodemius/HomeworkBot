using ModelInterfaceHub.Models;
using System.Data.SQLite;
using System.IO;

namespace Database
{
  /// <summary>
  /// Класс для управления базой данных SQLite.
  /// </summary>
  public partial class DatabaseManager
  {
    /// <summary>
    /// Строка подключения к базе данных.
    /// </summary>
    protected internal readonly string _connectionString;

    /// <summary>
    /// Инициализирует новый экземпляр класса DatabaseManager.
    /// </summary>
    /// <param name="connectionString">Строка подключения к базе данных.</param>
    public DatabaseManager(string connectionString)
    {
      _connectionString = connectionString;
    }

    /// <summary>
    /// Возвращает модель пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public UserModel GetUserById(long userId)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает модель пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<HomeWorkModel> GetAllHomeWorks(long userId)
    {
      throw new NotImplementedException();
    }
  }
}