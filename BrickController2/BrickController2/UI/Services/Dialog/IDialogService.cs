using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.Dialog
{
    public interface IDialogService
    {
        Task ShowMessageBoxAsync(string title, string message, string buttonText, CancellationToken token);
        Task<bool> ShowQuestionDialogAsync(string title, string message, string positiveButtonText, string negativeButtonText, CancellationToken token);
        Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText, KeyboardType keyboardType, Predicate<string> valuePredicate, CancellationToken token);
        Task<SelectionDialogResult<T>> ShowSelectionDialogAsync<T>(IEnumerable<T> items, string title, string cancelButtonText, CancellationToken token);
        Task ShowProgressDialogAsync(bool isDeterministic, Func<IProgressDialog, CancellationToken, Task> action, string title = null, string message = null, string cancelButtonText = null);
        Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText, CancellationToken token);
        Task<SequenceInputDialogResult> ShowSequenceInputDialogAsync(string title, string message, string valueText, float value, string durationText, int durationMs, string positiveButtonText, string negativeButtonText, Predicate<string> durationPredicate, CancellationToken token);
    }
}
