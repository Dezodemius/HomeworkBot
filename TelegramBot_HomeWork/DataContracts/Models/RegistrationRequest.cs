using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContracts.Models
{
  public class RegistrationRequest
  {
    public int RequestId { get; set; }
    public long TelegramChatId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int CourseId { get; set; }
    public string Status { get; set; }
  }
}
