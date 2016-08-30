using System;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Gmail10
{
    sealed partial class App : Common.BootStrapper
    {
        public App() : base()
        {
            this.InitializeComponent();
        }

        public override async Task OnInitializeAsync()
        {
            if (Window.Current.Content == null)
                Window.Current.Content = new Views.Shell(this.RootFrame);

            try
            {
                //var url = new Uri("ms-appx:///Cortana.xml");
                //var file = await StorageFile.GetFileFromApplicationUriAsync(url);
                //await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(file);
            }
            catch { }

            //return base.OnInitializeAsync();
        }

        public override Task OnLaunchedAsync(ILaunchActivatedEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(320, 500));

            // Apply shell drawn Back button
            this.RootFrame.Navigated += (s, a) =>
            {
                if (this.RootFrame.CanGoBack)
                {
                    // Setting this visible is ignored on Mobile and when in tablet mode!     
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                }
                else
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                }
            };

            //Common.DatabaseHelper._args = e;
            return Task.FromResult<object>(null);
        }

        protected async override Task OnSuspendingAsync(object s, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO Add any logic required on app suspension
            //await Task.Delay(500);

            deferral.Complete();
        }
    }
}
