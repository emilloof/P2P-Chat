using Demo.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Markup;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text.Json;


namespace Demo.Model
{

    public class Chat
    {
        public Guid ChatID { get; set; }

        public string Name { get; set; }

        public List<Messages> Messages { get; set; }



        public Chat()
        {
            ChatID = Guid.NewGuid();  // Generate a unique ID
            Messages = new List<Messages>();
        }

    }

    public class Messages
        {
            public string Name { get; set; }
            public string RequestType { get; set; }
            public string Message { get; set; }
            public string Answer { get; set; }
            public DateTime DateTime { get; set; }
        }
    internal class NetworkManager : INotifyPropertyChanged
    {
        List<Chat> list = new List<Chat>();

        private NetworkStream stream;

        public event PropertyChangedEventHandler PropertyChanged;

        private Chat chat;

        private void OnPropertyChanged(string propertyName = "") // raiser the property change
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

            }
        }
        private bool _isGridVisible;

        // Public property for binding to the UI
        public bool IsGridVisible
        {
            get => _isGridVisible;
            set
            {
                if (_isGridVisible != value)
                {
                    _isGridVisible = value;
                    OnPropertyChanged(nameof(IsGridVisible)); // Notify the UI of the change
                }
            }
        }

        private string _ConnectionGrid;
        public string ConnectionGrid
        {
            get { return _ConnectionGrid; }
            set
            {
               _ConnectionGrid= value;
                OnPropertyChanged("ConnectionGrid");
            }
        }

        private List<Chat> _ChatHistory = new List<Chat>();

        public List<Chat> Chathistory
        {
            get {
                Debug.WriteLine("Network chathistory get property");
                if (_ChatHistory == null)
                {
                    _ChatHistory = new List<Chat>();
                }
                return _ChatHistory;
            }
            set
            {
                
                if (value == null)
                {
                    return;
                }
                foreach(Chat chat in value)
                {
                    _ChatHistory.Add(chat);
                }
               // _ChatHistory = value;
                OnPropertyChanged("ChatHistory");
            }
        }

        
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string Nickname { get; set; }

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


        private Messages message;
        public Messages Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }

        public bool startConnection(string action)
        {

            updateHistory();
            Task.Factory.StartNew(() =>
            {
              
               

                if (action == "Host")
                {
                    startHost(); 
                }

                if (action == "Connect")
                {
                    startConnect();
                }
            });
            return true;
        }

        private void startHost()
        {
            ConnectionGrid = "False";
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), int.Parse(Port));
            TcpListener server = new TcpListener(ipEndPoint);
            TcpClient endPoint = null;

            server.Start();
            Debug.WriteLine("Start listening...");
            endPoint = server.AcceptTcpClient();
            Debug.WriteLine("Connection accepted!");
            chat = new Chat();
            
            handleConnection(endPoint);




        }

        private bool startConnect()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), int.Parse(Port));
            TcpListener server = new TcpListener(ipEndPoint);
            TcpClient endPoint = null;
            endPoint = new TcpClient();
            try
            {
                
                Debug.WriteLine("Connecting to the server...");
                try
                {
                    endPoint.Connect(ipEndPoint);
                }
                catch (Exception e)
                {
                    Messages noHost = new Messages { RequestType = "no_host" };
                    this.Message = noHost;
                    return false;
                }
                Debug.WriteLine("Connection established!");


                Messages p = new Messages { Name = Nickname, RequestType = "request" };
                string jsonString = JsonConvert.SerializeObject(p, Formatting.Indented);
                var message = Encoding.UTF8.GetBytes(jsonString);

                


                endPoint.GetStream().Write(message, 0, message.Length);
                stream = endPoint.GetStream();
                while (true) {

                    Debug.WriteLine("Enter while loop");
                    var buffer = new byte[1024];
                    int received = stream.Read(buffer, 0, 1024);
                    if (received == 0)
                    {
                        Debug.WriteLine("No data received. Closing connection.");
                        break;  // No data received, break the loop
                    }
                    string answer = Encoding.UTF8.GetString(buffer, 0, received);
                    try
                    {
                        // If the answer is not empty, attempt to deserialize it
                        if (!string.IsNullOrEmpty(answer))
                        {
                            Messages proto = JsonConvert.DeserializeObject<Messages>(answer);

                            if (proto != null)
                            {
                                if (proto.Answer == "True")
                                {
                                    //PROPERTY CHANGE TO ENABLE CHAT
                                    this.ConnectionGrid = "True";
                                    Messages accept = new Messages { RequestType = "accept_connect" };
                                    this.Message = accept;

                                    chat = new Chat();

                                    chat.Name = proto.Name;       /// funkar ej 
                                    Debug.WriteLine(chat.Name);

                                    break;  
                                }
                                else if (proto.Answer == "False")
                                {
                                    Messages denied = new Messages { RequestType = "denied_connect" };
                                    this.Message = denied;

                                    return false;  // Return false if the answer is "False"
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Failed to deserialize answer.");
                            }
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        Debug.WriteLine($"JSON deserialization error: {ex.Message}");
                    }
                }
              //  this.Message = "";
                handleConnection(endPoint);
            }
            finally
            {
                endPoint.Close();

            }
                                    
            return false;
        }


        public void HandleDisconnection(string reason)
        {
            cancelChatt();
            try
            {
                Debug.WriteLine($"Disconnection triggered: {reason}");

                // Perform cleanup

                stream?.Close(); // Close the stream
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during disconnection: {ex.Message}");
            }
        }


        private void updateHistory()
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\"); // Base directory of the application
            string relativePath = "Data\\Data.json";
            string fullPath = Path.Combine(basePath, relativePath);
            string ourfile = File.ReadAllText(fullPath);
            var list = JsonConvert.DeserializeObject<List<Chat>>(ourfile);
            Chathistory = list;

        }

        private void cancelChatt()
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\"); // Base directory of the application
            string relativePath = "Data\\Data.json";
            string fullPath = Path.Combine(basePath, relativePath);
            string ourfile = File.ReadAllText(fullPath);
            var list = JsonConvert.DeserializeObject<List<Chat>>(ourfile);
            if (list == null)
            {
                list = new List<Chat>();
            }

            foreach (var jsonChat in list)
            {
                if (jsonChat.ChatID == chat.ChatID)
                {
                    return;
                }
            }

            list.Add(chat);
            string jsonString = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(fullPath, jsonString);

            Chathistory = list;
        }


        private void handleConnection(TcpClient endPoint)
        {
            stream = endPoint.GetStream();
            while (true)
            {
                try
                {
                    var buffer = new byte[1024];
                    int received = stream.Read(buffer, 0, 1024);
                    string message = Encoding.UTF8.GetString(buffer, 0, received);

                    if (received == 0)
                    {
                        Messages disconnect = new Messages { RequestType = "disconnected" };
                        this.Message = disconnect;
                        this.ConnectionGrid = "False";

                        Debug.WriteLine("Connection closed by peer.");

                        cancelChatt();
                        break; // Exit loop when the connection is closed
                    }

                    try
                    {
                        Messages p = null;

                        if (!string.IsNullOrEmpty(message))
                        {
                            p = JsonConvert.DeserializeObject<Messages>(message);
                        }

                        if (p?.RequestType == "request")
                        {
                            // Update IsGridVisible to true on the server instance

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Debug.WriteLine("SHOW GRID");
                                this.Request_message = "Chat request sent by: " + p.Name;
                                this.IsGridVisible = true; // This will update the UI
                                chat.Name = p.Name;
                            });

                        }
                        else
                        {
                            
                            // Process other messages 
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                               
                                chat.Messages.Add(p);
                                this.Message = p;
                            });
                        }
                    }
                    catch (JsonReaderException)
                    {
                        cancelChatt();
                        break;
                    }
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"IOException (likely disconnected): {ex.Message}");
                    cancelChatt();
                    break; // Exit loop on disconnection or error
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine($"SocketException (likely disconnected): {ex.Message}");
                    cancelChatt();
                    break; // Exit loop on socket error
                }

            }

        }

        public Chat viewHistory(Guid ChatID)
        {
            if (stream != null)
            {
                HandleDisconnection("Open history");

            }
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\"); // Base directory of the application
            string relativePath = "Data\\Data.json";
            string fullPath = Path.Combine(basePath, relativePath);
            string ourfile = File.ReadAllText(fullPath);
            var list = JsonConvert.DeserializeObject<List<Chat>>(ourfile);

            foreach (Chat chat in list)
            {

                if (chat.ChatID == ChatID)
                {
                    ConnectionGrid = "False";
                    return chat; 
                }
            } return null;
            

        }
        public void sendChar(string str)
        {
            Task.Factory.StartNew(() =>
            {
                Messages p = new Messages { Name = Nickname, RequestType = "message", Message = str };
                p.DateTime = DateTime.Now;
                chat.Messages.Add(p);
                string jsonString = JsonConvert.SerializeObject(p, Formatting.Indented);
                var message = Encoding.UTF8.GetBytes(jsonString);
                var buffer = Encoding.UTF8.GetBytes(str);

                stream.Write(message, 0, message.Length);
            });
        }

        public void sendAnswer(bool answer)
        {
            this.IsGridVisible = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (answer)
                {
                    this.Message = new Messages { RequestType = "clear_chat" };
                    this.ConnectionGrid = "True";
                }
            });

                string str = answer.ToString();
            Task.Factory.StartNew(() =>
            {
                Messages p = new Messages { Name = Nickname, RequestType = "requestAnswer", Answer = str };
                string jsonString = JsonConvert.SerializeObject(p, Formatting.Indented);
                var message = Encoding.UTF8.GetBytes(jsonString);
                var buffer = Encoding.UTF8.GetBytes(str);

                stream.Write(message, 0, message.Length);
            });
        }
    }
}
