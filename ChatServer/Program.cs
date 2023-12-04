using ChatUtils;
using System.Net.Sockets;

namespace ChatServer
{
    class Program
    {
        struct Pack
        {
            public Socket client;
            public Utils ut;

            public Pack(in Socket s, in Utils u)
            {
                client = s;
                ut = u;
            }
        }

        public Program()
        {
            Utils ut = new();
            ut.Setup("0.0.0.0", 8000);

            List<Socket> sockets = new();

            while (true)
            {
                var client = ut.Accept();
                sockets.Add(client);

                Thread th = new Thread(Handler);
                th.Start(new Pack(client, ut));
            }
        }

        private void Handler(object? pack)
        {
            if (pack == null)
            {
                Utils.print("비정상적인 이벤트 핸들러가 호출되었습니다. 해당 요청을 무시합니다.");
                return;
            }

            Pack tmp = (Pack)pack;
            Socket client = tmp.client;
            Utils ut = tmp.ut;

            while (true)
            {
            }
        }

        static void Main(string[] args)
        {
            new Program();
        }
    }
}