using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MagicPattern.Core
{
    public class ClassItem : INotifyPropertyChanged
    {
        public ClassInfo ClassInfo { get; set; }
        public FileInfoItem DtoFileInfo { get; set; }
        public FileInfoItem ModelObjectFileInfo { get; set; }
        public FileInfoItem InterfaceServiceFileInfo { get; set; }
        public FileInfoItem ClassServiceFileInfo { get; set; }
        public FileInfoItem MapperFileInfo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
