﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TTPlayer.Classes
{
    class WebInterface
    {
        int port = 1414;
        Socket listener;
        bool fine = false;

        private const string vol_up = "vol:up";
        private const string vol_down = "vol:down";
        private const string next = "canzone:prev";
        private const string prev = "canzone:next";
        private const string start = "play:true";
        private const string stop = "play:false";
        private const string rnd = "random:true";
        private const string nrm = "random:false";
        private const string vocal = "speech:true";
        private const string novocal = "speech:false";

        private FMODSongManager manager;
        private TTSpeech speech;
        private Slider vol;
        private Dispatcher _dispatcher;
        private Image rndImg;

        public WebInterface(FMODSongManager m, TTSpeech sp, Slider s, Image _rnd)
        {
            manager = m;
            speech = sp;
            vol = s;
            _dispatcher = m.getDispatcher();
            this.rndImg = _rnd;
            Thread t = new Thread(this.receve);
            t.Start();
        }

        public void Dispose()
        {
            fine = true;
            listener.Dispose();
        }

        private void receve()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            listener = new Socket(AddressFamily.InterNetwork,
                 SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            listener.Bind(localEndPoint);
            listener.Listen(10);
            string data;

            while (!fine)
            {
                try
                {
                    // Start listening for connections.
                    Socket handler = listener.Accept();
                    data = "";
                    // An incoming connection needs to be processed.
                    bytes = new byte[1024];
                    int bytesRec;
                    do
                    {
                        bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    }
                    while (bytesRec > 0);
                    // Show the data on the console.

                    switch (data)
                    {
                        case vol_up: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.setVolumeUp)); break;
                        case vol_down: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.setVolumeDown)); break;
                        case next: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(manager.setNext)); break;
                        case prev: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(manager.setPrev)); break;
                        case start: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.setPlay)); break;
                        case stop: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.setPause)); break;
                        case rnd: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.setRandomOn)); break;
                        case nrm: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.setRandomOff)); break;
                        case vocal: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.setSpeechOn)); break;
                        case novocal: _dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.setSpeechOff)); break;
                        default: break;
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private void setPause()
        {
            manager.setPlayPause(true);
        }
        private void setPlay()
        {
            manager.setPlayPause(false);
        }
        private void setVolumeUp()
        {
            float v = (vol.Value < 0.9) ? (float)vol.Value : 0.9f; v += 0.1f; vol.Value = v;
            manager.setVolume(v);
            manager.createSoundEffect(@"./Music/volume.mp3");
        }
        private void setVolumeDown()
        {
            float v = (vol.Value > 0.1) ? (float)vol.Value : 0.1f; v -= 0.1f; vol.Value = v;
            manager.setVolume(v); manager.createSoundEffect(@"./Music/volume.mp3");
        }
        private void setRandomOn()
        {
            manager.setRandom(true);
            Image myImage3 = new Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri("img/media-shuffle.png", UriKind.Relative);
            bi3.EndInit();
            rndImg.Source = bi3;
        }
        private void setRandomOff()
        {
            manager.setRandom(false);
            Image myImage3 = new Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri("img/media-shuffle.png", UriKind.Relative);
            bi3.EndInit();
            rndImg.Source = bi3;
        }
        private void setSpeechOn()
        {
            speech.setActive(true);
        }
        private void setSpeechOff()
        {
            speech.setActive(false);
        }
    }
}
