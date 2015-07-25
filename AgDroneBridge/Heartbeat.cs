using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AgDroneBridge
{
    class Heartbeat
    {
        protected Thread mThread;
        protected bool mRunning;
        protected IPEndpoint mAgDrone;
        protected IPEndpoint mMissionPlanner;

        public Heartbeat(IPEndpoint mp, IPEndpoint ad)
        {
            mMissionPlanner = mp;
            mAgDrone = ad;
        }

        public void Start()
        {
            mRunning = true;
            mThread = new Thread(BeatHeart);
            mThread.Start();
        }

        public void Stop()
        {
            mRunning = false;
            mThread.Join();
        }

        protected void BeatHeart()
        {
            Thread.Sleep(1000);
            while (mRunning)
            {
                byte[] msg = mavlinklib.MavlinkProcessor.create_heartbeat();
                mAgDrone.Send(msg);
                mMissionPlanner.Send(msg);

                // NOTE: want Sleep to be last thing in loop, so Stop will most likely execute immediately before the check to mRunning
                Thread.Sleep(1000);
            }
        }
    }
}
