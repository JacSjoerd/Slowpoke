using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flintstones
{
  public static class Logger
  {
    public static void Debug(string message)
    {
#if DEBUG
      Console.WriteLine($"[DEBUG] {message}");
#endif
    }

    public static void Info(string message)
    {
      Console.WriteLine($"[INFO] {message}");
    }

    public static void Error(string message)
    {
      Console.WriteLine($"[ERROR] {message}");
    }
  }
}
