using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UCS.PacketProcessing;
using System.Configuration;

namespace UCS.Core
{
    class ApiManager2
    {
        private static HttpListener m_vListener;

        public ApiManager2()
        {
            string hostName = Dns.GetHostName();
            string IP = Dns.GetHostEntry(hostName).AddressList[0].ToString();

            m_vListener = new HttpListener();
            m_vListener.Prefixes.Add("http://localhost:4316/");
            //m_vListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            m_vListener.Start();
            Action action = RunServer;
            action.BeginInvoke(RunServerCallback, action);
        }

        private void StartListener(object data)
        {

            // Note: The GetContext method blocks while waiting for a request. 
            HttpListenerContext context = m_vListener.GetContext();

            m_vListener.Stop();
        }

        private void RunServer()
        {
            while (m_vListener.IsListening)
            {
                IAsyncResult result = m_vListener.BeginGetContext(Handle, m_vListener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private void Handle(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);


            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = "<HTML><BODY><PRE>";
            /*
            responseString += "Active Connections: ";
            int connectedUsers = 0;
            foreach(var client in ResourcesManager.GetConnectedClients())
            {
                var socket = client.Socket;
                if(socket != null)
                {
                    try
                    {
                        bool part1 = socket.Poll(1000, SelectMode.SelectRead);
                        bool part2 = (socket.Available == 0);
                        if (!(part1 && part2))
                            connectedUsers++;
                    }
                    catch(Exception){}
                }
            }
            responseString += connectedUsers + "\n";
            */

            responseString += "<details><summary>";
            responseString += "" + ResourcesManager.GetOnlinePlayers().Count + "</summary></details>";

            responseString += "</PRE></BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
        }

        private void RunServerCallback(IAsyncResult ar)
        {
            try
            {
                Action target = (Action)ar.AsyncState;
                target.EndInvoke(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Stop()
        {
            m_vListener.Stop();
            m_vListener.Close();
        }
    }
}