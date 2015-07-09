using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

public class IPServer
{
    public IPServer(AgDroneBridge.MainWindow app)
	{
        mApp = app;
	}

    private int mLocalPort;
    private int mRemotePort;
    private string mRemoteAddress;
    private Thread mServerThread;
    private Thread mClientThread;
    private AgDroneBridge.MainWindow mApp;
    private Stream mRemoteStream;
    private Socket mLocalSocket;
    private Int64 mADSent;
    private Int64 mADReceived;
    private Int64 mMPSent;
    private Int64 mMPReceived;

    public void Start(int localPort, string remoteAddress, int remotePort)
    {
        mLocalPort = localPort;
        mRemotePort = remotePort;
        mRemoteAddress = remoteAddress;

        mServerThread = new Thread(new ThreadStart(Server));
        mServerThread.Start();

        mClientThread = new Thread(new ThreadStart(Client));
        mClientThread.Start();
    }

    public void Stop()
    {
        mServerThread.Abort();
       // mServerThread.Join();

        mClientThread.Abort();
       // mServerThread.Join();
    }
    public void Server()
    {
        try 
        {
            IPAddress ipAd = IPAddress.Parse("127.0.0.1");
             // use local m/c IP address, and 
             // use the same in the client

    /* Initializes the Listener */
            TcpListener myList=new TcpListener(ipAd, mLocalPort);

    /* Start Listeneting at the specified port */        
            myList.Start();

            Console.WriteLine("The server is running at port " + mLocalPort);    
            Console.WriteLine("The local End point is  :" + 
                              myList.LocalEndpoint );
            Console.WriteLine("Waiting for a connection.....");

            mLocalSocket = myList.AcceptSocket();
            Console.WriteLine("Connection accepted from " + mLocalSocket.RemoteEndPoint);
        
            byte[] buffer=new byte[200];
            int len;
            while (mLocalSocket.Connected)
            {
                
                //lock (mLocalSocket)
                {
                    len = mLocalSocket.Receive(buffer);
                }

                mMPReceived += len;
                mApp.SetMPReceived(mMPReceived);

                try
                {
                    if (mRemoteStream != null)
                    {
                        //lock (mRemoteStream)
                        {
                            mRemoteStream.Write(buffer, 0, len);
                        }
                        mADSent += len;
                        mApp.SetADSent(mADSent);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error sending to AD " + e.ToString());
                }

            }
            Console.WriteLine("Local Socket was closed\n");
            mLocalSocket.Close();
            mLocalSocket = null;
            
            //myList.Stop();
            
        }
        catch (Exception e) {
            Console.WriteLine("Error..... " + e.StackTrace);
        }    
    }

    public void Client() {
        while (true)
        {

            try
            {
                TcpClient tcpclnt = new TcpClient();
                Console.WriteLine("Connecting to AgDrone at " + mRemoteAddress + " " + mRemotePort);

                tcpclnt.Connect(mRemoteAddress, mRemotePort);
                // use the ipaddress as in the server program

                Console.WriteLine("AgDrone Connected");
                Console.Write("Enter the string to be transmitted : ");

                mRemoteStream = tcpclnt.GetStream();

                byte[] buffer = new byte[200];
                int len;
                while (tcpclnt.Connected) // (tcpclnt.Available != 0)
                {
                    //lock (mRemoteStream)
                    {
                        len = mRemoteStream.Read(buffer, 0, 200);
                    }
                    mADReceived += len;
                    mApp.SetADReceived(mADReceived);
                    try
                    {
                        //lock (mLocalSocket)
                        {
                            mLocalSocket.Send(buffer, len, SocketFlags.None);
                        }
                        mMPSent += len;
                        mApp.SetMPSent(mMPSent);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error sending to MP " + e.ToString());
                    }

                }

                Console.WriteLine("Remote connection was closed\n");

                mRemoteStream.Close();
                mRemoteStream = null;

                tcpclnt.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }
    }


}
