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

        public Heartbeat()
        {
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
            while (mRunning)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
