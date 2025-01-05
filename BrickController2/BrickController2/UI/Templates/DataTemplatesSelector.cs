using BrickController2.UI.ViewModels.Settings;
using Microsoft.Maui.Controls;

namespace BrickController2.UI.Templates;

public class DataTemplatesSelector : DataTemplateSelector
{
    public DataTemplate BoolDataTemplate { get; set; } = default!;

    public DataTemplate EnumDataTemplate { get; set; } = default!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var itemType = item.GetType();

        if (itemType == typeof(BoolSettingViewModel))
            return BoolDataTemplate;
        if (itemType == typeof(EnumSettingViewModel))
            return EnumDataTemplate;

        return default!;
    }
}
