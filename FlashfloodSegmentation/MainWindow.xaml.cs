using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace FlashfloodSegmentation
{
    public partial class MainWindow : Window
    {
        System.Drawing.Color[,] colorArr;
        SectionMaster sm;
        Bitmap imageBmap;
        static int currentSectionID=0;

        public MainWindow()
        {
            InitializeComponent();
        }

       

        private System.Drawing.Color[,] recordPixels(Bitmap foo)
        {
            System.Drawing.Color[,] colorArr = new System.Drawing.Color[foo.Width, foo.Height];
            for (int x = 0; x < foo.Width; x++)
            {
                for (int y = 0; y < foo.Height; y++)
                {
                    colorArr[x, y] = foo.GetPixel(x, y);
                }
            }
            return colorArr;
        }



        private void startDraw(object sender, RoutedEventArgs e) //called when go button clicked
        {
            Bitmap foo = Bitmap.FromFile(@"lake.jpg") as Bitmap; //retrieves file
            colorArr = recordPixels(foo); //creates array of colors of image file
            sm = new SectionMaster(colorArr);
            sm.divide();
            imageBmap = sm.colorSections();
            textBlock.Text = sm.sections.Count.ToString();            
            image1.Source = BitmapToImageSource(imageBmap);
        }

        public BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
         
        
        //creates bitmap from provided array
        public Bitmap createBMap(int[,] diff)
        {
            System.Drawing.Color[,] bmap = new System.Drawing.Color[diff.GetLength(0), diff.GetLength(1)];
            byte c;
            Bitmap bbmap = new Bitmap(diff.GetLength(0), diff.GetLength(1));
            for (int x = 0; x < diff.GetLength(0); x++)
            {
                for (int y = 0; y < diff.GetLength(1); y++)
                {
                    c = (byte)diff[x, y];
                    bmap[x, y] = System.Drawing.Color.FromArgb(255, c, c, c);
                    bbmap.SetPixel(x, y, bmap[x, y]);

                }
            }
            return bbmap;
        }

        
    }
}