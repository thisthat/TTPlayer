﻿using System;
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

            FManager = new FMODSongManager(Slider_Search, Slider_vol, this.Dispatcher, this.Lbl_info, this.Lbl_time);
            _QueueManager = new QueueManager(FManager);

            FManager.setQueue(_QueueManager);

            CkCoro.Foreground = Brushes.PaleVioletRed;
            CkDelay.Foreground = Brushes.OrangeRed;
            CkDisorsione.Foreground = Brushes.Violet;
            CkEcho.Foreground = Brushes.SkyBlue;
            CkFlange.Foreground = Brushes.MidnightBlue;
            CkHighPass.Foreground = Brushes.Green;
            CkLowPass.Foreground = Brushes.Orange;
            CkTremolo.Foreground = Brushes.Red;
            CkRiverbero.Foreground = Brushes.Chocolate;
            CkSoftware.Foreground = Brushes.Teal;
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
        }



        private void Window_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {


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

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            FManager.Pause();
        }

        private void Slider_vol_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FManager.setVolume((float)Slider_vol.Value);
        }

        private void Slider_Search_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            uint val = (uint)Slider_Search.Value;
            float t = (float)(val - oldValue) / (float)Slider_Search.Maximum;
            if (t > 0.001)
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



    }
}
