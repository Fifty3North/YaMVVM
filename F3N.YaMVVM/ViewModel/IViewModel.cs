using System.Threading.Tasks;
using Xamarin.Forms;

namespace F3N.YaMVVM.ViewModel
{
    public interface IViewModel
    {
        Task Initialise();
        Task Destroy();
        Task OnReappearing();
        Task OnDisappearing();

        bool Initialised { get; }
    }
}
