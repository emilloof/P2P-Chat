using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Demo.ViewModel.Command
{
    internal class BuzzCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MainWindowViewModel parent;

        public BuzzCommand(MainWindowViewModel parent) {
            this.parent = parent;
        }

        public bool CanExecute(object parameter)
        {
            return true; 
        }





        public void Execute(object parameter)
        {
            parent.sendBuzz();
        }
    }
}
