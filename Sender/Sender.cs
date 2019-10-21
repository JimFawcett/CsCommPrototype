/////////////////////////////////////////////////////////////////////////
// Sender.cs - CommService Sender connects and sends messages          //
// ver 2.1                                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4    //
/////////////////////////////////////////////////////////////////////////
/*
 * - has a dedicated sendThread that reads from application queue,
 *   looks at message.toUrl and finds or creates a proxy for that
 *   destination, then sends message
 */
/*
 * Additions to C# Console Wizard generated code:
 * - Added reference to System.ServiceModel
 * - Added using System.ServiceModel
 * - Added references to ICommService, BlockingQueue, and Utilities
 */
/*
 * Maintenance History:
 * --------------------
 * ver 2.1 : 29 Oct 2015
 * - added statement to store proxy in connect
 * - moved proxyStore to data member instead of local variable
 * - renamed svc to proxy
 * - moved message creation out of connect attempt loop
 * - added overridable notifiers
 * - added shutDown()
 * ver 2.0 : 24 Oct 2015
 * - added sender queue and thread
 * - now, user just uses sendMessage(msg).  The sendThread examines
 *   msg destination and routes to the appropriate proxy, creating
 *   one if necessary.
 * - several helper functions added
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 1.0 : 20 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel;

namespace Project4Starter
{
  using Util = Utilities;

  ///////////////////////////////////////////////////////////////////////
  // Sender class
  // - provides a method, Message sendMessage(), to send messages
  //   to remote Receivers
  // - has public string properties for both local and remote 
  //   addresses and ports
  // - also has property to set the max number of connection retries
  //   before quitting
  //
  public class Sender
  {
    /*
     * Port should be that used by a Receiver listener.
     *
     * Address can be in any of these forms:
     * - localhost
     * - ipaddress, e.g., 192.168.1.100
     * - machine name, e.g., Godzilla
     * But must match format used by server.
     */
    public string localUrl { get; set; } = "http://localhost:8081/CommService";
    public string remoteUrl { get; set; } = "http://localhost:8080/CommService";
    public int MaxConnectAttempts { get; set; } = 10;

    ICommService proxy = null;
    SWTools.BlockingQueue<Message> sendQ = null;
    Dictionary<string, ICommService> proxyStore = new Dictionary<string, ICommService>();
    Action sendAction = null;

    //----< define send thread processing and start thread >-------------

    public Sender(string LocalUrl="http://localhost:8081/CommServer")
    {
      localUrl = LocalUrl;
      sendQ = defineSendProcessing();
      startSender();
    }
    //----< Proxy implements the service interface >---------------------
    /*
     * An instance of the proxy is what the client uses to make
     * calls on the server's remote service instance.  
     */
    ICommService CreateProxy(string remoteUrl)
    {
      BasicHttpBinding binding = new BasicHttpBinding();
      EndpointAddress address = new EndpointAddress(remoteUrl);
      ChannelFactory<ICommService> factory = new ChannelFactory<ICommService>(binding, address);
      return factory.CreateChannel();
    }
    //----< is sender connected to the specified url? >------------------

    public bool isConnected(string url)
    {
      return proxyStore.ContainsKey(url);
    }
    //----< Connect repeatedly tries to send messages to service >-------

    public bool Connect(string remoteUrl)
    {
      if(Util.verbose)
        sendMsgNotify("attempting to connect");
      if (isConnected(remoteUrl))
        return true;
      proxy = CreateProxy(remoteUrl);
      int attemptNumber = 0;
      Message startMsg = new Message();
      startMsg.fromUrl = localUrl;
      startMsg.toUrl = remoteUrl;
      startMsg.content = "connection start message";
      while (attemptNumber < MaxConnectAttempts)
      {
        try
        {
          proxy.sendMessage(startMsg);    // will throw if server isn't listening yet
          proxyStore[remoteUrl] = proxy;  // remember this proxy
          if(Util.verbose)
            sendMsgNotify("connected");
          return true;
        }
        catch
        {
          ++attemptNumber;
          sendAttemptNotify(attemptNumber);
          Thread.Sleep(100);
        }
      }
      return false;
    }
    //----< overridable message annunciator >----------------------------

    public virtual void sendMsgNotify(string msg)
    {
      Console.Write("\n  {0}\n", msg);
    }
    //----< overridable attemptHandler >---------------------------------

    public virtual void sendAttemptNotify(int attemptNumber)
    {
      Console.Write("\n  connection attempt #{0}", attemptNumber);
    }
    //----< close connection - not used in this demo >-------------------

    public void CloseConnection()
    {
      proxy = null;
    }
    //----< set send action >--------------------------------------------
    /*
     * Installs send thread action in sender.
     */
    public void setAction(Action sendAct)
    {
      sendAction = sendAct;
    }
    //----< send messages to remote Receivers >--------------------------

    public void startSender()
    {
      sendAction.Invoke();
    }
    //----< send a message to remote Receiver >--------------------------

    public bool sendMessage(Message msg)
    {
      sendQ.enQ(msg);
      return true;
    }
    //----< defines SendThread and its operations >----------------------
    /*
     * - asynchronous function defines Sender sendThread processing
     * - creates BlockingQueue<Message> to use inside Sender.sendMessage()
     * - creates and starts a thread executing that processing
     * - uses msg.toUrl to find or create a proxy for url destination
     */
    public virtual SWTools.BlockingQueue<Message> defineSendProcessing()
    {
      SWTools.BlockingQueue<Message> sendQ = new SWTools.BlockingQueue<Message>();
      Action sendAction = () =>
      {
        ThreadStart sendThreadProc = () =>
        {
          while (true)
          {
            try
            {
              Message smsg = sendQ.deQ();
              if (smsg.content == "closeSender")
              {
                Console.Write("\n  send thread quitting\n\n");
                break;
              }
              if (proxyStore.ContainsKey(smsg.toUrl))
              {
                // proxy already created so use it
                if(Util.verbose)
                  Console.Write("\n  sender sending message to service {0}", smsg.toUrl);
                proxyStore[smsg.toUrl].sendMessage(smsg);
              }
              else
              {
                // create new proxy with Connect, save it, and use it

                if (this.Connect(smsg.toUrl))  // if Connect succeeds it will set proxy and send start msg
                {
                  if(Util.verbose)
                    Console.Write("\n  sender created proxy and sending message {0}", smsg.toUrl);
                  proxyStore[smsg.toUrl] = this.proxy;  // save proxy
                  proxy.sendMessage(smsg);
                }
                else
                {
                  sendMsgNotify(String.Format("could not connect to {0}\n",smsg.toUrl));
                  continue;
                }
              }
            }
            catch(Exception ex)
            {
              sendExceptionNotify(ex);
              continue;
            }
          }
        };
        Thread t = new Thread(sendThreadProc);  // start the sendThread
        t.IsBackground = true;
        t.Start();
      };
      this.setAction(sendAction);
      return sendQ;
    }
    //----< overridable exception annunciator >--------------------------

    public virtual void sendExceptionNotify(Exception ex, string msg = "")
    {
      Console.Write("\n --- {0} ---\n", ex.Message);
    }
    //----< sets urls from CommandLine if defined there >----------------

    public void processCommandLine(string[] args)
    {
      if (args.Length > 0)
      {
        localUrl = Util.processCommandLineForLocal(args, localUrl);
        remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
      }
    }
    //----< send closeSender message to local sender >-------------------

    public void shutdown()
    {
      Message sdmsg = new Message();
      sdmsg.fromUrl = localUrl;
      sdmsg.toUrl = localUrl;
      sdmsg.content = "closeSender";
      Console.Write("\n  shutting down local sender");
      sendMessage(sdmsg);
    }
    //----< Test Stub >--------------------------------------------------

