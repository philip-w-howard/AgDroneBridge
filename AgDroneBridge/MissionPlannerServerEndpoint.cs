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
            mApp.SetMPReceived(mReceived);
            base.Send(buff);
            mApp.SetMPSent(mSent);
        }
    }
}
