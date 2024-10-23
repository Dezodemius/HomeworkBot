using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContracts.Data
{
  static public class ApplicationData
  {
    public static Config ConfigApp { get; set; }

    static ApplicationData()
    {
      ConfigApp = new Config("config.yaml");
    }
  }
}
