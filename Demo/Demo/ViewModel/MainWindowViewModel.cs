using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Demo.ViewModel.Command;
using Microsoft.Win32;


namespace Demo.ViewModel
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {

        private NetworkManager NetworkManager { get; set; }
        private ICommand startGame;
        private string text;
        private ICommand requestAccept;
        public ObservableCollection<string> ChatMessages { get; set; }

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
        private string request_message;

        public string Request_message
        {
            get { return request_message; }
            set
            {
                request_message = value;
                OnPropertyChanged("Request_message");
            }
        }
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
        public bool IsGridVisible
        {
            get => NetworkManager.IsGridVisible;
            set
            {
                if (NetworkManager.IsGridVisible != value)
                {
                    NetworkManager.IsGridVisible = value;
                    OnPropertyChanged(nameof(IsGridVisible));
                }
            }
        }

        private string _ConnectionGrid;
        public string ConnectionGrid
   
        {
            get { return _ConnectionGrid; }
            set
            {
                _ConnectionGrid = value;
                OnPropertyChanged(nameof(ConnectionGrid));
            }
        }

        public MainWindowViewModel(NetworkManager networkManager)
        {

            IpAddress = "127.0.0.1";
            Port = "8080"; //autofill ip and port for easier testing
            ChatMessages = new ObservableCollection<string>();
            NetworkManager = networkManager;
            networkManager.PropertyChanged += myModel_PropertyChanged;
        }

        private void myModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) // så det märker raiseat
        {
            if (e.PropertyName == "Message")
            {

                var message = NetworkManager.Message; 

                if (message.RequestType == "denied_connect") { MyText = "Host denied your request. Please close window."; }

                else if (message.RequestType == "accept_connect") { MyText = ""; }

                else if (message.RequestType == "no_host") { MyText = "No host waiting on the IP or port you tried. Try again."; }
                else
                {
                    string mes = message.Name + message.DateTime.ToString() + "\n" + message.Message;
                    addMessageToChat(mes);
                }
               
            }
            else if (e.PropertyName == nameof(NetworkManager.IsGridVisible))
            {
                OnPropertyChanged(nameof(IsGridVisible));
            }
            if (e.PropertyName == "Request_message")
            {
                this.Request_message = NetworkManager.Request_message;
            }
            else if (e.PropertyName == "ConnectionGrid")
            {
                OnPropertyChanged(nameof(ConnectionGrid));
                this.ConnectionGrid = NetworkManager.ConnectionGrid;
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

        public ICommand RequestAccept
        {
            get
            {
                if (requestAccept == null)
                {
                    requestAccept = new RequestAccept(this); 
                }
                return requestAccept;
            }
            set
            {
                
                requestAccept = value;
            }
        }

        private bool startConnection(string action)
        {

            if (IpAddress == null || Port == null)
            {
                IpAddress = null;
                Port = null;
                return false;
            }

            NetworkManager.IpAddress = IpAddress;
            NetworkManager.Port = port;
            NetworkManager.Nickname = Nickname;
    

            return NetworkManager.startConnection(action);
        }

        public void startGameBoard(string action)
        {

            if (action == "Host")
            {
                if (startConnection(action))
                {
                    open();
                    
                }
            }

            else if( action == "Connect")
            {
                // if accepted, start the conenction 
                //Use enable in xaml to just hde the window before accept
                MyText = "Trying to connect to host";
                ConnectionGrid = "False";
                startConnection(action);
                open();
            }
           
            else
            {
                MessageBox.Show("Cannot start connection!");
            }
  
        }
        public void open()
        {
            GameBoard board = new GameBoard();
            board.DataContext = this;
            board.Show();
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
            DateTime date = DateTime.Now;
            string mes = Nickname + date.ToString() + "\n" + MyText;
            ChatMessages.Add(mes);
            NetworkManager.sendChar(MyText);
            MyText = "";
        }

        public void answerRequest(bool answer)
        {
            NetworkManager.sendAnswer(answer);
        }

        private void addMessageToChat(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatMessages.Add(message);
            });
        }

    }
}
