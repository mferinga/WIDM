using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mol3
{
    public class Answer : INotifyPropertyChanged
    {
        public int id { get; set; }
        public int vraagId { get; set; }
        public string antwoordTekst { get; set; }
        public bool correct { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
