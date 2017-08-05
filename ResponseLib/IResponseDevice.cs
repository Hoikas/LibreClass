using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HidSharp;

namespace LibreClass.Response {
    internal class DeviceFactory {
        static Dictionary<Tuple<int, int>, Type> s_devices = new Dictionary<Tuple<int, int>, Type>();

        internal static Dictionary<Tuple<int, int>, Type> Devices {
            get {
                Check();
                return s_devices;
            }
        }

        static void Check() {
            if (s_devices.Count == 0) {
                s_devices.Add(new Tuple<int, int>(1932, 1792), typeof(EIGen3Hub));
            }
        }

        internal static bool Contains(int vendor, int product) {
            return s_devices.ContainsKey(new Tuple<int, int>(vendor, product));
        }

        internal static ResponseDevice Create(DeviceID device) {
            Check();

            Tuple<int, int> key = new Tuple<int, int>(device.Vendor, device.Product);
            if (!s_devices.ContainsKey(key))
                return null;
            Type imp = s_devices[key];
            return (ResponseDevice)Activator.CreateInstance(imp);
        }
    }

    public abstract class ResponseDevice {
        string Name { get; }

        /// <summary>
        /// The receiver device has been removed from the system
        /// </summary>
        public event EventHandler DeviceRemoved;

        /// <summary>
        /// Attempts to begin a soft removal of the device
        /// </summary>
        /// <returns>TRUE on success</returns>
        internal abstract bool IBeginSoftRemoval();

        /// <summary>
        /// The device has been removed from the system.
        /// </summary>
        internal virtual void IDeviceRemoved() {
            DeviceRemoved?.Invoke(this, null);
        }

        /// <summary>
        /// Initializes the HID response device
        /// </summary>
        internal abstract Task<bool> Initialize(HidDevice hid);

        public abstract Task<bool> StartClass(IRoster roster);

        public abstract Task<bool> EndClass();

    }
}
