using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasySaveClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // TODO : envoie commande Stop Continu Pause
        private Thread? tSocket;
        private Socket socket1;
        private bool threadWorking = true;
        private List<Item>? itemList;
        private readonly IPEndPoint servEndPoint;
        public MainWindow()
        {
            InitializeComponent();
            servEndPoint = new IPEndPoint(IPAddress.Loopback, 11111);
        }
        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            ToSend(sender);
        }
        private void ButtonContinu_Click(object sender, RoutedEventArgs e)
        {
            ToSend(sender);
        }
        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            ToSend(sender);
        }
        private void ToSend(object sender)
        {
            var toSend = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<ButtonPress>(new ButtonPress()
            {
                Id = (int)((Button)sender).Tag,
                Name = ((Button)sender).Name
            }));
            if (socket1.Connected)
                SendToNetwork(socket1, toSend);
        }
        public void SendToNetwork(Socket client, byte[] toSend)
        {
            try
            {
                client.SendAsync(toSend);
            }
            catch
            {

            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            threadWorking = true;
            socket1 = Connect();
            if (!socket1.Connected)
                return;
            tSocket = new Thread(() =>
            {
                while (threadWorking)
                {
                    itemList = ListenToNetwork(socket1);
                    int i = 0;
                    if (itemList != null)
                    {
                        foreach (var item in itemList)
                        {
                            if (ProgressListView.Items.Count < itemList.Count)
                                this.Dispatcher.Invoke(() => ProgressListView.Items.Add(item));
                            else
                                this.Dispatcher.Invoke(() => ProgressListView.Items[i] = item);
                            i++;
                        }
                    }
                }
            });
            tSocket.Start();
        }
        public Socket Connect()
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sock.Connect(servEndPoint);
                noServLabel.Visibility = Visibility.Hidden;
            }
            catch
            {
                noServLabel.Visibility = Visibility.Visible;
            }
            return sock;
        }
        public List<Item>? ListenToNetwork(Socket client)
        {
            var buffer = new byte[2048];
            try
            {
                client.Receive(buffer);
            }
            catch (Exception e)
            {

            }
            var msg = Encoding.UTF8.GetString(buffer);
            msg = msg.Trim('\0');
            List<Item>? list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<Item>>(msg);
            }
            catch (Exception e)
            {

            }
            return list;
        }
        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            threadWorking = false;
            if (itemList != null)
                itemList.Clear();
            ProgressListView.Items.Clear();
        }
    }
    public class Item()
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Progr { get; set; }
        public string? ProgrStr { get; set; }
        public string? Status { get; set; }
    }
    public class ButtonPress()
    {
        public int Id { set; get; }
        public string? Name { get; set; }
    }
}