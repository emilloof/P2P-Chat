using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Demo.ViewModel.Command
{
    internal class RequestAccept : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private MainWindowViewModel parent = null;

        public RequestAccept(MainWindowViewModel parent)
        {
            this.parent = parent;

        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

            string answer = parameter.ToString();
            if (answer == "Accept")
            {
                parent.answerRequest(true);
            }
            else if (answer == "Decline")
            {
                parent.answerRequest(false);
            }
        }
    }
}
