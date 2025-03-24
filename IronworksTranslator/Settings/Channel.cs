using IronworksTranslator.Core;
using Newtonsoft.Json;

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

        [JsonIgnore]
        public readonly ChatCode Code;

        public event SettingsChangedEventHandler OnSettingsChanged;
    }
}
