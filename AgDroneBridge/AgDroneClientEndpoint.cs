﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace AgDroneBridge
{
    class AgDroneClientEndpoint : IPEndpoint
    {
        protected string mNetAddr;
        protected int mPort;
        protected volatile bool mIsOpen = false;
        //protected TcpListener mListener;
        protected Socket mSocket;
        TcpClient mTCPCLient;
        protected MainWindow mApp;

        public AgDroneClientEndpoint(MainWindow app, string netAddr, int port) 
            : base()
        {
            mNetAddr = netAddr;
            mPort = port;
            mApp = app;
        }

        protected override void SetDisconnected()
        {
            mSocket.Shutdown(SocketShutdown.Both);
            mTCPCLient.Close();
            mIsOpen = false;
            mSocket = null;
            if (mRunning) mApp.SetADConnected(false);
        }

        protected override void MakeConnection()
        {
            try
            {
                Console.WriteLine("Connecting to AgDrone at " + mNetAddr + " " + mPort);

                mTCPCLient = new TcpClient();

                mTCPCLient.Connect(mNetAddr, mPort);

                Console.WriteLine("AgDrone Connected");

                mSocket = mTCPCLient.Client;
                //mRemoteStream = tcpclnt.GetStream();

                mIsOpen = true;
                mApp.SetADConnected(true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception while making connection: " + e.ToString());
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
                    int len = mSocket.Send(buff);
                    if (len == buff.Length)
                        mSent++;
                    else
                        Console.WriteLine("ERROR: Sent too few bytes");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing to endpoint\n" + e.ToString());
                SetDisconnected();
            }
        }

        protected override void UpdateCounters()
        {
            if (mRunning) mApp.SetADReceived(mReceived);
            if (mRunning) mApp.SetADSent(mSent);
        }
    }
}
