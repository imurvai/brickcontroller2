using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.Controls
{
    public class RadioButtonGroup : ContentView
    {

        public string ItemsCsv
        {
            get;
            set;
        }

        public StackOrientation Orientation
        {
            get;
            set;
        }

        public int SelectedIndex
        {
            get;
            set;
        }

        public ICommand SelectionChangedCommand
        {
            get;
            set;
        }
    }
}
