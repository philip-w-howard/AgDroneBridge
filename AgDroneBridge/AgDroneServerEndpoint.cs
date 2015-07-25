using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgDroneBridge
{
    class AgDroneServerEndpoint : IPServerEndpoint
    {
        public AgDroneServerEndpoint(MainWindow app, string netAddr, int port)
            : base(netAddr, port)
        {
            mApp = app;
            //mLogFile = new System.IO.StreamWriter(@"ad.log");
            mChannel = 1;
        }

        protected MainWindow mApp;

        protected override void MakeConnection()
        {
            base.MakeConnection();
            mApp.SetADConnected(true);
        }

        protected override void SetDisconnected()
        {
            base.SetDisconnected();
            if (mRunning) mApp.SetADConnected(false);
        }

        public override void Send(byte[] buff)
        {
            base.Send(buff);
            if (mRunning) UpdateCounters();
        }
        
        protected override void UpdateCounters()
        {
            if (mRunning) mApp.SetADReceived(mReceived);
            if (mRunning) mApp.SetADSent(mSent);
        }
    }
}
