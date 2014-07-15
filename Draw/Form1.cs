using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ZedGraph;

namespace Draw
{
    public partial class Form1 : Form
    {
        private const int LengthOfSize = 256;
        private const int WidthOfSize = 256;
        private const int ImageSize = LengthOfSize * WidthOfSize;
        private static int _a1, _a2, _a3, _b1, _b2, _b3, _lx, _ly;
        private static int _a, _b, _n;
        private static int _n1, _n2, _n3;
        private static float _th;
        private static byte[] _bytes1 = new byte[ImageSize];
        private static readonly byte[] HighBytes=new byte[ImageSize];
        private static readonly byte[] LowBytes=new byte[ImageSize];
        private static readonly byte[] HighBytes1 = new byte[ImageSize];
        private static readonly byte[] LowBytes1 = new byte[ImageSize];
        private static byte[] _bytes2 = new byte[ImageSize];
        private static readonly byte[] HighBytes2 = new byte[ImageSize];
        private static readonly byte[] LowBytes2 = new byte[ImageSize];
        private static byte[] _bytes3 = new byte[ImageSize];
        private static readonly byte[] HighBytes3 = new byte[ImageSize];
        private static readonly byte[] LowBytes3 = new byte[ImageSize];
        private static readonly float[] Floats = new float[ImageSize];
        private static readonly float[] Floats1 = new float[ImageSize];
        private static readonly float[] Floats2 = new float[ImageSize];
        private static readonly float[] Floats3 = new float[ImageSize];

        public Form1()
        {
            InitializeComponent();
            //设置窗体和插图填充颜色
            zedGraphControl1.GraphPane.Fill = new Fill(Color.AliceBlue);
            zedGraphControl1.GraphPane.Chart.Fill = new Fill(Color.Black);
            zedGraphControl2.GraphPane.Fill = new Fill(Color.AliceBlue);
            zedGraphControl2.GraphPane.Chart.Fill = new Fill(Color.Black);
            //设置显示信息
            zedGraphControl1.GraphPane.Title.IsVisible = false;
            zedGraphControl1.GraphPane.XAxis.Title.IsVisible = false;
            zedGraphControl1.GraphPane.YAxis.Title.IsVisible = false;
            //zedGraphControl1.GraphPane.Title.Text = "X轴显示";
            //zedGraphControl1.GraphPane.Title.Text = "灰度";

            zedGraphControl2.GraphPane.Title.IsVisible = false;
            zedGraphControl2.GraphPane.XAxis.Title.IsVisible = false;
            zedGraphControl2.GraphPane.YAxis.Title.IsVisible = false;

            //设置初始显示与否
            zedGraphControl1.Visible = false;
            zedGraphControl2.Visible = false;

            textBox1.Text = (trackBar1.Value/(float) 100).ToString();
            textBox2.Text = (trackBar2.Value/(float) 100).ToString();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private static Bitmap ToGrayBitmap(byte[] rawValues, int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);
            int stride = bmpData.Stride;
            int offset = stride - width;
            IntPtr iptr = bmpData.Scan0;
            int scanBytes = stride * height;
            int posScan = 0, posReal = 0;
            var pixelValues = new byte[scanBytes];
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    pixelValues[posScan++] = rawValues[posReal++];
                }
                posScan += offset;
            }
            Marshal.Copy(pixelValues, 0, iptr, scanBytes);
            bmp.UnlockBits(bmpData);

            ColorPalette tempPalette;
            using (var tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                tempPalette = tempBmp.Palette;
            }
            //int i1 = 192, i2 = 128, i3 = 64;
            for (int i = 0; i < 256; i++)
            {
                //tempPalette.Entries[i] = Color.FromArgb(i, i, i);
                if (i >= 192 && i < 256)
                    tempPalette.Entries[i] = Color.FromArgb(i, 0, 0);
                else if (i >= 128 && i < 192)
                    tempPalette.Entries[i] = Color.FromArgb(0, i + 64, 0);
                else if (i >= 64 && i < 128)
                    //tempPalette.Entries[i] = Color.FromArgb(i, 319-i, 0);
                    tempPalette.Entries[i] = Color.FromArgb(i + 128, i + 128, 0);
                    //else
                    //if (i>=64&&i<128)
                    //tempPalette.Entries[i] = Color.FromArgb(i, 192-i, 0);
                else
                    tempPalette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bmp.Palette = tempPalette;
            return bmp;
        }

        #region 阈值处理模块

