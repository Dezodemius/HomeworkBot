using ModelInterfaceHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
  static public class CommonDataModel
  {
    /// <summary>
    /// Возвращает модель пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public UserModel GetUserById(long userId)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает роль пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public UserRole GetUserRoleById(long userId)
    {
      throw new NotImplementedException();
    }
  }
}
