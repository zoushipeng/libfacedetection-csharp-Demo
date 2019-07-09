using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OneCardSystem.FeatureService
{
    public class FaceLibSDK
    {
        /**
        * 根据输入的图像检测出人脸位置
        * @return 人脸位置
        */
        [DllImport("libfacedetection.dll", EntryPoint = "facedetect_cnn")]
        public static extern IntPtr facedetect_cnn(IntPtr result_buffer, IntPtr rgb_image_data, int width, int height, int step);
    }
}
