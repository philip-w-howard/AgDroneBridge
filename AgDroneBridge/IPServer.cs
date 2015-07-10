using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

using mavlinklib;

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
    private Stream mLocalStream;
    private Int64 mADSent;
    private Int64 mADReceived;
    private Int64 mMPSent;
    private Int64 mMPReceived;

    private const int SERVER_CHANNEL = 0;
    private const int CLIENT_CHANNEL = 1;

    int ProcessBuffer(byte[] buff, int len, int channel, Stream dest)
    {
        int count = 0;

        byte[] recv;

        for (int ii=0; ii<len; ii++)
        {
            recv = MavlinkProcessor.process_byte(channel, buff[ii]);
            if (recv.Length != 0)
            {
                count++;

                try
                {
                    if (dest != null)
                    {
                        //lock (mRemoteStream)
                        {
                            dest.Write(recv, 0, recv.Length);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error sending to  " + dest.ToString() + " " + e.ToString());
                }
            }
        }

        return count;
    }

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
            TcpListener myList=new TcpListener(ipAd, mLocalPort);

            myList.Start();

            Console.WriteLine("The server is running at port " + mLocalPort);    
            Console.WriteLine("The local End point is  :" + 
                              myList.LocalEndpoint );
            Console.WriteLine("Waiting for a connection.....");

            Socket localSocket = myList.AcceptSocket();
            Console.WriteLine("Connection accepted from " + localSocket.RemoteEndPoint);
        
            mLocalStream = new NetworkStream(localSocket);
            byte[] buffer=new byte[200];
            int len;
            while (localSocket.Connected)
            {
                
                //lock (mLocalSocket)
                {
                    len = mLocalStream.Read(buffer, 0, 200);;
                }

                mMPReceived += ProcessBuffer(buffer, len, SERVER_CHANNEL, mRemoteStream);
                mApp.SetMPReceived(mMPReceived);
            }

            Console.WriteLine("Local Socket was closed\n");
            localSocket.Close();
            mLocalStream.Close();
            mLocalStream = null;
            
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

                    mADReceived += ProcessBuffer(buffer, len, CLIENT_CHANNEL, mLocalStream);
                    mApp.SetADReceived(mADReceived);
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
