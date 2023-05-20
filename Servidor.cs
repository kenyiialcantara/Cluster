using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Sockets.Servidor
{
    internal class Servidor
    {
        private int PORT = 3000;
        TcpListener listener;
        List<ServerThread> conexiones = new List<ServerThread>();
        List<Thread> T = new List<Thread>();
        Function function;
        int n = 0;
        double SUMATOTAL;

        public static void Main(string[] args)
        {
            Servidor s = new Servidor();
            s.iniciar();
        }

        public void iniciar()
        {
            Thread t = new Thread(new ThreadStart(ThreadProc));
            t.Start();
            string mensaje = "n";

            while (!mensaje.Equals("s"))
            {
                if (n > 0)
                {
                    Console.Write("Ingrese [funcion] [min] [max] [pasos] (\"s\" para salir): ");
                    mensaje = Console.ReadLine();
                    if (mensaje != null || !mensaje.Equals("s"))
                    {
                        string[] s = mensaje.Split(" ");
                        if (s.Length != 5) { continue; }
                        string f = s[0];
                        double a = double.Parse(s[1]);
                        double b = double.Parse(s[2]);
                        int steps = int.Parse(s[3]);
                        int threads = int.Parse(s[4]);

                        function = new FunctionParser(f, a, b, steps, threads).toObject();

                        ServidorEnvia(JsonSerializer.Serialize(function));
                    }
                }
            }
            Console.WriteLine("Adios");
            Environment.Exit(Environment.ExitCode); 

            return;
        }

        public void ThreadProc()
        {

            listener = new TcpListener(PORT);
            listener.Start();
            while (true)
            {
                Console.WriteLine("Esperando conexiones...");

                TcpClient cliente = listener.AcceptTcpClient();
                Console.WriteLine("Connexion establecida con " + (n + 1) + " clientes");

                conexiones.Add(new ServerThread(n, this, cliente));
                T.Add(new Thread(new ParameterizedThreadStart(CreaConexion)));
                T.Last().Start(conexiones.Last());
                n++;
            }
        }

        public void CreaConexion(object p)
        {
            ServerThread conexion = (ServerThread)p;
            conexion.manejaConexion();
        }

        public void ServidorEnvia(string mensaje)
        {
            
            Function f = JsonSerializer.Deserialize<Function>(mensaje);

            double size = (f.b - f.a) / n;
            for (int i = 0; i < n; ++i)
            {
                Function f_parcial = new Function();
                f_parcial.function = f.function;
                f_parcial.a = f.a + i * size;
                f_parcial.b = f.a + (i + 1) * size;
                f_parcial.segmentos = f.segmentos;
                f_parcial.threads = f.threads;
                mensaje = JsonSerializer.Serialize(f_parcial);
                if (conexiones[i].cliente.Connected)
                {
                    conexiones[i].enviaMensaje(mensaje);
                    //conexiones[i].recibido = false;
                }
            }
            


            for (int i=0; i < n;++i)
            {
                if (!conexiones[i].recibido)
                {
                    if (!conexiones[i].cliente.Connected)
                    {
                        conexiones.RemoveAt(i);
                        n = conexiones.Count;
                    }
                    --i;
                }
            }
            
            Console.WriteLine("TERMINADO");

            for (int i = 0; i < n; ++i)
            {
                SUMATOTAL += conexiones[i].suma;
                conexiones[i].suma = 0;
            }

            Console.WriteLine("Resultado: " + SUMATOTAL);
            Console.WriteLine("CONEXIONES ACTIVAS AL FINAL = " + n);
            if(n== 0)
            {
                Console.WriteLine("NO HAY CONEXIONES!\nEsperando conexiones...");
            }
            SUMATOTAL = 0;
            for(int i = 0; i<n; ++i) {
                conexiones[i].recibido = false;
                conexiones[i].ID= i;
            }
        }

        class ServerThread
        {
            public int ID;
            public TcpClient cliente;
            public Servidor servidor;
            public double suma;
            public bool recibido=false;
            StreamWriter output;
            NetworkStream stream;
            public ServerThread(int ID_, Servidor servidor_, TcpClient cliente_)
            {
                ID = ID_;
                servidor = servidor_;
                cliente = cliente_;
                stream = cliente_.GetStream();
                output = new StreamWriter(stream);
            }

            public void manejaConexion()
            {

                while (cliente.Connected)
                {
                    if (!recibido)
                    {
                        byte[] data = new byte[256];
                        string responseData = string.Empty;

                        try
                        {
                            int bytes = cliente.GetStream().Read(data, 0, data.Length);
                            responseData = Encoding.ASCII.GetString(data, 0, bytes).Trim();
                            
                            Dictionary<String, double> dict = JsonSerializer.Deserialize<Dictionary<String, double>>(responseData);
                            Console.WriteLine("Recibido de " + ID + ": " + responseData);
                            suma = dict["result"];
                            recibido = true;
                        }
                        catch(Exception ex)
                        {
                            try
                            {
                                cliente.Dispose();
                                enviaMensaje("?");
                                //recibido=true;
                                //break;
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Cliente " + ID + " cerrado");
                                
                                //cliente.Close();
                            }
                        }
                        
                    }
                }
            }

            public void enviaMensaje(string mensaje)
            {
                output.WriteLine(mensaje);
                Console.WriteLine("Enviado a " + ID + ": " + mensaje);
                output.Flush();
            }
        }
    }
}
