using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
        private ICommand disconnect;

        public ObservableCollection<string> ChatMessages { get; set; }
        public ObservableCollection<Chat> ChatHistoryCollection { get; set; }


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




        private string buttonHistory;
        public string ButtonHistory

        {
            get { return buttonHistory; }
            set
            {
                buttonHistory = value;
                OnPropertyChanged(nameof(ButtonHistory));
            }
        }



        private List<Chat> _ChatHistory;
        public List<Chat> ChatHistory
        {
            get { return _ChatHistory; }
            set
            {
                _ChatHistory = value;
                OnPropertyChanged("ChatHistory");
            }
        }

        public ICommand DisconnectCommand {
            get
            {
                if (disconnect == null)
                {
                    return new DisconnectCommand(this);
                }
                else
                {
                    return disconnect;

                }
            }
            set { disconnect = value; }
        }

        public MainWindowViewModel(NetworkManager networkManager)
        {

            IpAddress = "127.0.0.1";
            Port = "8080"; //autofill ip and port for easier testing
            ChatMessages = new ObservableCollection<string>();
            ChatHistoryCollection = new ObservableCollection<Chat>();

            NetworkManager = networkManager;
            networkManager.PropertyChanged += myModel_PropertyChanged;
            //DisconnectCommand = new DisconnectCommand(this);
        }

        public void Disconnect()
        {
            IsGridVisible = false;
            MyText = "";
            SearchTerm = "";
            ChatMessages.Clear();
            NetworkManager.HandleDisconnection();
        }




        private void myModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) // så det märker raiseat
        {
            if (e.PropertyName == "Message")
            {

                var message = NetworkManager.Message;

                if (message.RequestType == "denied_connect") { MyText = "Host denied your request. Please close window."; }

                else if (message.RequestType == "BUZZ")
                {
                    message.Message = "* Sent a Buzz *";
                    string mes = message.Name + " " + message.DateTime.ToString() + "\n" + message.Message;
                    addMessageToChat(mes);
               
                    Shaking = true;
                    Shaking = false;
                }

                else if (message.RequestType == "accept_connect") { MyText = ""; }

                else if (message.RequestType == "clear_chat")
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    { ChatMessages.Clear(); }
                    );
                }

                else if (message.RequestType == "disconnected") { MyText = "Other user have disconnected."; }

                else if (message.RequestType == "no_host") { MyText = "No host waiting on the IP or port you tried. Try again."; }
                else
                {
                    string mes = message.Name + " " + message.DateTime.ToString() + "\n" + message.Message;
                    addMessageToChat(mes);
                }

            }
            else if (e.PropertyName == nameof(NetworkManager.IsGridVisible))
            {
                OnPropertyChanged(nameof(IsGridVisible));
            }
            else if (e.PropertyName == "Request_message")
            {
                this.Request_message = NetworkManager.Request_message;
            }
            else if (e.PropertyName == "ConnectionGrid")
            {
                OnPropertyChanged(nameof(ConnectionGrid));
                this.ConnectionGrid = NetworkManager.ConnectionGrid;
            }
            else if (e.PropertyName == "ChatHistory")
            {
                OnPropertyChanged("ChatHistory");
                this.ChatHistory = NetworkManager.Chathistory;
                updateHistory();
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

        private ICommand viewhistory;
        public ICommand ViewHistory
        {
            get 
            { 
               
                if (viewhistory == null)
                {
                    viewhistory = new ViewHistoryCommand(this);
                }
                return viewhistory   ; }
            set
            {
                viewhistory = value;
            }


        }


        public ICommand RequestAccept
        {
            get
            {
                if (requestAccept == null)
                {
                    requestAccept = new RequestAcceptCommand(this);
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
            if (Nickname == null)
            {
                MessageBox.Show("Please enter a nickname");
                return;
            }

            if (action == "Host")
            {
                if (startConnection(action))
                {
                    open();

                }
            }

            else if (action == "Connect")
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
            ButtonHistory = "False";    
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
            string mes = Nickname + " " + date.ToString() + "\n" + MyText;
            ChatMessages.Add(mes);

            NetworkManager.sendChar(MyText);
            MyText = "";
        }

        public void updateHistory () 
        {

            if(ChatHistory == null)
             {
                return;
             }
            Application.Current.Dispatcher.Invoke(() =>
            {
                Debug.WriteLine("ChatHistoy: " + ChatHistory);
                ChatHistoryCollection.Clear();

                var sortedChatHistory = ChatHistory
                    .OrderByDescending(chat => chat.Messages[chat.Messages.Count() - 1].DateTime) // Sorting by the DateTime of the first message
                    .ToList();

                foreach (Chat chat in sortedChatHistory)
                {
                    ChatHistoryCollection.Add(chat);
                }
            });
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


        public void viewHistory(string chatId)
        {
            Guid chatID = Guid.Parse(chatId);

            Chat chat = NetworkManager.viewHistory(chatID);
            if(chat == null)
            {
                return;
            }
            
            ChatMessages.Clear();


            foreach (Messages message in chat.Messages)
                {
                    DateTime date = message.DateTime;
                    string mes = message.Name + " " + date.ToString() + "\n" + message.Message;
                    ChatMessages.Add(mes);
                }

    }

        public void SearchChatHistory(string letters)
        {

            if (string.IsNullOrWhiteSpace(letters))
            {
                // If search term is empty, show all chats
                updateHistory();
                return;
            }

            var searchResults = ChatHistory
                .Where(chat => chat.Name.IndexOf(letters, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(chat => chat.Messages[0].DateTime) // Sort results by date
                .ToList();
            Debug.WriteLine(searchResults);


            ChatHistoryCollection.Clear();
            foreach (Chat chat in searchResults)
            {
                ChatHistoryCollection.Add(chat);
            }
        }

        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
                SearchChatHistory(_searchTerm); // Call search on text change
            }
        }



        private ICommand buzz;
        private bool shaking;
        public bool Shaking
        {
            get { return shaking; }
            set { shaking = value; OnPropertyChanged(nameof(Shaking)); }
        }
        
        public ICommand BUZZ
        {
            get
            {
                if (buzz == null)
                {
                    buzz = new BuzzCommand(this);
                }
                return buzz;
            }


            set
            {

                buzz = value;
            }
        }

        public void sendBuzz()
        {
            string str = Nickname + " " + DateTime.Now.ToString() + "\n" + "* Sent a Buzz *";
            ChatMessages.Add(str);
            NetworkManager.sendBuzz();


        }

        private ICommand view_history;
        public ICommand View_History
        {
            get
            {
                if (view_history == null)
                {
                    view_history = new MainWindowViewHistoryCommand(this);
                }
                return view_history;
                
            }


            set
            {
                view_history = value;
                
            }
        }

        public void open_history()
        {
            NetworkManager.updateHistory();
            if (ChatHistory != null )
            {
                open();
                ConnectionGrid = "False";
                
                viewHistory(ChatHistory[0].ChatID.ToString());
            }
            else
            {
                MessageBox.Show("No history available.");
            }
            

        }


    }
}
