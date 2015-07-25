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

        private AgDroneBridge.MainWindow mApp;
        private volatile bool mRunning = true;
        private bool mConnectingAsServer = true;

        private IPEndpoint mMPEndpoint;
        private IPEndpoint mADEndpoint;
        private Heartbeat mHeartbeat;

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

            mHeartbeat = new Heartbeat(mMPEndpoint, mADEndpoint);
            mHeartbeat.Start();
        }

        public void Stop()
        {
            mRunning = false;

            mHeartbeat.Stop();
            mMPEndpoint.Stop();
            mADEndpoint.Stop();

        }
    }
}