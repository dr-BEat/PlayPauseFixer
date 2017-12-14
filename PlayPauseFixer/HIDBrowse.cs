using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PlayPauseFixer
{
    public class HIDBrowse
    {
        /// <summary>
        /// Enumerates all HID class devices
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<HIDInfo> EnumerateDevices()
        {
            // obtain hid guid
            Native.HidD_GetHidGuid(out var gHid);
            // get list of present hid devices
            var hInfoSet = Native.SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero,
                Native.DIGCF_DEVICEINTERFACE | Native.DIGCF_PRESENT);

            // allocate mem for interface descriptor
            var iface = new Native.DeviceInterfaceData();
            // set size field
            iface.Size = Marshal.SizeOf(iface);
            // interface index
            uint index = 0;

            // iterate through all interfaces
            while (Native.SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, 
                index, ref iface))
            {
                // get device path
                var path = GetPath(hInfoSet, ref iface);
                
                // open device
                var handle = Open(path);

                // device is opened?
                if (handle != Native.INVALID_HANDLE_VALUE) {
                    HIDInfo info;
                    try
                    {
                        var man = GetManufacturer(handle);
                        var prod = GetProduct(handle);
                        var serial = GetSerialNumber(handle);
                        GetVidPid(handle, out var vid, out var pid);
                        info = new HIDInfo(prod, serial, man, path, vid, pid);
                    }
                    finally
                    {
                        Close(handle);
                    }
                    yield return info;
                }
                index++;
            }

            // clean up
            if (!Native.SetupDiDestroyDeviceInfoList(hInfoSet))
            {
                throw new Win32Exception();
            }
        }

        private static IntPtr Open(string path)
        {
            // opens hid device file
            return Native.CreateFile(path,
                Native.GENERIC_READ | Native.GENERIC_WRITE,
                Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE,
                IntPtr.Zero, Native.OPEN_EXISTING, Native.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero);
        }

        private static void Close(IntPtr handle)
        {
            // try to close handle
            if (!Native.CloseHandle(handle)) {
                throw new Win32Exception();
            }
        }

        private static string GetPath(IntPtr hInfoSet, 
            ref Native.DeviceInterfaceData iface)
        {
            var detIface = new Native.DeviceInterfaceDetailData();
            var reqSize = (uint)Marshal.SizeOf(detIface);

            /* set size. The cbSize member always contains the size of the 
             * fixed part of the data structure, not a size reflecting the 
             * variable-length string at the end. */
            /* now stay with me and look at that x64/x86 maddness! */
            detIface.Size = Marshal.SizeOf(typeof(IntPtr)) == 8 ? 8 : 5;

            // get device path
            var status = Native.SetupDiGetDeviceInterfaceDetail(hInfoSet,
                ref iface, ref detIface, reqSize, ref reqSize, IntPtr.Zero);
            
            if (!status) {
                throw new Win32Exception();
            }
            return detIface.DevicePath;
        }

        private static string GetManufacturer(IntPtr handle)
        {
            var s = new StringBuilder(256);
            var rc = string.Empty;
            if (Native.HidD_GetManufacturerString(handle, s, s.Capacity)) {
                rc = s.ToString();
            }
            return rc;
        }

        private static string GetProduct(IntPtr handle)
        {
            var s = new StringBuilder(256);
            var rc = string.Empty;
            if (Native.HidD_GetProductString(handle, s, s.Capacity)) {
                rc = s.ToString();
            }
            return rc;
        }

        private static string GetSerialNumber(IntPtr handle)
        {
            var s = new StringBuilder(256);
            var rc = string.Empty;
            if (Native.HidD_GetSerialNumberString(handle, s, s.Capacity)) {
                rc = s.ToString();
            }
            return rc;
        }

        private static void GetVidPid(IntPtr handle, out short vid, out short pid)
        {
            var attr = new Native.HiddAttributtes();
            attr.Size = Marshal.SizeOf(attr);

            if (Native.HidD_GetAttributes(handle, ref attr) == false) {
                throw new Win32Exception();
            }

            vid = attr.VendorID;
            pid = attr.ProductID;
        }
    }
}
