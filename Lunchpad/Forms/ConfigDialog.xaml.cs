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
using System.Windows.Shapes;

using NAudio;
using NAudio.Wave;

namespace Lunchpad
{
  /// <summary>
  /// Interaktionslogik für ConfigDialog.xaml
  /// </summary>
  public partial class ConfigDialog : Window
  {
    LPConfig Config;
    public class ConfigDialogResult {
      public int selectedLaunchpad { get; set; }
      public int selectedOutputDevice { get; set; }
      public bool enablePTT { get; set; }
      public int pttButton { get; set; }
    }

    public ConfigDialogResult ConfigResult;
    public ConfigDialog()
    {
      InitializeComponent();
    }

    public ConfigDialog(LPConfig config) : this() {
      Config = config;      
      Initialize();
    }

    public void Initialize() {
      var pads = LaunchpadInterface.getConnectedLaunchpads();
      foreach(var Pad in pads) {
        cboLaunchpadIn.Items.Add(Pad._midiName);
      }
      cboLaunchpadIn.SelectedIndex = Config.Launchpad;

      for (int n = -1; n < WaveOut.DeviceCount; n++)
      {
        var caps = WaveOut.GetCapabilities(n);
        cboOutputDevice.Items.Add(caps.ProductName);
      }
      cboOutputDevice.SelectedIndex = Config.Output + 1;

      if (Config.PushToTalk == null)
      {
        Config.PushToTalk = new LPPushToTalk()
        {
          Enabled = false,
          MouseButton = PushToTalkButton.MOUSE4
        };
      }

      chkEnablePTT.IsChecked = Config.PushToTalk.Enabled;
      cboPTTKey.SelectedIndex = (int)Config.PushToTalk.MouseButton;
    }

    private void btnSaveClose_Click(object sender, RoutedEventArgs e)
    {
      var configResult = new ConfigDialogResult()
      {
        selectedLaunchpad = cboLaunchpadIn.SelectedIndex,
        selectedOutputDevice = cboOutputDevice.SelectedIndex - 1,
        enablePTT = chkEnablePTT.IsChecked.Value,
        pttButton = cboPTTKey.SelectedIndex
      };

      ConfigResult = configResult;
      DialogResult = true;
      Close();
    }
  }
}
