using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mol3
{
    public class Question : INotifyPropertyChanged
    {
        public Test test { get; set; }
        public int id { get; set; }
        public int testId { get; set; }
        public string vraagTekst { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
