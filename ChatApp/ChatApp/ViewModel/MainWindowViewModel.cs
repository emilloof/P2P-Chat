using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ViewModel
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<String> Messages { get; private set; }

        public MainWindowViewModel() {
            Messages = new ObservableCollection<string>();
            Messages.Add("Test message");


        }
    }
}
