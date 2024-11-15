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

namespace Demo.Model
{
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


        private string message;
        public string Message
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
            System.Diagnostics.Debug.WriteLine("Start listening...");
            endPoint = server.AcceptTcpClient();
            System.Diagnostics.Debug.WriteLine("Connection accepted!");
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
                
                System.Diagnostics.Debug.WriteLine("Connecting to the server...");
                endPoint.Connect(ipEndPoint);
                System.Diagnostics.Debug.WriteLine("Connection established!");

                var message = Encoding.UTF8.GetBytes("request");
                endPoint.GetStream().Write(message, 0, message.Length);
                stream = endPoint.GetStream();
                while (true) {
                    Debug.WriteLine("Enter while loop");
                    var buffer = new byte[1024];
                    int received = stream.Read(buffer, 0, 1024);
                    var answer = Encoding.UTF8.GetString(buffer, 0, received);
                    Debug.WriteLine("Answer:" + answer.ToString());
                    if (answer.ToString() == "True")
                    {

                        break;
                    }
                    else if (answer.ToString() == "False")
                    {
                        return false;
                    }
                }

                handleConnection(endPoint);
            }
            finally
            {
                endPoint.Close();

            }
                                                //
            return false;
        }

        


        private void handleConnection(TcpClient endPoint)
        {
            stream = endPoint.GetStream();
            while (true)
            {

                var buffer = new byte[1024];
                int received = stream.Read(buffer, 0, 1024);
                var message = Encoding.UTF8.GetString(buffer, 0, received);
                this.Message = message; // ´property change


                if (message == "request")
                {
                    // Update IsGridVisible to true on the server instance
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Request_message = "DSA";
                        IsGridVisible = true; // This will update the UI
                    });
                }
                else
                {
                    // Process other messages as needed
                    this.Message = message;
                }
            }

        }
        public void sendChar(string str)
        {
            Task.Factory.StartNew(() =>
            {
                var buffer = Encoding.UTF8.GetBytes(str);
                stream.Write(buffer, 0, str.Length);
            });
        }

        public void sendAnswer(bool answer)
        {
            string str = answer.ToString();
            Task.Factory.StartNew(() =>
            {
                var buffer = Encoding.UTF8.GetBytes(str);
                stream.Write(buffer, 0, str.Length);
            });
        }
    }
}
