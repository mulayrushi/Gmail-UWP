using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Gmail10.Services.NavigationService
{
    public interface INavigatable
    {
        Task OnNavigatedToAsync(string parameter, NavigationMode mode);
        Task OnNavigatedFromAsync(bool suspending);
    }
}
