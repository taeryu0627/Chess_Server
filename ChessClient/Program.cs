using ChessServer;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        MyServer a = new MyServer();
        a.AsyncServerStart();
    }
}