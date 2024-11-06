namespace TelegramBot.Roles.Teacher.ViewHomeWork
{
  internal static partial class ViewHomeWorkProcessing
  {
    /// <summary>
    /// Шаг просмотра дз. 
    /// </summary>
    internal enum ViewAssignmentStep
    {
      /// <summary>
      /// Отображение опций для выбранного курса.
      /// </summary>
      DisplayCourseOptions,

      /// <summary>
      /// Выбор опции для выбранного курса. 
      /// </summary>
      ChooseOptions,

      /// <summary>
      /// Функция : "По пользователям".
      /// </summary>
      ShowStudentInfo,

      /// <summary>
      /// Функция : "По заданиям".
      /// </summary>
      ShowAssignmentInfo,

      /// <summary>
      /// Выбор задания при функции : "По пользователям".
      /// </summary>
      SelectAssignment,

      /// <summary>
      /// Выбор студента при функции : "По заданиям".
      /// </summary>
      SelectStudent,

      /// <summary>
      /// Показать задания для выбранного студента при функции : "По пользователям".
      /// </summary>
      ShowAssignmentsForStudent,

      /// <summary>
      /// Показать студентов для выбранного задания при функции : "По заданиям".
      /// </summary>
      ShowStudentsForAssignment,

      /// <summary>
      /// Выбор статуса студента при функции : "По пользователям".
      /// </summary>
      SelectStudentStatus,

      /// <summary>
      /// Выбор статуса задания при функции : "По заданиям".
      /// </summary>
      SelectAssignmentStatus,

      /// <summary>
      /// Запрос на доработку.
      /// </summary>
      RequestRevision,
    }
  }
}
