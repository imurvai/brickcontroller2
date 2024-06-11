using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.Dialog
{
    public interface IDialogService
    {
        bool IsDialogOpen { get; }
        Task ShowMessageBoxAsync(string title, string message, string buttonText, CancellationToken token);
        Task<bool> ShowQuestionDialogAsync(string title, string message, string positiveButtonText, string negativeButtonText, CancellationToken token);
        Task<InputDialogResult> ShowInputDialogAsync(string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText, KeyboardType keyboardType, Predicate<string> valuePredicate, CancellationToken token);
        Task<SelectionDialogResult<T>> ShowSelectionDialogAsync<T>(IEnumerable<T> items, string title, string cancelButtonText, CancellationToken token) where T : notnull;
        Task<ProgressDialogResult> ShowProgressDialogAsync(bool isDeterministic, Func<IProgressDialog, CancellationToken, Task> action, string title, string? message = null, string? cancelButtonText = null, CancellationToken token = default);
        Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText, CancellationToken token);
    }
}
