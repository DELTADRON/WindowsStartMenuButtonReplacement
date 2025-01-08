using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinStartMenuReplacement
{
    public class Inf
    {
        public static string outmsg { get; set; }
        public static int Success { get; set; } = 0;
        public static bool Show { get; set; } = false;
        public static void Show_log()
        {
            if (Show) MessageBox.Show($"Result is {Success}/6\nIf you have more than 0 its Success.\n\nLog:\n" + outmsg);
        }
    }
    class StartButtonModifier
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenThemeDataForDpi(IntPtr hwnd, string pszClassList, uint dpi);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetThemeBitmap(IntPtr hTheme, int iPartId, int iStateId, int iPropId, uint dwFlags, out IntPtr phBitmap);

        [DllImport("uxtheme.dll", SetLastError = true)]
        private static extern void CloseThemeData(IntPtr hTheme);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern int GetObject(IntPtr hObject, int nSize, ref BITMAP lpObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern int GetBitmapBits(IntPtr hbmp, int cbBuffer, byte[] lpvBits);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern int SetBitmapBits(IntPtr hbmp, int cbBuffer, byte[] lpvBits);

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;
        }

        private const int GBF_DIRECT = 0x00000001;

        public byte[] ConvertToBitmapBytes(Bitmap img, int width, int height)
        {
            using (Bitmap bitmap = new Bitmap(img, new Size(width, height)))
            {

                BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int byteCount = bmpData.Stride * bitmap.Height;
                byte[] bitmapBytes = new byte[byteCount];
                Marshal.Copy(bmpData.Scan0, bitmapBytes, 0, byteCount);
                bitmap.UnlockBits(bmpData);

                return bitmapBytes;
            }
        }

        private bool UpdateBitmap(IntPtr hBitmap, byte[] bitmapBytes)
        {
            BITMAP bm = new BITMAP();
            GetObject(hBitmap, Marshal.SizeOf(typeof(BITMAP)), ref bm);

            if (hBitmap == IntPtr.Zero || bm.bmBitsPixel != 32 || bm.bmWidth > 64)
            {
                return false;
            }

            SetBitmapBits(hBitmap, bitmapBytes.Length, bitmapBytes);
            return true;
        }

        public void ModifyStyle(string classList, int partId, int stateId, int propId, byte[] bitmapBytes)
        {
            IntPtr hTheme = OpenThemeDataForDpi(IntPtr.Zero, classList, GetDpiForWindow(IntPtr.Zero));
            if (hTheme == IntPtr.Zero)
            {
                Inf.outmsg += "Failed to open theme data.\n";
                return;
            }

            if (GetThemeBitmap(hTheme, partId, stateId, propId, GBF_DIRECT, out IntPtr hBitmap) != 0 || hBitmap == IntPtr.Zero)
            {
                Inf.outmsg += $"Failed to get start menu bitmap. {partId}\n";
                CloseThemeData(hTheme);
                return;
            }

            if (UpdateBitmap(hBitmap, bitmapBytes))
            {
                Inf.outmsg += $"The start menu bitmap has been modified. {partId}\n";
                Inf.Success++;
            }
            else
            {
                Inf.outmsg += $"Failed to modify start menu bitmap. {partId}\n";
            }

            CloseThemeData(hTheme);
        }
    }
}
