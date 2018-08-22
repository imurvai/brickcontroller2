using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.Behaviors
{
    public class ListViewItemTappedBehavior : Behavior<ListView>
    {
        private ListView _target;

        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(ListViewItemTappedBehavior));
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(ListViewItemTappedBehavior));

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
    }
}
