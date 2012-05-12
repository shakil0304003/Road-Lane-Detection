using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace laneDetection_me2
{
    public partial class Form1 : Form
    {
        private int[][] A;//INDIVIDUAL REGIONS ARE TRACED BY INDIVIDUAL NUMBER
        private int[] B;//HOLDS THE NUMBER OF PIXELS INDIVIDUAL REGION HAVE
        private int count;
        private Bitmap sak;
        private Color removeObjectColor = Color.Red;
        private int maxLaneBlockSize = 1200;
        private int laneRGVAvgMin = 170;
        private int colorDif = 7;
        private int maxAllowableWidth;
        private int KernelWeight;
        private int KernelSize = 5;
        private double Sigma = 1;
        private int[,] GaussianKernel;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
            pictureBox2.Image = new Bitmap(openFileDialog1.FileName);
        }

        private Bitmap big_connect(Bitmap b)
        {
            int i, j, h, w;
            h = b.Height;
            w = b.Width;
            A = new int[w][];
            maxAllowableWidth = w / 18;
            sak = new Bitmap(b);

            for (i = 0; i < w; i++)
            {
                A[i] = new int[h];
                for (j = 0; j < h; j++)
                    A[i][j] = 0;
            }

            B = new int[(h * w) / 4];
            count = 1;

            //CALL BFS
            for (i = 0; i < w; i++)
                for (j = 0; j < h; j++)
                    if (!(b.GetPixel(i, j).R == removeObjectColor.R && b.GetPixel(i, j).G == removeObjectColor.G
                        && b.GetPixel(i, j).B == removeObjectColor.B) && A[i][j] == 0)
                    {
                        continuous_check(i, j);
                    }

            int max = 0, po = -1;

            //FIND THE REGION THAT HAVE MAXIMUM NUMBER OF PIXEL---THE REGION IS ROAD
            for (i = 1; i < count; i++)
                if (B[i] > max)
                {
                    max = B[i];
                    po = i;
                }

            int p, q;

            //ROAD IS MARKED BY RED COLOR
            for (i = 1; i < w - 1; i++)
                for (j = 1; j < h - 1; j++)
                    if (!(b.GetPixel(i, j).R == removeObjectColor.R && b.GetPixel(i, j).G == removeObjectColor.G
                        && b.GetPixel(i, j).B == removeObjectColor.B)
                        && A[i][j] == po)
                    {
                        b.SetPixel(i, j, removeObjectColor);
                        A[i][j] = 0;
                    }

            for (j = 0; j < h; j++)
            {
                //MARKED THE LEFT SIDE OF THE ROAD BY RED COLOR
                for (i = 0; i < w; i++)
                {
                    if (b.GetPixel(i, j).R == removeObjectColor.R && b.GetPixel(i, j).G == removeObjectColor.G
                        && b.GetPixel(i, j).B == removeObjectColor.B)
                        break;
                    else
                    {
                        b.SetPixel(i, j, removeObjectColor);
                        A[i][j] = 0;
                    }
                }

                //MARKED THE RIGHT SIDE OF THE ROAD BY RED COLOR
                for (i = w - 1; i >= 0; i--)
                {
                    if (b.GetPixel(i, j).R == removeObjectColor.R && b.GetPixel(i, j).G == removeObjectColor.G
                        && b.GetPixel(i, j).B == removeObjectColor.B)
                        break;
                    else
                    {
                        b.SetPixel(i, j, removeObjectColor);
                        A[i][j] = 0;
                    }
                }
            }

            //ANY OTHER BIG REGION THAT HAVE LARGE NUMBER OF PIXELS ARE MARKED BY RED
            for (i = 1; i < w - 1; i++)
                for (j = 1; j < h - 1; j++)
                    if (!(b.GetPixel(i, j).R == removeObjectColor.R && b.GetPixel(i, j).G == removeObjectColor.G
                        && b.GetPixel(i, j).B == removeObjectColor.B)
                        && (A[i][j] != po && B[A[i][j]] > maxLaneBlockSize))
                    {
                        b.SetPixel(i, j, removeObjectColor);
                        A[i][j] = 0;
                    }
            return b;
        }

        private void continuous_check(int h1, int h2)
        {
            B[count] = 1;
            int w, h, n, m;
            w = sak.Width;
            h = sak.Height;
            A[h1][h2] = count;
            int[] x;
            int[] y;
            x = new int[w * h];
            y = new int[w * h];
            x[0] = h1;
            y[0] = h2;
            n = 1;
            m = 0;
            int r, g, b;
            int p, q;
            while (n != m)
            {
                r = sak.GetPixel(x[m], y[m]).R;
                g = sak.GetPixel(x[m], y[m]).G;
                b = sak.GetPixel(x[m], y[m]).B;

                //1
                p = x[m] + 1;
                q = y[m];
                if (0 <= p && p < w && 0 <= q && q < h)
                    if (!(sak.GetPixel(p, q).R == removeObjectColor.R && sak.GetPixel(p, q).G == removeObjectColor.G
                        && sak.GetPixel(p, q).B == removeObjectColor.B) && A[p][q] == 0)
                    {
                        if (Math.Abs(r - sak.GetPixel(p, q).R) < colorDif && Math.Abs(g - sak.GetPixel(p, q).G) < colorDif && Math.Abs(b - sak.GetPixel(p, q).B) < colorDif)
                        {
                            A[p][q] = count;
                            B[count]++;
                            x[n] = p;
                            y[n] = q;
                            n++;
                        }
                    }

                //2
                p = x[m];
                q = y[m] + 1;
                if (0 <= p && p < w && 0 <= q && q < h)
                    if (!(sak.GetPixel(p, q).R == removeObjectColor.R && sak.GetPixel(p, q).G == removeObjectColor.G
                        && sak.GetPixel(p, q).B == removeObjectColor.B) && A[p][q] == 0)
                    {
                        if (Math.Abs(r - sak.GetPixel(p, q).R) < colorDif && Math.Abs(g - sak.GetPixel(p, q).G) < colorDif && Math.Abs(b - sak.GetPixel(p, q).B) < colorDif)
                        {
                            A[p][q] = count;
                            B[count]++;
                            x[n] = p;
                            y[n] = q;
                            n++;
                        }
                    }

                //3
                p = x[m] - 1;
                q = y[m];
                if (0 <= p && p < w && 0 <= q && q < h)
                    if (!(sak.GetPixel(p, q).R == removeObjectColor.R && sak.GetPixel(p, q).G == removeObjectColor.G
                        && sak.GetPixel(p, q).B == removeObjectColor.B) && A[p][q] == 0)
                    {
                        if (Math.Abs(r - sak.GetPixel(p, q).R) < colorDif && Math.Abs(g - sak.GetPixel(p, q).G) < colorDif && Math.Abs(b - sak.GetPixel(p, q).B) < colorDif)
                        {
                            A[p][q] = count;
                            B[count]++;
                            x[n] = p;
                            y[n] = q;
                            n++;
                        }
                    }

                //4
                p = x[m];
                q = y[m] - 1;
                if (0 <= p && p < w && 0 <= q && q < h)
                    if (!(sak.GetPixel(p, q).R == removeObjectColor.R && sak.GetPixel(p, q).G == removeObjectColor.G
                        && sak.GetPixel(p, q).B == removeObjectColor.B) && A[p][q] == 0)
                    {
                        if (Math.Abs(r - sak.GetPixel(p, q).R) < colorDif && Math.Abs(g - sak.GetPixel(p, q).G) < colorDif && Math.Abs(b - sak.GetPixel(p, q).B) < colorDif)
                        {
                            A[p][q] = count;
                            B[count]++;
                            x[n] = p;
                            y[n] = q;
                            n++;
                        }
                    }
                //5
                p = x[m] + 1;
                q = y[m] + 1;
                if (0 <= p && p < w && 0 <= q && q < h)
                    if (!(sak.GetPixel(p, q).R == removeObjectColor.R && sak.GetPixel(p, q).G == removeObjectColor.G
                        && sak.GetPixel(p, q).B == removeObjectColor.B) && A[p][q] == 0)
                    {
                        if (Math.Abs(r - sak.GetPixel(p, q).R) < colorDif && Math.Abs(g - sak.GetPixel(p, q).G) < colorDif && Math.Abs(b - sak.GetPixel(p, q).B) < colorDif)
                        {
                            A[p][q] = count;
                            B[count]++;
                            x[n] = p;
                            y[n] = q;
                            n++;
                        }
                    }

                //6
                p = x[m] + 1;
                q = y[m] - 1;
                if (0 <= p && p < w && 0 <= q && q < h)
                    if (!(sak.GetPixel(p, q).R == removeObjectColor.R && sak.GetPixel(p, q).G == removeObjectColor.G
                        && sak.GetPixel(p, q).B == removeObjectColor.B) && A[p][q] == 0)
                    {
                        if (Math.Abs(r - sak.GetPixel(p, q).R) < colorDif && Math.Abs(g - sak.GetPixel(p, q).G) < colorDif && Math.Abs(b - sak.GetPixel(p, q).B) < colorDif)
                        {
                            A[p][q] = count;
                            B[count]++;
                            x[n] = p;
                            y[n] = q;
                            n++;
                        }
                    }

                //7
                p = x[m] - 1;
                q = y[m] + 1;
                if (0 <= p && p < w && 0 <= q && q < h)
                    if (!(sak.GetPixel(p, q).R == removeObjectColor.R && sak.GetPixel(p, q).G == removeObjectColor.G
                        && sak.GetPixel(p, q).B == removeObjectColor.B) && A[p][q] == 0)
                    {
                        if (Math.Abs(r - sak.GetPixel(p, q).R) < colorDif && Math.Abs(g - sak.GetPixel(p, q).G) < colorDif && Math.Abs(b - sak.GetPixel(p, q).B) < colorDif)
                        {
                            A[p][q] = count;
                            B[count]++;
                            x[n] = p;
                            y[n] = q;
                            n++;
                        }
                    }

                //8
                p = x[m] - 1;
                q = y[m] - 1;
                if (0 <= p && p < w && 0 <= q && q < h)
                    if (!(sak.GetPixel(p, q).R == removeObjectColor.R && sak.GetPixel(p, q).G == removeObjectColor.G
                        && sak.GetPixel(p, q).B == removeObjectColor.B) && A[p][q] == 0)
                    {
                        if (Math.Abs(r - sak.GetPixel(p, q).R) < colorDif && Math.Abs(g - sak.GetPixel(p, q).G) < colorDif && Math.Abs(b - sak.GetPixel(p, q).B) < colorDif)
                        {
                            A[p][q] = count;
                            B[count]++;
                            x[n] = p;
                            y[n] = q;
                            n++;
                        }
                    }
                m++;
            }
            count++;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = big_connect(new Bitmap(pictureBox2.Image));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap btp = new Bitmap(pictureBox2.Image);
            int w = btp.Width;
            int h = btp.Height;
            int i = 0, j = 0;
            int[] regionWidth = new int[count];
            int c = 0;

            for(i = 0; i < count; i++)
                regionWidth[i]=0;

            //FIND OUT THE WIDTH OF EACH REGION
            for (j = 1; j < h - 1; j++)
            {
                c = 1;
                for (i = 1; i < w - 1; i++)
                {
                    if (A[i][j] != 0)
                    {
                        if (A[i][j] == A[i - 1][j])
                            c++;
                        else if (A[i][j] != A[i - 1][j])
                        {
                            if (regionWidth[A[i - 1][j]] == 0 || regionWidth[A[i - 1][j]] < c)
                            {
                                regionWidth[A[i - 1][j]] = c;
                                c = 1;
                            }
                        }
                    }
                }
                if (regionWidth[A[i - 1][j]] == 0 || regionWidth[A[i - 1][j]] < c)
                {
                    regionWidth[A[i - 1][j]] = c;
                    c = 1;
                }
            }
            

            //ANY REGION COLOR THAT IS NOT CLOSED TO WHITE IS MARKED BY RED
            for (i = 1; i < w - 1; i++)
                for (j = 1; j < h - 1; j++)
                    if (!(btp.GetPixel(i, j).R == removeObjectColor.R && btp.GetPixel(i, j).G == removeObjectColor.G
                        && btp.GetPixel(i, j).B == removeObjectColor.B) && A[i][j] != 0)
                    {
                        double avg = (btp.GetPixel(i, j).R + btp.GetPixel(i, j).G + btp.GetPixel(i, j).B) / 3;
                        if(regionWidth[A[i][j]]>maxAllowableWidth)
                        {
                            btp.SetPixel(i, j, Color.Yellow);
                            A[i][j] = 0;
                        }
                        else if (avg < laneRGVAvgMin)
                        {
                            btp.SetPixel(i, j, removeObjectColor);
                            A[i][j] = 0;
                        }
                    }
            pictureBox2.Image = btp;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Bitmap btp = new Bitmap(pictureBox2.Image);
            int w, h, i, j, max = 0, po = 0;
            w = btp.Width;
            h = btp.Height;

            for (i = 1; i < w - 1; i++)
            {
                for (j = 1; j < h - 1; j++)
                {
                    if (!(btp.GetPixel(i, j).R == removeObjectColor.R && btp.GetPixel(i, j).G == removeObjectColor.G
                        && btp.GetPixel(i, j).B == removeObjectColor.B) && A[i][j] != 0)
                    {
                        if (B[A[i][j]] > max)
                        {
                            max = B[A[i][j]];
                            po = A[i][j];
                        }
                    }
                }
            }

            int lanePosition = 0;
            bool flag = false;

            for (i = 1; i < w - 1; i++)
            {
                for (j = 1; j < h - 1; j++)
                {
                    if (!flag && A[i][j] == po)
                    {
                        lanePosition = i; flag = true;
                    }
                    if (A[i][j] == po)
                    {
                        btp.SetPixel(i, j, Color.Yellow);
                    }
                }
            }

            pictureBox2.Image = btp;
            if (lanePosition == 0)
            {
                MessageBox.Show("invalid");
            }
            else if (lanePosition <= w / 2)
            {
                MessageBox.Show("left");
            }
            else MessageBox.Show("right");
        }

/*        private Bitmap gaussianFilter(Bitmap obj)
        {
            KernelWeight = gaussianKernel(KernelSize, Sigma);
            Bitmap filteredImage = new Bitmap(obj);
            int i, j, k, l, w, h, data;
            int Limit = KernelSize / 2;
            double Sum = 0, avg;
            w = obj.Width;
            h = obj.Height;

            for (i = Limit; i <= ((w - 1) - Limit); i++)
            {
                for (j = Limit; j <= ((h - 1) - Limit); j++)
                {
                    Sum = 0;
                    for (k = -Limit; k <= Limit; k++)
                    {
                        for (l = -Limit; l <= Limit; l++)
                        {
                            avg = (obj.GetPixel(i + k, j + l).R + obj.GetPixel(i + k, j + l).G + obj.GetPixel(i + k, j + l).B)/3;
                            Sum = Sum + (avg * GaussianKernel[Limit + k, Limit + l]);
                        }
                    }
                    data = (int)Math.Round(Sum / (double)KernelWeight);
                    filteredImage.SetPixel(i, j, Color.FromArgb(data, data, data));
                }
            }
            return filteredImage;
        }
        private int gaussianKernel(int size, double sig)
        {
            int i, j;
            double pi = (float)Math.PI;
            double[,] Kernel = new double[size, size];
            GaussianKernel = new int[size, size];
            double D1 = 1 / (2 * pi * sig * sig);
            double D2 = 2 * sig * sig;
            double min = 1000;

            for (i = -size / 2; i <= size / 2; i++)
            {
                for (j = -size / 2; j <= size / 2; j++)
                {
                    Kernel[size / 2 + i, size / 2 + j] = ((1 / D1) * (double)Math.Exp(-(i * i + j * j) / D2));
                    if (Kernel[size / 2 + i, size / 2 + j] < min)
                        min = Kernel[size / 2 + i, size / 2 + j];
                }
            }
            int mult = (int)(1 / min);
            int sum = 0;
            if ((min > 0) && (min < 1))
            {
                for (i = -size / 2; i <= size / 2; i++)
                {
                    for (j = -size / 2; j <= size / 2; j++)
                    {
                        Kernel[size / 2 + i, size / 2 + j] = (double)Math.Round(Kernel[size / 2 + i, size / 2 + j] * mult, 0);
                        GaussianKernel[size / 2 + i, size / 2 + j] = (int)Kernel[size / 2 + i, size / 2 + j];
                        sum = sum + GaussianKernel[size / 2 + i, size / 2 + j];
                    }
                }
            }
            else
            {
                sum = 0;
                for (i = -size / 2; i <= size / 2; i++)
                {
                    for (j = -size / 2; j <= size / 2; j++)
                    {
                        Kernel[size / 2 + i, size / 2 + j] = (double)Math.Round(Kernel[size / 2 + i, size / 2 + j], 0);
                        GaussianKernel[size / 2 + i, size / 2 + j] = (int)Kernel[size / 2 + i, size / 2 + j];
                        sum = sum + GaussianKernel[size / 2 + i, size / 2 + j];
                    }
                }
            }
            return sum;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = gaussianFilter(new Bitmap(pictureBox2.Image));
        }*/
    }    
}
