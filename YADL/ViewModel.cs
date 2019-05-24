using System.Collections.ObjectModel;
using System.ComponentModel;

namespace YADL
{
    class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Playlists> Playlist
        {
            get { return _Playlist; }
            set
            {
                if (_Playlist != value)
                {
                    _Playlist = value;
                    OnPropertyChanged("Playlist");
                }
            }
        }

        public ViewModel()
        {
            Playlist = new ObservableCollection<Playlists>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Playlists> _Playlist;

        void OnPropertyChanged(string Property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Property));
        }
    }
}
