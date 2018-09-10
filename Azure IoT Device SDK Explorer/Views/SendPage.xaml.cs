using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Azure_IoT_Device_SDK_Explorer.Views
{
    public sealed partial class SendPage : Page, INotifyPropertyChanged
    {
        private DispatcherTimer timer = null;
        public SendPage()
        {
            InitializeComponent();
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

        private void btnSingle_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SendSingleMessage();
        }

        private void SendSingleMessage()
        {
            if (App.IoTHubClient == null)
            {
                tbOutput.Text += "\r\nDevice not connected to Azure IoT Hub.";
                return;
            }
            Message message = this.CreateMessage();
            try
            {
                string data = Encoding.UTF8.GetString(message.GetBytes());
                DateTime creationTime = DateTime.Now;

                App.IoTHubClient.SendEventAsync(message);

                tbOutput.Text += "Message sent at " + $"{creationTime}>\r\nData:[{data}]";
                if (message.Properties.Count > 0)
                {
                    tbOutput.Text += "\r\nProperties:\r\n";
                    foreach (var property in message.Properties)
                    {
                        tbOutput.Text += $"\t'{property.Key}': '{property.Value}'\r\n";
                    }
                }
            }
            catch (Microsoft.Azure.Devices.Client.Exceptions.UnauthorizedException exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                tbOutput.Text = exc.ToString();
            }
            catch (Microsoft.Azure.Devices.Client.Exceptions.DeviceNotFoundException exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                tbOutput.Text = exc.ToString();
            }
        }

        private void btnBatch_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (App.IoTHubClient == null)
            {
                tbOutput.Text += "\r\nDevice not connected to Azure IoT Hub.";
                return;
            }

            List<Message> messages = new List<Message>();
            int numberOfMessages = 0;
            if (int.TryParse(tbBatch.Text, out numberOfMessages))
            {
                for (int i=0; i<numberOfMessages; i++)
                {
                    Message message = this.CreateMessage();
                    messages.Add(message);
                    string data = Encoding.UTF8.GetString(message.GetBytes());
                    DateTime creationTime = DateTime.Now;
                    tbOutput.Text += "\r\nMessage sent at " + $"{creationTime}>\r\nData:[{data}]";
                    if (message.Properties.Count > 0)
                    {
                        tbOutput.Text += "\r\nProperties:\r\n";
                        foreach (var property in message.Properties)
                        {
                            tbOutput.Text += $"\t'{property.Key}': '{property.Value}'\r\n";
                        }
                    }
                }
                try
                {
                    App.IoTHubClient.SendEventBatchAsync(messages);
                }
                catch (Microsoft.Azure.Devices.Client.Exceptions.UnauthorizedException exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc.ToString());
                    tbOutput.Text = exc.ToString();
                }
                catch (Microsoft.Azure.Devices.Client.Exceptions.DeviceNotFoundException exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc.ToString());
                    tbOutput.Text = exc.ToString();
                }
            }
        }

        private void btnPeriod_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (App.IoTHubClient == null)
            {
                tbOutput.Text += "\r\nDevice not connected to Azure IoT Hub.";
                return;
            }

            btnStop.IsEnabled = true;
            btnSingle.IsEnabled = false;
            btnBatch.IsEnabled = false;
            btnPeriod.IsEnabled = false;

            timer = new DispatcherTimer();
            double seconds = 0d;
            if (double.TryParse(tbInterval.Text, out seconds))
            {
                timer.Interval = TimeSpan.FromSeconds(seconds);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            SendSingleMessage();
        }

        private void btnStop_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
            btnSingle.IsEnabled = true;
            btnBatch.IsEnabled = true;
            btnPeriod.IsEnabled = true;

            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
                timer = null;
            }
        }

        private Message CreateMessage()
        {
            Message message = null;
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();
            double currentTemperature = minTemperature + rand.NextDouble() * 15;
            double currentHumidity = minHumidity + rand.NextDouble() * 20;

            if (rbWell.IsChecked == true || rbRndProp.IsChecked == true)
            {
                var telemetryDataPoint = new
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                string messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                message = new Message(Encoding.ASCII.GetBytes(messageString));
            }
            else
            {
                int bodyLen = rand.Next(1, 300);
                byte[] bytes = new byte[bodyLen];
                rand.NextBytes(bytes);
                message = new Message(bytes);
            }

            if (rbWell.IsChecked == true || rbRndBody.IsChecked == true)
            {
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");
            }
            else
            {
                int numberOfProperties = rand.Next(1, 5);
                for (int i=0; i<numberOfProperties; i++)
                {
                    int nameLen = rand.Next(1, 20);
                    byte[] nameBytes = new byte[nameLen];
                    rand.NextBytes(nameBytes);
                    int valueLen = rand.Next(1, 20);
                    byte[] valueBytes = new byte[valueLen];
                    rand.NextBytes(valueBytes);
                    message.Properties.Add(Encoding.ASCII.GetString(nameBytes), Encoding.ASCII.GetString(valueBytes));
                }
            }

            return message;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(sender as TextBox, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                break;
            }
        }

        private async void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            if (App.IoTHubClient == null)
            {
                tbOutput.Text += "\r\nDevice not connected to Azure IoT Hub.";
                return;
            }

            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            StorageFile file =  await picker.PickSingleFileAsync();
            var stream = await file.OpenAsync(FileAccessMode.Read);
            try
            {
                await App.IoTHubClient.UploadToBlobAsync(file.Name, stream.AsStream());
                tbOutput.Text += "\r\n\r\nFile uploaded to blob storage: " + file.Name;
            }
            catch (Exception exc)
            {
                tbOutput.Text = exc.ToString();
                tbOutput.Text += "\r\n\r\nMake sure you have configured your IoT Hub for file upload:\r\nhttps://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-configure-file-upload";
            }
        }
    }
}
