using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;


using mavlinklib;

namespace AgDroneBridge
{
    abstract class IPEndpoint
    {
        protected IPEndpoint mDest;
        protected int mChannel;
        protected int mReceived = 0;
        protected int mSent = 0;
        protected bool mRunning = true;
        protected Thread mProcessor;

        public IPEndpoint()
        {
            Console.WriteLine("Starting processing thread");
            mProcessor = new Thread(ProcessInput);
        }
         
        public void Start()
        {
            mProcessor.Start();
        }

        public void Stop()
        {
            mRunning = false;
            mProcessor.Abort();
            //mProcessor.Join();
        }

        public void SetDest(IPEndpoint dest)
        {
            mDest = dest;
        }

        protected abstract void SetDisconnected();

        protected abstract void MakeConnection();

        protected abstract bool IsConnected();

        protected abstract int GetData(byte[] buffer);

        protected abstract void Send(byte[] buff);

        protected int ProcessBuffer(byte[] buff, int len)
        {
            int count = 0;

            byte[] recv;

            for (int ii = 0; ii < len; ii++)
            {
                recv = MavlinkProcessor.process_byte(mChannel, buff[ii]);
                if (recv.Length != 0)
                {
                    count++;

                    if (mDest != null)
                    {
                        mDest.Send(recv);
                    }
                }
            }

            return count;
        }

        protected void ProcessInput()
        {
            Console.WriteLine("Processing input for server");
            try
            {
                while (mRunning)
                {
                    byte[] buffer = new byte[200];
                    int len;
                    int numZeros = 0;

                    MakeConnection();

                    while (IsConnected())
                    {
                        len = GetData(buffer);

                        if (len > 0)
                        {
                            mReceived += ProcessBuffer(buffer, len);
                            numZeros = 0;
                        }
                        else
                        {
                            // If we get a bunch of zeros in a row, assume the connection is broken
                            if (++numZeros > 10) SetDisconnected();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.ToString());
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Server is shutting down******************************");
            }
        }

    }
}
