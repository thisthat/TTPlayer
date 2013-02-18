using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTPlayer.Classes
{
    class Song : INotifyPropertyChanged
    {

        //Costante URL image
        private const string image_url = "/img/icon_right.png";
        private const string image_nil = "/img/image_null.png";

        //Attribute of the class
        static private int ID_ = 0;
        private string _url;
        private string _name;
        private int _id = ID_++;    //to create an "order"
        private bool _isPlay = false;
        private Uri _image = new Uri(image_nil, UriKind.RelativeOrAbsolute);

        public int ID
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
            set { this._name = value; }
        }

        public string Url
        {
            get { return _url; }
            set { this._url = value; }
        }

        public bool isPlay
        {
            get { return _isPlay; }
            set
            {
                this._isPlay = value;
                if (this._isPlay)
                {
                    _image = new Uri(image_url, UriKind.RelativeOrAbsolute);
                }
                else
                {
                    _image = new Uri(image_nil, UriKind.RelativeOrAbsolute);
                }
                InvokePropertyChanged("isPlayImage");
            }
        }

        private void InvokePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public Uri isPlayImage
        {
            get { return _image; }
        }

        public Song(string url, string name)
        {
            this._name = name;
            this._url = url;
        }

        public Song(string url, string name, bool play)
        {
            this._name = name;
            this._url = url;
            this._isPlay = play;
        }

    }
}
