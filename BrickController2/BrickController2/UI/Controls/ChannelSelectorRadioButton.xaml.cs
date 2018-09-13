using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChannelSelectorRadioButton : ContentView
    {
        public ChannelSelectorRadioButton()
        {
            InitializeComponent();
        }

        public static BindableProperty ChannelProperty = BindableProperty.Create(nameof(Channel), typeof(int), typeof(ChannelSelectorRadioButton), 0, BindingMode.OneWay, null, OnChannelChanged);
        public static BindableProperty SelectedChannelProperty = BindableProperty.Create(nameof(SelectedChannel), typeof(int), typeof(ChannelSelectorRadioButton), 0, BindingMode.OneWay, null, OnSelectedChannelChanged);
        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ChannelSelectorRadioButton), null, BindingMode.OneWay);
        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ChannelSelectorRadioButton), null, BindingMode.OneWay);

        public int Channel
        {
            get => (int)GetValue(ChannelProperty);
            set => SetValue(ChannelProperty, value);
        }

        public int SelectedChannel
        {
            get => (int)GetValue(SelectedChannelProperty);
            set => SetValue(SelectedChannelProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        private static void OnChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ChannelSelectorRadioButton csrb)
            {
                csrb.TapGuestureRecognizer.CommandParameter = (int)newValue;
            }
        }

        private static void OnSelectedChannelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ChannelSelectorRadioButton csrb)
            {
                csrb.Frame.IsVisible = csrb.Channel == csrb.SelectedChannel;
            }
        }

        private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ChannelSelectorRadioButton csrb)
            {
                csrb.Label.Text = (string)newValue;
            }
        }

        private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ChannelSelectorRadioButton csrb)
            {
                csrb.TapGuestureRecognizer.Command = (ICommand)newValue;
            }
        }
    }
}