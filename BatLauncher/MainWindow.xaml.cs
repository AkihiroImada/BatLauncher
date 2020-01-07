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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;

namespace BatLauncher
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<FileInfo> m_batFileList = new ObservableCollection<FileInfo>();

        public MainWindow()
        {
            InitializeComponent();
            Config.Instance.Load();

            RefreshFileList();
            BatFileNameList.ItemsSource = m_batFileList;
        }

        private void RefreshFileList()
        {
            m_batFileList.Clear();
            foreach ( string dirPath in Config.Instance.Data.PathList )
            {
                var dirInfo = new DirectoryInfo( dirPath );
                if ( !dirInfo.Exists )
                {
                    MessageBox.Show( string.Format( "{0}が見つかりませんでした.", dirPath ) );
                    continue;
                }
                IEnumerable<FileInfo> fileInfos = dirInfo.EnumerateFiles( "*.bat", SearchOption.AllDirectories );

                foreach ( var fi in fileInfos )
                {
                    m_batFileList.Add( fi );
                }
            }
        }

        private void SearchBox_TextChanged( object sender, TextChangedEventArgs e )
        {
            var filterList =
                m_batFileList.Where( x => x.Name.Contains( SearchBox.Text ) ).ToList();
            BatFileNameList.ItemsSource = filterList;
        }

        private void BatFileNameList_KeyDown( object sender, KeyEventArgs e )
        {
            if(BatFileNameList.SelectedItems.Count <= 0 ) { return; }
            if(e.Key != Key.Return ) { return; }
            foreach( var item in BatFileNameList.SelectedItems )
            {
                var fi = item as FileInfo;
                Process process = new Process();
                process.StartInfo.FileName = fi.FullName;
                process.Start();
            }
        }

        private void BatFileNameList_GotFocus( object sender, RoutedEventArgs e )
        {
            // フォーカスが移ってすぐも何かは選択させる.
            if(BatFileNameList.SelectedItems.Count == 0 )
            {
                BatFileNameList.SelectedIndex = 0;
            }
        }

        private void MenuItem_Click( object sender, RoutedEventArgs e )
        {
            ConfigWindow win = new ConfigWindow();
            win.Owner = GetWindow( this );
            win.ShowDialog();
            RefreshFileList();
        }

        private void HelpAboutSoftwareMenuItem_Click( object sender, RoutedEventArgs e )
        {
            MessageBox.Show( 
                "BatLauncher\n" +
                "\n" +
                "このソフトウェアはオープンソースです．"+
                "The MIT License( MIT )\n" +
                "Copyright( c ) 2020 AkihiroImada\n" +
                "https://github.com/AkihiroImada/BatLauncher/blob/master/LICENSE" + "\n"+
                "\n" +
                "このソフトウェアは一部に以下のライセンスが適用されたソースコードを利用しています.\n" +
                "The MIT License( MIT )\n" +
                "Copyright( c ) 2007 James Newton - King\n" +
                "https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md"
                );
        }
    }

    public static class Define
    {
        public static readonly string CONFIG_FILE = "config.json";
    }

    public class Config
    {
        public static Config Instance { get; } = new Config();
        public ConfigData Data { get; set; } = null;

        private Config() { }

        public void Save()
        {
            using ( StreamWriter sw = new StreamWriter( Define.CONFIG_FILE ) )
            {
                string json = JsonConvert.SerializeObject( Data );
                sw.WriteLine( json );
            }
        }

        public void Load()
        {
            if ( !File.Exists( Define.CONFIG_FILE ) )
            {
                Data = new ConfigData();
                return;
            }
            using ( StreamReader sr = new StreamReader( Define.CONFIG_FILE, Encoding.UTF8 ) )
            {
                Data = JsonConvert.DeserializeObject<ConfigData>( sr.ReadToEnd() );
            }
        }

        [JsonObject( "Config" )]
        public class ConfigData
        {
            [JsonProperty( "PathList" )]
            public List<string> PathList { get; set; } = new List<string>();
        }
    }
}
