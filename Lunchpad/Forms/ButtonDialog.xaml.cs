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
using Microsoft.Win32;

namespace Lunchpad
{
  /// <summary>
  /// Interaktionslogik für ButtonDialog.xaml
  /// </summary>
  public partial class ButtonDialog : Window
  {
    public LPButton ButtonConfig;
    public ButtonDialog()
    {
      InitializeComponent();
    }

    public ButtonDialog(LPButton Button) : this() {
      ButtonConfig = Button;
      Title = String.Format("Configuring Button X: {0} / Y: {1}", Button.X, Button.Y);
      txtNormalColor.Text = Button.NormalColor.ToString();
      txtPressedColor.Text = Button.PressedColor.ToString();
      txtCaption.Text = Button.Caption;
      if (Button.Macro != null && Button.Macro.MacroType == LPMacroType.Sound) {
        var Macro = (LPSoundMacro)Button.Macro;
        txtSoundfile.Text = Macro.Soundfile;
        chkLoopable.IsChecked = Macro.Loop;
      } else {
        ButtonConfig.Macro = new LPSoundMacro("");
      }
    }

    private void btnPickSoundfile_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new OpenFileDialog();
      dialog.Filter = "All files (*.*)|*.*|Wave files (*.wav)|*.wav|Vorbis files (*.ogg)|*.ogg|MP3 files (*.mp3)|*.mp3";
      if (dialog.ShowDialog() == true) {
        var Macro = (LPSoundMacro)ButtonConfig.Macro;
        Macro.Soundfile = dialog.FileName;
        txtSoundfile.Text = dialog.FileName;
      }
    }

    private void chkLoopable_Checked(object sender, RoutedEventArgs e)
    {
      var Macro = (LPSoundMacro)ButtonConfig.Macro;
      Macro.Loop = chkLoopable.IsChecked.Value;
      ButtonConfig.Macro = new LPSoundMacro(Macro.Soundfile, Macro.Loop);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }

    private void btnAccept_Click(object sender, RoutedEventArgs e)
    {
      ButtonConfig.NormalColor = int.Parse(txtNormalColor.Text);
      ButtonConfig.PressedColor = int.Parse(txtPressedColor.Text);
      ButtonConfig.Caption = txtCaption.Text;

      DialogResult = true;
      Close();
    }
  }
}