#if (TEST_SENDER)
    static void Main(string[] args)
    {
      Util.verbose = false;

      Console.Write("\n  starting CommService Sender");
      Console.Write("\n =============================\n");

      Console.Title = "CommService Sender";

      Sender sndr = new Sender("http://localhost:8081/CommService");

      sndr.processCommandLine(args);

      int numMsgs = 5;
      int counter = 0;
      Message msg = null;
      while (true)
      {
        msg = new Message();
        msg.fromUrl = sndr.localUrl;
        msg.toUrl = sndr.remoteUrl;
        msg.content = "Message #" + (++counter).ToString();
        Console.Write("\n  sending {0}", msg.content);
        sndr.sendMessage(msg);
        Thread.Sleep(30);
        if (counter >= numMsgs)
          break;
      }
      // demonstrate:
      // - sending to listener
      // - attempting to send to nolistener
      // - sending to listener

      msg = new Message();
      msg.fromUrl = sndr.localUrl;
      msg.toUrl = "http://localhost:9999/CommService";
      msg.content = "no listener for this message";
      Console.Write("\n  sending {0}", msg.content);
      sndr.sendMessage(msg);
      msg = new Message();
      msg.fromUrl = sndr.localUrl;
      msg.toUrl = sndr.remoteUrl;
      msg.content = "Message #" + (++counter).ToString();
      Console.Write("\n  sending {0}", msg.content);
      sndr.sendMessage(msg);
      msg = new Message();
      msg.fromUrl = sndr.localUrl;
      msg.toUrl = sndr.remoteUrl;
      msg.content = "closeSender";  // message for self and Receiver
      Console.Write("\n  sending {0}", msg.content);
      sndr.sendMessage(msg);
    }
#endif
  }
}
