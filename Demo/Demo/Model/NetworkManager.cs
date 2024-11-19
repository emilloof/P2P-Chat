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

namespace Demo.Model
{
    //Kan man ha detta synligt för viewmodeln?
    public class Protocol
        {
            public string Name { get; set; }
            public string Request { get; set; }

            public string Message { get; set; }
            public string Answer { get; set; }

            public DateTime DateTime { get; set; }
        }
    internal class NetworkManager : INotifyPropertyChanged
    {

        private NetworkStream stream;

        public event PropertyChangedEventHandler PropertyChanged;

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


        private Protocol message;
        public Protocol Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }

        public bool startConnection(string action)
        {
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
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), int.Parse(Port));
            TcpListener server = new TcpListener(ipEndPoint);
            TcpClient endPoint = null;

            server.Start();
            Debug.WriteLine("Start listening...");
            endPoint = server.AcceptTcpClient();
            Debug.WriteLine("Connection accepted!");
            handleConnection(endPoint);

        }

        private bool startConnect()
        {

           // this.Message = "Trying to connect";
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
                    Debug.WriteLine("SADASDA");
                    Protocol noHost = new Protocol { Request = "no_host" };
                    this.Message = noHost;
                    return false;
                }
                Debug.WriteLine("Connection established!");


                Protocol p = new Protocol { Name = Nickname, Request = "request" };
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
                            Protocol proto = JsonConvert.DeserializeObject<Protocol>(answer);

                            if (proto != null)
                            {
                                Debug.WriteLine($"Received answer: {proto.Answer}");

                                if (proto.Answer == "True")
                                {
                                    //PROPERTY CHANGE TO ENABLE CHAT
                                    this.ConnectionGrid = "True";
                                    Protocol accept = new Protocol { Request = "accept_connect" };
                                    this.Message = accept;


                                    break;  
                                }
                                else if (proto.Answer == "False")
                                {
                         
                                    Protocol denied = new Protocol { Request = "denied_connect" };
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
                        Debug.WriteLine("Connection closed by peer.");
                        break; // Exit loop when the connection is closed
                    }

                    try
                    {
                        Protocol p = null;

                        if (!string.IsNullOrEmpty(message))
                        {
                            p = JsonConvert.DeserializeObject<Protocol>(message);
                        }

                        if (p?.Request == "request")
                        {
                            // Update IsGridVisible to true on the server instance

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Debug.WriteLine("SHOW GRID");
                                this.Request_message = "Chat request sent by: " + p.Name;
                                this.IsGridVisible = true; // This will update the UI
                            });

                        }
                        else
                        {
                            
                            // Process other messages 
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                p.DateTime = DateTime.Now;
                                this.Message = p;
                            });
                        }
                    }
                    catch (JsonReaderException)
                    {

                    }
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"IOException (likely disconnected): {ex.Message}");
                    break; // Exit loop on disconnection or error
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine($"SocketException (likely disconnected): {ex.Message}");
                    break; // Exit loop on socket error
                }

            }

        }
        public void sendChar(string str)
        {
            Task.Factory.StartNew(() =>
            {
                Protocol p = new Protocol { Name = Nickname, Request = "message", Message = str };
                string jsonString = JsonConvert.SerializeObject(p, Formatting.Indented);
                var message = Encoding.UTF8.GetBytes(jsonString);
                var buffer = Encoding.UTF8.GetBytes(str);

                stream.Write(message, 0, message.Length);
            });
        }

        public void sendAnswer(bool answer)
        {
            this.IsGridVisible = false;

            string str = answer.ToString();
            Task.Factory.StartNew(() =>
            {
                Protocol p = new Protocol { Name = Nickname, Request = "requestAnswer", Answer = str };
                string jsonString = JsonConvert.SerializeObject(p, Formatting.Indented);
                var message = Encoding.UTF8.GetBytes(jsonString);
                var buffer = Encoding.UTF8.GetBytes(str);

                stream.Write(message, 0, message.Length);
            });
        }
    }
}
