using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChessServer
{
    class ClientManager
    {
        // ConcurentBag과 마찬가지로 스레드로부터 안전한 해시맵의 자료구조입니다.
        // key는 클라이언트의 IP의 네번째 옥텟이며
        // Value는 연결된 클라이언트 객체입니다.
        
        public static ConcurrentDictionary<int, ClientData> clientDic = new ConcurrentDictionary<int, ClientData>();
        public event Action<string, string> messageParsingAction = null;
        public event Action<string, int> EventHandler = null;

        public void AddClient(TcpClient newClient)
        {
            ClientData currentClient = new ClientData(newClient);

            try
            {
                currentClient.tcpClient.GetStream().BeginRead(currentClient.readBuffer, 0, currentClient.readBuffer.Length, new AsyncCallback(DataReceived), currentClient);
                clientDic.TryAdd(currentClient.clientNumber, currentClient);
            }

            catch (Exception e)
            {

            }
        }

        private void DataReceived(IAsyncResult ar)
        {
            
            ClientData client = ar.AsyncState as ClientData;

            try
            {
                int byteLength = client.tcpClient.GetStream().EndRead(ar);

                string strData = Encoding.Default.GetString(client.readBuffer, 0, byteLength);

                client.tcpClient.GetStream().BeginRead(client.readBuffer, 0, client.readBuffer.Length, new AsyncCallback(DataReceived), client);

                if (string.IsNullOrEmpty(client.clientName))
                {
                    if (EventHandler != null)
                    {
                        if (CheckID(strData))
                        {
                            string userName = strData.Substring(3);
                            client.clientName = userName;
                            string accessLog = string.Format("[{0}] {1} Access Server", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), client.clientName);
                            EventHandler.Invoke(accessLog, StaticDefine.ADD_ACCESS_LOG);
                            return;
                        }
                    }

                }


                if (messageParsingAction != null)
                {
                    messageParsingAction.BeginInvoke(client.clientName, strData, null, null);
                }

            }
            catch (Exception e)
            {

            }
        }

        // 클라이언트는 최초 접속시 "%^&이름" 을 보내도록 구현되어있습니다.
        // '%^&' 기호가 왔다면 서버는 해당클라이언트에게 이름을 부여합니다.
        private bool CheckID(string ID)
        {
            if (ID.Contains("%^&"))
                return true;

            return false;
        }
    }
}
