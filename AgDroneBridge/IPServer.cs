using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using mavlinklib;

namespace AgDroneBridge
{

    public class IPServer
    {
        public IPServer(AgDroneBridge.MainWindow app)
        {
            mApp = app;
        }

        //private int mLocalPort;
        //private int mRemotePort;
        //private string mRemoteAddress;
        //private Thread mClientThread;
        private AgDroneBridge.MainWindow mApp;
        //private Stream mRemoteStream;
        //private Stream mLocalStream;
        //private Int64 mADSent;
        //private Int64 mADReceived;
        //private Int64 mMPSent;
        //private Int64 mMPReceived;
        private bool mRunning = true;
        private bool mConnectingAsServer = true;

        //private const int SERVER_CHANNEL = 0;
        //private const int CLIENT_CHANNEL = 1;

        private IPEndpoint mMPEndpoint;
        private IPEndpoint mADEndpoint;

        private int xxProcessBuffer(byte[] buff, int len, int channel, Stream dest)
        {
            int count = 0;

            byte[] recv;

            for (int ii = 0; ii < len; ii++)
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

        public void Start(string localPort, string remoteAddress, string remotePort, bool connectAsServer)
        {
            //mLocalPort = localPort;
            //mRemotePort = remotePort;
            //mRemoteAddress = remoteAddress;
            mConnectingAsServer = connectAsServer;

            mMPEndpoint = new MissionPlannerServerEndpoint(mApp, "127.0.0.1", Convert.ToInt32(localPort));

            if (connectAsServer)
            {
                mADEndpoint = new AgDroneServerEndpoint(mApp, remoteAddress, Convert.ToInt32(remotePort));
            }
            else
            {
                mADEndpoint = new AgDroneClientEndpoint(mApp, remoteAddress, Convert.ToInt32(remotePort));
            }

            mMPEndpoint.SetDest(mADEndpoint);
            mADEndpoint.SetDest(mMPEndpoint);

            mMPEndpoint.Start();
            mADEndpoint.Start();

        }

        public void Stop()
        {
            mRunning = false;

            mMPEndpoint.Stop();
            mADEndpoint.Stop();
        }
    }
}