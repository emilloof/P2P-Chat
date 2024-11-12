using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Demo.Model;
using Demo.View;
using Demo.View.Command;

namespace Demo.ViewModel
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {

        private NetworkManager NetworkManager { get; set; }
        private ICommand startGame;
        private string text;
        public string MyText { 
            get {

                return text; } 
            set 
            { 
                text = value;
                OnPropertyChanged("MyText");
            
            } 
        }

        private string ipAddress;
        private string port;
        private string nickname;

        public string IpAddress
        {
            get { return ipAddress; }
            set
            {
                ipAddress = value;
                OnPropertyChanged();
            }
        }

        public string Port
        {
            get { return port; }
            set
            {
                port = value;
                OnPropertyChanged();
            }
        }

        public string Nickname
        {
            get { return nickname; }
            set
            {
                nickname = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindowViewModel(NetworkManager networkManager)
        {
            NetworkManager = networkManager;
            networkManager.PropertyChanged += myModel_PropertyChanged;
        }

        private void myModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) // så det märker raiseat
        {
            if (e.PropertyName == "Message")
            {
                var message = NetworkManager.Message;
                this.MyText = message;
            }
        }

        public ICommand StartGame
        {
            get
            {
                if (startGame == null)
                    startGame = new StartGameCommand(this);
                return startGame;
            }
            set
            {
                startGame = value;
            }
        }

        private bool startConnection(string action)
        {

            Debug.WriteLine($"IPN : { IpAddress}");
            NetworkManager.IpAddress = IpAddress;
            NetworkManager.Port = port;
            NetworkManager.Nickname = Nickname;
    

            return NetworkManager.startConnection(action);
        }

        public void startGameBoard(string action)
        {

            if (startConnection(action))
            {
                GameBoard board = new GameBoard();
                board.DataContext = this;
                board.ShowDialog();
            }
            else
            {
                MessageBox.Show("Cannot start connection!");
            }
            
            
           
        }

        private ICommand enterCommand;
        public ICommand EnterCommand
        {
            get {
                if (enterCommand == null)
                {
                    return new Command.KeyEnterCommand(this);
                }
                else {
                    return enterCommand;

                }
            }
            set { enterCommand = value; }
        }

        public void sendMessage()
        {
            NetworkManager.sendChar(MyText);
        }

        public void showGameBoard()
        {
            GameBoard gameBoard = new GameBoard();
            gameBoard.DataContext = this;
            gameBoard.ShowDialog();
        }

        


    }
}
