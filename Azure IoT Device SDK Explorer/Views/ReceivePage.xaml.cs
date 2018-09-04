using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Azure_IoT_Device_SDK_Explorer.Views
{
    public sealed partial class ReceivePage : Page, INotifyPropertyChanged
    {
        public ReceivePage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            tbOutput.Text += "\r\n\r\n";
            await App.IoTHubClient.SetMethodDefaultHandlerAsync(MethodDefaultHandler, null);
            await App.IoTHubClient.SetMethodHandlerAsync("foo", MethodHandler, null);
            await App.IoTHubClient.SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback, null);
        }

        private async Task DesiredPropertyUpdateCallback(TwinCollection collection, object userContext)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbOutput.Text += string.Format("Received DesiredPropertyUpdate\r\n");
            });
            return;
        }

        private async Task<MethodResponse> MethodHandler(MethodRequest request, object userContext)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbOutput.Text += string.Format("Received call for registered method: {0}\r\n", request.Name);
                tbOutput.Text += string.Format("Data: {0}\r\n\r\n", request.DataAsJson);
            });
            return new MethodResponse(0);
        }

        private async Task<MethodResponse> MethodDefaultHandler(MethodRequest request, object userContext)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbOutput.Text += string.Format("Received call for non-registered method: {0}\r\n", request.Name);
                tbOutput.Text += string.Format("Data: {0}\r\n\r\n", request.DataAsJson);
            });
            return new MethodResponse(0);
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
    }
}
