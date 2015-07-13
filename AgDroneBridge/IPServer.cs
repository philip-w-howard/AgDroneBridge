using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using mavlinklib;

public class IPServer
{
    public IPServer(AgDroneBridge.MainWindow app)
	{
        mApp = app;
	}

    //private int mLocalPort;
    //private int mRemotePort;
    //private string mRemoteAddress;
    private Thread mServerThread1;
    private Thread mServerThread2;
    private Thread mClientThread;
    private AgDroneBridge.MainWindow mApp;
    private Stream mRemoteStream;
    private Stream mLocalStream;
    private Int64 mADSent;
    private Int64 mADReceived;
    private Int64 mMPSent;
    private Int64 mMPReceived;
    private bool mRunning = true;

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

    public void Start(string localPort, string remoteAddress, string remotePort)
    {
        //mLocalPort = localPort;
        //mRemotePort = remotePort;
        //mRemoteAddress = remoteAddress;

        mServerThread1 = new Thread(new ParameterizedThreadStart(Server));
        var params1 = new List<string>() { "127.0.0.1", localPort, "true" };
        mServerThread1.Start(params1);

        mServerThread2 = new Thread(new ParameterizedThreadStart(Server));
        var params2 = new List<string>() { remoteAddress, remotePort, "false" };
        mServerThread2.Start(params2);

//        mClientThread = new Thread(new ThreadStart(Client));
//        mClientThread.Start();
    }

    public void Stop()
    {
        mRunning = false;

        mServerThread1.Abort();
        // mServerThread.Join();

        mServerThread2.Abort();
        // mServerThread.Join();

        //mClientThread.Abort();
       // mServerThread.Join();
    }
//    public void Server(string netAddr, int port)

    /*{arams:
     *    string    IP Addr
     *    int       port
     *    Label     indicate connectedness
     *    Stream    destination for messages
     *    int       mavlink channel
     */ 
    public void Server(Object param)
    {
        string netaddr = "";
        int port = 0;
        bool isLocal = false;

        try
        {
            List<string> paramList = (List<string>) param;

            netaddr = paramList[0];
            port = Convert.ToInt32(paramList[1]);
            isLocal = Convert.ToBoolean(paramList[2]);

            Console.WriteLine("Server is starting up at " + netaddr + " " + port);

        }
        catch (Exception e)
        {
            Console.WriteLine("Invalid parameters to IP Server initializer");
            Console.WriteLine("Server is terminating ********************************");
            return;
        }

        try 
        {
            IPAddress ipAd = IPAddress.Parse(netaddr);
            TcpListener myList=new TcpListener(ipAd, port);

            myList.Start();

            Console.WriteLine("The server is running at port " + port);    
            Console.WriteLine("The local End point is  :" + 
                              myList.LocalEndpoint );
            Console.WriteLine("Waiting for a connection.....");

            while (mRunning)
            {
                Socket localSocket = myList.AcceptSocket();
                Console.WriteLine("Connection accepted from " + localSocket.RemoteEndPoint);

                mLocalStream = new NetworkStream(localSocket);
                byte[] buffer = new byte[200];
                int len;
                int numZeros = 0;

                if (isLocal)
                    mApp.SetMPConnected(true);
                else
                    mApp.SetADConnected(true);


                while (localSocket.Connected)
                {

                    //lock (mLocalSocket)
                    {
                        len = localSocket.Receive(buffer);
                    }

                    if (len > 0)
                    {
                        mMPReceived += ProcessBuffer(buffer, len, SERVER_CHANNEL, mRemoteStream);
                        mApp.SetMPReceived(mMPReceived);
                        numZeros = 0;
                    }
                    else
                    {
                        // If we get a bunch of zeros in a row, assume the connection is broken
                        if (++numZeros > 10) break;
                    }
                }

                if (isLocal)
                    mApp.SetMPConnected(false);
                else
                    mApp.SetADConnected(false);

                Console.WriteLine("Socket at " + netaddr + ":" + port + " was closed\n");
                localSocket.Close();
                mLocalStream.Close();
                mLocalStream = null;
            }
            myList.Stop();          
        }
        catch (Exception e) {
            Console.WriteLine("Error..... " + e.ToString());
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("Server is shutting down******************************");
        }    
    }

    // public void Client() {
    //    while (true)
    //    {

    //        try
    //        {
    //            TcpClient tcpclnt = new TcpClient();
    //            Console.WriteLine("Connecting to AgDrone at " + mRemoteAddress + " " + mRemotePort);

    //            tcpclnt.Connect(mRemoteAddress, mRemotePort);
    //            // use the ipaddress as in the server program

    //            Console.WriteLine("AgDrone Connected");

    //            mRemoteStream = tcpclnt.GetStream();

    //            byte[] buffer = new byte[200];
    //            int len;
    //            mApp.SetADConnected(true);
    //            while (tcpclnt.Connected) // (tcpclnt.Available != 0)
    //            {
    //                //lock (mRemoteStream)
    //                {
    //                    len = mRemoteStream.Read(buffer, 0, 200);
    //                }

    //                mADReceived += ProcessBuffer(buffer, len, CLIENT_CHANNEL, mLocalStream);
    //                mApp.SetADReceived(mADReceived);
    //            }

    //            mApp.SetADConnected(false);

    //            Console.WriteLine("Remote connection was closed\n");

    //            mRemoteStream.Close();
    //            mRemoteStream = null;

    //            tcpclnt.Close();
    //        }

    //        catch (Exception e)
    //        {
    //            Console.WriteLine("Error..... " + e.StackTrace);
    //        }
    //    }
    //}


}
