/////////////////////////////////////////////////////////////////////////
// SimpleServer.cs - Very simple server - good starting point for      //
// ver 1.0           Project #4 database server.                       //
//                                                                     //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4    //
/////////////////////////////////////////////////////////////////////////
/*
 * Purpose:
 *----------
 * This is an unadorned server - easy to understand and build upon.
 * Note: has fixed listen port = 8079
 *
 * Additions to C# Console Wizard generated code:
 * - references to ICommService, Sender, Receiver, and Utilities
 */
/*
 * Maintenance History:
 * --------------------
 * ver 1.0 : 29 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4Starter
{
  public class SimpleSender : Sender
  {
    public bool goodStatus { get; set; } = true;

    public override void sendMsgNotify(string msg)
    {
      if (msg.Contains("could not connect"))
        goodStatus = false;
    }
  }
  class SimpleServer
  {
    static string port = "8080";
    static string address = "localhost";

    static void Main(string[] args)
    {
      Console.Title = "Simple Server";
      String.Format("Simple Server Started listing on {0}", port).title('=');

      SimpleSender sndr = new SimpleSender();
      Receiver rcvr = new Receiver(port, address);
      rcvr.StartService();

      while(true)
      {
        Message msg = rcvr.getMessage();
        Console.Write("\n  Simple Server received:");
        Utilities.showMessage(msg);
        if (msg.content == "done")
        {
          Console.WriteLine();
          rcvr.shutDown();
          sndr.shutdown();
          break;
        }
        if (msg.content == "connection start message")
          continue;
        msg.content = "Simple Server received: " + msg.content;
        Utilities.swapUrls(ref msg);
        if(sndr.goodStatus == true)
        {
#if (TEST_WPFCLIENT)
          /////////////////////////////////////////////////
          // The statements below support testing the
          // WpfClient as it receives a stream of messages
          // - for each message received the Server
          //   sends back 1000 messages
          //
          int count = 0;
          for (int i = 0; i < 1000; ++i)
          {
            Message testMsg = new Message();
            testMsg.toUrl = msg.toUrl;
            testMsg.fromUrl = msg.fromUrl;
            testMsg.content = String.Format("test message #{0}", ++count);
            Console.Write("\n  sending testMsg: {0}", testMsg.content);
            sndr.sendMessage(testMsg);
          }
#else
          /////////////////////////////////////////////////
          // Use the statement below for normal operation
          sndr.sendMessage(msg);
#endif
        }
        else
        {
          Console.Write("\n  closing\n");
          rcvr.shutDown();
          sndr.shutdown();
          break;
        }
        Console.WriteLine();
      }
    }
  }
}
