using IronworksTranslator.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Windows.Media;

namespace IronworksTranslator.Settings
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Channel : SettingsChangedEvent
    {
        public Channel(ChatCode code)
        {
            Code = code;
            Show = true;
            MajorLanguage = ClientLanguage.Japanese;
            TranslateSpeaker = true;
            Color = Colors.White;
        }

        [JsonProperty]
        public bool Show
        {
            get => show;
            set
            {
                if (value != show)
                {
                    show = value;
                    OnSettingsChanged?.Invoke(this, nameof(Show), Show);
                }
            }
        }
        private bool show;

        [JsonProperty]
        public ClientLanguage MajorLanguage
        {
            get => majorLanguage;
            set
            {
                if (value != majorLanguage)
                {
                    majorLanguage = value;
                    OnSettingsChanged?.Invoke(this, nameof(MajorLanguage), MajorLanguage);
                }
            }
        }
        private ClientLanguage majorLanguage;

        [JsonProperty]
        public bool TranslateSpeaker
        {
            get => translateSpeaker;
            set
            {
                if (value != translateSpeaker)
                {
                    translateSpeaker = value;
                    OnSettingsChanged?.Invoke(this, nameof(TranslateSpeaker), TranslateSpeaker);
                }
            }
        }
        private bool translateSpeaker;

        [JsonProperty(PropertyName = "Color")]
        private string ColorString
        {
            get => Color.ToString();
            set
            {
                if (value != null)
                {
                    try
                    {
                        Color = (Color)ColorConverter.ConvertFromString(value);
                    }
                    catch
                    {
                        Color = Colors.White;
                    }
                }
            }
        }

        [JsonIgnore]
        public Color Color
        {
            get => color;
            set
            {
                if (value != color)
                {
                    color = value;
                    OnSettingsChanged?.Invoke(this, nameof(Color), Color);
                }
            }
        }
        private Color color = Colors.White;

        [JsonIgnore]
        public readonly ChatCode Code;

        public event SettingsChangedEventHandler OnSettingsChanged;
    }
}
