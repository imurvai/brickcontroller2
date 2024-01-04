namespace BrickController2.UI.Controls
{
    // Thanks to Wayne Creasey ( https://intellitect.com/xamarin-forms-enumbindablepicker/ )
    public class EnumPicker<T> : Picker where T: struct
    {
        public EnumPicker()
        {
            SelectedIndexChanged += OnSelectedIndexChanged;

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                Items.Add(item.ToString());
            }

            SelectedIndex = 0;
        }

        public new static BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(T), typeof(EnumPicker<T>), default(T), BindingMode.TwoWay, null, OnSelectedItemChanged);

        public new T SelectedItem
        {
            get { return (T)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs args)
        {
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
            {
                SelectedItem = default;
            }
            else
            {
                SelectedItem = (T)Enum.Parse(typeof(T), Items[SelectedIndex]);
            }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is EnumPicker<T> enumPicker && newValue != null)
            {
                enumPicker.SelectedIndex = enumPicker.Items.IndexOf(newValue.ToString());
            }
        }
    }
}
