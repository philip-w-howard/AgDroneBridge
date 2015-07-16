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
            mApp.SetADReceived(mReceived);
            base.Send(buff);
            mApp.SetADSent(mSent);
        }
    }
}
