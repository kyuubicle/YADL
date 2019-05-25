using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace YADL
{
    public partial class MainWindow : Window
    {
        ViewModel VM = new ViewModel();
        Dictionary<string, string> Parameters = new Dictionary<string, string>();
        string Settings_File = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YADL.cfg");
        string Folder_Playlists_New = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Playlists");
        string Folder_Playlists_Import = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Playlists");
        string Folder_Playlists_Open = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Playlists");
        string Folder_Playlists_Save_As = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Playlists");
        string Folder_SourcePort_Open = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sourceports");
        string Folder_IWad_Open = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wads", "IWads");
        string Folder_PWads_Import = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wads", "PWads");
        string Folder_PWads_Open = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wads", "PWads");
        string Folder_Savedir_Open = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Savedir");
        string Folder_Config_Open = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");

        ObservableCollection<string> UserCategories;

        bool PWadList_LoadColumn_Visible = true;
        bool PWadList_MergeColumn_Visible = true;
        bool PWadList_LocationColumn_Visible = true;

        bool Categories_Frozen = false;
        bool Categories_Changed = false;

        ContextMenu CategoryContextMenu;
        MenuItem CategoryHeader = new MenuItem();
        MenuItem RenameMenuItem;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = VM;

            UserCategories = new ObservableCollection<string>();
            CategoryContextMenu = (ContextMenu)this.ListView_Playlists.Resources["ItemContextMenu"];
            RenameMenuItem = (MenuItem)CategoryContextMenu.Items[0];

            if (File.Exists(Settings_File))
            {
                Helper_Settings_Read();
            }
            else
            {
                Helper_Settings_Write();
            }


            if (UserCategories.Count() > 0)
            {
                ComboBox_Categories.IsEnabled = true;

                for (int i = 0; i < UserCategories.Count(); i++)
                {
                    ComboBox_Categories.Items.Add(UserCategories[i]);
                }
            }
            else
            {
                ComboBox_Categories.IsEnabled = false;
            }

            Helper_UI();
        }

        private void MenuItem_PlaylistNew_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog Playlist_New_File = new SaveFileDialog
            {
                Title = "New playlist...",
                DefaultExt = ".dpf",
                InitialDirectory = Folder_Playlists_New,
                Filter = "Doom Playlist (*.dpf)|*.dpf"
            };
            if (Playlist_New_File.ShowDialog() == true)
            {
                Folder_Playlists_New = Path.GetDirectoryName(Playlist_New_File.FileName);
                Playlists Playlist = new Playlists(true, false, Path.GetFileNameWithoutExtension(Playlist_New_File.FileName), Path.GetFileNameWithoutExtension(Playlist_New_File.FileName), 0, Path.GetDirectoryName(Playlist_New_File.FileName), "", false, "", "");
                var Duplicate = VM.Playlist.SingleOrDefault(x => (x.Playlist_FileName.ToLower() == Path.GetFileNameWithoutExtension(Playlist_New_File.FileName).ToLower() & x.Playlist_Location.ToLower() == Path.GetDirectoryName(Playlist_New_File.FileName).ToLower()));
                if (Duplicate != null)
                {
                    int Position = VM.Playlist.IndexOf(Duplicate);
                    VM.Playlist.Remove(Duplicate);
                    VM.Playlist.Insert(Position, Playlist);
                }
                else
                {
                    VM.Playlist.Add(Playlist);
                }

                ListView_Playlists.SelectedIndex = ListView_Playlists.Items.IndexOf(Playlist);

                Helper_UI();
            }
        }

        private void MenuItem_PlaylistOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Playlist_Open_File = new OpenFileDialog
            {
                InitialDirectory = Folder_Playlists_Open,
                Filter = "Doom Playlist (*.dpf)|*.dpf"
            };
            if (Playlist_Open_File.ShowDialog() == true)
            {
                Folder_Playlists_Open = Path.GetDirectoryName(Playlist_Open_File.FileName);
                List<string> Files = new List<string>
                {
                    Path.GetFullPath(Playlist_Open_File.FileName)
                };
                Helper_Playlists(Files);
            }
        }

        private void MenuItem_PlaylistImport_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog Playlist_Import_Folder = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = Folder_Playlists_Import
            };
            Playlist_Import_Folder.Description = "Select a folder to import Playlists:";
            Playlist_Import_Folder.ShowNewFolderButton = false; System.Windows.Forms.DialogResult Playlist_Import_Browser = Playlist_Import_Folder.ShowDialog();

            if (Playlist_Import_Browser == System.Windows.Forms.DialogResult.OK)
            {
                Folder_Playlists_Import = Playlist_Import_Folder.SelectedPath;
                List<string> Playlist_Import_Files = Directory.GetFiles(Playlist_Import_Folder.SelectedPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => new string[] { ".dpf" }.Contains(Path.GetExtension(file).ToLower())).ToList();
                Helper_Playlists(Playlist_Import_Files);
            }
        }

        private void MenuItem_PlaylistSave_Click(object sender, RoutedEventArgs e)
        {
            foreach (Playlists Playlist_Selected in ListView_Playlists.SelectedItems)
            {
                Playlist_Selected.Playlist_Changed = false;
                string File = Path.Combine(Playlist_Selected.Playlist_Location, Playlist_Selected.Playlist_FileName) + ".dpf".ToString();
                Helper_Save(Playlist_Selected, File);
            }
        }

        private void MenuItem_PlaylistSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var Selected_Playlists = ListView_Playlists.SelectedItems.Cast<object>().ToList();
            foreach (Playlists Playlist_Selected in Selected_Playlists)
            {
                SaveFileDialog Playlist_Save_As_File = new SaveFileDialog
                {
                    Title = "Save playlist " + Playlist_Selected.Playlist_Name + " as...",
                    DefaultExt = ".dpf",
                    InitialDirectory = Folder_Playlists_Save_As,
                    Filter = "Doom Playlist (*.dpf)|*.dpf"
                };
                if (Playlist_Save_As_File.ShowDialog() == true)
                {
                    Playlists Playlist = new Playlists(false, false, Playlist_Selected.Playlist_Name, 
                        Path.GetFileNameWithoutExtension(Playlist_Save_As_File.FileName),
                        Playlist_Selected.Playlist_Files,
                        Path.GetDirectoryName(Playlist_Save_As_File.FileName),
                        Playlist_Selected.Playlist_SourcePort,
                        Playlist_Selected.Playlist_SourcePort_HasParameters,
                        Playlist_Selected.Playlist_SourcePort_Parameters,
                        Playlist_Selected.Playlist_IWad,
                        Playlist_Selected.Playlist_Categories,
                        Playlist_Selected.Playlist_Savedir,
                        Playlist_Selected.Playlist_HasSavedir,
                        Playlist_Selected.Playlist_HasConfig,
                        Playlist_Selected.Playlist_Config,
                        Playlist_Selected.Wadlist);
                    var Duplicate = VM.Playlist.SingleOrDefault(x =>
                        (x.Playlist_FileName.ToLower() == Path.GetFileNameWithoutExtension(Playlist_Save_As_File.FileName).ToLower()
                        & x.Playlist_Location.ToLower() == Path.GetDirectoryName(Playlist_Save_As_File.FileName).ToLower()));
                    if (Duplicate != null)
                    {
                        int Position = VM.Playlist.IndexOf(Duplicate);
                        VM.Playlist.Remove(Duplicate);
                        VM.Playlist.Insert(Position, Playlist);
                    }
                    else
                    {
                        VM.Playlist.Add(Playlist);
                    }
                    Folder_Playlists_Save_As = Path.GetDirectoryName(Playlist_Save_As_File.FileName);
                    Helper_Save(Playlist_Selected, Playlist_Save_As_File.FileName);
                }
            }
        }

        private void MenuItem_PlaylistClear_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Playlists> Playlists_Save = new ObservableCollection<Playlists>(VM.Playlist.Where(x => x.Playlist_Changed == true));
            if (Playlists_Save.Count > 0)
            {
                QuitWindow Quit = new QuitWindow(Playlists_Save)
                {
                    Owner = this
                };
                Quit.ShowDialog();
                if (Quit.DialogResult == true)
                {
                    foreach (Playlists Playlist_Selected in Playlists_Save)
                    {
                        if (Playlist_Selected.Playlist_Save == true)
                        {
                            string File = Path.Combine(Playlist_Selected.Playlist_Location, Playlist_Selected.Playlist_FileName) + ".dpf".ToString();
                            Helper_Save(Playlist_Selected, File);
                        }
                    }
                    VM.Playlist.Clear();
                }
                else if (Quit.DialogResult == false)
                {
                }
                else if (Quit.DialogResult == null)
                {
                }
            }
            else
            {
                VM.Playlist.Clear();
            }
            Helper_UI();
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            bool isWindowOpen = false;
            foreach (Window Win in Application.Current.Windows)
            {
                if (Win is AboutWindow)
                {
                    isWindowOpen = true;
                    Win.Activate();
                }
            }
            if (!isWindowOpen)
            {
                AboutWindow About = new AboutWindow
                {
                    Owner = this
                };
                About.ShowDialog();
            }
        }

        private void Button_SourcePort_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog SourcePort_Open_File = new OpenFileDialog
            {
                InitialDirectory = Folder_SourcePort_Open,
                Filter = "Executable file (*.exe)|*.exe"
            };
            if (SourcePort_Open_File.ShowDialog() == true)
            {
                Folder_SourcePort_Open = Path.GetDirectoryName(SourcePort_Open_File.FileName);
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_SourcePort = SourcePort_Open_File.FileName;
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_SourcePort_Description = SourcePort_Open_File.FileName;
                Helper_UI();
            }
        }

        private void Button_Iwad_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Iwad_Open_File = new OpenFileDialog
            {
                InitialDirectory = Folder_IWad_Open,
                Filter = "Doom (*.wad,*.pk3, *.ipk3, *.pk7)|*.wad;*.pk3;*.pk3;*.pk7|All files (*.*)|*.*"
            };
            if (Iwad_Open_File.ShowDialog() == true)
            {
                Folder_IWad_Open = Path.GetDirectoryName(Iwad_Open_File.FileName);
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_IWad = Iwad_Open_File.FileName;
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_IWad_Description = Iwad_Open_File.FileName;
                Helper_UI();
            }
        }

        private void Button_Pwad_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Pwad_Open_File = new OpenFileDialog
            {
                InitialDirectory = Folder_PWads_Open,
                Filter = "Doom (*.wad,*.pk3,*.pk7,*.deh,*.bex)|*.wad;*.pk3;*.pk7;*.deh;*.bex|All files (*.*)|*.*"
            };
            if (Pwad_Open_File.ShowDialog() == true)
            {
                Folder_PWads_Open = Path.GetDirectoryName(Pwad_Open_File.FileName);
                List<string> Files = new List<string>
                {
                    Path.GetFullPath(Pwad_Open_File.FileName)
                };
                Helper_Pwads(Files);
            }
        }

        private void Button_Pwad_Import_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog Pwad_Import_Folder = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = Folder_PWads_Import
            };
            Pwad_Import_Folder.Description = "Select a folder to import PWADs:";
            Pwad_Import_Folder.ShowNewFolderButton = false;

            System.Windows.Forms.DialogResult Pwad_Import_Browser = Pwad_Import_Folder.ShowDialog();

            if (Pwad_Import_Browser == System.Windows.Forms.DialogResult.OK)
            {
                Folder_PWads_Import = Pwad_Import_Folder.SelectedPath;
                List<string> Pwad_Import_Files = Directory.GetFiles(Pwad_Import_Folder.SelectedPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => new string[] { ".wad", ".pk3", ".pk7", ".deh", ".bex" }.Contains(Path.GetExtension(file).ToLower())).ToList();
                Helper_Pwads(Pwad_Import_Files);
            }
        }

        private void Button_Pwad_Clear_Click(object sender, RoutedEventArgs e)
        {
            if (ListView_Playlists.SelectedItems.Count == 1)
            {
                ((Playlists)ListView_Playlists.SelectedItem).Wadlist.Clear();
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_Files = ((Playlists)ListView_Playlists.SelectedItem).Wadlist.Count;
            }
        }

        private void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = Label_SourcePort.ToolTip.ToString();
            string Args = " -iwad " + AddQuotesIfRequired(@Label_IWad.ToolTip.ToString());
            if (ListView_Pwads.Items.Count > 0)
            {
                foreach (Wads Wad in ListView_Pwads.Items)
                {
                    if (Wad.Wad_Load == true)
                    {
                        if (Path.GetExtension(Path.Combine(Wad.Wad_Location, Wad.Wad_File)).ToLower() == ".wad"
                         | Path.GetExtension(Path.Combine(Wad.Wad_Location, Wad.Wad_File)).ToLower() == ".pk3"
                         | Path.GetExtension(Path.Combine(Wad.Wad_Location, Wad.Wad_File)).ToLower() == ".pk7")
                        {
                            if (Wad.Wad_Merge == false)
                            {
                                Args = Args + " -file " + AddQuotesIfRequired(@Path.Combine(Wad.Wad_Location, Wad.Wad_File).ToString());
                            }
                            else
                            {
                                Args = Args + " -merge " + AddQuotesIfRequired(@Path.Combine(Wad.Wad_Location, Wad.Wad_File).ToString());
                            }
                        }
                        else if (Path.GetExtension(Path.Combine(Wad.Wad_Location, Wad.Wad_File)).ToLower() == ".deh")
                        {
                            Args = Args + " -deh " + AddQuotesIfRequired(@Path.Combine(Wad.Wad_Location, Wad.Wad_File).ToString());
                        }
                        else if (Path.GetExtension(Path.Combine(Wad.Wad_Location, Wad.Wad_File)).ToLower() == ".bex")
                        {
                            Args = Args + " -bex " + AddQuotesIfRequired(@Path.Combine(Wad.Wad_Location, Wad.Wad_File).ToString());
                        }
                        else
                        {
                            if (Wad.Wad_Merge == false)
                            {
                                Args = Args + " -file " + AddQuotesIfRequired(@Path.Combine(Wad.Wad_Location, Wad.Wad_File).ToString());
                            }
                            else
                            {
                                Args = Args + " -merge " + AddQuotesIfRequired(@Path.Combine(Wad.Wad_Location, Wad.Wad_File).ToString());
                            }
                        }
                    }
                }
                //Console.WriteLine(Args);
            }


            if (CheckBox_Config.IsChecked == true)
            {
                if (((Playlists)ListView_Playlists.SelectedItem).Playlist_Config.ToString() != "" && ((Playlists)ListView_Playlists.SelectedItem).Playlist_Config.ToString() != "Not found")
                {
                    Args = Args + " -config " + AddQuotesIfRequired(((Playlists)ListView_Playlists.SelectedItem).Playlist_Config.ToString());
                }
            }

            if (CheckBox_SaveDir.IsChecked == true)
            {
                if (((Playlists)ListView_Playlists.SelectedItem).Playlist_Savedir.ToString() != "" && ((Playlists)ListView_Playlists.SelectedItem).Playlist_Savedir.ToString() != "Not found")
                {
                    Args = Args + " -savedir " + AddQuotesIfRequired(((Playlists)ListView_Playlists.SelectedItem).Playlist_Savedir.ToString());
                }
            }

            if (((Playlists)ListView_Playlists.SelectedItem).Playlist_SourcePort_Parameters.ToString() != "")
            {
                Args = Args + " " + ((Playlists)ListView_Playlists.SelectedItem).Playlist_SourcePort_Parameters.ToString();
            }
            process.StartInfo.Arguments = Args;
            process.Start();
        }

        private void ItemContextMenu_Playlists_Remove_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Playlists> Playlists_Save_Selected = new ObservableCollection<Playlists>();
            foreach (Playlists Playlist in ListView_Playlists.SelectedItems)
            {
                Playlists_Save_Selected.Add(Playlist);
            }
            ObservableCollection<Playlists> Playlists_Save = new ObservableCollection<Playlists>(Playlists_Save_Selected.Where(x => x.Playlist_Changed == true));
            if (Playlists_Save.Count > 0)
            {
                QuitWindow Quit = new QuitWindow(Playlists_Save)
                {
                    Owner = this
                };
                Quit.ShowDialog();
                if (Quit.DialogResult == true)
                {
                    foreach (Playlists Playlist_Selected in Playlists_Save)
                    {
                        if (Playlist_Selected.Playlist_Save == true)
                        {
                            string File = Path.Combine(Playlist_Selected.Playlist_Location, Playlist_Selected.Playlist_FileName) + ".dpf".ToString();
                            Helper_Save(Playlist_Selected, File);
                        }
                    }
                    var Selected_Playlists = ListView_Playlists.SelectedItems.Cast<object>().ToList();
                    foreach (Playlists Playlist in Selected_Playlists)
                    {
                        VM.Playlist.Remove(Playlist);
                    }
                }
                else if (Quit.DialogResult == false)
                {
                }
                else if (Quit.DialogResult == null)
                {
                }
            }
            else
            {
                var Selected_Playlists = ListView_Playlists.SelectedItems.Cast<object>().ToList();
                foreach (Playlists Playlist in Selected_Playlists)
                {
                    VM.Playlist.Remove(Playlist);
                }
            }
        }

        private void ItemContextMenu_Pwads_Toggle_Load_Click(object sender, RoutedEventArgs e)
        {
            var Selected_Pwads = ListView_Pwads.SelectedItems;
            foreach (Wads Pwad in Selected_Pwads)
            {
                if (Pwad.Wad_Load.Equals(true))
                {
                    Pwad.Wad_Load = false;
                }
                else
                {
                    Pwad.Wad_Load = true;
                }
            }

            Helper_UI();
        }

        private void ItemContextMenu_Pwads_Remove_Click(object sender, RoutedEventArgs e)
        {
            var Selected_Pwads = ListView_Pwads.SelectedItems.Cast<object>().ToList();
            foreach (Wads Pwad in Selected_Pwads)
            {
                ((Playlists)ListView_Playlists.SelectedItem).Wadlist.Remove(Pwad);
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_Files = ((Playlists)ListView_Playlists.SelectedItem).Wadlist.Count;
            }
        }

        private void ListView_Playlists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Helper_UI();
        }

        private void ListView_Pwads_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Helper_UI();
        }

        private void CheckBox_Load_Toogle(object sender, RoutedEventArgs e)
        {
            ((Playlists)ListView_Playlists.SelectedItem).Playlist_Changed = true;
        }

        private void CheckBox_Merge_Toogle(object sender, RoutedEventArgs e)
        {
            ((Playlists)ListView_Playlists.SelectedItem).Playlist_Changed = true;
        }

        private void ListView_Pwads_Drop(object sender, DragEventArgs e)
        {
            if (((Playlists)ListView_Playlists.SelectedItem).Playlist_Files > 1)
            {
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_Changed = true;
            }
        }

        private void ProgressBar_Load_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TextBlock_Load.Visibility = (ProgressBar_Load.Value == 0 | ProgressBar_Load.Value == 100) ? Visibility.Collapsed : Visibility.Visible;
            ProgressBar_Load.Visibility = (ProgressBar_Load.Value == 0 | ProgressBar_Load.Value == 100) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            int Playlist_Save = 0;

            ListView_Playlists.Items.Filter = null;

            foreach (Playlists Playlist in VM.Playlist)
            {
                if (Playlist.Playlist_Changed == true)
                {
                    Playlist_Save++;
                }
            }
            if (Playlist_Save > 0)
            {
                ObservableCollection<Playlists> Playlists_Save = new ObservableCollection<Playlists>(VM.Playlist.Where(x => x.Playlist_Changed == true));
                QuitWindow Quit = new QuitWindow(Playlists_Save)
                {
                    Owner = this
                };
                Quit.ShowDialog();
                if (Quit.DialogResult == true)
                {
                    foreach (Playlists Playlist_Selected in Playlists_Save)
                    {
                        if (Playlist_Selected.Playlist_Save == true)
                        {
                            string File = Path.Combine(Playlist_Selected.Playlist_Location, Playlist_Selected.Playlist_FileName) + ".dpf".ToString();
                            Helper_Save(Playlist_Selected, File);
                        }
                    }
                }
                else if (Quit.DialogResult == false)
                {
                    e.Cancel = true;
                }
                else if (Quit.DialogResult == null)
                {
                }
            }
            Helper_Settings_Write();
        }

        private async void Helper_Playlists(List<string> Files)
        {
            foreach (var File in Files)
            {
                FileInfo File_Info = new FileInfo(File);
                if (File_Info.Extension.ToLower().Equals(".dpf"))
                {
                    string Playlist_Name = "";

                    PlaylistIO Dpf = new PlaylistIO(File_Info.FullName);
                    if (Dpf.KeyExists("Name", "Info"))
                        Playlist_Name = Dpf.Read("Name", "Info");
                    else
                        Playlist_Name = Path.GetFileNameWithoutExtension(File_Info.Name);

                    int Categories = 0;
                    if (Dpf.KeyExists("Counter", "Categories"))
                    {
                        Categories = Int32.Parse(Dpf.Read("Counter", "Categories"));
                    }

                    int Playlist_Files = Int32.Parse(Dpf.Read("Counter", "PWAD"));

                    string Playlist_Location = File_Info.DirectoryName;

                    string Playlist_Sourceport = Dpf.Read("Path", "SourcePort");
                    bool Playlist_SourcePort_HasParameters = Dpf.KeyExists("Parameters", "SourcePort");
                    string Playlist_SourcePort_Parameters = "";
                    if (Playlist_SourcePort_HasParameters == true)
                    {
                        Playlist_SourcePort_Parameters = Dpf.Read("Parameters", "SourcePort");
                    }

                    string Playlist_Iwad = Dpf.Read("Path", "IWAD");

                    bool Playlist_HasConfig = Dpf.KeyExists("Enable", "Config");
                    string Playlist_Config = Dpf.Read("Path", "Config");

                    bool Playlist_HasSavedir = Dpf.KeyExists("Enable", "Savedir");
                    string Playlist_Savedir = Dpf.Read("Path", "Savedir");

                    ObservableCollection<string> Playlist_Categories = new ObservableCollection<string>();
                    ObservableCollection<Wads> Playlist_Wads = new ObservableCollection<Wads>();

                    var Progress = new Progress<int>(Value => ProgressBar_Load.Value = Value);
                    TextBlock_Load.Text = "Loading playlist " + Playlist_Name + " pwad list...";

                    await Task.Run(() =>
                    {
                        for (int i = 1; i <= Playlist_Files; ++i)
                        {
                            int Result = (int)Math.Round((double)i / Playlist_Files * 100);
                            string Wad = Dpf.Read("Path" + i.ToString(), "PWAD");
                            string Wad_File_Path = Wad;
                            bool Wad_Load = true;
                            bool Wad_Merge = false;
                            if (Wad.Contains("|"))
                            {
                                List<string> Wad_Holder = Wad.Split('|').ToList();
                                Wad_Merge = Boolean.Parse(Wad_Holder[2]);
                                Wad_Load = Boolean.Parse(Wad_Holder[1]);
                                Wad_File_Path = Wad_Holder[0];
                            }
                            FileInfo Wad_File_Info = new FileInfo(Wad_File_Path);
                            string Wad_File = Path.GetFileName(Wad_File_Info.Name);
                            string Wad_Location = Wad_File_Info.DirectoryName;
                            Wads Pwad = new Wads(true, Wad_Load, Wad_File, Wad_Merge, Wad_Location);
                            Playlist_Wads.Add(Pwad);
                            ((IProgress<int>)Progress).Report(Result);
                        }
                    });

                    Progress = new Progress<int>(Value => ProgressBar_Load.Value = Value);
                    TextBlock_Load.Text = "Loading playlist " + Playlist_Name + "categories...";

                    await Task.Run(() =>
                    {
                        for (int i = 1; i <= Categories; ++i)
                        {
                            int Result = (int)Math.Round((double)i / Categories * 100);
                            Playlist_Categories.Add(Dpf.Read("Category" + i.ToString(), "Categories"));
                            ((IProgress<int>)Progress).Report(Result);
                        }
                    });

                    var Duplicate = VM.Playlist.SingleOrDefault(x => (x.Playlist_FileName.ToLower() == Path.GetFileNameWithoutExtension(File_Info.Name).ToLower() & x.Playlist_Location.ToLower() == Playlist_Location.ToLower()));
                    if (Duplicate == null)
                    {
                        Playlists Playlist = new Playlists(false, false, Playlist_Name, Path.GetFileNameWithoutExtension(File_Info.Name), Playlist_Files, Playlist_Location, Playlist_Sourceport, Playlist_SourcePort_HasParameters, Playlist_SourcePort_Parameters, Playlist_Iwad, Playlist_Categories, Playlist_Savedir, Playlist_HasSavedir, Playlist_HasConfig, Playlist_Config, Playlist_Wads);
                        VM.Playlist.Add(Playlist);
                    }
                }
            }
            Helper_UI();
        }

        private void Helper_Pwads(List<string> Files)
        {
            foreach (var File in Files)
            {
                var Duplicate = ((Playlists)ListView_Playlists.SelectedItem).Wadlist.SingleOrDefault(x => (x.Wad_File.ToLower() == Path.GetFileName(File).ToLower() & x.Wad_Location.ToLower() == Path.GetDirectoryName(File).ToLower()));
                if (Duplicate == null)
                {
                    Wads Pwad = new Wads(true, true, Path.GetFileName(File), false, Path.GetDirectoryName(File));
                    ((Playlists)ListView_Playlists.SelectedItem).Wadlist.Add(Pwad);
                }
            }
            ((Playlists)ListView_Playlists.SelectedItem).Playlist_Files = ((Playlists)ListView_Playlists.SelectedItem).Wadlist.Count;
            Helper_UI();
        }

        private void Helper_Save(Playlists Playlist, string File)
        {
            PlaylistIO Dpf = new PlaylistIO(File);

            Dpf.DeleteSection("Info");
            Dpf.DeleteSection("SourcePort");
            Dpf.DeleteSection("IWAD");
            Dpf.DeleteSection("PWAD");
            Dpf.DeleteSection("Config");
            Dpf.DeleteSection("Savedir");
            Dpf.DeleteSection("Categories");

            Dpf.Write("Name", Playlist.Playlist_Name, "Info");

            Dpf.Write("Path", Playlist.Playlist_SourcePort.ToString(), "SourcePort");
            if (Playlist.Playlist_SourcePort_HasParameters == true)
            {
                Dpf.Write("Parameters", Playlist.Playlist_SourcePort_Parameters.ToString(), "SourcePort");
            }

            Dpf.Write("Path", Playlist.Playlist_IWad.ToString(), "IWAD");

            Dpf.Write("Counter", Playlist.Playlist_Files.ToString(), "PWAD");
            for (int i = 0; i < Playlist.Wadlist.Count; i++)
            {
                Dpf.Write("Path" + (i + 1).ToString(), Path.Combine(
                    Playlist.Wadlist[i].Wad_Location,
                    Playlist.Wadlist[i].Wad_File) + "|" +
                    Playlist.Wadlist[i].Wad_Load.ToString() + "|" +
                    Playlist.Wadlist[i].Wad_Merge.ToString()
                    , "PWAD");
            }

            if (Playlist.Playlist_HasConfig == true)
                Dpf.Write("Enable", Playlist.Playlist_HasConfig.ToString(), "Config");
            if (!String.IsNullOrEmpty(Playlist.Playlist_Config))
                Dpf.Write("Path", Playlist.Playlist_Config.ToString(), "Config");

            if (Playlist.Playlist_HasSavedir == true)
                Dpf.Write("Enable", Playlist.Playlist_HasConfig.ToString(), "Savedir");
            if(!String.IsNullOrEmpty(Playlist.Playlist_Savedir))
                Dpf.Write("Path", Playlist.Playlist_Savedir.ToString(), "Savedir");

            Dpf.Write("Counter", Playlist.Playlist_Categories.Count().ToString(), "Categories");
            for(int i = 0; i < Playlist.Playlist_Categories.Count(); i++)
            {
                Dpf.Write("Category" + (i + 1).ToString(), Playlist.Playlist_Categories[i], "Categories");
            }

            Helper_UI();
        }

        private void Helper_Settings_Read()
        {
            PlaylistIO Settings = new PlaylistIO(Settings_File);
            if (Boolean.Parse(Settings.Read("Maximized", "Window Settings")))
            {
                Width = Double.Parse(Settings.Read("Width", "Window Settings"));
                Height = Double.Parse(Settings.Read("Height", "Window Settings"));
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                Width = Double.Parse(Settings.Read("Width", "Window Settings"));
                Height = Double.Parse(Settings.Read("Height", "Window Settings"));
            }

            if (Settings.KeyExists("X_Position", "Window Settings"))
            {
                Left = Double.Parse(Settings.Read("X_Position", "Window Settings"));
            }
            if (Settings.KeyExists("Y_Position", "Window Settings"))
            {
                Top = Double.Parse(Settings.Read("Y_Position", "Window Settings"));
            }
            if (Settings.KeyExists("Splitter", "Window Settings"))
            {
                Grid_Left.ColumnDefinitions[0].Width = new GridLength(Double.Parse(Settings.Read("Splitter", "Window Settings")), GridUnitType.Pixel);
            }

            Folder_Playlists_New = Settings.Read("Playlists_New", "Paths");
            Folder_Playlists_Import = Settings.Read("Playlists_Import", "Paths");
            Folder_Playlists_Open = Settings.Read("Playlists_Open", "Paths");
            Folder_Playlists_Save_As = Settings.Read("Playlists_Save_As", "Paths");
            Folder_SourcePort_Open = Settings.Read("SourcePort_Open", "Paths");
            Folder_IWad_Open = Settings.Read("IWad_Open", "Paths");
            Folder_PWads_Import = Settings.Read("PWads_Import", "Paths");
            Folder_PWads_Open = Settings.Read("PWads_Open", "Paths");

            if(Settings.KeyExists("Config_Open","Paths"))
                Folder_Config_Open = Settings.Read("Config_Open", "Paths");
            if(Settings.KeyExists("Savedir_Open","Paths"))
                Folder_Savedir_Open = Settings.Read("Savedir_Open", "Paths");

            if (Settings.KeyExists("PWad_List_Location_Visible", "Options"))
                PWadList_LocationColumn_Visible = Boolean.Parse(Settings.Read("PWad_List_Location_Visible", "Options"));

            if (Settings.KeyExists("PWad_List_Load_Visible", "Options"))
                PWadList_LoadColumn_Visible = Boolean.Parse(Settings.Read("PWad_List_Load_Visible", "Options"));

            if (Settings.KeyExists("PWad_List_Merge_Visible", "Options"))
                PWadList_MergeColumn_Visible = Boolean.Parse(Settings.Read("PWad_List_Merge_Visible", "Options"));

            if (Settings.KeyExists("Save", "Session"))
            {
                if (Boolean.Parse(Settings.Read("Save", "Session")))
                {
                    MenuItem_SaveSession.IsChecked = true;
                }
                else
                {
                    MenuItem_SaveSession.IsChecked = false;
                }
            }
            if (Settings.KeyExists("Counter", "Playlists"))
            {
                int Playlist_Files = Int32.Parse(Settings.Read("Counter", "Playlists"));
                List<string> Files = new List<string>();
                for (int i = 1; i <= Playlist_Files; ++i)
                {
                    if (File.Exists(Settings.Read("Playlist" + i.ToString(), "Playlists")))
                    {
                        Files.Add(Settings.Read("Playlist" + i.ToString(), "Playlists"));
                    }
                }
                Helper_Playlists(Files);
            }

            if (Settings.KeyExists("Counter", "Categories"))
            {
                int Categories = Int32.Parse(Settings.Read("Counter", "Categories"));
                CategoryHeader = new MenuItem();
                CategoryHeader.Header = "Category";

                for (int i = 1; i <= Categories; i++)
                {
                    UserCategories.Add(Settings.Read("Category" + i.ToString(), "Categories"));

                    TabItem ti = new TabItem();
                    ti.Header = UserCategories[i - 1];
                    tabControl.Items.Insert(tabControl.Items.Count - 1, ti);

                    MenuItem mi = new MenuItem();
                    mi.Header = UserCategories[i - 1];
                    mi.Click += MenuItem_Category_Click;
                    CategoryHeader.Items.Add(mi);
                }

                CategoryContextMenu.Items.Add(CategoryHeader);
            }

            if (!Settings.KeyExists("Config","Hidden_Features"))
            {
                LabelConfig.Visibility = Visibility.Hidden;
                RowConfig.Visibility = Visibility.Hidden;
            }
            if (!Settings.KeyExists("SaveDir", "Hidden_Features"))
            {
                LabelSaveDir.Visibility = Visibility.Hidden;
                RowSaveDir.Visibility = Visibility.Hidden;
            }
        }

        private void MenuItem_Category_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ListView_Playlists.SelectedItems.Count; i++)
            {
                Playlists p = (Playlists)ListView_Playlists.SelectedItems[i];

                if (p.Playlist_Categories.Contains(((MenuItem)sender).Header.ToString()))
                {
                    p.Playlist_Categories.Remove(((MenuItem)sender).Header.ToString());
                }
                else
                {
                    p.Playlist_Categories.Add(((MenuItem)sender).Header.ToString());
                }

                p.Playlist_Changed = true;

            }

            Helper_UI();
        }

        private void Helper_Settings_Write()
        {
            PlaylistIO Settings = new PlaylistIO(Settings_File);
            switch (WindowState)
            {
                case (WindowState.Maximized):
                {
                    Settings.Write("Maximized", "True", "Window Settings");
                    break;
                }
                case (WindowState.Minimized):
                {
                    Settings.Write("Maximized", "False", "Window Settings");
                    Settings.Write("Width", Width.ToString(), "Window Settings");
                    Settings.Write("Height", Height.ToString(), "Window Settings");
                    break;
                }
                case (WindowState.Normal):
                {
                    Settings.Write("Maximized", "False", "Window Settings");
                    Settings.Write("Width", Width.ToString(), "Window Settings");
                    Settings.Write("Height", Height.ToString(), "Window Settings");
                    break;
                }
            }
            Settings.Write("X_Position", Left.ToString(), "Window Settings");
            Settings.Write("Y_Position", Top.ToString(), "Window Settings");
            Settings.Write("Splitter", Grid_Left.ColumnDefinitions[0].ActualWidth.ToString(), "Window Settings");
            Settings.Write("Playlists_New", Folder_Playlists_New.ToString(), "Paths");
            Settings.Write("Playlists_Import", Folder_Playlists_Import.ToString(), "Paths");
            Settings.Write("Playlists_Open", Folder_Playlists_Open.ToString(), "Paths");
            Settings.Write("Playlists_Save_As", Folder_Playlists_Save_As.ToString(), "Paths");
            Settings.Write("SourcePort_Open", Folder_SourcePort_Open.ToString(), "Paths");
            Settings.Write("IWad_Open", Folder_IWad_Open.ToString(), "Paths");
            Settings.Write("PWads_Import", Folder_PWads_Import.ToString(), "Paths");
            Settings.Write("PWads_Open", Folder_PWads_Open.ToString(), "Paths");
            Settings.Write("Config_Open", Folder_Config_Open.ToString(), "Paths");
            Settings.Write("Savedir_Open", Folder_Savedir_Open.ToString(), "Paths");
            Settings.Write("Save", MenuItem_SaveSession.IsChecked.ToString(), "Session");

            Settings.DeleteSection("Options");
            Settings.Write("PWad_List_Location_Visible", PWadList_LocationColumn_Visible.ToString(), "Options");
            Settings.Write("PWad_List_Load_Visible", PWadList_LoadColumn_Visible.ToString(), "Options");
            Settings.Write("PWad_List_Merge_Visible", PWadList_MergeColumn_Visible.ToString(), "Options");

            Settings.DeleteSection("Categories");
            Settings.Write("Counter", UserCategories.Count().ToString(), "Categories");
            if (UserCategories.Count() > 0)
            {
                for (int i = 0; i < UserCategories.Count(); i++)
                {
                    Settings.Write("Category" + (i + 1).ToString(), UserCategories[i], "Categories");
                }
            }

            if (MenuItem_SaveSession.IsChecked)
            {
                Settings.DeleteSection("Playlists");
                Settings.Write("Counter", VM.Playlist.Count().ToString(), "Playlists");
                for (int i = 0; i < ListView_Playlists.Items.Count; i++)
                {
                    Settings.Write("Playlist" + (i + 1).ToString(), Path.Combine(
                        ((Playlists)ListView_Playlists.Items.GetItemAt(i)).Playlist_Location,
                        ((Playlists)ListView_Playlists.Items.GetItemAt(i)).Playlist_FileName + ".dpf")
                        , "Playlists");
                }
            }
        }

        private void Helper_UI()
        {

            if(Categories_Changed)
            {
                Categories_Frozen = true;

                while(UserCategories.Contains("All"))
                {
                    UserCategories.Remove("All");
                }

                while (UserCategories.Contains("+"))
                {
                    UserCategories.Remove("+");
                }

                //Clear the Categories from context menu and combo box
                CategoryHeader.Items.Clear();
                ComboBox_Categories.Items.Clear();

                for (int i = 0; i < UserCategories.Count; i++)
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = UserCategories[i];
                    mi.Click += MenuItem_Category_Click;
                    CategoryHeader.Items.Add(mi);

                    ComboBox_Categories.Items.Add(UserCategories[i]);
                }

                Categories_Frozen = false;
                Categories_Changed = false;
            }

            if (ListView_Playlists.View is GridView GV_Playlists)
            {
                foreach (GridViewColumn GVC_Playlists in GV_Playlists.Columns)
                {
                    GVC_Playlists.Width = GVC_Playlists.ActualWidth;
                    GVC_Playlists.Width = Double.NaN;
                }
            }

            if (PWadList_LoadColumn_Visible)
            {
                PwadList_LoadColumn.Width = PwadList_LoadColumn.ActualWidth;
                PwadList_LoadColumn.Width = Double.NaN;
            }
            else
            {
                PwadList_LoadColumn.Width = 0;
            }

            if (PWadList_MergeColumn_Visible)
            {
                PwadList_MergeColumn.Width = PwadList_MergeColumn.ActualWidth;
                PwadList_MergeColumn.Width = Double.NaN;
            }
            else
            { 
                PwadList_MergeColumn.Width = 0;
            }
            if (PWadList_LocationColumn_Visible)
            {
                PwadList_LocationColumn.Width = PwadList_LocationColumn.ActualWidth;
                PwadList_LocationColumn.Width = Double.NaN;
            }
            else
            { 
                PwadList_LocationColumn.Width = 0;
            }
            //if (ListView_Pwads.View is GridView GV_Pwads)
            //{

            //    foreach (GridViewColumn GVC_Pwads in GV_Pwads.Columns)
            //    {
            //        GVC_Pwads.Width = GVC_Pwads.ActualWidth;
            //        GVC_Pwads.Width = Double.NaN;
            //    }
            //}

            if (ListView_Playlists.Items.Count > 0)
            {
                MenuItem_Clear.IsEnabled = true;
            }
            else
            {
                MenuItem_Clear.IsEnabled = false;
            }

            if (TextBlock_SourcePort.Text.ToString() == "Not found"
                | TextBlock_Iwad.Text.ToString() == "Not found"
                | TextBlock_Config.Text.ToString() == "Not found"
                | String.IsNullOrEmpty(TextBlock_Iwad.Text.ToString())
                | String.IsNullOrEmpty(TextBlock_SourcePort.Text.ToString())
                | (CheckBox_Config.IsChecked == true && (TextBlock_Config.Text.ToString() == "Not found" | String.IsNullOrEmpty(TextBlock_Config.Text.ToString())))
                | (CheckBox_SaveDir.IsChecked == true && (TextBlock_Savedir.Text.ToString() == "Not found" | String.IsNullOrEmpty(TextBlock_Savedir.Text.ToString())))
                | ListView_Playlists.SelectedItems.Count > 1)
            {
                Button_Play.IsEnabled = false;
            }
            else
            {
                Button_Play.IsEnabled = true;
            }

            if (String.IsNullOrEmpty(TextBlock_SourcePort.Text.ToString()) | ListView_Playlists.SelectedItems.Count > 1)
            {
                CheckBox_Parameters.IsEnabled = false;
            }
            else
            {
                CheckBox_Parameters.IsEnabled = true;
            }

            if (ListView_Playlists.SelectedItems.Count < 1)
            {
                Button_Iwad_Open.IsEnabled = false;
                MenuItem_Save.IsEnabled = false;
                MenuItem_SaveAs.IsEnabled = false;
                Button_Pwad_Clear.IsEnabled = false;
                Button_Pwad_Import.IsEnabled = false;
                Button_Pwad_Open.IsEnabled = false;
                Button_SourcePort_Open.IsEnabled = false;
                CheckBox_Config.IsEnabled = false;
                CheckBox_SaveDir.IsEnabled = false;
                ListView_Pwads.IsEnabled = false;

                Button_Config_Open.IsEnabled = false;
                Button_Savedir_Open.IsEnabled = false;
                ComboBox_Categories.IsEnabled = false;
                TextBox_PWad_Filter.IsEnabled = false;

                CategoryHeader.IsEnabled = false;

                RenameMenuItem.IsEnabled = false;
            }
            else if (ListView_Playlists.SelectedItems.Count == 1)
            {
                Button_Iwad_Open.IsEnabled = true;
                MenuItem_Save.IsEnabled = true;
                MenuItem_SaveAs.IsEnabled = true;
                Button_Pwad_Clear.IsEnabled = true;
                Button_Pwad_Import.IsEnabled = true;
                Button_Pwad_Open.IsEnabled = true;
                Button_SourcePort_Open.IsEnabled = true;
                CheckBox_Config.IsEnabled = true;
                CheckBox_SaveDir.IsEnabled = true;
                ListView_Pwads.IsEnabled = true;

                Button_Config_Open.IsEnabled = true;
                Button_Savedir_Open.IsEnabled = true;
                TextBox_PWad_Filter.IsEnabled = true;

                RenameMenuItem.IsEnabled = true;

                if (UserCategories.Count() > 0)
                {
                    Categories_Frozen = true;

                    ComboBox_Categories.SelectedItems.Clear();
                    ComboBox_Categories.IsEnabled = true;
                    CategoryHeader.IsEnabled = true;

                    for (int i = 0; i < ((Playlists)ListView_Playlists.SelectedItem).Playlist_Categories.Count; i++)
                    {
                        ComboBox_Categories.SelectedItems.Add(((Playlists)ListView_Playlists.SelectedItem).Playlist_Categories[i]);
                    }

                    for (int i = 0; i < UserCategories.Count; i++)
                    {
                        ((MenuItem)CategoryHeader.Items[i]).IsChecked = ((Playlists)ListView_Playlists.SelectedItem).Playlist_Categories.Contains(UserCategories[i]);
                    }

                    Categories_Frozen = false;
                }
                  
            }
            else
            {
                Button_Iwad_Open.IsEnabled = false;
                MenuItem_Save.IsEnabled = true;
                MenuItem_SaveAs.IsEnabled = true;
                Button_Pwad_Clear.IsEnabled = false;
                Button_Pwad_Import.IsEnabled = false;
                Button_Pwad_Open.IsEnabled = false;
                Button_SourcePort_Open.IsEnabled = false;
                CheckBox_Config.IsEnabled = false;
                CheckBox_SaveDir.IsEnabled = false;
                ListView_Pwads.IsEnabled = false;

                Button_Config_Open.IsEnabled = false;
                Button_Savedir_Open.IsEnabled = false;
                ComboBox_Categories.IsEnabled = false;
                TextBox_PWad_Filter.IsEnabled = false;

                CategoryHeader.IsEnabled = true;

                RenameMenuItem.IsEnabled = false;

                for (int i=0; i < UserCategories.Count; i++)
                {
                    ((MenuItem)CategoryHeader.Items[i]).IsChecked = true;

                    for(int j = 0; j < ListView_Playlists.SelectedItems.Count; j++)
                    {
                        if (((Playlists)ListView_Playlists.SelectedItems[j]).Playlist_Categories.Contains(UserCategories[i]) == false)
                        {
                            ((MenuItem)CategoryHeader.Items[i]).IsChecked = false;
                            break;
                        }
                    }
                }
            }

            if (ListView_Pwads.Items.Count < 1 | ListView_Playlists.SelectedItems.Count > 1)
            {
                Button_Pwad_Clear.IsEnabled = false;
            }
            else
            {
                Button_Pwad_Clear.IsEnabled = true;
            }

            if (CheckBox_Config.IsChecked == true)
            {
                Button_Config_Open.IsEnabled = true;
            }
            else
            {
                Button_Config_Open.IsEnabled = false;
            }

            if (CheckBox_SaveDir.IsChecked == true)
            {
                Button_Savedir_Open.IsEnabled = true;
            }
            else
            {
                Button_Savedir_Open.IsEnabled = false;
            }
        }

        private string AddQuotesIfRequired(string Path_String)
        {
            return !string.IsNullOrEmpty(Path_String) ?
                Path_String.Contains(" ") ? "\"" + Path_String + "\"" : Path_String
                : string.Empty;
        }

        private void Button_Config_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Config_Open_File = new OpenFileDialog
            {
                InitialDirectory = Folder_Config_Open,
                Filter = "GZDoom Config (*.cfg,*.ini)|*.cfg;*.ini|All files (*.*)|*.*"
            };
            if (Config_Open_File.ShowDialog() == true)
            {
                Folder_Config_Open = Path.GetDirectoryName(Config_Open_File.FileName);
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_Config = Config_Open_File.FileName;
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_Config_Description = Config_Open_File.FileName;
                Helper_UI();
            }
        }

        private void Button_Savedir_Open_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog Saves_Folder = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = Folder_Savedir_Open
            };
            Saves_Folder.Description = "Select a folder for this profile's saved games:";
            Saves_Folder.ShowNewFolderButton = true;

            System.Windows.Forms.DialogResult SaveFolder_Browser = Saves_Folder.ShowDialog();

            if (SaveFolder_Browser == System.Windows.Forms.DialogResult.OK)
            {
                Folder_Savedir_Open = Saves_Folder.SelectedPath;
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_Savedir = Saves_Folder.SelectedPath;
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_Savedir_Description = Saves_Folder.SelectedPath;
                Helper_UI();
            }
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBox_Playlist_Filter_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.TextBox_Playlist_Filter.Text == "Type here to filter playlists...")
            {
                this.TextBox_Playlist_Filter.Text = "";
                this.TextBox_Playlist_Filter.FontStyle = FontStyles.Normal;
            }
        }

        private void TextBox_Playlist_Filter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.TextBox_Playlist_Filter.Text == "")
            {
                this.TextBox_Playlist_Filter.Text = "Type here to filter playlists...";
                this.TextBox_Playlist_Filter.FontStyle = FontStyles.Italic;
            }
        }

        private void TextBox_Playlist_Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helper_Filter();
        }

        private void ListView_Pwads_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var Selected_Pwads = ListView_Pwads.SelectedItems;
            foreach (Wads Pwad in Selected_Pwads)
            {
                if (Pwad.Wad_Load.Equals(true))
                {
                    Pwad.Wad_Load = false;
                }
                else
                {
                    Pwad.Wad_Load = true;
                }
            }

            Helper_UI();
        }

        private void ItemContextMenu_Pwads_Toggle_Merge_Click(object sender, RoutedEventArgs e)
        {
            var Selected_Pwads = ListView_Pwads.SelectedItems;
            foreach (Wads Pwad in Selected_Pwads)
            {
                if (Pwad.Wad_Merge.Equals(true))
                {
                    Pwad.Wad_Merge = false;
                }
                else
                {
                    Pwad.Wad_Merge = true;
                }
            }

            Helper_UI();
        }

        private void ColumnLoad_View_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (PWadList_LoadColumn_Visible)
                PWadList_LoadColumn_Visible = false;
            else
                PWadList_LoadColumn_Visible = true;

            Helper_UI();
        }

        private void ColumnMerge_View_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (PWadList_MergeColumn_Visible)
                PWadList_MergeColumn_Visible = false;
            else
                PWadList_MergeColumn_Visible = true;

            Helper_UI();
        }

        private void ColumnLocation_View_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (PWadList_LocationColumn_Visible)
                PWadList_LocationColumn_Visible = false;
            else
                PWadList_LocationColumn_Visible = true;

            Helper_UI();
        }

        private void TabControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TabItem ti = tabControl.SelectedItem as TabItem;
            
            if (ti.Header.ToString() == "+")
            {
                string promptValue = Prompt.ShowDialog("Category: ", "Enter name for new category (leave blank to cancel)");

                if (promptValue == "+" || promptValue == "All")
                {
                    MessageBox.Show("Cannot name a new category \"+\" or \"All\"");
                    tabControl.SelectedIndex = 0;
                }
                else if (promptValue != "")
                {
                    if (UserCategories == null)
                    {
                        UserCategories = new ObservableCollection<string>();
                    }

                    UserCategories.Add(promptValue);

                    TabItem newTab = new TabItem();

                    newTab.Header = promptValue;
                    tabControl.Items.Insert(tabControl.Items.IndexOf(ti),newTab);

                    MenuItem mi = new MenuItem();
                    mi.Header = promptValue;
                    mi.Click += MenuItem_Category_Click;

                    CategoryHeader.Items.Add(mi);
                }
                else
                {
                    tabControl.SelectedIndex = 0;
                }
            }

            Helper_Filter();
        }

        private void Helper_Filter()
        {
            //Gotta put this is a try/catch block so it doesn't crash before everything is setup and such...
            try
            {
                TabItem ti = tabControl.SelectedItem as TabItem;
                Predicate<object> Filter = null;

                //Filter by category
                if (ti.Header.ToString() != "All" & ti.Header.ToString() != "+")
                {
                    if (String.IsNullOrEmpty(TextBox_Playlist_Filter.Text) != true & TextBox_Playlist_Filter.Text != "Type here to filter playlists...")
                        Filter = new Predicate<object>(o => ((Playlists)o).Playlist_Categories.Contains(ti.Header.ToString()) && ((Playlists)o).Playlist_FileName.ToLower().Contains(TextBox_Playlist_Filter.Text.ToLower()));
                    else
                        Filter = new Predicate<object>(o => ((Playlists)o).Playlist_Categories.Contains(ti.Header.ToString()));
                }
                else if (String.IsNullOrEmpty(TextBox_Playlist_Filter.Text) != true & TextBox_Playlist_Filter.Text != "Type here to filter playlists...")
                {
                    Filter = new Predicate<object>(o => ((Playlists)o).Playlist_FileName.ToLower().Contains(TextBox_Playlist_Filter.Text.ToLower()));
                }

                ListView_Playlists.Items.Filter = null;
                ListView_Playlists.Items.Filter = Filter;

                Helper_UI();
            }
            catch (Exception)
            {

            }

        }

        private void CheckBox_SaveDirConfig_Click(object sender, RoutedEventArgs e)
        {
            Helper_UI();
        }

        private void ComboBox_Categories_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            try
            {
                ObservableCollection<string> PlaylistCategories = ((Playlists)ListView_Playlists.SelectedItem).Playlist_Categories;
                bool madeChanges = false;

                if (!Categories_Frozen)
                {
                    //Remove any Categories that aren't in the ComboBox's selected items
                    for (int i = 0; i < PlaylistCategories.Count; i++)
                    {
                        if (!ComboBox_Categories.SelectedItems.Contains(PlaylistCategories[i]))
                        {
                            PlaylistCategories.Remove(PlaylistCategories[i]);
                            madeChanges = true;
                        }
                    }

                    //Add any playlists that are in the ComboBox's selected items, but aren't in the Playlist's Categories
                    for (int i = 0; i < ComboBox_Categories.SelectedItems.Count; i++)
                    {
                        if (!PlaylistCategories.Contains(ComboBox_Categories.SelectedItems[i]))
                        {
                            PlaylistCategories.Add(ComboBox_Categories.SelectedItems[i].ToString());
                            madeChanges = true;
                        }
                    }

                    if (madeChanges)
                    {
                        ((Playlists)ListView_Playlists.SelectedItem).Playlist_Changed = true;
                    }

                    Helper_Filter();
                }

            }
            catch (Exception)
            {

            }
        }

        private void TextBox_PWad_Filter_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_PWad_Filter_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_PWad_Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBox_PWad_Filter.Text != "")
            {
                ListView_Pwads.Items.Filter = new Predicate<object>(o => ((Wads)o).Wad_File.ToLower().Contains(TextBox_PWad_Filter.Text.ToLower()));
            }
            else
            {
                ListView_Pwads.Items.Filter = null;
            }
        }

        private void ListView_Pwads_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space || e.Key == System.Windows.Input.Key.Enter)
            {
                var Selected_Pwads = ListView_Pwads.SelectedItems;
                foreach (Wads Pwad in Selected_Pwads)
                {
                    if (Pwad.Wad_Load.Equals(true))
                    {
                        Pwad.Wad_Load = false;
                    }
                    else
                    {
                        Pwad.Wad_Load = true;
                    }
                }

                Helper_UI();
            }

            e.Handled = true;
        }

        private void TabControl_Drop(object sender, DragEventArgs e)
        {
            var tabSource = tabControl.SelectedItem as TabItem;
            var tabTarget = e.Source as TabItem;

            if (tabSource.Header.ToString() == "All" || tabSource.Header.ToString() == "+")
                return;
            if (tabTarget.Header.ToString() == "All" || tabTarget.Header.ToString() == "+")
                return;

            UserCategories.Remove(tabSource.Header.ToString());
            UserCategories.Insert(tabControl.Items.IndexOf(tabTarget) - 1, tabSource.Header.ToString());

            Categories_Changed = true;

            Helper_UI();
        }

        private void ItemContextMenu_Playlists_Rename_Click(object sender, RoutedEventArgs e)
        {
            var promptResponse = Prompt.ShowDialog("Playlist Name: ", "Enter a new name for playlist (leave blank to cancel)");

            if (promptResponse != "")
            {
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_Name = promptResponse;
                ((Playlists)ListView_Playlists.SelectedItem).Playlist_Changed = true;
            }

            Helper_UI();
        }
    }
}
