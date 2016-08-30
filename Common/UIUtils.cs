using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Gmail10.Common
{
    public class UIUtils
    {
        /// <summary>Invokes the specified action from the UI thread.</summary>
        public static async Task InvokeFromUIThread(Action action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () => action());
        }
    }
}
