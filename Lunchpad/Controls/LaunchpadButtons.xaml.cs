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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lunchpad
{
  /// <summary>
  /// Interaktionslogik für LaunchpadButtons.xaml
  /// </summary>
  public partial class LaunchpadButtons : UserControl
  {
    public class ButtonPosition {
      public int X { get; set; }
      public int Y { get; set; }
      public ButtonPosition(int x, int y) {
        X = x;
        Y = y;
      }
    }

    public class ButtonPressedEventArgs : EventArgs {
      private int _x;
      private int _y;
      
      public int GetX() {
        return _x;
      }
      public int GetY() {
        return _y;
      }

      public ButtonPressedEventArgs(int x, int y) {
        _x = x;
        _y = y;
      }
    }

    public class ButtonSwitchedEventArgs : EventArgs
    {
      public ButtonPosition Source { get; set; }
      public ButtonPosition Target { get; set; }
    }

    public delegate void ButtonPresedEventHandler(object source, ButtonPressedEventArgs e);
    public delegate void ButtonSwitchedEventHandler(object source, ButtonSwitchedEventArgs e);

    public event ButtonPresedEventHandler OnButtonPressed;
    public event ButtonSwitchedEventHandler OnButtonSwitched;

    private Button[,] Buttons;
    private Point startPoint;
    private DragEventArgs lastDrag;

    public LaunchpadButtons()
    {
      InitializeComponent();
      RenderButtons();
    }

    private void RenderButtons() {
      Buttons = new Button[8, 8];
      for (int x = 0; x < 8; x++) {
        for (int y = 0; y < 8; y++) {
          var btn = new Button();
          Grid.SetRow(btn, y);
          Grid.SetColumn(btn, x);
          btn.Click += Btn_Click; //new ButtonPresedEventHandler(ButtonPressed);

          btn.Tag = new ButtonPosition(x, y);
          Buttons[x, y] = btn;

          var lbl = new Label();
          lbl.Foreground = new SolidColorBrush(Colors.White);
          lbl.FontWeight = FontWeights.Bold;
          lbl.FontSize = 16;
          lbl.Effect = new DropShadowEffect
          {
            Color = new Color { A = 255, R = 0, G = 0, B = 0 },
            Direction = 0,
            ShadowDepth = 0,
            BlurRadius = 2,
            Opacity = 1
          };

          btn.HorizontalContentAlignment = HorizontalAlignment.Stretch;
          btn.VerticalContentAlignment = VerticalAlignment.Stretch;
          lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
          lbl.VerticalContentAlignment = VerticalAlignment.Center;
          btn.Content = lbl;
          btn.AllowDrop = true;
          btn.PreviewMouseLeftButtonDown += Btn_PreviewMouseLeftButtonDown;
          btn.PreviewMouseMove += Btn_MouseMove;
          
          btn.DragEnter += Btn_DragEnter;

          btn.PreviewDrop += Btn_Drop;
          
          ButtonGrid.Children.Add(btn);
        }
      }
    }

    private void Btn_DragEnter(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent("SourcePos") || sender == e.Source) {
        e.Effects = DragDropEffects.None;
      }
    }

    private void Btn_Drop(object sender, DragEventArgs e)
    {
      if (lastDrag == null) {
        lastDrag = e;
        return;
      }
      
      if (e.Data.GetDataPresent("SourcePos"))
      {
        ButtonPosition SourcePos = e.Data.GetData("SourcePos") as ButtonPosition;
        ButtonPosition TargetPos = ((Button)sender).Tag as ButtonPosition;
        OnButtonSwitched?.Invoke(this, new ButtonSwitchedEventArgs() { Source = SourcePos, Target = TargetPos });
        //System.Diagnostics.Debug.Print(String.Format("Droped from: {0}, {1}", SourcePos.X, SourcePos.Y));
        //System.Diagnostics.Debug.Print(e.Handled.ToString());
      }

      lastDrag = null;
    }

    private void Btn_MouseMove(object sender, MouseEventArgs e)
    {
      Point mousePos = e.GetPosition(null);
      Vector diff = startPoint - mousePos;

      if (e.LeftButton == MouseButtonState.Pressed && (
       Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
       Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
      {
        ButtonPosition pos = ((Button)sender).Tag as ButtonPosition;
        var Data = new DataObject("SourcePos", pos);
        DragDrop.DoDragDrop((DependencyObject)sender, Data, DragDropEffects.Move);
      }
    }

    private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      startPoint = e.GetPosition(null);
    }

    private void Btn_Click(object sender, RoutedEventArgs e)
    {
      if(OnButtonPressed != null)  {
        var data = (ButtonPosition)((Button)sender).Tag;
        OnButtonPressed(this, new ButtonPressedEventArgs(data.X, data.Y));
      }
    }

    public Button GetButton(int x, int y) {
      return Buttons[x, y];
    }
  
    public void UpdateButtons(List<LPButton> Buttons) {
      foreach(var Button in Buttons) {
        var CurrentButton = GetButton(Button.X, Button.Y);
        var lbl = (Label)CurrentButton.Content;
        lbl.Content = Button.Caption;

        CurrentButton.Background = (SolidColorBrush) (new BrushConverter().ConvertFrom(LaunchpadColors.Colors[Button.NormalColor]));
      }
    }
  }
}
