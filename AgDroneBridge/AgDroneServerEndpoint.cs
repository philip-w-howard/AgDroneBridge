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
            mLogFile = new System.IO.StreamWriter(@"ad.log");
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
            mApp.SetADConnected(false);
        }

        protected override void Send(byte[] buff)
        {
            base.Send(buff);
            UpdateCounters();
        }
        
        protected override void UpdateCounters()
        {
            mApp.SetADReceived(mReceived);
            mApp.SetADSent(mSent);
        }
    }
}
