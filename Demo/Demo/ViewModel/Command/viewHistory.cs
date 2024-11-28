using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Demo.ViewModel.Command
{
    internal class viewHistory : ICommand
    {
        private MainWindowViewModel parent;
        public event EventHandler CanExecuteChanged;

        public viewHistory(MainWindowViewModel parent) {
            this.parent = parent;
        }

        public bool CanExecute(object parameter)
        {

            return true;
        }

        public void Execute(object parameter)
        {
            Guid chatId = (Guid)parameter;  // Cast the parameter to Guid
            string chatID = chatId.ToString();

            Debug.WriteLine(chatID);
            parent.viewHistory(chatID);
       
        }


    }
}
