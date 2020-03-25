using System.ComponentModel;

namespace MagicPattern.Core
{
    public class FileInfoItem : INotifyPropertyChanged
    {
        public string ClassText { get; set; }
        public string TargetPath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
