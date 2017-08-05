using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Classroom {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        // TESTING!!!
        LibreClass.Response.DeviceManager dm = new LibreClass.Response.DeviceManager();

        public MainWindow() {
            InitializeComponent();

            // TESTING!!!
            dm.DeviceDiscovered += (object sender, LibreClass.Response.DeviceEventArgs e) => { e.Device.StartClass(null); };
            dm.BeginDiscovery();
        }
    }
}
