using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTPlayer.Classes
{

    class QueueManager : ObservableCollection<Song>
    {
        int minID = 0;
        int maxID = 0;
        bool random = false;
        int curElm = 0;

        List<int> l = new List<int>();

        public QueueManager()
            : base()
        {
            
        }
        //Singolo
        public void AddElement(string abc)
        {
            this.Add(new Song(abc, abc));
            this.minID = this.First().ID;
            this.maxID = this.Last().ID;
        }

        //Lista Elm Drop
        public void AddElement(StringCollection files)
        {
            HundredMilesSoftware.UltraID3Lib.UltraID3 u;
            foreach (string path in files)
            {
                if (is_dir(path))
                {
                    //Apro la dir
                    AddElement(openDir(path));
                }
                else if (validExt(path))
                {
                    u = Utility.getTag(path);
                    this.Add(new Song(path, Utility.getTagTitle(path), u));
                }
            }
            this.minID = this.First().ID;
            this.maxID = this.Last().ID;
        }

        //Lista Path
        public void AddElement(string[] paths)
        {
            HundredMilesSoftware.UltraID3Lib.UltraID3 u;
            foreach (string path in paths)
            {
                if (is_dir(path))
                {
                    //Apro la dir
                    AddElement(openDir(path));
                }
                else if (validExt(path))
                {
                    u = Utility.getTag(path);
                    this.Add(new Song(path, Utility.getTagTitle(path) , u));
                }
            }
            this.minID = (this.Count != 0) ? this.First().ID : 0;
            this.maxID = (this.Count != 0) ? this.Last().ID : 0;
        }

        public void setRandom(bool v)
        {
            this.random = v;
            if (v)
            {
                l.Clear();
                for (int i = 0; i <= maxID; i++)
                {
                    l.Add(i);
                }
                this.Shuffle(l);
            }
        }

        //Shuffle della lista di appoggio per il random
        private void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        //Restituiamo la canzone successiva
        public Song Next()
        {
            Song elm;
            curElm++;
            if (curElm > this.maxID)
            {
                curElm = 0;
            }
            Console.WriteLine("c: {0}", curElm);
            if (random)
            {
                elm = this.First(I => I.ID == this.l[curElm]);
            }
            else
            {
                elm = this.First(I => I.ID == this.curElm);
            }
            return elm;
        }

        //Restituiamo la canzone precedente
        public Song Prev()
        {
            Song elm;
            curElm--;
            if (curElm < this.minID)
            {
                curElm = this.maxID;
            }
            Console.WriteLine("c: {0}", curElm);
            if (random)
            {
                elm = this.First(I => I.ID == this.l[curElm]);
            }
            else
            {
                elm = this.First(I => I.ID == this.curElm);
            }
            return elm;
        }

        //Data la dir return path dei file
        private string[] openDir(string path)
        {
            return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        }

        private bool is_dir(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        private bool validExt(string file)
        {

            string[] ext = { ".mp3", ".wma" };
            string extFile = getExt(file);

            foreach (string e in ext)
            {
                if (e == extFile)
                {
                    return true;
                }
            }
            return false;
        }

        private string getExt(string file)
        {
            return Path.GetExtension(file);
        }

        //Metodo per tenere allineati i due oggetti
        public void setPlay(Song s)
        {
            this.curElm = s.ID;
        }
    }
}
