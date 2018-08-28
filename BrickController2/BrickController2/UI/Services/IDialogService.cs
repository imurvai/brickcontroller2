using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.UI.Services
{
    public interface IDialogService
    {
        Task<IInputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string positiveButtonText, string negativeButtonText);

        Task<IProgress> ShowProgressDialogAsync(string title, string message, string cancelButtonText = null, CancellationTokenSource tokenSource = null, int minValue = 0, int maxValue = 0);

        Task<IGameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText);
    }
}
