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
    }
}
