using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.UI.Services
{
    public interface IDialogService
    {
        Task ShowMessageBoxAsync(string title, string message = null, string buttonText = null);

        Task<bool> ShowQuestionDialogAsync(string title, string message = null, string positiveButtonText = null, string negativeButtonText = null);

        Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText);

        IProgress ShowProgressDialog(bool isDeterministic = false, string title = null, string message = null, string cancelButtonText = null, CancellationTokenSource tokenSource = null);

        Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText);
    }
}
