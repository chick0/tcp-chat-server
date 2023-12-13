using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    internal class Program
    {
        private struct Pack
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
            ut.Setup();

            new Thread(Monitor).Start(ut);

            while (true)
            {
                Socket client = ut.Accept();

                Utils.print($"{client.RemoteEndPoint}가 접속했습니다.");

                new Thread(Handler).Start(new Pack(client, ut));
            }
        }

        private void Monitor(object? pack)
        {
            if (pack == null)
            {
                Utils.error("MonitorInit", "서버 정보 출력 스레드에 서버 유틸 클래스가 전달되지 않았습니다.");
                return;
            }

            Utils utils = (Utils)pack;

            while (true)
            {
                Utils.print($"총 {utils.ClientCount}개의 클라이언트가 접속되어있습니다.");
                Thread.Sleep(5000);
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

            if (client.RemoteEndPoint == null)
            {
                return;
            }

            IPEndPoint ep = (IPEndPoint)client.RemoteEndPoint;

            Utils serverUtils = tmp.ut;
            Utils clientUtils = new(ref client);

            while (true)
            {
                OpCode op;
                object result;
                bool status;

                try
                {
                    status = clientUtils.Receive(out op, out result);
                }
                catch (Exception e)
                {
                    Utils.error(ep.Port, e.Message);
                    serverUtils.Pop(ep.Port);
                    return;
                }

                if (op == OpCode.Join)
                {
                    string name = (string)result;
                    serverUtils.Push(ref client, name);

                    if (status)
                    {
                        Utils.print($"{serverUtils.GetName(ref client).Name}님이 접속했습니다.");
                    }
                }
                else if (op == OpCode.Quit)
                {
                    if (status)
                    {
                        Utils.print($"{serverUtils.GetName(ref client)}님이 접속을 종료했습니다.");
                    }
                }
                else if (op == OpCode.Message)
                {
                    serverUtils.Broadcast((string)result);
                }
            }
        }

        private static void Main(string[] args)
        {
            _ = new Program();
        }
    }
}