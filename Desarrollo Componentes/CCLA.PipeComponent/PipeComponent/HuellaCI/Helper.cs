using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;

namespace TPAuto.HuellaCI
{
    class Helper
    {
        public static Bitmap ByteArrayToBitmap(byte[] imageData, int imageWidth, int imageHeight)
        {
            int colorVal;
            Bitmap bmp = new Bitmap(imageWidth, imageHeight);

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    colorVal = imageData[(j * imageWidth) + i];
                    bmp.SetPixel(i, j, Color.FromArgb(colorVal, colorVal, colorVal));
                }
            }

            return bmp;
        }
    }
}
