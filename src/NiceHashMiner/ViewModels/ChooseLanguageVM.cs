using DCLCore.Configs;

namespace Decloud.ViewModels
{
    class ChooseLanguageVM : BaseVM
    {
        public TranslationsSettings TranslationsSettings => TranslationsSettings.Instance;
    }
}
