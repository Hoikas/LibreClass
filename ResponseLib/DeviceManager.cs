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
using System.Linq;
using System.Threading;
using HidSharp;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.DeviceNotify.Info;
using NLog;

namespace LibreClass.Response {
    internal struct DeviceID {
        public int Product { get; private set; }
        public int Vendor { get; private set; }
        public string Serial { get; private set; }

        public DeviceID(int pID, int vID, string serial) {
            Product = pID;
            Vendor = vID;
            Serial = serial;
        }

        public DeviceID(IUsbDeviceNotifyInfo info) {
            Product = info.IdProduct;
            Vendor = info.IdVendor;
            Serial = info.SerialNumber;
        }

        public DeviceID(HidDevice device) {
            Product = device.ProductID;
            Vendor = device.VendorID;
            Serial = device.SerialNumber;
        }
    }

    public sealed class DeviceEventArgs {

        public ResponseDevice Device { get; private set; }
        public DeviceEventType Event { get; private set; }
        public bool Deny { get; set; }

        public DeviceEventArgs(ResponseDevice d, DeviceEventType e) {
            Device = d;
            Event = e;
            Deny = false;
        }
    }

    public enum DeviceEventType {
        /// <summary>
        /// The device was present in the system when the device manager initialized.
        /// </summary>
        DeviceDiscovered,

        /// <summary>
        /// The device was just added to the system.
        /// </summary>
        DeviceAdded,

        /// <summary>
        /// The device has been removed from the system.
        /// </summary>
        DeviceRemoved,
    }

    public class DeviceManager {
        Dictionary<DeviceID, ResponseDevice> m_devices = new Dictionary<DeviceID, ResponseDevice>();
        Logger m_log = LogManager.GetCurrentClassLogger();
        IDeviceNotifier m_usbNotifier;

        public ResponseDevice[] Devices {
            get { return m_devices.Values.ToArray(); }
        }

        public event EventHandler<DeviceEventArgs> DeviceDiscovered;
        public event EventHandler<DeviceEventArgs> DeviceRemoved;

        public void BeginDiscovery() {
            if (m_usbNotifier != null)
                throw new InvalidOperationException();

            // Test all the loaded HID devices to see if they're valid
            HidDeviceLoader loader = new HidDeviceLoader();
            foreach (var device in loader.GetDevices()) {
                DeviceID id = new DeviceID(device);
                IInitDevice(id, DeviceEventType.DeviceDiscovered);
            }

            // Register for new device insertions
            m_usbNotifier = DeviceNotifier.OpenDeviceNotifier();
            m_usbNotifier.OnDeviceNotify += IOnUsbNotify;
        }

        private void IDestroyDevice(DeviceID id) {
            ResponseDevice device = m_devices[id];
            DeviceRemoved?.Invoke(this, new DeviceEventArgs(device, DeviceEventType.DeviceRemoved));
            m_devices.Remove(id);
            device.IDeviceRemoved();
        }

        private async void IInitDevice(DeviceID id, DeviceEventType e, HidDevice hid=null) {
            ResponseDevice device = DeviceFactory.Create(id);
            if (device != null) {
                m_log.Trace("ResponseDevice detected...");
                if (hid == null) {
                    HidDeviceLoader loader = new HidDeviceLoader();
                    for (int i = 0; i < 10; i++) {
                        hid = loader.GetDeviceOrDefault(vendorID: id.Vendor, productID: id.Product);
                        if (hid == null) {
                            m_log.Trace("... HID device matching USB insert not found, retrying in 50ms");
                            Thread.Sleep(50); // :(
                        } else {
                            break;
                        }
                    }
                    if (hid == null) {
                        m_log.Warn("Could not find HID device [V:{0:X}] [P:{1:X}] [S:{2}] after USB insert", id.Vendor, id.Product, id.Serial);
                        return;
                    }
                }
                m_log.Info("Discovered `{0}` [V:{1:X}] [P:{2:X}] [S:{3}]", hid.ProductName, id.Vendor, id.Product, id.Serial);
                if (await device.Initialize(hid)) {
                    m_devices.Add(id, device);
                    DeviceDiscovered?.Invoke(this, new DeviceEventArgs(device, e));
                } else {
                    m_log.Warn("Failed to init `{0}` [V:{1:X}] [P:{2:X}] [S:{3}]", hid.ProductName, id.Vendor, id.Product, id.Serial);
                }
            }
        }

        private void IOnUsbNotify(object sender, DeviceNotifyEventArgs e) {
            DeviceID id = new DeviceID(e.Device);

            // Do we give any shits about this device?
            if (!DeviceFactory.Contains(id.Vendor, id.Product))
                return;

            // Wazzup?
            switch (e.EventType) {
                case EventType.DeviceArrival: {
                        IInitDevice(id, DeviceEventType.DeviceAdded);
                        break;
                    }
                case EventType.DeviceQueryRemove: {
                        ResponseDevice device = m_devices[id];
                        bool canRemove = device.IBeginSoftRemoval();
                        /// TODO: deny this request
                        break;
                    }
                case EventType.DeviceRemoveComplete: {
                        IDestroyDevice(id);
                        break;
                    }
            }
        }
    }
}
