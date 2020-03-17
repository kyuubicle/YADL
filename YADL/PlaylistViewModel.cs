using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using GongSolutions.Wpf.DragDrop;


namespace YADL
{
    class PlaylistViewModel : INotifyPropertyChanged,GongSolutions.Wpf.DragDrop.IDropTarget
    {
        public MainWindow window;

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

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            var dataObject = dropInfo.Data as IDataObject;

            // look for drag&drop new files
            if (dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var dataObject = dropInfo.Data as DataObject;

            // look for drag&drop new files
            if (dataObject != null && dataObject.ContainsFileDropList())
            {
                this.HandleDropActionAsync(dropInfo, dataObject.GetFileDropList());
            }
            else
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
            }
        }

        private void HandleDropActionAsync(IDropInfo dropInfo, StringCollection stringCollection)
        {

            if (dropInfo.TargetCollection.GetType().ToString() == this.Playlist.GetType().ToString())
            {
                List<string> files = new List<string>();

                foreach (string f in stringCollection)
                {
                    files.Add(f);
                }

                window.Helper_Playlists(files);
            }
            else
            {
                List<string> files = new List<string>();

                foreach (string f in stringCollection)
                {
                    files.Add(f);
                }

                window.Helper_Pwads(files);
            }
        }

        public PlaylistViewModel()
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
