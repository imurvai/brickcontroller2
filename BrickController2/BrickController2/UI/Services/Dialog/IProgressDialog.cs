using System;

namespace BrickController2.UI.Services.Dialog
{
    public interface IProgressDialog
    {
        string Title { set; }
        string Message { set; }
        int Percent { get; set; }
    }
}
