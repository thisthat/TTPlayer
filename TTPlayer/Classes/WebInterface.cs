﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TTPlayer.Classes
{
    class WebInterface
    {
        int port = 1414;
        Socket listener;
        bool fine = false;
        public WebInterface()
        {
            
            Thread t = new Thread(this.receve);
            t.Start();
        }

        public void Dispose()
        {
            fine = true;
            listener.Dispose();
        }

        private void receve()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
           listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            listener.Bind(localEndPoint);
            listener.Listen(10);
            string data;

            while (!fine)
            {
                try
                {          
                    // Start listening for connections.
                    Socket handler = listener.Accept();
                    data = "";
                    // An incoming connection needs to be processed.
                    bytes = new byte[1024];
                    int bytesRec;
                    do
                    {
                        bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    }
                    while (bytesRec > 0);
                    // Show the data on the console.
                    Console.WriteLine("Text received : {0}", data);
                    
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}