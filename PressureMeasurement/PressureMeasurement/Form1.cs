using System;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;
using System.IO.Ports;

namespace PressureMeasurement
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //设置窗体和插图填充颜色
            zedGraphControl1.GraphPane.Fill = new Fill(Color.AliceBlue);
            zedGraphControl1.GraphPane.Chart.Fill = new Fill(Color.Black);
            //设置显示信息
            zedGraphControl1.GraphPane.Title.IsVisible = false;
            zedGraphControl1.GraphPane.XAxis.Title.IsVisible = false;
            zedGraphControl1.GraphPane.YAxis.Title.IsVisible = false;
            //zedGraphControl1.GraphPane.Title.Text = "X轴显示";
            //zedGraphControl1.GraphPane.Title.Text = "灰度";

            //设置初始显示与否
            zedGraphControl1.Visible = true;

            //串口号
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
                comboBox1.SelectedIndex = 0;
            }
            //波特率
            /*comboBox2.Items.Add("9600");
            comboBox2.Items.Add("14400");
            comboBox2.SelectedIndex = 0;*/
            string[] Bauds = { "9600" , "1200", "2400", "4800",  "14400", "19200", "28800", "38400", "57600", "115200" };
            foreach (string Baud in Bauds)
            {
                comboBox2.Items.Add(Baud);
                comboBox2.SelectedIndex = 0;
            }
            //数据位
            comboBox3.Items.Add("8");
            comboBox3.SelectedIndex = 0;
            //停止位
            comboBox4.Items.Add("1");
            comboBox4.SelectedIndex = 0;
            //校验位
            comboBox5.Items.Add("无");
            comboBox5.SelectedIndex = 0;
            //接收数据点数
            string[] total ={"100","200","300","400","500","600","700","800","900","1000"};
            foreach (string number in total)
            {
                comboBox6.Items.Add(number);
                comboBox6.SelectedIndex = 0;
            }
            
            SendValue = int.Parse(comboBox6.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        bool _openState =false;
        public static string strPortName = "";
        public static string strBaudRate = "";
        public static string strDataBits = "";
        public static string strStopBits = "";

        /// <summary>
        /// 打开、关闭串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenPort_Click(object sender, EventArgs e)
        {
            if (!_openState)
            {
                _openState = true;
                OpenPort.Text = "关闭串口";
                this.pictureBox1.Image = global::PressureMeasurement.Properties.Resources.btnOk2;

                strPortName = comboBox1.Text;
                strBaudRate = comboBox2.Text;
                strDataBits = comboBox3.Text;
                strStopBits = comboBox4.Text;

                serialPort1.PortName = strPortName;
                serialPort1.BaudRate = int.Parse(strBaudRate);
                serialPort1.DataBits = int.Parse(strDataBits);
                serialPort1.StopBits = (StopBits)int.Parse(strStopBits);
                serialPort1.ReadTimeout = 500;
                //打开
                serialPort1.Open();
                serialPort1.DataReceived +=new SerialDataReceivedEventHandler(DataReceived);
            }
            else
            {
                _openState = false;
                OpenPort.Text = "打开串口";
                this.pictureBox1.Image = global::PressureMeasurement.Properties.Resources.can1;

                serialPort1.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
                serialPort1.Close();
            }
        }

        int SendValue;
        int i;
        int j = 1;
        int k = 1;
        int[] Received = new int[4096];
        byte[] RecieveBuf = new byte[4096];
        /// <summary>
        /// 串口接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DataReceived(object sender, SerialDataReceivedEventArgs e)//接收数据
        {

            for (i = 0; i < 4; i++)
            {
                RecieveBuf[i] = (byte)serialPort1.ReadByte();
            }
            int CC = RecieveBuf[0] + RecieveBuf[1] * 256 + RecieveBuf[2] * 65536;
            if (j <= SendValue)
            {
                //Received[j++] = (CC - 138000) / 5447;//4439
                Received[j++] = CC;
                RealTimeCulve();
                Invoke(new MethodInvoker(delegate()
                {
                    textBox1.Text += Received[j - 1] + " ";
                }));
            }
            if ((j - 1) == SendValue)
            {
                timer1.Enabled = false;
                //Save();
            }
        }

        /// <summary>
        ///实时显示压力数据
        /// </summary>
        public void RealTimeCulve()
        {
            GraphPane myPane1 = zedGraphControl1.GraphPane;
            myPane1.CurveList.Clear();
            myPane1.GraphObjList.Clear();
            PointPairList list1x = new PointPairList();
            for (int h = 0; h < j; h++)
            {
                int x = h;
                int y = Received[h];
                list1x.Add(x, y);
            }
            LineItem myCurve1x = myPane1.AddCurve("", list1x, Color.Red, SymbolType.None);
            myPane1.Title.Text = "压力数据";
            myPane1.YAxis.Title.Text = "g";
            myPane1.XAxis.Title.IsVisible = false;
            //myCurve1x.Symbol.Fill = new Fill(Color.White);
            myPane1.YAxis.Scale.Align = AlignP.Inside;
            myPane1.YAxis.Scale.FontSpec.FontColor = Color.Black;
            myPane1.YAxis.MajorGrid.IsZeroLine = false;
            myPane1.YAxis.Scale.Align = AlignP.Inside;
            //myPane1.YAxis.Scale.Min = 0;
            //myPane1.YAxis.Scale.Max = 3100;//3800
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        char[] SendBuf = new char[1];
        private void timer1_Tick(object sender, EventArgs e)
        {
            SendBuf[0] = (char)1;
            serialPort1.WriteTimeout = 100;
            serialPort1.Write(SendBuf, 0, 1);
            if (k == SendValue)
            {
                timer1.Enabled = false;
            }
            k++;
        }

        /// <summary>
        /// 主运行键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            j = 1;
            textBox1.Clear();
            if (serialPort1.IsOpen)
            {
                textBox1.Clear();
                timer1.Start();
            }
            else
            {
                MessageBox.Show("串口没有打开，请打开串口!");
            }
        }

        /// <summary>
        /// 获取设置的接收点数值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendValue = int.Parse(comboBox6.Text);
            Console.WriteLine(SendValue);
        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
