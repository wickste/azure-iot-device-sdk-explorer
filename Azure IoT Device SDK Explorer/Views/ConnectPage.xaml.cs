using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Azure_IoT_Device_SDK_Explorer.Views
{
    public sealed partial class ConnectPage : Page, INotifyPropertyChanged
    {
        public ConnectPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("deviceConnectionString"))
            {
                tbConnectionString.Text = ApplicationData.Current.LocalSettings.Values["deviceConnectionString"].ToString();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void btnCreate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TransportType transportType = TransportType.Mqtt;
            if (rbAmqp.IsChecked == true)
            {
                transportType = TransportType.Amqp;
            }
            else if (rbAmqpTcp.IsChecked == true)
            {
                transportType = TransportType.Amqp_Tcp_Only;
            }
            else if (rbAmqpWeb.IsChecked == true)
            {
                transportType = TransportType.Amqp_WebSocket_Only;
            }
            else if (rbMqttTcp.IsChecked == true)
            {
                transportType = TransportType.Mqtt_Tcp_Only;
            }
            else if (rbMqttWeb.IsChecked == true)
            {
                transportType = TransportType.Mqtt_WebSocket_Only;
            }
            else if (rbHttp1.IsChecked == true)
            {
                transportType = TransportType.Http1;
            }

            try
            {
                App.IoTHubClient = DeviceClient.CreateFromConnectionString(tbConnectionString.Text, transportType);
                App.IoTHubClient.SetConnectionStatusChangesHandler(new ConnectionStatusChangesHandler(this.ConnectionStatusHandler));
                await App.IoTHubClient.OpenAsync();
            }
            catch
            {
                MessageDialog dlg = new MessageDialog("Make sure you enter a valid device connection string.", "DeviceClient creation failed");
                await dlg.ShowAsync();
            }
        }
        private async void ConnectionStatusHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            bool isConnected = false;
            string deviceId = "Unknown";
            if (status == ConnectionStatus.Connected)
            {
                isConnected = true;                
            }
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (isConnected)
                {
                    statusBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                    deviceId = ExtractDeviceId(tbConnectionString.Text);
                    tbDeviceName.Text = "DeviceName = " + deviceId.ToString();
                    tbConnectionStatus.Text = "ConnectionStatus = " + status.ToString();
                    tbConnectionStatusChangedReason.Text = "ChangedReason = " + reason.ToString();
                    ApplicationData.Current.LocalSettings.Values["deviceConnectionString"] =  tbConnectionString.Text;
                }
                else
                {
                    statusBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                    tbDeviceName.Text = "DeviceName = Unknown";
                    tbConnectionStatus.Text = "ConnectionStatus = " + status.ToString();
                    tbConnectionStatusChangedReason.Text = "ChangedReason = " + reason.ToString();
                }
            });
        }

        private string ExtractDeviceId(string connectionString)
        {
            string ret = "";
            string []temp = connectionString.Split(';');
            for (int i=0; i<temp.Length; i++)
            {
                if (temp[i].StartsWith("DeviceId"))
                {
                    ret = temp[i];
                    break;
                }
            }
            return ret;
        }
    }
}
