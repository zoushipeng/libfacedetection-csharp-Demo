using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BitmapData = System.Drawing.Imaging.BitmapData;
using ImageLockMode = System.Drawing.Imaging.ImageLockMode;

namespace OneCardSystem.FeatureService
{
    public class FaceLib
    {

        private static FaceLib instance;
        IntPtr faceResPtr = Marshal.AllocHGlobal(0x20000);
        private static readonly object Lock = new object();

        public static FaceLib GetInstance()
        {
            if (instance == null)
            {
                lock (Lock)
                {
                    if (instance == null)
                    {
                        instance = new FaceLib();
                    }
                }
            }
            return instance;
        }

        private static byte[] getBGR(Bitmap image, ref int width, ref int height, ref int pitch)
        {
            //Bitmap image = new Bitmap(imgPath);

            const System.Drawing.Imaging.PixelFormat mPixelFormat = PixelFormat.Format24bppRgb;

            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, mPixelFormat);

            IntPtr ptr = data.Scan0;

            int ptr_len = data.Height * Math.Abs(data.Stride);

            byte[] ptr_bgr = new byte[ptr_len];

            Marshal.Copy(ptr, ptr_bgr, 0, ptr_len);

            width = data.Width;

            height = data.Height;

            pitch = Math.Abs(data.Stride);

            int line = width * 3;

            int bgr_len = line * height;

            byte[] bgr = new byte[bgr_len];

            for (int i = 0; i < height; ++i)
            {
                Array.Copy(ptr_bgr, i * pitch, bgr, i * line, line);
            }

            pitch = line;

            image.UnlockBits(data);

            return bgr;
        }

        public int FaceDetect(Bitmap picFiles, int nWidth, int nHeight, out List<FaceLibRect> rects, int thresold = 70)
        {
            lock (Lock)
            {
                int faceNum = 0;
                rects = new List<FaceLibRect>();

                try
                {
                    int width = 0;
                    int height = 0;
                    int pitch = 0;

                    byte[] imageData = getBGR(picFiles, ref width, ref height, ref pitch);

                    IntPtr imageDataPtr = Marshal.AllocHGlobal(imageData.Length);

                    Marshal.Copy(imageData, 0, imageDataPtr, imageData.Length);

                    //人脸检测
                    IntPtr resPtr = FaceLibSDK.facedetect_cnn(faceResPtr, imageDataPtr, width, height, pitch);

                    byte[] res = new byte[856];
                    Marshal.Copy(resPtr, res, 0, 856);

                    faceNum = res[3] * 256 * 256 * 256 + res[2] * 256 * 256 + res[1] * 256 + res[0];

                    if (faceNum > 0)
                    {
                        for (int i = 0; i < faceNum && i < 3; i++)
                        {
                            int x = res[5 + 284 * i] * 256 + res[4 + 284 * i];
                            int y = res[7 + 284 * i] * 256 + res[6 + 284 * i];
                            int w = res[9 + 284 * i] * 256 + res[8 + 284 * i];
                            int h = res[11 + 284 * i] * 256 + res[10 + 284 * i];
                            int confidence = res[13 + 284 * i] * 256 + res[12 + 284 * i];
                            int angle = res[15 + 284 * i] * 256 + res[14 + 284 * i];

                            if (confidence > thresold)
                            {
                                var rect = new FaceLibRect
                                {
                                    rect = new Rectangle(x, y, w, h),
                                    angle = angle,
                                    confindence = confidence
                                };
                                rects.Add(rect);
                            }
                        }
                        faceNum = rects.Count;
                    }

                    picFiles.Dispose();
                    imageData = null;

                    Marshal.FreeHGlobal(imageDataPtr);
                }
                catch (Exception e)
                {
                    // 
                }
                return faceNum;
            }
        }

        public bool Uninit()
        {
            try
            {
                Marshal.FreeHGlobal(faceResPtr);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Bitmap ResizeImage(Image imgToResize, float persent)
        {
            try
            {
                //获取图片宽度
                int sourceWidth = imgToResize.Width;
                //获取图片高度
                int sourceHeight = imgToResize.Height;

                //期望的宽度
                int destWidth = (int)(sourceWidth * persent);
                //期望的高度
                int destHeight = (int)(sourceHeight * persent);

                Bitmap b = new Bitmap(destWidth, destHeight);
                Graphics g = Graphics.FromImage((System.Drawing.Image)b);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //绘制图像
                g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
                g.Dispose();
                return b;
            }
            catch (Exception e)
            {

            }
            return null;
        }
    }

    public class FaceLibRect
    {
        public int confindence;
        public int angle;
        public Rectangle rect;
    }
}
