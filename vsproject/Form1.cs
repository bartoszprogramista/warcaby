using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace proszedzialaj
{
    public partial class Form1 : Form
    {
        int licznik = 0;
        int razemzielone =12;
        int razemczerwone = 12;
        public static bool polaczono = false;
        const int cellSize = 50;
        bool innyNiebieski = false;
        int turagracza = 1;
        int[,] map = new int[10, 10];
        int prevX;
        int prevY;
        int x;
        int y;
        int biciewturze = 0;
        int multibicie = 0;
        Image czerwonyPion;
        Image zielonyPion;
        Image czerwonaDama;
        Image zielonaDama;
        Image niebieski;
        int obecnyGracz = 99;

        public Form1()
        {
            InitializeComponent();
            this.Text = "Warcaby";
            czerwonyPion = new Bitmap(new Bitmap(@"C:\Users\GamingPC\source\repos\proszedzialaj\img\r.png"), new Size(cellSize - 4, cellSize - 4));
            zielonyPion = new Bitmap(new Bitmap(@"C:\Users\GamingPC\source\repos\proszedzialaj\img\g.png"), new Size(cellSize - 4, cellSize - 4));
            niebieski = new Bitmap(new Bitmap(@"C:\Users\GamingPC\source\repos\proszedzialaj\img\b.png"), new Size(cellSize - 4, cellSize - 4));
            czerwonaDama = new Bitmap(new Bitmap(@"C:\Users\GamingPC\source\repos\proszedzialaj\img\dr.png"), new Size(cellSize - 4, cellSize - 4));
            zielonaDama = new Bitmap(new Bitmap(@"C:\Users\GamingPC\source\repos\proszedzialaj\img\dg.png"), new Size(cellSize - 4, cellSize - 4));
        }

        //usuwa niebieskie pola(są to pola gdzie moze ruszyć się naduszony pion)
        public void czyscNiebieskie()
        {
            for (int i = 1; i < 9; i++)
            {
                for (int j = 1; j < 9; j++)
                {
                    if (map[i, j] == 3)
                    {
                        map[i, j] = 0;
                        //MessageBox.Show("czyscze"+i+" "+j);
                    }
                }
            }
        }
        //na podstawie tablicy następuje aktualizacja plnaszy gry
        public void updateBoard(int[,] map)
        {
            int liczzielone = 0;
            int liczczerwone = 0;
            for (int i = 1; i < 9; i++)
            {
                for (int j = 1; j < 9; j++)
                {
                    Button button = new Button();
                    button.Location = new Point(j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
                    button.Click += new EventHandler(pionClick);
                    if (map[i, j] == 1)
                    {
                        button.BackgroundImage = czerwonyPion;
                        liczczerwone++;
                        if (obecnyGracz == 2)
                        {
                            button.Enabled = false;
                        }
                    }
                    else if (map[i, j] == 2)
                    {
                        button.BackgroundImage = zielonyPion;
                        liczzielone++;
                        if (obecnyGracz == 1)
                        {
                            button.Enabled = false;
                        }
                    }
                    else if (map[i, j] == 3)
                    {
                        button.BackgroundImage = niebieski;
                    }
                    else if (map[i, j] == 4)
                    {
                        button.BackgroundImage = czerwonaDama;
                        liczczerwone++;
                        if (obecnyGracz == 2)
                        {
                            button.Enabled = false;
                        }
                    }
                    else if (map[i, j] == 5)
                    {
                        button.BackgroundImage = zielonaDama;
                        liczzielone++;
                        if (obecnyGracz == 1)
                        {
                            button.Enabled = false;
                        }
                    }
                    else if (map[i, j] == 0)
                    {
                        button.Enabled = false;
                    }
                    if (i % 2 != 0)
                    {
                        if (j % 2 == 0)
                        {
                            button.BackColor = Color.Black;
                        }
                        else
                        {
                            button.BackColor = Color.White;
                        }
                    }
                    if (i % 2 == 0)
                    {
                        if (j % 2 != 0)
                        {
                            button.BackColor = Color.Black;
                        }
                        else
                        {
                            button.BackColor = Color.White;
                        }
                    }
                    panel1.Controls.Add(button);
                }
            }

            label4.Text = Convert.ToString(razemczerwone - liczczerwone);
            label3.Text = Convert.ToString(razemzielone - liczzielone);
            //sprawdzenie który gracz wygrał, czy zbił 12 pionów
            if (razemczerwone-liczczerwone == 12)
            {
                
                label1.Hide();
                label2.Hide();
                label6.Hide();
                label5.Text = "Wygrał zielony";
                turagracza = 3;
            }
            else if (razemzielone-liczzielone == 12)
            {
                label1.Hide();
                label2.Hide();
                label6.Hide();
                label5.Text = "Wygrał czerwony";
                turagracza = 3;
            }
            
        }

        //1 ruch na ture
        public void zmienTure()
        {
           if (turagracza == 1)
            {
                turagracza = 2;
                label6.Text = "Tura gracza: zielony";
            }
            else if (turagracza == 2)
            {
                turagracza = 1;
                label6.Text = "Tura gracza: czerwony";
            }
        }

        //przygotowanie i wysłanie danych do serwera o rozkładzie pionów na planszy
        public void wyslijmap(int[,] xmap)
        {
            string wyslijMap = "";
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    wyslijMap += Convert.ToString(xmap[i, j]);
                }
            }
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(wyslijMap);
            Globals.stream.Write(data, 0, data.Length);
            zmienTure();
        }

        //wyswietlenie możliwych ruchów dla poszczególnych kolorów
        public void sprawdzBiciezielony(int y,int x)
        {
            try
            {
                if (map[y - 1, x + 1] == 1 || map[y - 1, x + 1] == 4)
                {
                    if (map[y - 2, x + 2] == 0)
                    {
                        map[y - 2, x + 2] = 3;
                        if (biciewturze == 1)
                        {
                            multibicie = 1;
                        }
                    }
                }
                if (map[y - 1, x - 1] == 1 || map[y - 1, x - 1] == 4)
                {
                    if (map[y - 2, x - 2] == 0)
                    {
                        map[y - 2, x - 2] = 3;
                        if (biciewturze == 1)
                        {
                            multibicie = 1;
                        }
                    }
                }
                if (map[y + 1, x - 1] == 1 || map[y + 1, x - 1] == 4)
                {
                    if (map[y + 2, x - 2] == 0)
                    {
                        map[y + 2, x - 2] = 3;
                        if (biciewturze == 1)
                        {
                            multibicie = 1;
                        }
                    }
                }
                if (map[y + 1, x + 1] == 1 || map[y + 1, x + 1] == 4)
                {
                    if (map[y + 2, x + 2] == 0)
                    {
                        map[y + 2, x + 2] = 3;
                        if (biciewturze == 1)
                        {
                            multibicie = 1;
                        }
                    }
                }
            }
            catch { }
        }
        public void sprawdzBicieczerwony(int y, int x)
        {
            try
            {
                if (map[y - 1, x + 1] == 2 || map[y - 1, x + 1] == 5)
                {
                    if (map[y - 2, x + 2] == 0)
                    {
                        map[y - 2, x + 2] = 3;
                        if (biciewturze == 1)
                        {
                            multibicie = 1;
                        }
                    }
                }
                if (map[y - 1, x - 1] == 2 || map[y - 1, x - 1] == 5)
                {
                    if (map[y - 2, x - 2] == 0)
                    {
                        map[y - 2, x - 2] = 3;
                        if (biciewturze == 1)
                        {
                            multibicie = 1;
                        }
                    }
                }
                if (map[y + 1, x - 1] == 2 || map[y + 1, x - 1] == 5)
                {
                    if (map[y + 2, x - 2] == 0)
                    {
                        map[y + 2, x - 2] = 3;
                        if (biciewturze == 1)
                        {
                            multibicie = 1;
                        }
                    }
                }
                if (map[y + 1, x + 1] == 2 || map[y + 1, x + 1] == 5)
                {
                    if (map[y + 2, x + 2] == 0)
                    {
                        map[y + 2, x + 2] = 3;
                        if (biciewturze == 1)
                        {
                            multibicie = 1;
                        }
                    }
                }
            }
            catch { }
        }
        
        //po kliknięciu na pion wyświetlają sie możliwe ruchy, po kliknięciu na niebieskie pole(możliwy ruch) następuje update planszy
        public void pionClick(object sender, EventArgs e)
        {
            biciewturze = 0;
            multibicie = 0;
             Button pressedButton = sender as Button;
             prevX = x;
             prevY = y;
             int poprzedniaWartosc = map[prevY, prevX];
             x = pressedButton.Location.X / cellSize;
             y = pressedButton.Location.Y / cellSize;

            if (turagracza == obecnyGracz && map[y,x]!=0)
            {
                //MessageBox.Show("prevX: " + prevX + " prevY: " + prevY+ "mapval: "+poprzedniaWartosc);
                //MessageBox.Show("x: " + x + " y: " + y + "mapValue: " + map[y, x]);

                if ((map[y, x] == 2 && obecnyGracz == 2) || (map[y,x]==5&&obecnyGracz==2))
                {
                    //czyscNiebieskie();
                    if (innyNiebieski)
                    {
                        czyscNiebieskie();
                        innyNiebieski = false;
                    }
                    innyNiebieski = true;
                    //ruch w góre pionem zielonym
                    if (map[y - 1, x - 1] ==0)
                    {
                        map[y - 1, x - 1] = 3;
                    }
                    if (map[y - 1, x + 1] ==0)
                    {
                        map[y - 1, x + 1] = 3;
                    }
                    if (map[y, x] == 5)
                    {
                        if (map[y + 1, x + 1] == 0)
                        {
                            map[y + 1, x + 1] = 3;
                        }
                        if (map[y + 1, x - 1] == 0)
                        {
                            map[y + 1, x - 1] = 3;
                        }
                    }

                    sprawdzBiciezielony(y,x);

                }

                if (map[y, x] == 1 && obecnyGracz == 1 || (map[y, x] == 4 && obecnyGracz == 1))
                {
                    // czyscNiebieskie();
                    if (innyNiebieski)
                    {
                        czyscNiebieskie();
                        innyNiebieski = false;
                    }
                    innyNiebieski = true;
                    //ruch w doł pionem czerwonym
                    if (map[y + 1, x - 1] == 0)
                    {
                        map[y + 1, x - 1] = 3;
                    }
                    if (map[y + 1, x + 1] ==0)
                    {
                        map[y + 1, x + 1] = 3;
                    }
                    if (map[y, x] == 4)
                    {
                        if (map[y - 1, x + 1] == 0)
                        {
                            map[y - 1, x + 1] = 3;
                        }
                        if (map[y - 1, x - 1] == 0)
                        {
                            map[y - 1, x - 1] = 3;
                        }
                    }
                    sprawdzBicieczerwony(y, x);
                }

                if (map[y, x] == 3)
                {
                    if (prevX - x == 2 && prevY - y == -2)
                    {
                        map[y - 1, x + 1] = 0;
                        biciewturze = 1;
                    }else if (prevX - x == -2 && prevY - y == 2)
                    {
                        map[y + 1, x - 1] = 0;
                        biciewturze = 1;
                    }
                    else if (prevX - x == 2 && prevY - y == 2)
                    {
                        map[y + 1, x + 1] = 0;
                        biciewturze = 1;
                    }
                    else if (prevX - x == -2 && prevY - y == -2)
                    {
                        map[y - 1, x - 1] = 0;
                        biciewturze = 1;
                    }

                    if (obecnyGracz == 1)
                    {
                        map[y, x] = poprzedniaWartosc;
                        map[prevY, prevX] = 0;
                        if (y == 8)
                        {
                            map[y, x]= 4;
                        }

                        czyscNiebieskie();
                        if (biciewturze == 1)
                        {
                            sprawdzBicieczerwony(y,x);
                        }
                        if (multibicie == 0)
                        {
                            wyslijmap(map);
                        }
                        
                    }
                    else if (obecnyGracz == 2)
                    {
                        map[y, x] = poprzedniaWartosc;
                        map[prevY, prevX] = 0;                     
                        if (y == 1)
                        {
                            map[y, x] = 5;
                        }
                        czyscNiebieskie();                        
                        if (biciewturze == 1)
                        {
                            sprawdzBiciezielony(y, x);
                        }
                        if (multibicie == 0)
                        {
                            wyslijmap(map);
                        }

                    }
                }
            
                panel1.Controls.Clear();
                updateBoard(map);
            }

        
        
        }

        public void CreateBoard()
        {
            this.Width = 15 * cellSize;
            this.Height = 10 * cellSize;
            //Plnasza gry: 1 - gracz czerwony, 2 - gracz zielony, 3 - mozliwy ruch, 4 - czerwonaDama, 5 - zielonaDama, 9 granica, 0 puste pole
            panel1.Size = new Size(450, 450);
            map = new int[10, 10]
            {
                {9,9,9,9,9,9,9,9,9,9},
                {9,0,1,0,1,0,1,0,1 ,9},
                {9,1,0,1,0,1,0,1,0 ,9},
                {9,0,1,0,1,0,1,0,1 ,9},
                {9,0,0,0,0,0,0,0,0 ,9},
                {9,0,0,0,0,0,0,0,0 ,9},
                {9,2,0,2,0,2,0,2,0 ,9},
                {9,0,2,0,2,0,2,0,2 ,9},
                {9,2,0,2,0,2,0,2,0,9},
                {9,9,9,9,9,9,9,9,9,9}
            };
            //generowanie planszy
            updateBoard(map);
           
        }
        
        public void ruchPrzeciwnika(IAsyncResult ar)
        {
            bool czytaj = true;
            if (ar.IsCompleted)
            {
                var bytesIn = Globals.server.GetStream().EndRead(ar);
                if (bytesIn > 0)
                {
                    var tmp = new byte[bytesIn];
                    Array.Copy(Globals.buffer, 0, tmp, 0, bytesIn);
                    var str = Encoding.ASCII.GetString(tmp);
                    string xd = Convert.ToString(str);
                    BeginInvoke((Action)(() =>
                    {
                        //gdy otrzymana jest wiadomosci q, o końcu gry
                        if (xd[0]=='q')
                        {
                            panel1.Hide();
                            label3.Hide();
                            label4.Hide();
                            label5.Hide();
                            label2.Hide();
                            label6.Hide();
                            label1.Show();
                            label1.Text = "Konic gry. Twoj przeciwnik się rozłączył";
                            label1.Top = 200;
                            label1.Left = 150;
                            czytaj = false;
                        }//w pozostałych przypadkach klient otrzymuje dane na temat planszy od serwera(wysłał je przeciwnik)
                        else
                        {
                            int k = 0;
                            for (int i = 0; i < 10; i++)
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                    try
                                    {
                                        int x = Int32.Parse(Convert.ToString(str[k]));
                                        map[i, j] = x;
                                        k++;
                                    }
                                    catch { }
                                } 
                            }
                            licznik++;
                            if (licznik == 2)
                            {
                                zmienTure();
                                licznik = 0;
                            }
                            panel1.Controls.Clear();
                            updateBoard(map);
                        }
                    }));
                }
                Array.Clear(Globals.buffer, 0, Globals.buffer.Length);

            }
            if (czytaj)
            {
                Globals.server.GetStream().BeginRead(Globals.buffer, 0, Globals.buffer.Length, ruchPrzeciwnika, null);
            }
        }


        public void odbieranieWiadomosci()
        {
            //odbieranie wiadomosci na temat nazwy przeciwnika
            byte[] bytes = new byte[Globals.server.ReceiveBufferSize];
            int bytesRead = Globals.stream.Read(bytes, 0, bytes.Length);
            string response = Encoding.ASCII.GetString(bytes, 0, bytesRead);

            label1.Show();
            label3.Show();
            label4.Show();
            label1.Text = response;

            //odbieranie wiadomosci na temat nazwy koloru gracza
            bytes = new byte[Globals.server.ReceiveBufferSize];
            bytesRead = Globals.stream.Read(bytes, 0, bytes.Length);
            response = Encoding.ASCII.GetString(bytes, 0, bytesRead);
            bool result = response.Equals("c");
            label5.Show();
            label6.Show();
            label2.Show();
            label1.Font = new Font(label1.Font.FontFamily, 15);
            label2.Font = new Font(label1.Font.FontFamily, 15);
            if (result)
            {
                label1.Left = 460;
                label1.Top = 393;
                label2.Top = 100;
                label2.Left = 460;
                obecnyGracz = 1;
                label2.Text = Globals.nickname;
                label5.Text = "Twój kolor: czerwony";

            }
            else
            {
                label1.Left = 460;
                label1.Top = 100;
                label2.Left = 460;
                label2.Top = 393;
                obecnyGracz = 2;
                label2.Text = Globals.nickname;
                label5.Text = "Twój kolor: zielony";
            }
            label6.Text = "Tura gracza: czerwony";
            //utworzenie początkowej planszy i rozpoczęcie odbierania wiadomości od przeciwnika
            CreateBoard();
            Globals.server.GetStream().BeginRead(Globals.buffer, 0, Globals.buffer.Length, ruchPrzeciwnika, null);
        }

        public void wysylanieWiadomosci(String nickname)
        {
            //wysyłanie do servera naszego nicku
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(nickname);
            Globals.stream.Write(data, 0, data.Length);
            textBox1.Hide();
            label1.Hide();
            button1.Hide();
            label5.Hide();
            label2.Hide();
            label3.Hide();
            odbieranieWiadomosci();
        }

        //po nacisnieciu nastepuje próba połączenia z serwerem oraz walidacja nicku
        public void button1_Click_1(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0 && textBox1.Text.Length < 30)
            {
                Globals.server = new TcpClient();
                Globals.nickname = textBox1.Text;
                string serverip = textBox2.Text;
                string port = textBox3.Text;
                
                Connect(serverip,port);
            }
            else
            {
                MessageBox.Show("Wprowadź poprawną nazwę użytkownika (1-30 liter)");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }
        public void Connect(string serverip, string port)
        {
            try
            {
                //próba polączenia z serverem /

                Globals.server = new TcpClient();
                Globals.server.Connect(serverip, Int32.Parse(port));
                textBox2.Hide();
                textBox3.Hide();
                MessageBox.Show("Połaczono z serwerem, czekaj na dobranie przeciwnika");
                Globals.stream = Globals.server.GetStream();
                wysylanieWiadomosci(Globals.nickname);
            }
            catch
            {
                MessageBox.Show("blad");
                textBox2.Show();
                textBox3.Show();
                label1.Show();
                textBox1.Show();
                button1.Show();
            }
        }


    }
    class Globals
    {
        public static string nickname;
        public static Int32 port = 8081;
        public static TcpClient server;
        public static NetworkStream stream;
        public static byte[] buffer = new byte[4096];
    }
}
