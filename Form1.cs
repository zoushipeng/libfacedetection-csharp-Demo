using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace 人脸特征服务开源版
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = @"图片文件|*.bmp;*.jpg;*.jpeg;*.png|所有文件|*.*;";

            openFile.Multiselect = false;

            openFile.FileName = "";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = null;
                pictureBox2.Image = null;
                label1.Text = "";

                Image image = Image.FromFile(openFile.FileName);
                Bitmap source = GetResizeBitmap(image);

                pictureBox1.Image = source;

                image.Dispose();

                //TODO检测人脸，提取特征
                if (source != null)
                {
                    DetectPicture(pictureBox1.Image);
                }
            }
        }

        private void DetectPicture(Image imageParam)
        {
            Bitmap bitmap = new Bitmap(imageParam);

            int faceNum = FaceLib.GetInstance().FaceDetect(bitmap, bitmap.Width, bitmap.Height, out List<FaceLibRect> rects);
            if (faceNum > 0)
            {
                Bitmap tmp = new Bitmap(imageParam);
                string info = $"人脸数目{faceNum}\n";
                int i = 0;
                foreach (var rect in rects)
                {
                    DrawRectangleInPicture(tmp, rect.rect, i);
                    info += $"置信度:{rect.confindence}, 角度{rect.angle}, 人脸{i++}\n";
                }

                pictureBox2.Image = tmp;
                label1.Text = info;
            }

        }

        private Bitmap GetResizeBitmap(Image src)
        {
            Bitmap copy = new Bitmap(src);
            int mMax = Math.Max(copy.Width, copy.Height);
            try
            {
                if (mMax > 480)
                {
                    float persent = 480.0f / mMax;
                    return FaceLib.ResizeImage(copy, persent);
                }
            }
            catch (Exception E)
            {
                // 
            }
            
            return copy;
        }

        public static Bitmap DrawRectangleInPicture(Bitmap bmp, Rectangle rectangle, int i)
        {
            if (bmp == null) return null;


            Graphics g = Graphics.FromImage(bmp);
            Brush brush;

            switch (i)
            {
                case 0:
                    brush = new SolidBrush(Color.Blue);
                    break;
                case 1:
                    brush = new SolidBrush(Color.Red);
                    break;
                case 2:
                    brush = new SolidBrush(Color.Yellow);
                    break;
                default:
                    brush = new SolidBrush(Color.Black);
                    break;
            }
            Pen pen = new Pen(brush, 3) {DashStyle = DashStyle.DashDot};

            g.DrawRectangle(pen, rectangle);

            g.Dispose();

            return bmp;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FaceLib.GetInstance().Uninit();
        }
    }
}
