using System.ComponentModel;

namespace IronworksTranslator.Core
{
    public enum TranslatorEngine
    {
        [Description("Papago")]
        Papago = 0,
        [Description("Gemini")]
        Gemini = 1
    }
}
