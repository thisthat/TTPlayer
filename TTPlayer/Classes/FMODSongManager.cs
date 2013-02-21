using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using HundredMilesSoftware.UltraID3Lib;
using System.ComponentModel;

namespace TTPlayer.Classes
{
    class FMODSongManager
    {
        //UI Element
        private Slider s_song, s_vol;
        private Label info, time;

        //FMOD 
        private FMOD.System system = null;
        private FMOD.Sound sound = null;
        private FMOD.Channel channel = null;
        private FMOD.RESULT result;
        FMOD.Sound currentsound = null;

        private uint ms = 0;
        private uint lenms = 0;
        private float volume = 0.2f;
        private bool playing = false;
        private bool _isPaused = false;

        //Thread Di Aggiornamento UI
        private Dispatcher _dispatcher;
        private System.Threading.Thread newThread;
        private bool _isThRunning = true;

        //Lista canzoni
        private QueueManager lstCanzoni;

        //Song corrente
        private Song current = null;

        //Time Per la UI
        private string _time;


        //Costruttore, necessita di tutti gli UI Element a cui fare l'update
        public FMODSongManager(Slider l, Slider v, Dispatcher d, Label i, Label t)
        {
            this.s_song = l;
            this.s_vol = v;
            this._dispatcher = d;
            this.info = i;
            this.time = t;

            /*
                Create a System object and initialize.
            */
            result = FMOD.Factory.System_Create(ref system);
            ERRCHECK(result);
            uint version = 0;
            result = system.getVersion(ref version);
            ERRCHECK(result);
            if (version < FMOD.VERSION.number)
            {
                MessageBox.Show("Error!  You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + FMOD.VERSION.number.ToString("X") + ".");
                Environment.Exit(-1);
            }

            result = system.init(32, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
            ERRCHECK(result);

            //Avvio il thread di aggiornamento UI
            newThread = new System.Threading.Thread(this.checkState);
            newThread.Start();

        }

        public void setQueue(QueueManager q)
        {
            this.lstCanzoni = q;
        }


        //Check error per FMOD
        private void ERRCHECK(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                //MessageBox.Show("A:FMOD error! " + result + " - " + FMOD.Error.String(result));
                //Environment.Exit(-1);
                Console.WriteLine("A:FMOD error! " + result + " - " + FMOD.Error.String(result));
            }
        }



        //Data la path o una song eseguiamo la canzone relativa
        public void Play(string path)
        {
            result = system.createSound(path, FMOD.MODE.SOFTWARE, ref sound);
            ERRCHECK(result);
            result = sound.setMode(FMOD.MODE.LOOP_OFF);
            ERRCHECK(result);

            result = system.playSound(FMOD.CHANNELINDEX.FREE, sound, false, ref channel);
            ERRCHECK(result);

        }

        public void Play(Song s)
        {
            if (current != null)
                current.isPlay = false;

            current = s;
            s.isPlay = true;
            this.lstCanzoni.setPlay(s);

            //CHIUDO LA VECCHIA
            if (channel != null)
            {
                result = channel.stop();
                //ERRCHECK(result);
            }
            if (sound != null)
            {
                result = sound.release();
                ERRCHECK(result);
            }

            result = system.createSound(s.Url, FMOD.MODE.CREATESTREAM, ref sound);
            ERRCHECK(result);
            result = sound.setMode(FMOD.MODE.LOOP_OFF);
            ERRCHECK(result);

            result = system.playSound(FMOD.CHANNELINDEX.FREE, sound, false, ref channel);
            ERRCHECK(result);
            //Ripristino il volume dell'utente
            this.setVolume(this.volume);
        }

        //THREAD che se c'è una canzone in exe calcola i tempi
        private void checkState()
        {
            uint mn = 0;
            uint s = 0;
            string tmp = "";
            while (_isThRunning)
            {
                if (!_isPaused)
                {
                    if (channel != null)
                    {

                        result = channel.isPlaying(ref playing);
                        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE) && (result != FMOD.RESULT.ERR_CHANNEL_STOLEN))
                        {
                            ERRCHECK(result);
                        }

                        result = channel.getPosition(ref ms, FMOD.TIMEUNIT.MS);
                        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE) && (result != FMOD.RESULT.ERR_CHANNEL_STOLEN))
                        {
                            ERRCHECK(result);
                        }

                        channel.getCurrentSound(ref currentsound);
                        if (currentsound != null)
                        {
                            result = currentsound.getLength(ref lenms, FMOD.TIMEUNIT.MS);
                            if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE) && (result != FMOD.RESULT.ERR_CHANNEL_STOLEN))
                            {
                                ERRCHECK(result);
                            }
                        }

                        #region "Calcola Tempo Canzone"
                        mn = ms / 1000 / 60;
                        s = ms / 1000 % 60;
                        if (mn < 10)
                        {
                            tmp = "0" + mn;
                        }
                        else
                        {
                            tmp = mn + "";
                        }
                        if (s < 10)
                        {
                            tmp += ":0" + s + "/";
                        }
                        else
                        {
                            tmp += ":" + s + "/";
                        }
                        mn = lenms / 1000 / 60;
                        s = lenms / 1000 % 60;
                        if (mn < 10)
                        {
                            tmp += "0" + mn;
                        }
                        else
                        {
                            tmp += mn + "";
                        }
                        if (s < 10)
                        {
                            tmp += ":0" + s;
                        }
                        else
                        {
                            tmp += ":" + s;
                        }
                        _time = tmp;
                        #endregion

                        //Dobbiamo passare alla successiva?
                        if (ms == lenms && ms != 0)
                        {
                            Song sn = lstCanzoni.Next();
                            this.Play(sn);
                        }
                    }
                    _dispatcher.Invoke(DispatcherPriority.Normal, new Action(this.setState));
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        //UPDATE THE UI
        private void setState()
        {
            s_song.Value = ms;
            s_song.Maximum = lenms;
            s_song.Minimum = 0;

            s_vol.Value = volume;
            s_vol.Minimum = 0;
            s_vol.Maximum = 1;

            time.Content = _time;
        }

        //Pausa e Resume
        public void Pause()
        {
            if (playing)
            {
                _isPaused = !_isPaused;
                result = channel.setPaused(_isPaused);
                ERRCHECK(result);
            }
            else
            {
                _isPaused = false;
            }
        }

        //Spostiamo la canzone in avanti e dietro
        public void setPosition(uint p)
        {
            if (channel != null)
            {
                channel.setPosition(p, FMOD.TIMEUNIT.MS);
            }
        }

        public void setVolume(float v)
        {
            if (channel != null)
            {
                channel.setVolume(v);
                this.volume = v;
            }
        }

        public void Close()
        {
            FMOD.RESULT result;
            if (sound != null)
            {
                result = sound.release();
                ERRCHECK(result);
            }
            if (system != null)
            {
                result = system.close();
                ERRCHECK(result);
                result = system.release();
                ERRCHECK(result);
            }

            //Chiudo i miei thread
            _isThRunning = false;
        }

        //GET ID3 TAG
        public string getTagTitle(string path)
        {
            UltraID3 u = new UltraID3();
            u.Read(path);
            return u.Title;
        }

    }
}
