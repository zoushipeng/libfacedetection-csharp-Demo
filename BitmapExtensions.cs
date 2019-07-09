using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace 人脸特征服务开源版
{
    public static class BitmapExtensions
    {
        /// <summary>
        ///     将图像转换为RGB图像
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Bitmap ConvertToRgb24(this Bitmap self)
        {
            if (self.PixelFormat != PixelFormat.Format24bppRgb)
            {
                var convertImage = new Bitmap(self.Width, self.Height, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(self))
                {
                    g.DrawImage(self, 0, 0);
                }

                return convertImage;
            }
            return self;
        }

        /// <summary>
        ///     获取位图数据的像素数据
        /// </summary>
        /// <param name="self"></param>
        /// <param name="useNativePixelFormat"></param>
        /// <returns></returns>
        public static byte[] GetBitmapData(this Bitmap self, bool useNativePixelFormat = false)
        {
            byte[] res;
            var rect = new Rectangle(0, 0, self.Width, self.Height);
            var bmpData = self.LockBits(rect, ImageLockMode.ReadOnly,
                useNativePixelFormat ? self.PixelFormat : PixelFormat.Format24bppRgb);
            var dataPtr = bmpData.Scan0;
            var bytesCount = Math.Abs(bmpData.Stride) * self.Height;

            var data = new byte[bytesCount];
            Marshal.Copy(dataPtr, data, 0, bytesCount);

            res = data;
            if (bmpData.Width * 3 != Math.Abs(bmpData.Stride))
            {
                int width = bmpData.Width;
                int height = bmpData.Height;
                int pitch = Math.Abs(bmpData.Stride);
                int line = bmpData.Width * 3;

                int bgr_len = line * height;

                byte[] bgr = new byte[bgr_len];

                for (int i = 0; i < height; ++i)
                {
                    Array.Copy(data, i * pitch, bgr, i * line, line);
                }
                res = bgr;
            }

            self.UnlockBits(bmpData);

            return res;
        }

        public static byte[] GrayByRgb24(byte[] rgb24, int width, int height)
        {
            if (rgb24 == null)
            {
                return null;
            }
            byte[] rgb8 = new byte[width * height];
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    int dst_pos = i * width + j;
                    int src_pos = dst_pos * 3;
                    rgb8[dst_pos] = Convert.ToByte((rgb24[src_pos + 0] * 38 + rgb24[src_pos + 1] * 75 + rgb24[src_pos + 2] * 15) >> 7);
                }
            }
            return rgb8;
        }
    }
}
