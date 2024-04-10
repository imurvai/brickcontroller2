using BrickController2.Resources;
using System.Reflection;
using System.Resources;

namespace BrickController2.Helpers
{
    public static class ResourceHelper
    {
        private static ResourceManager _translationResourceManager = null;

        public static ResourceManager TranslationResourceManager
        {
            get
            {
                if (_translationResourceManager == null)
                {
                    _translationResourceManager = new ResourceManager("BrickController2.Resources.TranslationResources", typeof(TranslationResources).GetTypeInfo().Assembly);
                }

                return _translationResourceManager;
            }
        }

        public static string ImageResourceRootNameSpace => "BrickController2.UI.Images";

        public static ImageSource GetImageResource(string resourceName)
        {
            // define sourceAssembly parameter to avoid looking for assembly by Xamarin (via reflection of Assembly.GetCallingAssembly)
            return ImageSource.FromResource($"{ImageResourceRootNameSpace}.{resourceName}", typeof(ResourceHelper).Assembly);
        }
    }
}
