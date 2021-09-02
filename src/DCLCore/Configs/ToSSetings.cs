using DCL.Common;

namespace DCLCore.Configs
{
    public class ToSSetings : NotifyChangedBase
    {
        public static ToSSetings Instance { get; } = new ToSSetings();

        private ToSSetings() { }

        private string _hwid = "";
        public string Hwid
        {
            get => _hwid;
            set
            {
                _hwid = value;
                OnPropertyChanged(nameof(Hwid));
            }
        }

        private int _agreedWithTOS = 0;
        public int AgreedWithTOS
        {
            get => _agreedWithTOS;
            set
            {
                _agreedWithTOS = value;
                OnPropertyChanged(nameof(AgreedWithTOS));
            }
        }

        private int _use3rdPartyDecloudTOS = 0;
        public int Use3rdPartyDecloudTOS
        {
            get => _use3rdPartyDecloudTOS;
            set
            {
                _use3rdPartyDecloudTOS = value;
                OnPropertyChanged(nameof(Use3rdPartyDecloudTOS));
            }
        }

    }
}
