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
        protected volatile bool mRunning = true;
        protected Thread mProcessor;
        protected System.IO.StreamWriter mLogFile;

        public IPEndpoint()
        {
            Console.WriteLine("Starting processing thread");
            mProcessor = new Thread(ProcessInput);
        }
         
        public void Start()
        {
            mProcessor.Start();
        }

        virtual public void Stop()
        {
            mRunning = false;
            SetDisconnected();

            //mProcessor.Abort();
            mProcessor.Join();
        }

        public void SetDest(IPEndpoint dest)
        {
            mDest = dest;
        }

        protected abstract void UpdateCounters();

        protected abstract void SetDisconnected();

        protected abstract void MakeConnection();

        protected abstract bool IsConnected();

        protected abstract int GetData(byte[] buffer);

        public abstract void Send(byte[] buff);

        protected int ProcessBuffer(byte[] buff, int len)
        {
            int count = 0;

            byte[] recv;

            for (int ii = 0; ii < len; ii++)
            {
                recv = MavlinkProcessor.process_byte(mChannel, buff[ii]);
                if (recv.Length != 0)
                {
                    if (mLogFile != null)
                    {
                        mLogFile.Write("Channel: " + mChannel + " len: " + recv.Length + " ");
                        mLogFile.WriteLine(string.Format("{0,4:X} {1,4:X} {2,4:X} {3,4:X} {4,4:X} {5,4:X}",
                            recv[0], recv[1], recv[2], recv[3], recv[4], recv[5]));
                    }
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
            Console.WriteLine("Processing input for server on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            try
            {
                while (mRunning)
                {
                    byte[] buffer = new byte[200];
                    int len;
                    int numZeros = 0;

                    MakeConnection();

                    while (IsConnected() && mRunning)
                    {
                        len = GetData(buffer);

                        if (len > 0)
                        {
                            mReceived += ProcessBuffer(buffer, len);
                            UpdateCounters();
                            numZeros = 0;
                        }
                        else
                        {
                            // If we get a bunch of zeros in a row, assume the connection is broken
                            if (++numZeros > 10)
                            {
                                SetDisconnected();
                                Console.WriteLine("Disconnecting channel " + mChannel + " due to too many zeros");
                            }
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
            Console.WriteLine("Done processing input for server on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId);
        }

    }
}
