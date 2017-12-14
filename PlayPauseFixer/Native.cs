using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PlayPauseFixer
{
    internal class Native
    {
        /* invalid handle value */
        public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);


        #region kernel32.dll


        /* read access */
        public const uint GENERIC_READ = 0x80000000;
        /* write access */
        public const uint GENERIC_WRITE = 0x40000000;
        /* Enables subsequent open operations on a file or device to request 
         * write access.*/
        public const uint FILE_SHARE_WRITE = 0x2;
        /* Enables subsequent open operations on a file or device to request
         * read access. */
        public const uint FILE_SHARE_READ = 0x1;
        /* The file or device is being opened or created for asynchronous I/O. */
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        /* Opens a file or device, only if it exists. */
        public const uint OPEN_EXISTING = 3;
        /* Opens a file, always. */
        public const uint OPEN_ALWAYS = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        /* opens files that access usb hid devices */
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPStr)] string strName, 
            uint nAccess, uint nShareMode, IntPtr lpSecurity, 
            uint nCreationFlags, uint nAttributes, IntPtr lpTemplate);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        /* closes file */
        public static extern bool CloseHandle(IntPtr hObject);

        #endregion

        #region user32.dll

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;
        public const int VK_MEDIA_STOP = 0xB2;
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        public static void KeyPress(byte virtualKey)
        {
            keybd_event(virtualKey, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
            keybd_event(virtualKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        #endregion

        #region hid.dll
        
        /* The HIDD_ATTRIBUTES structure contains vendor information about a 
         * HIDClass device.*/
        [StructLayout(LayoutKind.Sequential)]
        public struct HiddAttributtes
        {
            /* size in bytes */
            public int Size;
            /* vendor id */
            public short VendorID;
            /* product id */
            public short ProductID;
            /* hid vesion number */
            public short VersionNumber;
        }

        [DllImport("hid.dll", SetLastError = true)]
        /* gets HID class Guid */
        public static extern void HidD_GetHidGuid(out Guid gHid);

        /* gets hid device attributes */
        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetAttributes(IntPtr hFile,
            ref HiddAttributtes attributes);

        /* gets usb manufacturer string */
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool HidD_GetManufacturerString(IntPtr hFile,
            StringBuilder buffer, int bufferLength);

        /* gets product string */
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool HidD_GetProductString(IntPtr hFile,
            StringBuilder buffer, int bufferLength);

        /* gets serial number string */
        [DllImport("hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool HidD_GetSerialNumberString(IntPtr hDevice,
            StringBuilder buffer, int bufferLength);


        #endregion
        #region setupapi.dll


        /* Return only devices that are currently present in a system. */
        public const int DIGCF_PRESENT = 0x02;
        /* Return devices that support device interfaces for the specified 
         * device interface classes. */
        public const int DIGCF_DEVICEINTERFACE = 0x10;

        /* structure returned by SetupDiEnumDeviceInterfaces */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DeviceInterfaceData
        {
            /* size of fixed part of structure */
            public int Size;
            /* The GUID for the class to which the device interface belongs. */
            public Guid InterfaceClassGuid;
            /* Can be one or more of the following: SPINT_ACTIVE, 
             * SPINT_DEFAULT, SPINT_REMOVED */
            public int Flags;
            /* do not use */
            public IntPtr Reserved;
        }

        /* A structure contains the path for a device interface.*/
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DeviceInterfaceDetailData
        {
            /* size of fixed part of structure */
            public int Size;
            /* device path, as to be used by CreateFile */
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string DevicePath;
        }

        /* function returns a handle to a device information set that contains
         * requested device information elements for a local computer */
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, 
            [MarshalAs(UnmanagedType.LPStr)] string strEnumerator, 
            IntPtr hParent, uint nFlags);

        /* The function enumerates the device interfaces that are contained in 
         * a device information set.*/
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr lpDeviceInfoSet, uint nDeviceInfoData, ref Guid gClass,
            uint nIndex, ref DeviceInterfaceData oInterfaceData);

        /* The SetupDiGetDeviceInterfaceDetail function returns details about 
         * a device interface.*/
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr lpDeviceInfoSet, ref DeviceInterfaceData oInterfaceData,
            ref DeviceInterfaceDetailData oDetailData, 
            uint nDeviceInterfaceDetailDataSize, ref uint nRequiredSize,
            IntPtr lpDeviceInfoData);

        /* destroys device list */
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet);


        #endregion
  
    }
}
