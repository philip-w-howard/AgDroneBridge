using System;
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

using System.Net.NetworkInformation;

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

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in nics)
            {
                foreach (UnicastIPAddressInformation addr in ni.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        byte[] bytes = addr.Address.GetAddressBytes();
                        if (bytes[0] != 169 && bytes[0] != 127)
                        {
                            AgDroneAddress.Text = addr.Address.ToString();
                        }
                    }
                }
            }
        }

        private IPServer mIPServer;
       
         private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartButton.Content.Equals("Stop"))
            {
                StartButton.Content = "Start";
                mIPServer.Stop();
            }
            else 
            {
                StartButton.Content = "Stop";
                mIPServer.Start(LocalPort.Text, AgDroneAddress.Text, AgDronePort.Text);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected delegate void UpdateTextCallback(string message);
        protected delegate void UpdateBrushCallback(Brush brush);

        public void SetMPConnected(bool connected)
        {
            Brush color;

            if (connected)
                color = Brushes.ForestGreen;
            else
                color = Brushes.Red;

            MPLabel.Dispatcher.Invoke(new UpdateBrushCallback(UpdateMPLabel), new object[] { color });
        }
        public void SetADConnected(bool connected)
        {
            Brush color;

            if (connected)
                color = Brushes.ForestGreen;
            else
                color = Brushes.Red;

            ADLabel.Dispatcher.Invoke(new UpdateBrushCallback(UpdateADLabel), new object[] { color });
        }
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

        protected void UpdateMPLabel(Brush brush)
        {
            MPLabel.Background = brush;
        }
        protected void UpdateADLabel(Brush brush)
        {
            ADLabel.Background = brush;
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
