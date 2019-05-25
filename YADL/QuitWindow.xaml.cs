using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace YADL
{
    public partial class QuitWindow : Window
    {
        ViewModel VM_Save = new ViewModel();

        public QuitWindow(object Data)
        {
            foreach (Playlists Playlist in Data as ObservableCollection<Playlists>)
            {
                Playlist.Playlist_Save = true;
                VM_Save.Playlist.Add(Playlist);
            }
            DataContext = VM_Save;
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        private void Button_Continue_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Helper_Save_Toggle()
        {
            foreach (Playlists Playlist in ListView_Save.SelectedItems)
            {
                if (Playlist.Playlist_Save == true)
                {
                    Playlist.Playlist_Save = false;
                }
                else
                {
                    Playlist.Playlist_Save = true;
                }
            }
        }

        private void ItemContextMenu_Selection_Toggle(object sender, RoutedEventArgs e)
        {
            Helper_Save_Toggle();
        }

        public static class IconHelper
        {
            [DllImport("user32.dll")]
            static extern int GetWindowLong(IntPtr hwnd, int index);

            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

            [DllImport("user32.dll")]
            static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x,
        int y, int width, int height, uint flags);

            [DllImport("user32.dll")]
            static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr
        lParam);

            const int GWL_EXSTYLE = -20;
            const int WS_EX_DLGMODALFRAME = 0x0001;
            const int SWP_NOSIZE = 0x0001;
            const int SWP_NOMOVE = 0x0002;
            const int SWP_NOZORDER = 0x0004;
            const int SWP_FRAMECHANGED = 0x0020;
            const uint WM_SETICON = 0x0080;

            public static void RemoveIcon(Window window)
            {
                // Get this window's handle
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                // Change the extended window style to not show a window icon
                int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);
                // Update the window's non-client area to reflect the changes
                SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                SendMessage(hwnd, WM_SETICON, new IntPtr(1), IntPtr.Zero);
                SendMessage(hwnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private void ListView_Save_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space || e.Key == System.Windows.Input.Key.Enter)
            {
                Helper_Save_Toggle();
            }

            e.Handled = true;
        }

        private void ListView_Save_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Helper_Save_Toggle();
        }
    }
}
