/////////////////////////////////////////////////////////////////////////
// Server.cs - CommService server                                      //
// ver 2.2                                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4    //
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - This server now receives and then sends back received messages.
 */
/*
 * Plans:
 * - Add message decoding and NoSqlDb calls in performanceServiceAction.
 * - Provide requirements testing in requirementsServiceAction, perhaps
 *   used in a console client application separate from Performance 
 *   Testing GUI.
 */
/*
 * Maintenance History:
 * --------------------
 * ver 2.3 : 29 Oct 2015
 * - added handling of special messages: 
 *   "connection start message", "done", "closeServer"
 * ver 2.2 : 25 Oct 2015
 * - minor changes to display
 * ver 2.1 : 24 Oct 2015
 * - added Sender so Server can echo back messages it receives
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 2.0 : 20 Oct 2015
 * - Defined Receiver and used that to replace almost all of the
 *   original Server's functionality.
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project4Starter
{
  using Util = Utilities;

  class Server
  {
    string address { get; set; } = "localhost";
    string port { get; set; } = "8080";

    //----< quick way to grab ports and addresses from commandline >-----

    public void ProcessCommandLine(string[] args)
    {
      if (args.Length > 0)
      {
        port = args[0];
      }
      if (args.Length > 1)
      {
        address = args[1];
      }
    }
    static void Main(string[] args)
    {
      Util.verbose = false;
      Server srvr = new Server();
      srvr.ProcessCommandLine(args);

      Console.Title = "Server";
      Console.Write(String.Format("\n  Starting CommService server listening on port {0}", srvr.port));
      Console.Write("\n ====================================================\n");

      Sender sndr = new Sender(Util.makeUrl(srvr.address, srvr.port));
      //Sender sndr = new Sender();
      Receiver rcvr = new Receiver(srvr.port, srvr.address);

      // - serviceAction defines what the server does with received messages
      // - This serviceAction just announces incoming messages and echos them
      //   back to the sender.  
      // - Note that demonstrates sender routing works if you run more than
      //   one client.

      Action serviceAction = () =>
      {
        Message msg = null;
        while (true)
        {
          msg = rcvr.getMessage();   // note use of non-service method to deQ messages
          Console.Write("\n  Received message:");
          Console.Write("\n  sender is {0}", msg.fromUrl);
          Console.Write("\n  content is {0}\n", msg.content);

          if (msg.content == "connection start message")
          {
            continue; // don't send back start message
          }
          if (msg.content == "done")
          {
            Console.Write("\n  client has finished\n");
            continue;
          }
          if (msg.content == "closeServer")
          {
            Console.Write("received closeServer");
            break;
          }
          msg.content = "received " + msg.content + " from " + msg.fromUrl;

          // swap urls for outgoing message
          Util.swapUrls(ref msg);

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
      };

      if (rcvr.StartService())
      {
        rcvr.doService(serviceAction); // This serviceAction is asynchronous,
      }                                // so the call doesn't block.
      Util.waitForUser(); 
    }
  }
}
