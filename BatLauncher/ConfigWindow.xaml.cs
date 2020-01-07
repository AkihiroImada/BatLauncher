//========================================================================================
// 設定ウィンドウ.
// Auther: Akihiro Imada
//========================================================================================
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace BatLauncher
{
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private ObservableCollection<BindingProxy<string>> DirPathProxyList { get; set; } = new ObservableCollection<BindingProxy<string>>( Config.Instance.Data.PathList.Select( x => new BindingProxy<string>( x ) ) );

        public ConfigWindow()
        {
            InitializeComponent();
            PathList.ItemsSource = DirPathProxyList;
        }

        private void RemovePathListItemButton_Click( object sender, RoutedEventArgs e )
        {
            if(sender is Control ctl && ctl.DataContext is BindingProxy<string> data )
            {
                DirPathProxyList.Remove( data );
            }
        }

        private void SubmitButton_Click( object sender, RoutedEventArgs e )
        {
            Config.Instance.Data.PathList = new List<string>( DirPathProxyList.Select( x => x.Value ) );
            Config.Instance.Save();
            Close();
        }

        private void CancelButton_Click( object sender, RoutedEventArgs e )
        {
            Close();
        }

        private void DirRefButton_Click( object sender, RoutedEventArgs e )
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            var result = dialog.ShowDialog();
            if ( result != CommonFileDialogResult.Ok ) { return; }
            DirPathProxyList.Add( new BindingProxy<string>( dialog.FileName ) );
        }

        private void AddPathListItemTextBox_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key != Key.Return ) { return; }
            DirPathProxyList.Add( new BindingProxy<string>( AddPathListItemTextBox.Text ) );
        }


    }

    public class BindingProxy<T>
    {
        public T Value { get; set; }
        public BindingProxy( T value )
        {
            Value = value;
        }
    }
}
