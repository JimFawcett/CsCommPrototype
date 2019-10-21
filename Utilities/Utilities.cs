/////////////////////////////////////////////////////////////////////////
// Utilities.cs - CommService helpers                                  //
// ver 2.1                                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4    //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - none
 */
/*
 * Maintenance History:
 * --------------------
 * ver 1.1 : 24 Oct 2015
 * - added url parsing functions and other helpers
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Project4Starter
{
  static public class Utilities
  {
    public static void title(this string aString, char underline = '-')
    {
      Console.Write("\n  {0}", aString);
      Console.Write("\n {0}", new string(underline, aString.Length + 2));
    }
    //----< helper makes it easy to grab endpoints >---------------------

    static public string processCommandLineForLocal(string[] args, string localUrl)
    {
      for (int i = 0; i < args.Length; ++i)
      {
        if ((args.Length > i + 1) && (args[i] == "/l" || args[i] == "/L"))
        {
          localUrl = args[i + 1];
        }
      }
      return localUrl;
    }

    static public string processCommandLineForRemote(string[] args, string remoteUrl)
    {
      for (int i = 0; i < args.Length; ++i)
      {
        if ((args.Length > i + 1) && (args[i] == "/r" || args[i] == "/R"))
        {
          remoteUrl = args[i + 1];
        }
      }
      return remoteUrl;
    }

    //----< helper functions to construct url strings >------------------
    public static string makeUrl(string address, string port)
    {
      return "http://" + address + ":" + port + "/CommService";
    }
    public static string urlPort(string url)
    {
      int posColon = url.LastIndexOf(':');
      int posSlash = url.LastIndexOf('/');
      string port = url.Substring(posColon + 1, posSlash - posColon - 1);
      return port;
    }
    public static string urlAddress(string url)
    {
      int posFirstColon = url.IndexOf(':');
      int posLastColon = url.LastIndexOf(':');
      string port = url.Substring(posFirstColon + 3, posLastColon - posFirstColon - 3);
      return port;
    }

    public static void swapUrls(ref Message msg)
    {
      string temp = msg.fromUrl;
      msg.fromUrl = msg.toUrl;
      msg.toUrl = temp;
    }

    public static bool verbose { get; set; } = false;

    public static void waitForUser()
    {
      Thread.Sleep(200);
      Console.Write("\n  press any key to quit: ");
      Console.ReadKey();
    }

    public static void showMessage(Message msg)
    {
      Console.Write("\n  msg.fromUrl: {0}", msg.fromUrl);
      Console.Write("\n  msg.toUrl:   {0}", msg.toUrl);
      Console.Write("\n  msg.content: {0}", msg.content);
    }

    static void Main(string[] args)
    {
      "testing utilities".title('=');
      Console.WriteLine();

      "testing makeUrl".title();
      string localUrl = Utilities.makeUrl("localhost", "7070");
      string remoteUrl = Utilities.makeUrl("localhost", "7071");
      Console.Write("\n  localUrl  = {0}", localUrl);
      Console.Write("\n  remoteUrl = {0}", remoteUrl);
      Console.WriteLine();

      "testing url parsing".title();
      string port = urlPort(localUrl);
      string addr = urlAddress(localUrl);
      Console.Write("\n  local port = {0}", port);
      Console.Write("\n  local addr = {0}", addr);
      Console.WriteLine();

      "testing processCommandLine".title();
      localUrl = Utilities.processCommandLineForLocal(args, localUrl);
      remoteUrl = Utilities.processCommandLineForRemote(args, remoteUrl);
      Console.Write("\n  localUrl  = {0}", localUrl);
      Console.Write("\n  remoteUrl = {0}", remoteUrl);
      Console.WriteLine();

      "testing swapUrls(ref Message msg)".title();
      Message msg = new Message();
      msg.toUrl = "http://localhost:8080/CommService";
      msg.fromUrl = "http://localhost:8081/CommService";
      msg.content = "swapee";
      Utilities.showMessage(msg);
      Console.WriteLine();

      Utilities.swapUrls(ref msg);
      Utilities.showMessage(msg);
      Console.Write("\n\n");
    }
  }
}
