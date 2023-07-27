using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mol3
{
    public class KanidaatVraag : INotifyPropertyChanged
    {
        public int kanidaatId { get; set; }
        public int vraagId { get; set; }
        public int? antwoordId { get; set; }
        public int testId { get; set; }
        public double? tijd { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
