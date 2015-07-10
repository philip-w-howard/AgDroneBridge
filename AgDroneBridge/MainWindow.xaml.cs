﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using mavlinklib;

namespace AgDroneBridge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mIPServer = new IPServer(this);
        }

        private IPServer mIPServer;

        private void callcpp()
        {
            byte[] buff = MavlinkProcessor.create_heartbeat(45, 82);
            byte[] recv;

            foreach (byte val in buff)
            {
                recv = MavlinkProcessor.process_byte(0, val);
                if (recv.Length != 0)
                {
                    Console.Write("Recv on channel 0: ");
                    foreach (byte value in recv)
                    {
                        Console.Write(value + " ");
                    }
                    Console.WriteLine("");
                }

                recv = MavlinkProcessor.process_byte(1, val);
                if (recv.Length != 0)
                {
                    Console.Write("Recv on channel 1: ");
                    foreach (byte value in recv)
                    {
                        Console.Write(value + " ");
                    }
                    Console.WriteLine("");
                }
            }

        }
         private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] buffer = new byte[500];
            int len;

            callcpp();

             /*
            if (StartButton.Content.Equals("Stop"))
            {
                StartButton.Content = "Start";
                mIPServer.Stop();
            }
            else 
            {
                StartButton.Content = "Stop";
                mIPServer.Start(Convert.ToInt32(LocalPort.Text), AgDroneAddress.Text, Convert.ToInt32(AgDronePort.Text));
            }
            */
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected delegate void UpdateTextCallback(string message);

        public void SetMPReceived(Int64 count)
        {
            MPReceived.Dispatcher.Invoke(
                new UpdateTextCallback(UpdateMPReceived),
                new object[] { count.ToString() }
            );
        }

        public void SetADReceived(Int64 count)
        {
            ADReceived.Dispatcher.Invoke(
                new UpdateTextCallback(UpdateADReceived),
                new object[] { count.ToString() }
            );
        }
        public void SetADSent(Int64 count)
        {
            ADSent.Dispatcher.Invoke(
                new UpdateTextCallback(UpdateADSent),
                new object[] { count.ToString() }
            );
        }
        public void SetMPSent(Int64 count)
        {
            MPSent.Dispatcher.Invoke(
                new UpdateTextCallback(UpdateMPSent),
                new object[] { count.ToString() }
            );
        }

        protected void UpdateMPReceived(string text)
        {
            MPReceived.Text = text;
        }
        protected void UpdateADReceived(string text)
        {
            ADReceived.Text = text;
        }
        protected void UpdateADSent(string text)
        {
            ADSent.Text = text;
        }
        protected void UpdateMPSent(string text)
        {
            MPSent.Text = text;
        }
    }
}
