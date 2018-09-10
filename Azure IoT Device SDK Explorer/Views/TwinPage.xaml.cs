using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Azure_IoT_Device_SDK_Explorer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TwinPage : Page, INotifyPropertyChanged
    {
        public TwinPage()
        {
            this.InitializeComponent();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            tbNew.Text = @"{";
            tbNew.Text += "\r\n 'properties': {";
            tbNew.Text += "\r\n  'desired': { }";
            tbNew.Text += "\r\n }";
            tbNew.Text += "\r\n}";

            if (App.IoTHubClient != null)
            {
                Twin twin = await App.IoTHubClient.GetTwinAsync();
                tbTwin.Text = FormatJson(twin.ToJson());
            }
        }

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

        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (App.IoTHubClient != null)
            {
                try
                {
                    TwinCollection reportedProperties = new TwinCollection(tbNew.Text);
                    await App.IoTHubClient.UpdateReportedPropertiesAsync(reportedProperties);

                    Twin twin = await App.IoTHubClient.GetTwinAsync();
                    tbTwin.Text = FormatJson(twin.ToJson());
                }
                catch (Exception exc)
                {
                    MessageDialog dlg = new MessageDialog(exc.ToString(), "ERROR");
                    await dlg.ShowAsync();
                }
            }
        }

        private string FormatJson(string json)
        {
            var parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}
