using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace AgDroneBridge
{
    abstract class IPServerEndpoint : IPEndpoint
    {
        protected string mNetAddr;
        protected int mPort;
        protected bool mIsOpen = false;
        protected TcpListener mListener;
        protected Socket mSocket;

        public IPServerEndpoint(string netAddr, int port) 
            : base()
        {
            mNetAddr = netAddr;
            mPort = port;
            try 
            {
                IPAddress ipAd = IPAddress.Parse(netAddr);
                 mListener = new TcpListener(ipAd, port);

                mListener.Start();
                Console.WriteLine("The server is running at port " + port);    
                Console.WriteLine("The local End point is  :" + mListener.LocalEndpoint );
            }
            catch (Exception e)
            {
                Console.WriteLine("Error setting up server listener\n" + e.ToString());
            }
        }

        virtual public void Stop()
        {
            mRunning = false;
            mListener.Stop();
            base.Stop();
        }
        protected override void SetDisconnected()
        {
            if (mSocket != null)
            {
                mSocket.Shutdown(SocketShutdown.Both);
                mSocket.Close();
            }

            mIsOpen = false;
            mSocket = null;
        }

        protected override void MakeConnection()
        {
            try
            {
                Console.WriteLine("Waiting for a connection on " + mListener.LocalEndpoint);
                while (!mListener.Pending())
                {
                    Thread.Sleep(500);
                    if (!mRunning)
                    {
                        Console.WriteLine("Server shutting down on " + mListener.LocalEndpoint);
                        return;   // <<<<----- Exit if we are no longer running
                    }
                }
                mSocket = mListener.AcceptSocket();
                mIsOpen = true;
                Console.WriteLine("Connection formed on " + mSocket.LocalEndPoint + " to " + mSocket.RemoteEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while making connection on " +
                                  mListener.LocalEndpoint + ": " + e.ToString());
            }
        }

        protected override bool IsConnected()
        {
            return mIsOpen && mSocket != null && mSocket.Connected;
        }

        protected override int GetData(byte[] buffer)
        {
            try
            {
                return mSocket.Receive(buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting data on channel " + mChannel + " so disconnecting.\n" + e.ToString());
                SetDisconnected();
                return 0;
            }
        }

        public override void Send(byte[] buff)
        {
            try
            {
                if (mSocket != null)
                {
                    mSocket.Send(buff);
                    mSent++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing to endpoint\n" + e.ToString());
                SetDisconnected();
            }
        }

    }
}
