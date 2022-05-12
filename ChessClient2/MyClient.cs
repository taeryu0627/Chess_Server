using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChessClient2
{
    internal class MyClient
    {
        TcpClient client = null;
        Thread receiveMessageThread = null;
        ConcurrentBag<string> sendMessageListToView = null;
        ConcurrentBag<string> receiveMessageListToView = null;
        private string name = null;
        public void Run()
        {
            sendMessageListToView = new ConcurrentBag<string>();
            receiveMessageListToView = new ConcurrentBag<string>();
            receiveMessageThread = new Thread(receiveMessage);

            while (true)
            {
                Console.WriteLine("==========클라이언트==========");
                Console.WriteLine("1.서버연결");
                Console.WriteLine("2.Message 보내기");
                Console.WriteLine("3.받은 메시지 확인");
                Console.WriteLine("==============================");

                string key = Console.ReadLine();
                int order = 0;

                // 입력받은 key의 값을 int.TryParse를 사용해 int형으로 형변환해줍니다.
                // 형변환에 성공하면 order에 변환된 값이 저장됩니다.
                // 형변환에 실패하면 false를 리턴합니다.
                if (int.TryParse(key, out order))
                {
                    switch (order)
                    {
                        case 1:
                            {
                                if (client != null)
                                {
                                    Console.WriteLine("이미 연결되어있습니다.");
                                    Console.ReadKey();
                                }
                                else
                                {
                                    Connect();
                                }

                                break;
                            }
                        case 2:
                            {
                                if (client == null)
                                {
                                    Console.WriteLine("먼저 서버와 연결해주세요");
                                    Console.ReadKey();
                                }
                                else
                                {
                                    SendMessage();
                                }
                                break;
                            }
                        case 3:
                            {
                                ReceiveMessageVIew();
                                break;
                            }
                    }
                }

                else
                {
                    Console.WriteLine("잘못 입력하셨습니다.");
                    Console.ReadKey();
                }
                Console.Clear();
            }
        }
        private void ReceiveMessageVIew()
        {
            if (receiveMessageListToView.Count == 0)
            {
                Console.WriteLine("받은 메시지가 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var item in receiveMessageListToView)
            {
                Console.WriteLine(item);
            }
            Console.ReadKey();
        }

        private void receiveMessage()
        {
            string receiveMessage = "";
            List<string> receiveMessageList = new List<string>();
            while (true)
            {
                byte[] receiveByte = new byte[1024];
                client.GetStream().Read(receiveByte, 0, receiveByte.Length);

                receiveMessage = Encoding.Default.GetString(receiveByte);

                string[] receiveMessageArray = receiveMessage.Split('>');
                foreach (var item in receiveMessageArray)
                {
                    if (!item.Contains('<'))
                        continue;
                    // 관리자<TEST>는 서버에서 보내는 하트비트 메시지이니 무시해줍니다. 
                    if (item.Contains("관리자<TEST"))
                        continue;
                    receiveMessageList.Add(item);

                }
                ParsingReceiveMessage(receiveMessageList);

                Thread.Sleep(500);
            }
        }
        private void ParsingReceiveMessage(List<string> messageList)
        {
            foreach (var item in messageList)
            {
                string sender = "";
                string message = "";

                if (item.Contains('<'))
                {
                    string[] splitedMsg = item.Split('<');

                    sender = splitedMsg[0];
                    message = splitedMsg[1];

                    if (sender == "관리자")
                    {
                        string userList = "";
                        string[] splitedUser = message.Split('$');
                        foreach (var el in splitedUser)
                        {
                            if (string.IsNullOrEmpty(el))
                                continue;
                            userList += el + " ";
                        }
                        Console.WriteLine(string.Format("[현재 접속인원] {0}", userList));
                        messageList.Clear();
                        return;
                    }

                    Console.WriteLine(string.Format("[메시지가 도착하였습니다] {0} : {1}", sender, message));
                    receiveMessageListToView.Add(string.Format("[{0}] Sender : {1}, Message : {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sender, message));
                }
            }
            messageList.Clear();
        }

        private void SendMessage()
        {
            Console.WriteLine("보낼 message를 입력해주세요");
            string message = Console.ReadLine();
            byte[] byteData = new byte[message.Length];

            byteData = Encoding.Default.GetBytes(message);

            client.GetStream().Write(byteData, 0, byteData.Length);
            Console.WriteLine("전송성공");
            Console.ReadKey();
        }


        private void Connect()
        {
            // 이전게시물에서 다룬 내용이니 따로 다루지 않겠습니다.
            client = new TcpClient();
            client.Connect("127.0.0.2", 9999);
            Console.WriteLine("서버연결 성공 이제 Message를 입력해주세요");
            Console.ReadKey();
        }
    }
}
