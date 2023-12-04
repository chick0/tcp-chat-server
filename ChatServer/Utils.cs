using System;
using System.Net.Sockets;

namespace ChatUtils
{
    /// <summary>
    /// 서버로 부터 받은 데이터의 형식을 의미합니다.
    /// </summary>
    enum ContextType
    {
        Message,
        Error,
        Command,
    }

    class Context
    {
        public string Sender;
        public string Value;
        public string SubValue;
    }

    class Utils
    {
        private Socket socket;

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
        /// 서버용 소켓을 생성합니다. (리슨 소켓)
        /// </summary>
        /// <param name="BindAddr"></param>
        /// <param name="BindPort"></param>
        public void Setup(in string BindAddr, in int BindPort)
        {
        }

        /// <summary>
        /// 서버에 접속합니다.
        /// </summary>
        /// <param name="SererAddr">서버의 IP 주소 값</param>
        /// <param name="ServerPort">서버의 포트 번호</param>
        public void Connect(in string SererAddr, in int ServerPort) 
        {
        }

        /// <summary>
        /// 서버로 메시지를 전송합니다.
        /// </summary>
        public void SendMessage()
        {
        }

        /// <summary>
        /// 서버로 명령어를 전송합니다.
        /// </summary>
        public void SendCommand()
        {
        }

        /// <summary>
        /// 서버로부터 메시지 또는 명령어를 수신받습니다.
        /// </summary>
        public void ReceiveContext()
        {
        }

        /// <summary>
        /// 서버가 새로운 클라이언트 연결을 받습니다.
        /// </summary>
        public Socket Accept()
        {
            return socket.Accept();
        }

        public static void Receive(in Socket client, out ContextType type, out ContextType ctx)
        {
            byte[] buf = new byte[4];

            OpCode op;

            int packetSize;

            if (SafeReceive(in client, buf, 1))
            {
                op = (OpCode)BitConverter.ToChar(buf, 0);
            }
            else
            {
                throw new Exception("OP CODE를 가져오는 과정에서 오류가 발생했습니다.");
            }

            if (SafeReceive(in client, buf, 4))
            {
                packetSize = BitConverter.ToInt32(buf);
            }
            else
            {
                throw new Exception("패킷 사이즈를 가져오는 과정에서 오류가 발생했습니다.");
            }

            if (SafeReceive(in client, buf, packetSize))
            {
                switch (op)
                {
                    case OpCode.Join:
                        break;

                    case OpCode.Quit:
                        break;

                    case OpCode.SetName:
                        break;

                    case OpCode.GetNames:
                        break;

                    case OpCode.Message:
                        break;

                    case OpCode.DirectMessage:
                        break;

                    default:
                        throw new Exception("OPCODE가 올바르지 않습니다.");
                }
            }
            else
            {
                throw new Exception("패킷 데이터를 가져오는 과정에서 오류가 발생했습니다.");
            }
        }

        static bool SafeReceive(in Socket socket, byte[] buf, in int length)
        {
            int received = 0;

            while (received <= 0)
            {
                try
                {
                    received += socket.Receive(buf, 0, length, SocketFlags.None);

                    if (received == 0)
                    {
                        print("소켓으로 부터 받은 데이터가 없습니다!!");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    print(e.Message);
                    return false;
                }
            }

            return true;
        }
    }
}
