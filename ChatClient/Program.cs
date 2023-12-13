namespace ChatClient
{
    internal class Program
    {
        private readonly Utils ut;

        public Program(string[] args)
        {
            ut = new();

            if (args.Length > 0)
            {
                ut.Connect(args[0]);
            }
            else
            {
                ut.Connect("127.0.0.1");
            }

            while (true)
            {
                string? name;
                Console.Write("이름을 입력해주세요: ");
                name = Console.ReadLine();

                if (name != null && name.Length > 0)
                {
                    ut.SendMessage(OpCode.Join, name);
                    break;
                }
            }

            new Thread(ReadAndSend).Start();
            new Thread(ReceiveAndPrint).Start();
        }

        private void ReadAndSend()
        {
            string? message;

            while (true)
            {
                Console.Write(">> ");
                message = Console.ReadLine();

                if (message == null || message.Length == 0)
                {
                    continue;
                }

                ut.SendMessage(OpCode.Message, message);
            }
        }

        private void ReceiveAndPrint()
        {
            while (true)
            {
                bool status = ut.Receive(out _, out object tmp);

                if (status)
                {
                    string? message = (string)tmp;

                    if (message != null && message.Length != 0)
                    {
                        Console.WriteLine();
                        Utils.print(message);
                    }
                }
            }
        }

        private static void Main(string[] args)
        {
            _ = new Program(args);
        }
    }
}
