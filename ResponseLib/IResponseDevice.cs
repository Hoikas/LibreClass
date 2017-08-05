/*    This file is part of LibreClass.
 *
 *    LibreClass is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    LibreClass is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with LibreClass.  If not, see <http://www.gnu.org/licenses/>.
 */

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
