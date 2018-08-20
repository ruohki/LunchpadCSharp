using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using NAudio.Vorbis;
using System.Runtime.InteropServices;

namespace Lunchpad
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref Win32Point pt);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point
    {
      public Int32 X;
      public Int32 Y;
    };
    public static Point GetMousePosition()
    {
      Win32Point w32Mouse = new Win32Point();
      GetCursorPos(ref w32Mouse);
      return new Point(w32Mouse.X, w32Mouse.Y);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
    const int MIDDLEDOWN = 0x0020;
    const int MIDDLEUP = 0x0040;
    const int XDOWN = 0x00000080;
    const int XUP = 0x00000100;
    const int XBUTTON1 = 0x00000001;
    const int XBUTTON2 = 0x00000002;

    string configFile;
    LaunchpadInterface Launchpad;
   
    LPConfig Config;
    LPPage SelectedPage;
     HashSet<KeyValuePair<string, WaveOut>> WaveStack = new HashSet<KeyValuePair<string, WaveOut>>();

    public MainWindow()
    {
      InitializeComponent();

      this.Top = Properties.Settings.Default.Top;
      this.Left = Properties.Settings.Default.Left;
      this.Height = Properties.Settings.Default.Height;
      this.Width = Properties.Settings.Default.Width;
      // Very quick and dirty - but it does the job
      if (Properties.Settings.Default.Maximized)
      {
        WindowState = WindowState.Maximized;
      }

      FileInfo exeFile = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
      configFile = String.Format("{0}\\config.xml", exeFile.DirectoryName);
      if (!File.Exists(configFile)) {
        LPConfig newConfig = new LPConfig();
        for (int i = 0; i < 8; i++) {
          var Page = new LPPage(i);
          Page.Initialize();
          newConfig.Pages.Add(Page);
        }
        newConfig.Save(configFile);
      }

      Launchpad = new LaunchpadInterface();
      

      Launchpad.OnLaunchpadKeyPressed += Launchpad_OnLaunchpadKeyPressed;
      Launchpad.OnLaunchpadTopKeyPressed += Launchpad_OnLaunchpadTopKeyPressed;
      Launchpad.OnLaunchpadCCKeyPressed += Launchpad_OnLaunchpadCCKeyPressed;


      ((App)Application.Current).Config = LPConfig.Load(configFile);
      Config = ((App)Application.Current).Config;

      Initialize();
      SelectedPage = Config.Pages.FirstOrDefault();

      BuildLight();
      BuildButtons();
    }
    
    public void Initialize() {
      Launchpad.Disconnect();

      var Interfaces = LaunchpadInterface.getConnectedLaunchpads();
      Launchpad.Connect(Interfaces[Config.Launchpad]);
    }

    private void Launchpad_OnLaunchpadCCKeyPressed(object source, LaunchpadInterface.LaunchpadCCKeyEventArgs e)
    {
      //Stop All
      if (e.GetVal() == 7) {
        if (e.GetPressed()) {
          Launchpad.setSideLED(7, 72);
          ReleasePushToTalk();
          var tempStack = new List<WaveOut>();
          foreach (var Pair in WaveStack) {
            tempStack.Add(Pair.Value);
          }

          foreach(var Wave in tempStack) {
            Wave.Stop();
          }

        } else {
          Launchpad.setSideLED(7, 0);
        }
      }
    }

    private void Launchpad_OnLaunchpadTopKeyPressed(object source, LaunchpadInterface.LaunchpadTOPKeyEventArgs e)
    {
      var pageExists = Config.GetPage(e.GetVal()) != null;

      if (e.GetPressed()) {
        if (pageExists) {
          Launchpad.setTopLEDs(e.GetVal(), 52);
          SelectPage(e.GetVal());
        } else {
          Launchpad.setTopLEDs(e.GetVal(), 72);
        }
      } else {
        if (SelectedPage.Number != e.GetVal()) {
          Launchpad.setTopLEDs(e.GetVal(), 0);
        }
      }      
    }

    private void BuildLight() {
      for (int x = 0; x < 8; x++) {
        for(int y = 0; y < 8; y++) {
          LPButton Button = SelectedPage.GetButton(x,y);
          Launchpad.setLED(x, y, Button.NormalColor);
        }
      }
    }
    
    private void BuildButtons() {
      new Thread(() =>
      {
        this.Dispatcher.BeginInvoke(new Action(() =>
        {
          Buttons.UpdateButtons(SelectedPage.Buttons);
        }
        ));
      }).Start();
    }

    private void SelectPage(int Number) {
      if ((Config.Pages.Count > Number) && (Number >= 0)) {
        if (SelectedPage.Number > 7) {
          Launchpad.setSideLED(SelectedPage.Number - 8, 0);
        } else {
          Launchpad.setTopLEDs(SelectedPage.Number, 0);
        }

        SelectedPage = Config.GetPage(Number);
        if (SelectedPage.Number > 7)
        {
          Launchpad.setSideLED(Number - 8, 52);
        }
        else
        {
          Launchpad.setTopLEDs(Number, 52);
        }

        BuildLight();
        BuildButtons();

      }
    }

    private void Launchpad_OnLaunchpadKeyPressed(object source, LaunchpadInterface.LaunchpadKeyEventArgs e)
    {
      var PressedButton = SelectedPage.GetButton(e.GetX(), e.GetY());

      // Light
      Launchpad.setLED(PressedButton.X, PressedButton.Y, e.GetPressed() ? PressedButton.PressedColor : PressedButton.NormalColor);

      // Macro
      var Macro = PressedButton.Macro;
      if (Macro != null) {
        switch (Macro.MacroType)
        {
          case LPMacroType.Sound:
            LPSoundMacro SoundMacro = (LPSoundMacro)Macro;
            if (e.GetPressed()) {
              PlaySound(SoundMacro.Soundfile, SoundMacro.Loop);
            } else {
              if (SoundMacro.Loop) {
                StopSound(SoundMacro.Soundfile);
              }
            }
            break;
        }
      }
      /*var CurrentButton = CurrentPage.Buttons[e.GetX(), e.GetY()];
      LaunchpadMacro CurrentMacro = CurrentButton.Macro;

      if (e.GetPressed()) {
        CurrentButton.SetColor(CurrentButton.ButtonDownColor);
        
        if(CurrentMacro != null) {
          switch (CurrentMacro.MacroType) {
            case MacroTypeEnum.PlaySound:
              LaunchpadSoundMacro Macro = (LaunchpadSoundMacro)CurrentMacro;
              Macro.Play();
              break;
          }
        }
      } else {
        if (CurrentMacro != null)
        {
          switch (CurrentMacro.MacroType)
          {
            case MacroTypeEnum.PlaySound:
              LaunchpadSoundMacro Macro = (LaunchpadSoundMacro)CurrentMacro;
              if (Macro.Looping) {
                Macro.Stop();
              }
              break;
          }
        }
        CurrentButton.SetColor(CurrentButton.ButtonColor);
      }*/
      /*Random r = new Random();
      if (e.GetPressed())
      {
        System.Diagnostics.Debug.Print(String.Format("X: {0}, Y: {1}", e.GetX(), e.GetY()));
        Launchpad.setLED(, e.GetY(), r.Next(0, 127));
      }*/
    }

    private void PressPushToTalk() {
      if (Config.PushToTalk == null || !Config.PushToTalk.Enabled) return;
      Point mouseLoc = GetMousePosition();
      uint Button = XUP;
      uint XButton = 0;

      switch (Config.PushToTalk.MouseButton)
      {
        case PushToTalkButton.MIDDLE:
          Button = MIDDLEDOWN;
          break;
        case PushToTalkButton.MOUSE4:
          Button = XDOWN;
          XButton = XBUTTON1;
          break;
        case PushToTalkButton.MOUSE5:
          Button = XDOWN;
          XButton = XBUTTON2;
          break;
      }
      mouse_event(Button, (uint)mouseLoc.X, (uint)mouseLoc.Y, XButton, 0);
    }

    private void ReleasePushToTalk() {
      if (Config.PushToTalk == null || !Config.PushToTalk.Enabled) return;
      Point mouseLoc = GetMousePosition();
      uint Button = XUP;
      uint XButton = 0;

      switch (Config.PushToTalk.MouseButton) {
        case PushToTalkButton.MIDDLE:
          Button = MIDDLEUP;
          break;
        case PushToTalkButton.MOUSE4:
          Button = XUP;
          XButton = XBUTTON1;
          break;
        case PushToTalkButton.MOUSE5:
          Button = XUP;
          XButton = XBUTTON2;
          break;
      }
      mouse_event(Button, (uint)mouseLoc.X, (uint)mouseLoc.Y, XButton, 0);
    }

    private void PlaySound(string filename, bool Loop = false) {
      var file = new FileInfo(filename);

      IWaveProvider reader;
      switch (file.Extension.ToLower())
      {
        case ".ogg":
          reader = new VorbisWaveReader(filename);
          break;
        case ".mp3":
          reader = new Mp3FileReader(filename);
          break;
        default:
          reader = new WaveFileReader(filename);
          break;
      }

      var currentWaveOut = new WaveOut();
      currentWaveOut.DeviceNumber = Config.Output;

      if (Loop)
      {
        LoopStream loop = new LoopStream(reader);
        currentWaveOut.Init(loop);
      }
      else
      {
        currentWaveOut.Init(reader);
      }
      currentWaveOut.PlaybackStopped += CurrentWaveOut_PlaybackStopped; ;
      WaveStack.Add(new KeyValuePair<string, WaveOut>(filename, currentWaveOut));
      currentWaveOut.Play();

      PressPushToTalk();
    }

    private void CurrentWaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
    {
      WaveStack.RemoveWhere(p => p.Value.PlaybackState == PlaybackState.Stopped);
      if (WaveStack.Count <= 0) {
        ReleasePushToTalk();

        Point mouseLoc = GetMousePosition();
        mouse_event(XUP, (uint)mouseLoc.X, (uint)mouseLoc.Y, XBUTTON1, 0);
      }
    }

    private void StopSound(string filename) {
      List<WaveOut> tempStack = new List<WaveOut>();
      foreach (var Pair in WaveStack) {
        if (Pair.Key == filename) {
          tempStack.Add(Pair.Value);
        }
      }

      foreach(var Wave in tempStack) {
        Wave.Stop();
        Wave.Dispose();
      }

      WaveStack.RemoveWhere(p => p.Key == filename);
    }

    private void Buttons_OnButtonPressed(object source, LaunchpadButtons.ButtonPressedEventArgs e)
    {
      var buttonConfigDialog = new ButtonDialog(SelectedPage.GetButton(e.GetX(), e.GetY()));
      if (buttonConfigDialog.ShowDialog() == true) {
        var result = buttonConfigDialog.ButtonConfig;
        var btn = SelectedPage.GetButton(result.X, result.Y);
        btn.Macro = result.Macro;
        btn.NormalColor = result.NormalColor;
        btn.PressedColor = result.PressedColor;
        btn.Caption = result.Caption;

        BuildLight();
        BuildButtons();
        Config.Save(configFile);
      }
    }

    private void btnConfig_Click(object sender, RoutedEventArgs e)
    {
      ConfigDialog confDialog = new ConfigDialog(Config);
      if (confDialog.ShowDialog() == true) {
        var result = confDialog.ConfigResult;
        Config.Launchpad = result.selectedLaunchpad;
        Config.Output = result.selectedOutputDevice;
        Config.PushToTalk = new LPPushToTalk()
        {
          Enabled = result.enablePTT,
          MouseButton = (PushToTalkButton)result.pttButton
        };
        Config.Save(configFile);
      };
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (WindowState == WindowState.Maximized)
      {
        // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
        Properties.Settings.Default.Top = RestoreBounds.Top;
        Properties.Settings.Default.Left = RestoreBounds.Left;
        Properties.Settings.Default.Height = RestoreBounds.Height;
        Properties.Settings.Default.Width = RestoreBounds.Width;
        Properties.Settings.Default.Maximized = true;
      }
      else
      {
        Properties.Settings.Default.Top = this.Top;
        Properties.Settings.Default.Left = this.Left;
        Properties.Settings.Default.Height = this.Height;
        Properties.Settings.Default.Width = this.Width;
        Properties.Settings.Default.Maximized = false;
      }

      Properties.Settings.Default.Save();
    }

    private void Buttons_OnButtonSwitched(object source, LaunchpadButtons.ButtonSwitchedEventArgs e)
    {
      System.Diagnostics.Debug.Print(String.Format("Switching {0},{1} with {2},{3}", e.Source.X, e.Source.Y, e.Target.X, e.Target.Y));
      var SourceButton = SelectedPage.GetButton(e.Source.X, e.Source.Y);
      var TargetButton = SelectedPage.GetButton(e.Target.X, e.Target.Y);
      SourceButton.X = e.Target.X;
      SourceButton.Y = e.Target.Y;
      TargetButton.X = e.Source.X;
      TargetButton.Y = e.Source.Y;

      BuildLight();
      BuildButtons();
      Config.Save(configFile);
    }
  }
}
