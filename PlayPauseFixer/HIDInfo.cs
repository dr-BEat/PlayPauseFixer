using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace PlayPauseFixer
{
    public class HIDInfo
    {
        public HIDInfo(string product, string serial, string manufacturer,
            string path, short vid, short pid)
        {
            Product = product;
            SerialNumber = serial;
            Manufacturer = manufacturer;
            Path = path;
            Vid = vid;
            Pid = pid;
        }

        /// <summary>
        /// Device Path
        /// </summary>
        public string Path { get; }
        
        /// <summary>
        /// Vendor Id
        /// </summary>
        public short Vid { get; }
        
        /// <summary>
        /// Product Id
        /// </summary>
        public short Pid { get; }
        
        /// <summary>
        /// Usb Product String
        /// </summary>
        public string Product { get; }

        public string Manufacturer { get; }

        public string SerialNumber { get; }

        public bool TryOpen(out FileStream deviceStream)
        {
            // opens hid device file
            var handle = Native.CreateFile(Path,
                Native.GENERIC_READ | Native.GENERIC_WRITE,
                Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE,
                IntPtr.Zero, Native.OPEN_EXISTING, Native.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero);

            if (handle == Native.INVALID_HANDLE_VALUE)
            {
                deviceStream = null;
                return false;
            }
            var shandle = new SafeFileHandle(handle, true);
            deviceStream = new FileStream(shandle, FileAccess.ReadWrite, 32, true);
            return true;
        }

        public override string ToString()
        {
            return "VID = " + Vid.ToString("X4") +
                   " PID = " + Pid.ToString("X4") +
                   " Product: " + Product +
                   " Path " + Path;
        }
    }
}