using System.Windows.Input;

namespace BrickController2.UI.Behaviors
{
    public class ListViewItemTappedBehavior : Behavior<ListView>
    {
        private ListView _target;

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ListViewItemTappedBehavior), null, BindingMode.OneWay, null, OnCommandChanged);
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ListViewItemTappedBehavior), null, BindingMode.OneWay, null, OnCommandParameterChanged);

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttachedTo(ListView listView)
        {
            base.OnAttachedTo(listView);
            listView.ItemTapped += OnItemTapped;
            listView.BindingContextChanged += OnBindingContextChanged;
            _target = listView;
        }

        protected override void OnDetachingFrom(ListView listView)
        {
            listView.ItemTapped -= OnItemTapped;
            listView.BindingContextChanged -= OnBindingContextChanged;
            _target = null;
            base.OnDetachingFrom(listView);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            BindingContext = _target?.BindingContext;
        }

        private void OnBindingContextChanged(object sender, EventArgs eventArgs)
        {
            OnBindingContextChanged();
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs eventArgs)
        {
            if (Command == null)
            {
                return;
            }

            var parameter = CommandParameter ?? eventArgs.Item;
            if (Command.CanExecute(parameter))
            {
                Command.Execute(parameter);
            }
        }

        private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListViewItemTappedBehavior behavior)
            {
                behavior.Command = (ICommand)newValue;
            }
        }

        private static void OnCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ListViewItemTappedBehavior behavior)
            {
                behavior.CommandParameter = newValue;
            }
        }
    }
}
