using MagicPattern.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace AppMapp
{
    public class MainViewModel : INotifyPropertyChanged
    {

        private ICommand mapJsonCommand;
        public ICommand MapJsonCommand => mapJsonCommand ?? (mapJsonCommand = new RelayCommand(() =>
        {
            MapJson();
        }));

        private void MapJson()
        {
            if (ClassItems?.Any() != true)
            {
                service = new GeneratorService
                {
                    Example = Json,
                    MainClass = MainClass,
                    Namespace = DefaultNamespace,
                    PropertyAttribute = UseJsonProperty ? "JsonProperty" : "",
                    AlwaysUsePublicValues = UsePublicValues
                };

                service.GenerateClasses();
            }

            HandleGeneratedClasses(service);
        }

        private void HandleGeneratedClasses(GeneratorService service)
        {
            var classItems = new ClassItemService().HandleGeneratedClasses(service);
            ClassItems = new ObservableCollection<ClassItem>(classItems);
        }

        public string Json { get; set; }
        public string DefaultNamespace { get; set; } = "SimpleNamespace";
        public string MainClass { get; set; } = "RootObject";
        public bool UseJsonProperty { get; set; } = true;
        public bool UsePublicValues { get; set; } = true;
        public ObservableCollection<ClassItem> ClassItems { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private GeneratorService service;
    }
}
