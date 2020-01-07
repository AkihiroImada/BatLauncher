//========================================================================================
// メインウィンドウ.
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.ComponentModel;

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
            // ウィンドウのサイズを復元
            RecoverWindowBounds();
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            // ウィンドウのサイズを保存
            SaveWindowBounds();
            base.OnClosing( e );
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
            var filterList = m_batFileList.OrderBy( x => {
                string withoutExtName = System.IO.Path.GetFileNameWithoutExtension( x.Name );
                float baseCost = withoutExtName.NormalizedLevenshteinDistance( SearchBox.Text, false );
                float containCost = withoutExtName.Contains( SearchBox.Text ) ? 0.0f : 1.0f;
                return baseCost + containCost;
            } );
            BatFileNameList.ItemsSource = filterList.Take( Define.CANDIDATE_MAX );
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

        private void ConfigMenuItem_Click( object sender, RoutedEventArgs e )
        {
            ConfigWindow win = new ConfigWindow();
            win.Owner = GetWindow( this );
            win.ShowDialog();
            RefreshFileList();
        }

        private void RefreshMenuItem_Click( object sender, RoutedEventArgs e )
        {
            RefreshFileList();
        }

        private void HelpAboutSoftwareMenuItem_Click( object sender, RoutedEventArgs e )
        {
            MessageBox.Show( 
                "BatLauncher ver1.1.0\n" +
                "\n" +
                "このソフトウェアはオープンソースです．\n"+
                "The MIT License( MIT )\n" +
                "Copyright( c ) 2020 AkihiroImada\n" +
                "https://github.com/AkihiroImada/BatLauncher/blob/master/LICENSE"
                );
        }

        /// <summary>
        /// ウィンドウの位置・サイズを保存します。
        /// </summary>
        private void SaveWindowBounds()
        {
            var settings = Properties.Settings.Default;
            settings.WindowMaximized = WindowState == WindowState.Maximized;
            WindowState = WindowState.Normal; // 最大化解除
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
            settings.Save();
        }

        /// <summary>
        /// ウィンドウの位置・サイズを復元します。
        /// </summary>
        void RecoverWindowBounds()
        {
            var settings = Properties.Settings.Default;
            // 左
            if ( settings.WindowLeft >= 0 &&
                (settings.WindowLeft + settings.WindowWidth) < SystemParameters.VirtualScreenWidth )
            { Left = settings.WindowLeft; }
            // 上
            if ( settings.WindowTop >= 0 &&
                (settings.WindowTop + settings.WindowHeight) < SystemParameters.VirtualScreenHeight )
            { Top = settings.WindowTop; }
            // 幅
            if ( settings.WindowWidth > 0 &&
                settings.WindowWidth <= SystemParameters.WorkArea.Width )
            { Width = settings.WindowWidth; }
            // 高さ
            if ( settings.WindowHeight > 0 &&
                settings.WindowHeight <= SystemParameters.WorkArea.Height )
            { Height = settings.WindowHeight; }
            // 最大化
            if ( settings.WindowMaximized )
            {
                // ロード後に最大化
                Loaded += ( o, e ) => WindowState = WindowState.Maximized;
            }
        }
    }

    public static class Define
    {
        public static readonly int CANDIDATE_MAX = 20;
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
