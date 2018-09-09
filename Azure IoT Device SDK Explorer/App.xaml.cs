using System;

using Azure_IoT_Device_SDK_Explorer.Services;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Azure.Devices.Client;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace Azure_IoT_Device_SDK_Explorer
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;
        public static DeviceClient IoTHubClient = null;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        public App()
        {
            InitializeComponent();
           
            AppCenter.Start("3ba53d16-f11a-4880-9ebd-60361525639b", typeof(Analytics), typeof(Crashes));

            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(Views.ConnectPage), new Lazy<UIElement>(CreateShell));
        }

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }
    }
}
