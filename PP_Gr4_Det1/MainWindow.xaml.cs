using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;

namespace PP_Gr4_Det1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ParallelOptions ParallelOptions { get; set; }
        Bitmap Bitmap1 { get; set; }
        Bitmap Bitmap2 { get; set; }

        public MainWindow()
        {
            this.ParallelOptions = new ParallelOptions();
            this.ParallelOptions.MaxDegreeOfParallelism = 1;
            InitializeComponent();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var op = new OpenFileDialog
            {
                Title = "Select a picture",
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png"
            };

            if (op.ShowDialog() == true)
            {
                this.Bitmap1 = new Bitmap(op.FileName);
                imgPhoto1.Source = new BitmapImage(new Uri(op.FileName));
            }
        }

        private void BtnLoad2_Click(object sender, RoutedEventArgs e)
        {
            var op = new OpenFileDialog
            {
                Title = "Select a picture",
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png"
            };

            if (op.ShowDialog() == true)
            {
                this.Bitmap2 = new Bitmap(op.FileName);
                imgPhoto2.Source = new BitmapImage(new Uri(op.FileName));
            }
        }

        private void ChangedThreads(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtThread.Text, out int threads))
            {
                this.ParallelOptions.MaxDegreeOfParallelism = threads;
            }
            else
            {
                MessageBox.Show("Please write integer value!");
            }
        }

        private void BtnResult_Click(object sender, RoutedEventArgs e)
        {
            if (this.Bitmap1 == null || this.Bitmap2 == null)
            {
                MessageBox.Show("Please upload images!");
                return;
            }

            if (this.Bitmap1.Height != this.Bitmap2.Height || this.Bitmap1.Width != this.Bitmap2.Width)
            {
                MessageBox.Show("Please select images with same height and width!");
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var results = Compare();
                stopwatch.Stop();
                TxtResult.Text = $"{ Math.Round(results * 100, 2) } % (Same)";
                TxtTime.Text = $"{ stopwatch.ElapsedMilliseconds } ms";
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
            
        }

        private unsafe double Compare()
        {
            BitmapData bmData = null;
            BitmapData bmData2 = null;
            double total = 1;
            var sum = 0;

            try
            {
                bmData = this.Bitmap1.LockBits(new Rectangle(0, 0, this.Bitmap1.Width, this.Bitmap1.Height), 
                    ImageLockMode.ReadOnly, this.Bitmap1.PixelFormat);
                bmData2 = this.Bitmap2.LockBits(new Rectangle(0, 0, this.Bitmap2.Width, this.Bitmap2.Height), 
                    ImageLockMode.ReadOnly, this.Bitmap2.PixelFormat);
                
                var scan0 = bmData.Scan0;
                var scan02 = bmData2.Scan0;
                var stride = bmData.Stride;
                var stride2 = bmData2.Stride;
                var bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(this.Bitmap1.PixelFormat) / 8;
                
                int nWidth = bmData.Width;
                int nHeight = bmData.Height;
                total = nWidth * nHeight;

                Parallel.For(0, nHeight, this.ParallelOptions, y =>
                {
                    byte* p = (byte*)scan0 + y * stride;
                    byte* p2 = (byte*)scan02 + y * stride2;

                    var subtotal = 0;

                    for (int x = 0; x < nWidth * bytesPerPixel; x = x + bytesPerPixel)
                    {
                        if (p[x] == p2[x])
                        {
                            subtotal++;
                        }
                    }

                    Thread.Sleep(10);
                    Interlocked.Add(ref sum, subtotal);
                });

                this.Bitmap1.UnlockBits(bmData);
                this.Bitmap2.UnlockBits(bmData2);
            }
            catch
            {
                try
                {
                    if (bmData != null)
                    {
                        this.Bitmap1.UnlockBits(bmData);
                    }

                    if (bmData2 != null)
                    {
                        this.Bitmap2.UnlockBits(bmData2);
                    }
                }
                catch
                { }
            }

            return sum / total;
        }
    }
}
