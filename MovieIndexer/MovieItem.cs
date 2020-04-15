using System.ComponentModel;

namespace MovieIndexer
{
    public class MovieItem : Movie, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
