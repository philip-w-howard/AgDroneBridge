using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgDroneBridge
{
    class MissionPlannerServerEndpoint : IPServerEndpoint
    {
        public MissionPlannerServerEndpoint(MainWindow app, string netAddr, int port)
            : base(netAddr, port)
        {
            mApp = app;
            //mLogFile = new System.IO.StreamWriter(@"mp.log");
            mChannel = 0;
        }

        protected MainWindow mApp;

        protected override void MakeConnection()
        {
            base.MakeConnection();
            mApp.SetMPConnected(true);
        }

        protected override void SetDisconnected()
        {
            base.SetDisconnected();
            mApp.SetMPConnected(false);
        }

        protected override void Send(byte[] buff)
        {
            base.Send(buff);
            UpdateCounters();
        }

        protected override void UpdateCounters()
        {
            mApp.SetMPReceived(mReceived);
            mApp.SetMPSent(mSent);
        }

    }
}
