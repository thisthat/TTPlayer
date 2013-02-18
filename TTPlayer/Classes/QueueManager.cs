using System;
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
        FMODSongManager FMODManager;
        public QueueManager(FMODSongManager manager)
            : base()
        {
            this.FMODManager = manager;
        }
        //Singolo
        public void AddElement(string abc)
        {
            this.Add(new Song(abc, abc));
        }

        //Lista Elm Drop
        public void AddElement(StringCollection files)
        {
            foreach (string path in files)
            {
                
                if (is_dir(path))
                {
                    //Apro la dir
                    AddElement(openDir(path));
                }
                else if(validExt(path))
                {
                    Console.WriteLine(path);
                    this.Add(new Song(path, FMODManager.getTagTitle(path)));
                }

            }
        }

        //Lista Path
        public void AddElement(string[] paths)
        {
            foreach (string path in paths)
            {
                if (is_dir(path))
                {
                    //Apro la dir
                    AddElement(openDir(path));
                }
                else if(validExt(path))
                {
                    this.Add(new Song(path, FMODManager.getTagTitle(path)));
                }
            }
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

    }
}
