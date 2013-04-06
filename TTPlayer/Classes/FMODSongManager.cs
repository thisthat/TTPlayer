using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace TTPlayer.Classes
{
    class FMODSongManager
    {
        //UI Element
        private Slider s_song, s_vol;
        private Label info, time;
        private Image play_pausa;
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

        //DSP
        private FMOD.DSP lowPass = null; private bool isLowPass = false;
        private FMOD.DSP highPass = null; private bool isHighPass = false;
        private FMOD.DSP distorsione = null; private bool isDistorsione = false;
        private FMOD.DSP delay = null; private bool isDelay = false;
        private FMOD.DSP flange = null; private bool isFlange = false;
        private FMOD.DSP tremolo = null; private bool isTremolo = false;
        private FMOD.DSP echo = null; private bool isEcho = false;
        private FMOD.DSP chorus = null; private bool isChorus = false;
        private FMOD.DSPConnection dspCon = null;

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
        public FMODSongManager(Slider l, Slider v, Dispatcher d, Label i, Label t, Image im)
        {
            this.s_song = l;
            this.s_vol = v;
            this._dispatcher = d;
            this.info = i;
            this.time = t;
            this.play_pausa = im;

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

            Init_DSP();

            //Avvio il thread di aggiornamento UI
            newThread = new System.Threading.Thread(this.checkState);
            newThread.Start();

        }

        public void setQueue(QueueManager q)
        {
            this.lstCanzoni = q;
        }

        public Dispatcher getDispatcher()
        {
            return _dispatcher;
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
            this.changeButton();
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
            //this.playing = true;

            //Ripristino il volume dell'utente
            this.setVolume(this.volume);
            //Ripristino i DSP
            if (this.isLowPass)
            {
                result = channel.addDSP(lowPass, ref dspCon);
                ERRCHECK(result);
            }
            if (this.isHighPass)
            {
                result = channel.addDSP(highPass, ref dspCon);
                ERRCHECK(result);
            }
            if (this.isChorus)
            {
                result = channel.addDSP(chorus, ref dspCon);
                ERRCHECK(result);
            }
            if (this.isDelay)
            {
                result = channel.addDSP(delay, ref dspCon);
                ERRCHECK(result);
            }
            if (this.isDistorsione)
            {
                result = channel.addDSP(distorsione, ref dspCon);
                ERRCHECK(result);
            }
            if (this.isEcho)
            {
                result = channel.addDSP(echo, ref dspCon);
                ERRCHECK(result);
            }
            if (this.isFlange)
            {
                result = channel.addDSP(flange, ref dspCon);
                ERRCHECK(result);
            }
            if (this.isTremolo)
            {
                result = channel.addDSP(tremolo, ref dspCon);
                ERRCHECK(result);
            }
            this.changeButton();
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
            if (current != null)
                info.Content = current.Tag.Artist;
        }

        public void setNext()
        {
            if (lstCanzoni.Count > 0)
            {
                Song sn = lstCanzoni.Next();
                this.Play(sn);
            }
        }

        public void setPrev()
        {
            if (lstCanzoni.Count > 0)
            {
                Song sn = lstCanzoni.Prev();
                this.Play(sn);
            }
        }

        //Pausa e Resume
        public bool Pause()
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
            return _isPaused;
        }

        //Pausa e Resume
        public bool PauseFix()
        {
            _isPaused = true;
            if (channel != null)
            {
                result = channel.setPaused(_isPaused);
                ERRCHECK(result);
            }
            return _isPaused;
        }
        //Pausa e Resume
        public bool PlayFix()
        {
            _isPaused = false;
            if (channel != null)
            {
                result = channel.setPaused(_isPaused);
                ERRCHECK(result);
            }
            return _isPaused;
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

        private void changeButton()
        {
            Image myImage3 = new Image();
            System.Windows.Media.Imaging.BitmapImage bi3 = new System.Windows.Media.Imaging.BitmapImage();
            bi3.BeginInit();

            //Play
            bi3.UriSource = new Uri("img/pausa.png", UriKind.Relative);
            bi3.EndInit();
            this.play_pausa.Source = bi3;
            this._isPaused = false;
        }

        public void setPlayPause()
        {
            Image myImage3 = new Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            if (this.Pause())
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
            play_pausa.Source = bi3;
        }
        public void setPlayPause(bool f)
        {

            if (playing)
            {
                _isPaused = f;
                result = channel.setPaused(_isPaused);
                ERRCHECK(result);
            }
            else
            {
                _isPaused = false;
            }

            Image myImage3 = new Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            if (f)
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
            play_pausa.Source = bi3;
        }

        //CreateSoundEffect
        public void createSoundEffect(string path_sound)
        {
            FMOD.Sound sound = null;
            FMOD.Channel channel = null;
            result = system.createSound(path_sound, FMOD.MODE.CREATESTREAM, ref sound);
            ERRCHECK(result);
            result = sound.setMode(FMOD.MODE.LOOP_OFF);
            ERRCHECK(result);

            result = system.playSound(FMOD.CHANNELINDEX.FREE, sound, false, ref channel);
            ERRCHECK(result);
        }

        #region "DSP"

        private void Init_DSP()
        {

            result = system.createDSPByType(FMOD.DSP_TYPE.LOWPASS, ref lowPass);
            ERRCHECK(result);

            result = system.createDSPByType(FMOD.DSP_TYPE.HIGHPASS, ref highPass);
            ERRCHECK(result);

            result = system.createDSPByType(FMOD.DSP_TYPE.DISTORTION, ref distorsione);
            ERRCHECK(result);

            result = system.createDSPByType(FMOD.DSP_TYPE.DELAY, ref delay);
            ERRCHECK(result);

            result = system.createDSPByType(FMOD.DSP_TYPE.FLANGE, ref flange);
            ERRCHECK(result);

            result = system.createDSPByType(FMOD.DSP_TYPE.TREMOLO, ref tremolo);
            ERRCHECK(result);

            result = system.createDSPByType(FMOD.DSP_TYPE.ECHO, ref echo);
            ERRCHECK(result);

            result = system.createDSPByType(FMOD.DSP_TYPE.CHORUS, ref chorus);
            ERRCHECK(result);
        }

        private bool setDspLowPass()
        {
            if (this.isLowPass)
            {
                result = lowPass.remove();
                ERRCHECK(result);
            }
            else
            {
                if (channel != null)
                {
                    result = channel.addDSP(lowPass, ref dspCon);
                    ERRCHECK(result);
                }
            }
            this.isLowPass = !this.isLowPass;
            return this.isLowPass;
        }
        private bool setDspHighPass()
        {
            if (this.isHighPass)
            {
                result = highPass.remove();
                highPass.disconnectAll(true, true);
                ERRCHECK(result);
            }
            else
            {
                if (channel != null)
                {
                    result = channel.addDSP(highPass, ref dspCon);
                    ERRCHECK(result);
                }
            }
            this.isHighPass = !this.isHighPass;
            return this.isHighPass;
        }
        private bool setDspDistorsione()
        {
            if (this.isDistorsione)
            {
                result = distorsione.remove();
                ERRCHECK(result);
            }
            else
            {
                if (channel != null)
                {
                    result = channel.addDSP(distorsione, ref dspCon);
                }
            }
            this.isDistorsione = !this.isDistorsione;
            return this.isDistorsione;
        }
        private bool setDspDelay()
        {
            if (this.isDelay)
            {
                result = delay.remove();
                ERRCHECK(result);
            }
            else
            {
                if (channel != null)
                {
                    result = channel.addDSP(delay, ref dspCon);
                }
            }
            this.isDelay = !this.isDelay;
            return this.isDelay;
        }
        private bool setDspFlange()
        {
            if (this.isFlange)
            {
                result = flange.remove();
                ERRCHECK(result);
            }
            else
            {
                if (channel != null)
                {
                    result = channel.addDSP(flange, ref dspCon);
                }
            }
            this.isFlange = !this.isFlange;
            return this.isFlange;
        }
        private bool setDspTremolo()
        {
            if (this.isTremolo)
            {
                result = tremolo.remove();
                ERRCHECK(result);
            }
            else
            {
                if (channel != null)
                {
                    result = channel.addDSP(tremolo, ref dspCon);
                }
            }
            this.isTremolo = !this.isTremolo;
            return this.isTremolo;
        }
        private bool setDspEcho()
        {
            if (this.isEcho)
            {
                result = echo.remove();
                ERRCHECK(result);
            }
            else
            {
                if (channel != null)
                {
                    result = channel.addDSP(echo, ref dspCon);
                }
            }
            this.isEcho = !this.isEcho;
            return this.isEcho;
        }
        private bool setDspChorus()
        {
            if (this.isChorus)
            {
                result = chorus.remove();
                ERRCHECK(result);
            }
            else
            {
                if (channel != null)
                {
                    result = channel.addDSP(chorus, ref dspCon);
                }
            }
            this.isChorus = !this.isChorus;
            return this.isChorus;
        }

        public bool setDsp(FMOD.DSP_TYPE type)
        {
            switch (type)
            {
                case FMOD.DSP_TYPE.LOWPASS: return this.setDspLowPass();
                case FMOD.DSP_TYPE.HIGHPASS: return this.setDspHighPass();
                case FMOD.DSP_TYPE.DISTORTION: return this.setDspDistorsione();
                case FMOD.DSP_TYPE.DELAY: return this.setDspDelay();
                case FMOD.DSP_TYPE.FLANGE: return this.setDspFlange();
                case FMOD.DSP_TYPE.TREMOLO: return this.setDspTremolo();
                case FMOD.DSP_TYPE.ECHO: return this.setDspEcho();
                case FMOD.DSP_TYPE.CHORUS: return this.setDspChorus();
                default: return false;
            }
        }

        #endregion


    }
}
