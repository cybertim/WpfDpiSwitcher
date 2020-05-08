using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

namespace WpfDpiSwitcher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		[DllImport("DpiApi.dll")]
		static public extern UInt32 GetIDHigh(UInt32 index);
		[DllImport("DpiApi.dll")]
		static public extern UInt32 GetIDLow(UInt32 index);
		[DllImport("DpiApi.dll")]
		static public extern UInt32 GetIDSource(UInt32 index);
		[DllImport("DpiApi.dll")]
		static public extern UInt32 GetIndexCount();
		[DllImport("DpiApi.dll")]
		static public extern void SetDPIScaling(UInt32 adapterIDHigh, UInt32 adapterIDlow, UInt32 sourceID, UInt32 dpiPercentToSet);

		DisplayItem tabletDPI;
		DisplayItem desktopDPI;
		DisplayItem displayItem;
		DispatcherTimer dispatcherTimer;
		int currentMode = 0;
		bool active = false;

		int Mode
		{
			get
			{
				return (int)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell", "TabletMode", 0);
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			MinimizeToTray.Enable(this);

			var i = GetIndexCount();
			for (int j = 0; j < i; j++)
			{
				DisplayList.Add(new DisplayItem() { Name = "Display " + j, Id = UInt32.Parse("" + j) });
			}
			var items = new ArrayList() {
				new DisplayItem() { Name = "100%", Id = 100 },
				new DisplayItem() { Name = "125%", Id = 125 },
				new DisplayItem() { Name = "150%", Id = 150 },
				new DisplayItem() { Name = "175%", Id = 175 },
				new DisplayItem() { Name = "200%", Id = 200 },
				new DisplayItem() { Name = "225%", Id = 225 }
			};

			foreach (var item in items)
			{
				DesktopDPIList.Add(item);
				TabletDPIList.Add(item);
			}

			currentMode = Mode;
			dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += new EventHandler(Background);
			dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
			dispatcherTimer.Start();

			//var watcher = System.Devices.DeviceInformation.createWatcher(); // need uwp for that - in uwp the dll doesn't work..

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{

			if (!active)
			{
				displayItem = (DisplayItem)DisplayComboBox.SelectedItem;
				tabletDPI = (DisplayItem)TabletDPIComboBox.SelectedItem;
				desktopDPI = (DisplayItem)DesktopDPIComboBox.SelectedItem;

				if (displayItem == null || tabletDPI == null || desktopDPI == null)
				{
					MessageBoxResult result = MessageBox.Show("Please set the display and DPI values first.", "Cannot start", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				HighIDTextBox.Text = "" + GetIDHigh(displayItem.Id);
				LowIDTextBox.Text = "" + GetIDLow(displayItem.Id);

				active = true;

				StartButton.IsCancel = true;
				StartButton.Content = "Stop";
			}
			else
			{
				StartButton.IsCancel = false;
				StartButton.Content = "Start";
				active = false;
			}
		}

		void Background(object sender, EventArgs e)
		{
			var mode = Mode;
			if (active && mode != currentMode)
			{
				if (mode == 0)
				{
					// Mouse
					SetDPIScaling(
						UInt32.Parse("" + GetIDHigh(displayItem.Id)),
						UInt32.Parse("" + GetIDLow(displayItem.Id)),
						UInt32.Parse("" + displayItem.Id),
						UInt32.Parse("" + desktopDPI.Id));
				}
				else
				{
					// Tablet
					SetDPIScaling(
						UInt32.Parse("" + GetIDHigh(displayItem.Id)),
						UInt32.Parse("" + GetIDLow(displayItem.Id)),
						UInt32.Parse("" + displayItem.Id),
						UInt32.Parse("" + tabletDPI.Id));
				}
				currentMode = mode;
			}
		}

		public class DisplayItem
		{
			public string Name { get; set; }
			public UInt32 Id { get; set; }
		}
	}
}
