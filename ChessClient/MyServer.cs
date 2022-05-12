using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChessServer
{
    internal class MyServer
    {

        ConcurrentBag<string> chattingLog = null;
        ConcurrentBag<string> AccessLog = null;
        Thread conntectCheckThread = null;

        string[,] Map = new string[,]
        {
            { "WRL", "WNL", "WBL", " WQ", " WK", "WBR", "WNR", "WRR" },
            { "WPA", "WPB", "WPC", "WPD", "WPE", "WPF", "WPG", "WPH" },
            { "   ", "   ", "   ", "   ", "   ", "   ", "   ", "   " },
            { "   ", "   ", "   ", "   ", "   ", "   ", "   ", "   " },
            { "   ", "   ", "   ", "   ", "   ", "   ", "   ", "   " },
            { "   ", "   ", "   ", "   ", "   ", "   ", "   ", "   " },
            { "BPA", "BPB", "BPC", "BPD", "BPE", "BPF", "BPG", "BPH" },
            { "BRL", "BNL", "BBL", " BQ", " BK", "BBR", "BNR", "BRR" }
        };
        public MyServer()
        {

        }

        public void AsyncServerStart()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 9999));
            listener.Start();
            Console.WriteLine("서버를 시작합니다.");

            // 클라이언트의 접속을 확인하면 스레드풀에서 해당클라이언트의 메시지를 읽도록 대기시키고
            // while문을 통해 다시 클라이언트의 접속을 기다립니다.
            while (true)
            {
                TcpClient acceptClient = listener.AcceptTcpClient();

                ClientData clientData = new ClientData(acceptClient);

                clientData.client.GetStream().BeginRead(clientData.readByteData, 0, clientData.readByteData.Length, new AsyncCallback(DataReceived), clientData);
            }
        }

        //
        private void DataReceived(IAsyncResult ar)
        {
            ClientData callbackClient = ar.AsyncState as ClientData;

            int bytesRead = callbackClient.client.GetStream().EndRead(ar);

            string readString = Encoding.Default.GetString(callbackClient.readByteData, 0, bytesRead);

            if(callbackClient.clientNumber == 1)Console.WriteLine("White Move : {0}",  readString);
            else Console.WriteLine("Black Move : {0}", readString);

            char sp = ',';
            string[] pieceData = readString.Split(sp);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (pieceData[0] == Map[i, j])
                    {
                        Console.WriteLine("{0} : 확인했습니다.", Map[i, j]);
                        Map[i, j] = "   ";

                    }
                }
            }
            Map[Int32.Parse(pieceData[1]), Int32.Parse(pieceData[2])] = pieceData[0];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Console.Write(Map[i, j] + " ");
                }
                Console.WriteLine();
            }

            callbackClient.client.GetStream().BeginRead(callbackClient.readByteData, 0, callbackClient.readByteData.Length, new AsyncCallback(DataReceived), callbackClient);
        }
    }
}
