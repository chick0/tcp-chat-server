using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatUtils
{
    struct ClientData
    {
        public string Name { get; }
        public Socket Socket { get; }

        public ClientData(string name, ref Socket socket)
        {
            Name = name;
            Socket = socket;
        }
    }

    class Utils
    {
        private Socket socket;
        private int Port { get; }

        private Dictionary<int, ClientData> clientMap;

        public int ClientCount { get { return clientMap.Count; } }

        public Utils()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Port = 8000;
            clientMap = new();
        }

        public Utils(ref Socket socket)
        {
            this.socket = socket;
            Port = 8000;
        }

        /// <summary>
        /// 메시지 출력용 함수
        /// </summary>
        /// <param name="msg">출력할 메시지</param>
        public static void print(string msg)
        {
            DateTime dt = DateTime.Now;
            Console.WriteLine($"[{dt.Year:D4}-{dt.Month:D2}-{dt.Day:D2} {dt.Hour:D2}:{dt.Minute:D2}:{dt.Second:D2}] {msg}");
        }

        /// <summary>
        /// 오류 메시지를 출력할 때 사용하는 함수
        /// </summary>
        /// <param name="by"></param>
        /// <param name="msg"></param>
        public static void error(object by, string msg)
        {
            print($"[ERROR] {by}: {msg}");
        }

        /// <summary>
        /// 소켓의 식별자 값을 가져옴..?
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int GetClientIDFromSocket(in Socket socket)
        {
            if (socket.RemoteEndPoint == null)
            {
                throw new Exception("연결 정보가 없는 소켓 객체의 고유 ID 값을 가져올 수 없습니다.");
            }

            IPEndPoint addr = (IPEndPoint)socket.RemoteEndPoint;

            return addr.Port;
        }

        /// <summary>
        /// 해당 클라이언트의 정보를 가져옵니다.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public ClientData GetName(ref Socket socket)
        {
            if (socket.RemoteEndPoint == null)
            {
                print("연결된 소켓이 아닙니다.");
                Environment.Exit(-4);
            }

            IPEndPoint ep = (IPEndPoint)socket.RemoteEndPoint;

            if (ep == null)
            {
                print("클라이언트 정보 접근 과정에서 오류가 발생했습니다.");
                Environment.Exit(-5);
            }

            return clientMap[ep.Port];
        }

        /// <summary>
        /// 서버용 소켓을 생성합니다. (리슨 소켓)
        /// </summary>
        public void Setup()
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            socket.Listen(int.MaxValue);
            print($"서버가 {Port} 포트에서 시작되었습니다.");
        }

        /// <summary>
        /// 서버에 접속합니다.
        /// </summary>
        /// <param name="SererAddr">서버의 IP 주소 값</param>
        public void Connect(in string SererAddr)
        {
            IPEndPoint ep = IPEndPoint.Parse(SererAddr);
            ep.Port = Port;

            socket.Connect(ep);
        }

        /// <summary>
        /// 상대방한테 메시지를 전송합니다.
        /// </summary>
        public void SendMessage(OpCode code, in string message)
        {
            // OPCode 전송
            socket.Send(BitConverter.GetBytes((char)code));

            // 패킷 크기 전송
            int packetSize = message.Length;
            socket.Send(BitConverter.GetBytes(packetSize));

            // 데이터 패킷 전송
            socket.Send(Encoding.Default.GetBytes(message));
        }

        /// <summary>
        /// 서버에 접속한 모든 클라이언트한테 메시지를 전송합니다.
        /// </summary>
        /// <param name="message"></param>
        public void Broadcast(in string message)
        {
            if (clientMap == null)
            {
                return;
            }

            List<int> clientIds = new(clientMap.Keys);

            for (int i = 0; i < clientMap.Count; i++)
            {
                Socket target = clientMap[clientIds[i]].Socket;

                if (target.Connected)
                {
                    Utils tmpUtils = new Utils(ref target);
                    tmpUtils.SendMessage(OpCode.Message, message);
                }
                else
                {
                    clientMap.Remove(clientIds[i]);
                }
            }

            print($"{clientMap.Count}개의 클라이언트한테 메시지를 전달했습니다.");
        }

        /// <summary>
        /// 서버가 새로운 클라이언트 연결을 받습니다.
        /// </summary>
        public Socket Accept()
        {
            return socket.Accept();
        }

        /// <summary>
        /// 현재 연결된 대상으로부터 데이터를 수신받습니다.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="op"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Receive(out OpCode op, out object result)
        {
            byte[] buf;
            int packetSize;

            // 1. OP CODE 크기는 1 바이트
            buf = new byte[1];

            if (SafeReceive(in socket, buf, 1))
            {
                op = (OpCode)int.Parse(BitConverter.ToString(buf), NumberStyles.HexNumber);
            }
            else
            {
                throw new Exception("OP CODE를 가져오는 과정에서 오류가 발생했습니다.");
            }

            // 2. 패킷 크기 정보는 4 바이트
            buf = new byte[4];

            if (SafeReceive(in socket, buf, 4))
            {
                packetSize = BitConverter.ToInt32(buf);
            }
            else
            {
                throw new Exception("패킷 사이즈를 가져오는 과정에서 오류가 발생했습니다.");
            }

            // 3. 데이터 크기는 위에서 구한 크기를 사용
            buf = new byte[packetSize];

            if (SafeReceive(in socket, buf, packetSize))
            {
                switch (op)
                {
                    case OpCode.Join:
                        // 클라이언트 이름 설정
                        result = Encoding.Default.GetString(buf, 0, packetSize);
                        return true;

                    case OpCode.Quit:
                        // 클라이언트 정보 삭제
                        result = 0;
                        return true;

                    case OpCode.Message:
                        result = Encoding.Default.GetString(buf, 0, packetSize);
                        return true;

                    default:
                        throw new Exception("OP CODE가 올바르지 않습니다.");
                }
            }
            else
            {
                throw new Exception("패킷 데이터를 가져오는 과정에서 오류가 발생했습니다.");
            }
        }

        /// <summary>
        /// 전체 데이터를 크기 안전하게 수신받음
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buf"></param>
        /// <param name="length">패킷의 크기</param>
        /// <param name="received">실제 크기</param>
        /// <returns>수신받은 데이터의 신뢰여부</returns>
        static bool SafeReceive(in Socket socket, byte[] buf, in int length)
        {
            int received;
            int left = length;

            int offset = 0;            

            while (left > 0)
            {
                try
                {
                    received = socket.Receive(buf, offset, left, SocketFlags.None);

                    if (received == 0)
                    {
                        print("소켓으로 부터 받은 데이터가 없습니다!!");
                        return false;
                    }

                    left -= received;
                    offset += received;
                }
                catch (Exception e)
                {
                    print(e.Message);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 서버에 클라이언트를 등록합니다.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="name"></param>
        public void Push(ref Socket socket, in string name)
        {
            if (socket.RemoteEndPoint == null)
            {
                print("연결된 소켓이 아닙니다!");
                return;
            }

            IPEndPoint ep = (IPEndPoint)socket.RemoteEndPoint;

            clientMap[ep.Port] = new ClientData(name, ref socket);
        }

        /// <summary>
        /// 서버에 등록된 클라이언트를 삭제합니다.
        /// </summary>
        /// <param name="port"></param>
        public void Pop(in int port)
        {
            if (!clientMap.ContainsKey(port))
            {
                print("등록된 클라이언트가 아닙니다.");
                return;
            }

            clientMap[port].Socket.Disconnect(true);
            clientMap.Remove(port);
        }
    }
}
