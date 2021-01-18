using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SP_Tema5_Ejer2
{
    class Program
    {
        static List<Socket> users = new List<Socket>();
        static object l = new object();
        static void Main(string[] args)
        {
            bool free = false;
            int port = 31416;

            IPEndPoint ie = new IPEndPoint(IPAddress.Any, port);

            using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                
                while (!free)
                {
                    try
                    {
                        s.Bind(ie);
                        free = true;
                    }
                    catch
                    {
                        port++;
                        ie = new IPEndPoint(IPAddress.Any, port);
                    }

                    s.Listen(10);
                    while (true)
                    {
                        Socket cliente = s.Accept();
                        Thread hilo = new Thread(EnSala);
                        hilo.Start(cliente);
                    }
                }
            }
        }

        static void EnSala(object socket)
        {
            String mensaje = "";
            String user = "";
            Socket cliente = (Socket)socket;
            IPEndPoint ie = (IPEndPoint)cliente.RemoteEndPoint;
            users.Add(cliente);
            Console.WriteLine("Conecto {0} en puerto {1}",
            ie.Address, ie.Port);

            using(NetworkStream ns = new NetworkStream(cliente))
            using (StreamWriter sw = new StreamWriter(ns))
            using(StreamReader sr = new StreamReader(ns))
            {
                sw.WriteLine("Estas pa entrar ya pero... Quien cono eres?");
                sw.Flush();
                user = sr.ReadLine();
                sw.WriteLine("Ya puedes escribir");
                sw.Flush();
                while (mensaje != null)
                {
                    try
                    {
                        mensaje = sr.ReadLine();
                    }
                    catch (IOException)
                    {
                        mensaje = null;
                    }
                    lock (l)
                    {
                        if (mensaje != null)
                        {
                            mensaje = user + "@" + ie.Address + ": " + mensaje;
                            Console.WriteLine(mensaje);
                            Escribe(mensaje, cliente);
                        }
                    }
                }
                
                Escribe(user + " se desconecto", cliente);
                users.Remove(cliente);
                cliente.Close();
            }


        }

        static void Escribe(string mensaje, Socket emisario)
        {
            foreach(Socket user in users)
            {
                if (user != emisario)
                {
                    using (NetworkStream ns = new NetworkStream(user))
                    using (StreamWriter sw = new StreamWriter(ns))
                    using (StreamReader sr = new StreamReader(ns))
                    {
                        sw.WriteLine(mensaje);
                        sw.Flush();
                    }
                }
            }
        }
    }
}
