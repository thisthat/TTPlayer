using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TTPlayer.Classes
{
    class TTSpeech
    {
        FMODSongManager Manager;
        //Configure the input to the recognizer.
        SpeechRecognitionEngine sre = new SpeechRecognitionEngine();
        Image _play;
        Slider vol;

        private const string prossima = "prossima";
        private const string next = "next";
        private const string precedente = "precedente";
        private const string prev = "prav";
        private const string play = "start";
        private const string pausa = "stop";
        private const string ferma = "ferma";
        private const string up = "up";
        private const string down = "down";

        private bool state = false;

        public TTSpeech(FMODSongManager q, Image p,Slider v)
        {
            Manager = q;
            _play = p;
            vol = v;
            this.controllaInputVocale();
        }

        private void controllaInputVocale()
        {
            // Create a new SpeechRecognitionEngine instance.
            sre.SetInputToDefaultAudioDevice();

            // Create a simple grammar.
            Choices control = new Choices();
            control.Add(new string[] { prossima, next, precedente, prev, play, pausa, ferma,up,down });

            // Create a GrammarBuilder object and append the Choices object.
            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(control);

            // Create the Grammar instance and load it into the speech recognition engine.
            Grammar g = new Grammar(gb);
            sre.LoadGrammar(g);

            // Register a handler for the SpeechRecognized event.
            sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);

            /// Start asynchronous, continuous speech recognition.
            //sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void setActive()
        {
            Console.WriteLine(state);
            if (state)
            {
                sre.RecognizeAsyncCancel();
            }
            else
            {
                sre.RecognizeAsync(RecognizeMode.Multiple);
            }
            state = !state;
            Manager.createSoundEffect(@"./Music/Purr.mp3");
        }

        

        void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine(e.Result.Confidence + " : " + e.Result.Text);
            if (e.Result.Confidence > 0.80)
            {
                float v;
                switch (e.Result.Text)
                {
                    case prev:
                    case precedente: Manager.setPrev(); break;
                    case next:
                    case prossima: Manager.setNext(); break;
                    case play:
                    case pausa:
                    case ferma: Manager.setPlayPause(); break;
                    case up: v = (vol.Value < 0.9) ? (float)vol.Value : 0.9f; v += 0.1f; vol.Value = v; 
                            Manager.setVolume(v); Manager.createSoundEffect(@"./Music/volume.mp3"); break;
                    case down: v = (vol.Value > 0.1) ? (float)vol.Value : 0.1f; v -= 0.1f; vol.Value = v; 
                            Manager.setVolume(v); Manager.createSoundEffect(@"./Music/volume.mp3"); break;
                    default: return;
                }
            }
        }
    }
}
