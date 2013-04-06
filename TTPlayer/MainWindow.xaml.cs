using System;
using System.Collections.Generic;
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
using FMOD;
using TTPlayer.Classes;
using System.Windows.Threading;

namespace TTPlayer
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private FMODSongManager FManager;
        private QueueManager _QueueManager;
        private TTSpeech _TTSpeech;
        private WebInterface ww;
        private uint oldValue = 0;

        public MainWindow()
        {
            InitializeComponent();

            SetUpGUI();
            

            //Test ListView 
            Lst_song.ItemsSource = _QueueManager;

        }


        private void SetUpGUI()
        {

            FManager = new FMODSongManager(Slider_Search, Slider_vol, this.Dispatcher, this.Lbl_info, this.Lbl_time, this.play);
            _QueueManager = new QueueManager();
            

            FManager.setQueue(_QueueManager);
            _TTSpeech = new TTSpeech(FManager,this.play,this.Slider_vol);
            ww = new WebInterface(FManager, this.Slider_vol);

            CkCoro.Foreground = Brushes.PaleVioletRed;
            CkDelay.Foreground = Brushes.OrangeRed;
            CkDisorsione.Foreground = Brushes.Violet;
            CkEcho.Foreground = Brushes.SkyBlue;
            CkFlange.Foreground = Brushes.MidnightBlue;
            CkHighPass.Foreground = Brushes.Green;
            CkLowPass.Foreground = Brushes.Orange;
            CkTremolo.Foreground = Brushes.Red;
            //CkRiverbero.Foreground = Brushes.Chocolate;
            //CkSoftware.Foreground = Brushes.Teal;
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_icon_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FManager.Close();
            ww.Dispose();
        }

        private void test_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            Image myImage3 = new Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            if (FManager.Pause())
            {
                //Siamo in pausa
                bi3.UriSource = new Uri("img/play.png", UriKind.Relative);
            }
            else
            {
                //Play
                bi3.UriSource = new Uri("img/pausa.png", UriKind.Relative);
            }
            bi3.EndInit();
            play.Source = bi3;
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            FManager.setNext();
        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            FManager.setPrev();
        }


        private void Lst_song_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            if (Lst_song.SelectedIndex == -1)
            {
                return;
            }
            Song sel = Lst_song.Items[Lst_song.SelectedIndex] as Song;
            FManager.Play(sel);
        }

        

        private void Slider_vol_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FManager.setVolume((float)Slider_vol.Value);
        }

        private void Slider_Search_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            uint val = (uint)Slider_Search.Value;
            float t = (float)(val - oldValue) / (float)Slider_Search.Maximum;
            if (t > 0.01)
                FManager.setPosition(val);
            oldValue = val;
        }

        private void Lst_song_Drop(object sender, DragEventArgs e)
        {
            if (e.Data is System.Windows.DataObject &&
                ((System.Windows.DataObject)e.Data).ContainsFileDropList())
            {
                _QueueManager.AddElement(((System.Windows.DataObject)e.Data).GetFileDropList());


            }
        }

        #region "DSP"
        private void CkLowPass_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;
            c.IsChecked = FManager.setDsp(DSP_TYPE.LOWPASS);
        }

        private void CkHighPass_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;
            c.IsChecked = FManager.setDsp(DSP_TYPE.HIGHPASS);
        }

        private void CkDisorsione_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;
            c.IsChecked = FManager.setDsp(DSP_TYPE.DISTORTION);
        }

        private void CkDelay_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;
            c.IsChecked = FManager.setDsp(DSP_TYPE.DELAY);
        }

        private void CkFlange_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;
            c.IsChecked = FManager.setDsp(DSP_TYPE.FLANGE);
        }

        private void CkTremolo_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;
            c.IsChecked = FManager.setDsp(DSP_TYPE.TREMOLO);
        }

        private void CkEcho_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;
            c.IsChecked = FManager.setDsp(DSP_TYPE.ECHO);
        }

        private void CkCoro_Click(object sender, RoutedEventArgs e)
        {
            CheckBox c = (CheckBox)sender;
            c.IsChecked = FManager.setDsp(DSP_TYPE.CHORUS);
        }
        #endregion

        
        private void KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                _TTSpeech.setActive();
            }
        }
    }
}
