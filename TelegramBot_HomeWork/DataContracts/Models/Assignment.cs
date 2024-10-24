namespace DataContracts.Models
{
  /// <summary>
  /// Перечисление шагов регистрации.
  /// </summary>
  public enum CreateHomeworkStep
  {
    Start,
    Course,
    Name,
    Description,
    DueDate,
    Completed
  }
  public class Assignment
  {
    public int AssignmentId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }

    private CreateHomeworkStep Step { get; set; }

    public CreateHomeworkStep GetStep()
    {
      return Step;
    }

    public void SetStep(CreateHomeworkStep registrationStep)
    {
      Step = registrationStep;
    }
  }
}
