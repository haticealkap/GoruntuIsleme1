using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmguCV_Shapes
{
    public partial class Form1 : Form
    {
        int toplamSekilSayisi = 0;
        int secilenRenktekiSekilSayisi = 0;
        int secilenSekildeFarkliRenkSayisi = 0;


        Image<Bgr, byte> rgbImage;
        public Form1()
        {
            InitializeComponent();
        }

        public void DrawContour(Image<Gray, byte> src)
        {
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                //CvInvoke.FindContours(src, contours, null, RetrType.External,
                //                        ChainApproxMethod.ChainApproxSimple);
                CvInvoke.FindContours(src, contours, null, RetrType.External,
                       ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    using (VectorOfPoint approxContour = new VectorOfPoint())
                    {

                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        if (CvInvoke.ContourArea(approxContour, false) > 250) //only consider contours with area greater than 250
                        {
                            if (approxContour.Size == 4) //The contour has 4 vertices.
                            {

                                toplamSekilSayisi++;
                                #region determine if all the angles in the contour are within [80, 100] degree
                                bool isRectangle = true;
                                Point[] pts = approxContour.ToArray();
                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                                for (int j = 0; j < edges.Length; j++)
                                {
                                    double angle = Math.Abs(
                                        edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (angle < 80 || angle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                if (isRectangle)
                                {
                                    #endregion
                                    // Original contour


                                    // CvInvoke.DrawContours(rgbImage, contours, i, new MCvScalar(255, 0, 255, 255), 3);


                                    // approximate contour
                                    CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.02, true);
                                    //Bulunan nesnenin orta noktasını bulmak için
                                    int totalX = 0;
                                    int totalY = 0;
                                    for (int x = 0; x < pts.Length; x++)
                                    {
                                        Point p1 = new Point(pts[x].X, pts[x].Y);
                                        totalX += p1.X;
                                        totalY += p1.Y;
                                    }

                                    string hexCode = getHexCode(rgbImage, totalY / 4, totalX / 4);
                                    if (comboBox1.SelectedItem != null && hexCode == comboBox1.SelectedItem.ToString())
                                    {
                                        //Çizim işlemi yaptırmak için
                                        for (int j = 0; j < pts.Length; j++)
                                        {
                                            Point p1 = new Point(pts[j].X, pts[j].Y);

                                            Point p2;
                                            if (j == pts.Length - 1)
                                                p2 = new Point(pts[0].X, pts[0].Y);
                                            else
                                                p2 = new Point(pts[j + 1].X, pts[j + 1].Y);


                                            CvInvoke.Line(rgbImage, p1, p2, new MCvScalar(255, 0, 0, 0), 3);
                                        }
                                        secilenRenktekiSekilSayisi++;
                                    }
                                    else
                                    {
                                        secilenSekildeFarkliRenkSayisi++;
                                    }
                                }


                            }
                        }


                    }
                }
            }
        }
        string getHexCode(Image<Bgr, byte> src, int x, int i)
        {
            Color myColor = Color.FromArgb(Convert.ToInt32(src[x, i].Red), Convert.ToInt32(src[x, i].Green), Convert.ToInt32(src[x, i].Blue));
            return myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2");
        }

        public List<string> getColors(Image<Bgr, byte> src)
        {
            List<string> colors = new List<string>();

            for (int i = 0; i < src.Width; i++)
            {
                for (int x = 0; x < src.Height; x++)
                {

                    string hex = getHexCode(src, x, i);

                    if (colors.IndexOf(hex) == -1 && hex != "FFFFFF")
                    {
                        colors.Add(hex);
                    }
                }
            }
            return colors;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                string filePath = openFileDialog1.FileName;

                rgbImage = new Image<Bgr, byte>(filePath);

                Image i = Image.FromStream(File.OpenRead(openFileDialog1.FileName));
                pictureBox1.Image = i;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> unprocessedImage = new Image<Bgr, byte>(openFileDialog1.FileName);
            List<string> colors = getColors(unprocessedImage);
            comboBox1.Items.Clear();
            for (int i = 0; i < colors.Count; i++)
            {

                comboBox1.Items.Add(colors[i]);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(openFileDialog1.FileName))
            {
                toplamSekilSayisi = 0;
                secilenRenktekiSekilSayisi = 0;
                secilenSekildeFarkliRenkSayisi = 0;

                Image<Gray, byte> grayImage = new Image<Gray, byte>(openFileDialog1.FileName);

                Image<Gray, byte> CannyImage = grayImage.Clone();
                CvInvoke.Canny(grayImage, CannyImage, 255, 255, 5, true);
                DrawContour(CannyImage);
                rgbImage.Save("processedImage.jpg");
                Image i = Image.FromStream(File.OpenRead("processedImage.jpg"));
                pictureBox1.Image = i;
                label4.Text = toplamSekilSayisi.ToString();
                label6.Text = secilenRenktekiSekilSayisi.ToString();
                label8.Text = secilenSekildeFarkliRenkSayisi.ToString();
                button4.BackColor = (Color)new ColorConverter().ConvertFromString("#" + comboBox1.SelectedItem.ToString());


            }
        }
    }
}
