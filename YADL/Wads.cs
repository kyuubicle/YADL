using System;
using System.ComponentModel;
using System.IO;

namespace YADL
{
    class Wads : INotifyPropertyChanged
    {
        public bool Wad_Enabled
        {
            get
            {
                if (_Wad_Status != "")
                {
                    _Wad_Enabled = false;
                }
                else
                {
                    _Wad_Enabled = true;
                }
                return _Wad_Enabled;
            }
            set
            {
                if (_Wad_Enabled != value)
                {
                    _Wad_Enabled = value;
                    OnPropertyChanged("Wad_Enabled");
                }
            }
        }

        public bool Wad_Load
        {
            get
            {
                //if (_Wad_Enabled == false)
                //{
                //    _Wad_Load = false;
                //}
                return _Wad_Load;
            }
            set
            {
                if (_Wad_Load != value)
                {
                    _Wad_Load = value;
                    OnPropertyChanged("Wad_Load");
                }
            }
        }

        public string Wad_File
        {
            get { return _Wad_File; }
            set
            {
                if (_Wad_File != value)
                {
                    _Wad_File = value;
                    OnPropertyChanged("Wad_File");
                }
            }
        }

        public string Wad_Location
        {
            get
            {
                return _Wad_Location;
            }
            set
            {
                if (_Wad_Location != value)
                {
                    _Wad_Location = value;
                    OnPropertyChanged("Wad_Location");
                }
            }
        }

        public string Wad_Status
        {
            get
            {
                if (File.Exists(Path.Combine(_Wad_Location,_Wad_File)) != true)
                {
                    _Wad_Status = "Not found: ";
                }
                else
                {
                    _Wad_Status = "";
                }
                return _Wad_Status;
            }
            set
            {
                if (_Wad_Status != value)
                {
                    _Wad_Status = value;
                    OnPropertyChanged("Wad_Status");
                }
            }
        }

        public bool Wad_Merge
        {
            get
            {
                return _Wad_Merge;
            }
            set
            {
                if (_Wad_Merge != value)
                {
                    _Wad_Merge = value;
                    OnPropertyChanged("Wad_Merge");
                }
            }
        }

        public string Wad_Description
        {
            get
            {
                //_Wad_Description = Get_Wad_Description(Path.Combine(_Wad_Location, _Wad_File));
                return _Wad_Description;
            }
            set
            {
                if (_Wad_Description != value)
                {
                    _Wad_Description = value;
                    OnPropertyChanged("Wad_Description");
                }
            }
        }

        public Wads(bool Wad_Enabled, bool Wad_Load, string Wad_File, bool Wad_Merge, string Wad_Location, string Wad_Status=null, string Wad_Description=null)
        {
            _Wad_Enabled = Wad_Enabled;
            _Wad_Load = Wad_Load;
            _Wad_File = Wad_File;
            _Wad_Location = Wad_Location;
            _Wad_Status = Wad_Status;
            _Wad_Merge = Wad_Merge;
            _Wad_Description = Wad_Description;
        }

        public Wads()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _Wad_Enabled;
        private bool _Wad_Load;
        private string _Wad_File;
        private string _Wad_Status;
        private string _Wad_Location;
        private bool _Wad_Merge;
        private string _Wad_Description;

        void OnPropertyChanged(string Property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Property));
        }

        public string Get_Wad_Description(string Wad_File)
        {
            string Wad_Description = "";
            if (File.Exists(Wad_File) is true)
            {
                PlaylistIO Mappings = new PlaylistIO("Names.map");
                using (var MD5 = System.Security.Cryptography.MD5.Create())
                {
                    using (var Stream = File.OpenRead(Wad_File))
                    {
                        var Hash = MD5.ComputeHash(Stream);
                        string Value = BitConverter.ToString(Hash).Replace("-", "").ToLowerInvariant();
                        Wad_Description = Mappings.Read(Value, "Mappings");
                    }
                }
            }
            return Wad_Description;
        }

    }
}
