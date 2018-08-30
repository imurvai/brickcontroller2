using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.UI.Services
{
    public interface IDialogService
    {
        Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText);

        IProgress ShowProgressDialog(string title, string message = null, string cancelButtonText = null, CancellationTokenSource tokenSource = null, int maxValue = 0);

        Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText);
    }
}
