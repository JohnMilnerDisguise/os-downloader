using System;
using System.ComponentModel;

namespace OSDownloader.Models
{
    public class PropertyModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name = String.Empty;
        private string _value = String.Empty;

        public PropertyModel(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if( _name != value )
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged("Value");
                }
            }
        }

        //ViewModel Event Handlers
        protected void RaisePropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


    }
}
