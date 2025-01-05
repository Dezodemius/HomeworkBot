using System.ComponentModel;
using System.Reflection;

namespace HomeWorkTelegramBot.Bot.Function.Teacher
{
  /// <summary>
  /// Предоставляет методы расширения для работы с перечислениями (Enum).
  /// </summary>
  public static class EnumExtentions
  {
    /// <summary>
    /// Получает значение атрибута Description.
    /// </summary>
    /// <param name="value">Значение перечисления, для которого требуется получить описание</param>
    /// <returns>Строка с описанием из атрибута Description, если атрибут существует;
    /// в противном случае возвращает строковое представление значения перечисления.</returns>
    public static string GetDescription(this Enum value)
    {
      var field = value.GetType().GetField(value.ToString());
      var attribute = field.GetCustomAttribute<DescriptionAttribute>();
      return attribute?.Description ?? value.ToString();
    }
  }
}