        /// <summary>
        ///     Threshold 阈值设置处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void thresholdToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void highCutoffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null || pictureBox2.Image != null || pictureBox3.Image != null)
            {
                textBox2.Visible = false;
                trackBar2.Visible = false;
                textBox1.Show();
                trackBar1.Show();
                _th = (trackBar1.Value / (float)100);
                

                /*textBox1.Location = new Point(23, b1 + 50);
                    trackBar1.Location = new Point(textBox1.Width + 23, b1 + 50);
                    trackBar1.Size = new Size(2 * a1, 45);*/

                /*if (pictureBox1.Image != null && pictureBox2.Image != null && pictureBox3.Image != null)
                {
                    textBox1.Location = new System.Drawing.Point(5, b1 + 50);
                    trackBar1.Location = new System.Drawing.Point((byte)(0.5 * a1 + 5), b1 + 50);
                    trackBar1.Size = new System.Drawing.Size(2 * a1, 45);
                }*/

                /*_th = (trackBar1.Value/(float) 100);
                Console.WriteLine(_th);
                textBox1.Text = _th.ToString();
                if (pictureBox1.Image != null)
                {
                    for (int i = 0; i < _n1; i++)
                    {
                        if (_floats1[i] <= _th)
                        {
                            //HighBytes1[i] = (byte)(floats1[i] * 255);
                            HighBytes1[i] = (byte) (_floats1[i]*1/_th*255);
                        }
                        else
                            HighBytes1[i] = 255;
                    }
                    Bitmap bmp1 = ToGrayBitmap(HighBytes1, A1, B1);
                    pictureBox1.Image = bmp1;
                    Bytes2 = HighBytes2;
                }
                if (pictureBox2.Image != null)
                {
                    for (int i = 0; i < _n2; i++)
                    {
                        if (_floats2[i] <= _th)
                        {
                            HighBytes2[i] = (byte) (_floats2[i]*255);
                        }
                        else
                            HighBytes2[i] = 255;
                    }
                    Bitmap bmp2 = ToGrayBitmap(HighBytes2, A2, B2);
                    pictureBox2.Image = bmp2;
                    Bytes2 = HighBytes2;
                }
                if (pictureBox3.Image != null)
                {
                    for (int i = 0; i < _n3; i++)
                    {
                        if (_floats3[i] <= _th)
                        {
                            HighBytes3[i] = (byte) (_floats3[i]*255);
                        }
                        else
                            HighBytes3[i] = 255;
                    }
                    Bitmap bmp3 = ToGrayBitmap(HighBytes3, A3, B3);
                    pictureBox3.Image = bmp3;
                    Bytes3 = HighBytes3;
                }
                _frm2.function();*/
            }
            else
            {
                MessageBox.Show("请先打开文件！");
            }
        }

        private void lowCutoffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null || pictureBox2.Image != null || pictureBox3.Image != null)
            {
                textBox1.Visible = false;
                trackBar1.Visible = false;
                textBox2.Show();
                trackBar2.Show();

                /*if (pictureBox1.Image != null && pictureBox2.Image != null && pictureBox3.Image != null)
                {
                    textBox2.Location = new System.Drawing.Point(5, b1 + 50);
                    trackBar2.Location = new System.Drawing.Point((byte)(0.5 * a1 + 5), b1 + 50);
                    trackBar2.Size = new System.Drawing.Size(2 * a1, 45);
                }*/

                /*textBox2.Location = new Point(23, b1 + 50);
                trackBar2.Location = new Point(textBox1.Width + 23, b1 + 50);
                trackBar2.Size = new Size(2 * a1, 45);*/

                /*_th = (trackBar2.Value/(float) 100);
                Console.WriteLine(_th);
                textBox2.Text = _th.ToString();
                if (pictureBox1.Image != null)
                {
                    for (int i = 0; i < _n1; i++)
                    {
                        if (_floats1[i] >= _th)
                        {
                            LowBytes1[i] = (byte) (_floats1[i]*255);
                        }
                        else
                            LowBytes1[i] = 0;
                    }
                    Bitmap bmp1 = ToGrayBitmap(LowBytes1, A1, B1);
                    pictureBox1.Image = bmp1;
                    Bytes1 = LowBytes1;
                }
                if (pictureBox2.Image != null)
                {
                    for (int i = 0; i < _n2; i++)
                    {
                        if (_floats2[i] >= _th)
                        {
                            LowBytes2[i] = (byte) (_floats2[i]*255);
                        }
                        else
                            LowBytes2[i] = 0;
                    }
                    Bitmap bmp2 = ToGrayBitmap(LowBytes2, A2, B2);
                    pictureBox2.Image = bmp2;
                    Bytes2 = LowBytes2;
                }
                if (pictureBox3.Image != null)
                {
                    for (int i = 0; i < _n3; i++)
                    {
                        if (_floats3[i] >= _th)
                        {
                            LowBytes3[i] = (byte) (_floats3[i]*255);
                        }
                        else
                            LowBytes3[i] = 0;
                    }
                    Bitmap bmp3 = ToGrayBitmap(LowBytes3, A3, B3);
                    pictureBox3.Image = bmp3;
                    Bytes3 = LowBytes3;
                }
                frm.function();*/
            }
            else
            {
                MessageBox.Show("请先打开文件！");
            }
        }

        /// <summary>
        /// 高位截断阈值处理函数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="n"></param>
        private void HighThresholdProcess(float[] data,int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (data[i] <= _th)
                {
                    HighBytes[i] = (byte)(data[i] * 1 / _th * 255);
                }
                else
                    HighBytes[i] = 255;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            _th = (trackBar1.Value/(float) 100);
            textBox1.Text = _th.ToString();
            if (pictureBox1.Image != null)
            {
                HighThresholdProcess(Floats1,_n1);
                HighBytes.CopyTo(HighBytes1,0);
                Bitmap bmp1 = ToGrayBitmap(HighBytes1, _a1, _b1);
                pictureBox1.Image = bmp1;
                _bytes1 = HighBytes1;
            }
            if (pictureBox2.Image != null)
            {
                HighThresholdProcess(Floats2, _n1);
                HighBytes.CopyTo(HighBytes2, 0);
                Bitmap bmp2 = ToGrayBitmap(HighBytes2, _a2, _b2);
                pictureBox2.Image = bmp2;
                _bytes2 = HighBytes2;
            }
            if (pictureBox3.Image != null)
            {
                HighThresholdProcess(Floats3, _n1);
                HighBytes.CopyTo(HighBytes3, 0);
                Bitmap bmp3 = ToGrayBitmap(HighBytes3, _a3, _b3);
                pictureBox3.Image = bmp3;
                _bytes3 = HighBytes3;
            }
            FPlot();
        }

        /// <summary>
        /// 低位截断阈值处理函数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="n"></param>
        private void LowThreaholdProcess(float[] data, int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (data[i] >= _th)
                {
                    LowBytes[i] = (byte)(data[i] * 255);
                }
                else
                    LowBytes[i] = 0;
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            _th = (trackBar2.Value/(float) 100);
            textBox2.Text = _th.ToString();
            if (pictureBox1.Image != null)
            {
                LowThreaholdProcess(Floats1,_n1);
                LowBytes.CopyTo(LowBytes1,0);
                Bitmap bmp1 = ToGrayBitmap(LowBytes1, _a1, _b1);
                pictureBox1.Image = bmp1;
                _bytes1 = LowBytes1;
            }
            if (pictureBox2.Image != null)
            {
                LowThreaholdProcess(Floats2, _n2);
                LowBytes.CopyTo(LowBytes2, 0);
                Bitmap bmp2 = ToGrayBitmap(LowBytes2, _a2, _b2);
                pictureBox2.Image = bmp2;
                _bytes2 = LowBytes2;
            }
            if (pictureBox3.Image != null)
            {
                LowThreaholdProcess(Floats3, _n3);
                LowBytes.CopyTo(LowBytes3, 0);
                Bitmap bmp3 = ToGrayBitmap(LowBytes3, _a3, _b3);
                pictureBox3.Image = bmp3;
                _bytes3 = LowBytes3;
            }
            FPlot();
        }

        #endregion

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {
        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {
        }

        private void zedGraphControl2_Load(object sender, EventArgs e)
        {
        }

        #region 作图

        private void FPlot()
        {
            zedGraphControl1.Visible = true;
            zedGraphControl2.Visible = true;
            GraphPane myPane1 = zedGraphControl1.GraphPane;
            GraphPane myPane2 = zedGraphControl2.GraphPane;
            myPane1.CurveList.Clear();
            myPane1.GraphObjList.Clear();
            myPane2.CurveList.Clear();
            myPane2.GraphObjList.Clear();

            var list1X = new PointPairList();
            var list1Y = new PointPairList();
            var list2X = new PointPairList();
            var list2Y = new PointPairList();
            var list3X = new PointPairList();
            var list3Y = new PointPairList();

            for (int i = 0; i < _a1; i++)
            {
                var x = (byte) i;
                byte y = _bytes1[_ly*_a1 + i];
                list1X.Add(x, y);
            }
            for (int i = 0; i < _b1; i++)
            {
                var x = (byte) i;
                byte y = _bytes1[_lx + _a1*i];
                list1Y.Add(x, y);
            }
            LineItem myCurve1X = myPane1.AddCurve("", list1X, Color.Red, SymbolType.None);
            LineItem myCurve1Y = myPane2.AddCurve("", list1Y, Color.Red, SymbolType.None);

            for (int i = 0; i < _a2; i++)
            {
                var x = (byte) i;
                byte y = _bytes2[_ly*_a2 + i];
                list2X.Add(x, y);
            }
            for (int i = 0; i < _b2; i++)
            {
                var x = (byte) i;
                byte y = _bytes2[_lx + _a2*i];
                list2Y.Add(x, y);
            }
            LineItem myCurve2X = myPane1.AddCurve("", list2X, Color.Green, SymbolType.None);
            LineItem myCurve2Y = myPane2.AddCurve("", list2Y, Color.Green, SymbolType.None);

            for (int i = 0; i < _a3; i++)
            {
                var x = (byte) i;
                byte y = _bytes3[_ly*_a3 + i];
                list3X.Add(x, y);
            }
            for (int i = 0; i < _b3; i++)
            {
                var x = (byte) i;
                byte y = _bytes3[_lx + _a3*i];
                list3Y.Add(x, y);
            }
            LineItem myCurve3X = myPane1.AddCurve("", list3X, Color.Blue, SymbolType.None);
            LineItem myCurve3Y = myPane2.AddCurve("", list3Y, Color.Blue, SymbolType.None);

            //myCurve1X.Symbol.Fill = new Fill(Color.White);
            myPane1.YAxis.Scale.Align = AlignP.Inside;
            myPane1.YAxis.Scale.FontSpec.FontColor = Color.Black;
            myPane1.YAxis.MajorGrid.IsZeroLine = false;
            myPane1.YAxis.Scale.Align = AlignP.Inside;
            myPane1.YAxis.Scale.Min = 0;
            myPane1.YAxis.Scale.Max = 260;
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            myCurve1X.IsSelectable = true;

            //myCurve1Y.Symbol.Fill = new Fill(Color.White);
            myPane2.YAxis.Scale.Align = AlignP.Inside;
            myPane2.YAxis.Scale.FontSpec.FontColor = Color.Black;
            myPane2.YAxis.MajorGrid.IsZeroLine = false;
            myPane2.YAxis.Scale.Align = AlignP.Inside;
            myPane2.YAxis.Scale.Min = 0;
            myPane2.YAxis.Scale.Max = 260;
            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();
        }

        #endregion

        #region 控件及事件

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        #endregion

        #region 打开二进制文件

        private void openImagingFile()
        {
            var data = new OpenFileDialog();
            
            try
            {
                if (data.ShowDialog() == DialogResult.OK)
                {
                    var fs = new FileStream(data.FileName, FileMode.OpenOrCreate);
                    var s = new BinaryReader(fs);
                    _a = (byte)s.ReadSingle();
                    _b = (byte)s.ReadSingle();
                    _n = _a * _b;
                    Console.WriteLine(_a + " " + _b + " " + _n);
                    if (_n <= ImageSize)
                    {
                        for (int i = 0; i < _n; i++)
                        {
                            Floats[i] = s.ReadSingle();
                        }
                    }
                    s.Close();
                    fs.Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("二进制文件格式错误！");
                throw;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void data1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openImagingFile();
            if ((_a <= LengthOfSize && _b <= WidthOfSize && _n <= ImageSize) && (_a != 0 && _b != 0 && _n != 0))
            {
                _a1 = _a;
                _b1 = _b;
                _n1 = _a1*_b1;
                Floats.CopyTo(Floats1, 0);
                float ma = Floats1.Max();
                float mi = Floats1.Min();
                for (int i = 0; i < _n1; i++)
                {
                    Floats1[i] = (Floats1[i] - mi)/(ma - mi);
                }
                for (int i = 0; i < _n1; i++)
                {
                    _bytes1[i] = (byte) (Int32) (Floats1[i]*255);
                }
                Bitmap bmp = ToGrayBitmap(_bytes1, _a1, _b1);
                pictureBox1.Visible = true;
                pictureBox1.Image = bmp;
                pictureBox1.Height = bmp.Height;
                pictureBox1.Width = bmp.Width;
                //pictureBox1.Cursor = cross;
            }
            else
            {
                MessageBox.Show("二进制文件格式错误！");
            }
        }

        private void data2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openImagingFile();
            if ((_a <= LengthOfSize && _b <= WidthOfSize && _n <= ImageSize) && (_a != 0 && _b != 0 && _n != 0))
            {
                _a2 = _a;
                _b2 = _b;
                _n2 = _a2 * _b2;
                Floats.CopyTo(Floats2, 0);
                float ma = Floats2.Max();
                float mi = Floats2.Min();
                for (int i = 0; i < _n2; i++)
                {
                    Floats2[i] = (Floats2[i] - mi) / (ma - mi);
                }
                for (int i = 0; i < _n2; i++)
                {
                    _bytes2[i] = (byte)(Int32)(Floats2[i] * 255);
                }

                Bitmap bmp = ToGrayBitmap(_bytes2, _a2, _b2);
                pictureBox2.Visible = true;
                pictureBox2.Image = bmp;
                pictureBox2.Height = bmp.Height;
                pictureBox2.Width = bmp.Width;
            }
            else
            {
                MessageBox.Show("二进制文件格式错误！");
            }
            
        }

        private void data3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openImagingFile();
            if ((_a <= LengthOfSize && _b <= WidthOfSize && _n <= ImageSize) && (_a != 0 && _b != 0 && _n != 0))
            {
                _a3 = _a;
                _b3 = _b;
                _n3 = _a3 * _b3;
                Floats.CopyTo(Floats3, 0);
                float ma = Floats3.Max();
                float mi = Floats3.Min();
                for (int i = 0; i < _n3; i++)
                {
                    Floats3[i] = (Floats3[i] - mi) / (ma - mi);
                }
                for (int i = 0; i < _n3; i++)
                {
                    _bytes3[i] = (byte)(Int32)(Floats3[i] * 255);
                }
                Bitmap bmp = ToGrayBitmap(_bytes3, _a3, _b3);
                pictureBox3.Visible = true;
                pictureBox3.Image = bmp;
                pictureBox3.Height = bmp.Height;
                pictureBox3.Width = bmp.Width;
            }
            else
            {
                MessageBox.Show("二进制文件格式错误！");
            }
            
        }

        #endregion

        #region 保存bitmap

        private void SaveFile()
        {
            var data = new SaveFileDialog();
            data.Filter = "bmp文件(*.bmp)|*.bmp|jpg文件(*.jpg)|*.jpg|tiff文件(*.tiff)|*.tiff";
            if (data.ShowDialog() == DialogResult.OK)
            {
                string fileName = data.FileName;
                if (!String.IsNullOrEmpty(fileName))
                {
                    string fileExtName = fileName.Substring(fileName.LastIndexOf(".") + 1);
                    if (fileExtName != "")
                    {
                        switch (fileExtName)
                        {
                            case "bmp":
                                pictureBox1.Image.Save(data.FileName, ImageFormat.Bmp);
                                break;
                            case "jpg":
                                pictureBox1.Image.Save(data.FileName, ImageFormat.Jpeg);
                                break;
                            case "tiff":
                                pictureBox1.Image.Save(data.FileName, ImageFormat.Tiff);
                                break;
                            default:
                                MessageBox.Show("只能存取为: jpg,bmp,gif 格式");
                                break;
                        }
                    }
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void picture1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
               SaveFile();
            }
        }

        private void picture2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                SaveFile();
            }
        }

        private void picture3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image != null)
            {
                SaveFile();
            }
        }

        #endregion

        #region 获取坐标点

        /// <summary>
        ///     获取坐标点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _lx = e.X;
            _ly = e.Y;
            string str = " ( " + _lx + " , " + _ly + " ) ";
            toolStripStatusLabel2.Text = str;
            FPlot();
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            _lx = e.X;
            _ly = e.Y;
            string str = " ( " + _lx + " , " + _ly + " ) ";
            toolStripStatusLabel2.Text = str;
            FPlot();
        }

        private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        {
            _lx = e.X;
            _ly = e.Y;
            string str = " ( " + _lx + " , " + _ly + " ) ";
            toolStripStatusLabel2.Text = str;
            FPlot();
        }

        #endregion

    }
}