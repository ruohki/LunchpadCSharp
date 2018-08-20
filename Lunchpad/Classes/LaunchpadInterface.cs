using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Midi.Devices;
using Midi.Enums;
using Midi.Messages;

namespace Lunchpad
{
  public class LaunchpadInterface
  {
    private Pitch[,] notes = new Pitch[8, 8] {
            { Pitch.A5, Pitch.ASharp5, Pitch.B5, Pitch.C6, Pitch.CSharp6, Pitch.D6, Pitch.DSharp6, Pitch.E6 },
            { Pitch.B4, Pitch.C5, Pitch.CSharp5, Pitch.D5, Pitch.DSharp5, Pitch.E5, Pitch.F5, Pitch.FSharp5 },
            { Pitch.CSharp4, Pitch.D4, Pitch.DSharp4, Pitch.E4, Pitch.F4, Pitch.FSharp4, Pitch.G4, Pitch.GSharp4 },
            { Pitch.DSharp3, Pitch.E3, Pitch.F3, Pitch.FSharp3, Pitch.G3, Pitch.GSharp3, Pitch.A3, Pitch.ASharp3 },
            { Pitch.F2, Pitch.FSharp2, Pitch.G2, Pitch.GSharp2, Pitch.A2, Pitch.ASharp2, Pitch.B2, Pitch.C3 },
            { Pitch.G1, Pitch.GSharp1, Pitch.A1, Pitch.ASharp1, Pitch.B1, Pitch.C2, Pitch.CSharp2, Pitch.D2 },
            { Pitch.A0, Pitch.ASharp0, Pitch.B0, Pitch.C1, Pitch.CSharp1, Pitch.D1, Pitch.DSharp1, Pitch.E1 },
            { Pitch.BNeg1, Pitch.C0, Pitch.CSharp0, Pitch.D0, Pitch.DSharp0, Pitch.E0, Pitch.F0, Pitch.FSharp0 }
        };

    private Pitch[] rightLEDnotes = new Pitch[] {
            Pitch.F6, Pitch.G5, Pitch.A4, Pitch.B3, Pitch.CSharp3, Pitch.DSharp2, Pitch.F1, Pitch.G0
        };

    public InputDevice targetInput;
    public OutputDevice targetOutput;

    public delegate void LaunchpadKeyEventHandler(object source, LaunchpadKeyEventArgs e);

    public delegate void LaunchpadCCKeyEventHandler(object source, LaunchpadCCKeyEventArgs e);
    public delegate void LaunchpadTOPKeyEventHandler(object source, LaunchpadTOPKeyEventArgs e);

    public event LaunchpadKeyEventHandler OnLaunchpadKeyPressed;
    public event LaunchpadCCKeyEventHandler OnLaunchpadCCKeyPressed;
    public event LaunchpadTOPKeyEventHandler OnLaunchpadTopKeyPressed;

    public class LaunchpadTOPKeyEventArgs : EventArgs
    {
      private bool pressed;
      private int val;
      public LaunchpadTOPKeyEventArgs(int _val, bool _pressed)
      {
        pressed = _pressed;
        val = _val;
      }
      public bool GetPressed()
      {
        return pressed;
      }
      public int GetVal()
      {
        return val;
      }
    }
    public class LaunchpadCCKeyEventArgs : EventArgs
    {
      private bool pressed;
      private int val;
      public LaunchpadCCKeyEventArgs(int _val, bool _pressed)
      {
        pressed = _pressed;
        val = _val;
      }
      public int GetVal()
      {
        return val;
      }
      public bool GetPressed()
      {
        return pressed;
      }
    }

    public class LaunchpadKeyEventArgs : EventArgs
    {
      private bool pressed;
      private int x;
      private int y;

      public LaunchpadKeyEventArgs(int _pX, int _pY, bool _pressed)
      {
        x = _pX;
        y = _pY;
        pressed = _pressed;
      }
      public int GetX()
      {
        return x;
      }
      public int GetY()
      {
        return y;
      }
      public bool GetPressed()
      {
        return pressed;
      }
    }

    private void sysExAnswer(SysExMessage m)
    {
      byte[] msg = m.Data;
      byte[] stopBytes = { 240, 0, 32, 41, 2, 24, 21, 247 };
    }

    private void midiPress(NoteOnMessage msg)
    {
      if (OnLaunchpadKeyPressed != null && !rightLEDnotes.Contains(msg.Pitch))
      {
        OnLaunchpadKeyPressed(this, new LaunchpadKeyEventArgs(midiNoteToLed(msg.Pitch)[0], midiNoteToLed(msg.Pitch)[1], msg.Velocity == 127 ? true : false));
      }
      else if (OnLaunchpadCCKeyPressed != null && rightLEDnotes.Contains(msg.Pitch))
      {
        OnLaunchpadCCKeyPressed(this, new LaunchpadCCKeyEventArgs(midiNoteToSideLED(msg.Pitch), msg.Velocity == 127 ? true : false));
      }
    }

    private void controlPress(ControlChangeMessage msg)
    {
      OnLaunchpadTopKeyPressed?.Invoke(this, new LaunchpadTOPKeyEventArgs(int.Parse(msg.Control.ToString()) - 104, msg.Value == 127 ? true : false));
    }

    public int midiNoteToSideLED(Pitch p)
    {
      for (int y = 0; y <= 7; y++)
      {
        if (rightLEDnotes[y] == p)
        {
          return y;
        }
      }
      return 0;
    }

    public int[] midiNoteToLed(Pitch p)
    {
      for (int x = 0; x <= 7; x++)
      {
        for (int y = 0; y <= 7; y++)
        {
          if (notes[x, y] == p)
          {
            int[] r1 = { y, x };
            return r1;
          }
        }
      }
      int[] r2 = { 0, 0 };
      return r2;
    }

    public Pitch ledToMidiNote(int x, int y)
    {
      return notes[x, y];
    }

    public void clearAllLEDs()
    {
      for (int x = 0; x < 8; x++)
      {
        for (int y = 0; y < 8; y++)
        {
          setLED(x, y, 0);
        }
      }

      for (int ry = 0; ry < 8; ry++)
      {
        setSideLED(ry, 0);
      }

      for (int tx = 1; tx < 9; tx++)
      {
        setTopLEDs(tx, 0);
      }
    }

    public void fillTopLEDs(int startX, int endX, int velo)
    {
      for (int x = 1; x < 9; x++)
      {
        if (x >= startX && x <= endX)
        {
          setTopLEDs(x, velo);
        }
      }
    }

    public void fillSideLEDs(int startY, int endY, int velo)
    {
      for (int y = 0; y < rightLEDnotes.Length; y++)
      {
        if (y >= startY && y <= endY)
        {
          setSideLED(y, velo);
        }
      }
    }

    public void fillLEDs(int startX, int startY, int endX, int endY, int velo)
    {
      for (int x = 0; x < notes.Length; x++)
      {
        for (int y = 0; y < notes.Length; y++)
        {
          if (x >= startX && y >= startY && x <= endX && y <= endY)
            setLED(x, y, velo);
        }
      }
    }


    public void setTopLEDs(int x, int velo)
    {
      byte[] data = { 240, 0, 32, 41, 2, 24, 10, Convert.ToByte(104 + x), Convert.ToByte(velo), 247 };
      targetOutput.SendSysEx(data);
    }


    public void setSideLED(int y, int velo)
    {
      targetOutput.SendNoteOn(Channel.Channel1, rightLEDnotes[y], velo);
    }


    public void setLED(int x, int y, int velo)
    {
      try
      {
        targetOutput.SendNoteOn(Channel.Channel1, notes[y, x], velo);
      }
      catch (Exception ex)
      {
        Console.WriteLine("<< LAUNCHPAD.NET >> Midi.DeviceException");
        throw;
      }
    }

    public static LaunchpadDevice[] getConnectedLaunchpads()
    {
      List<LaunchpadDevice> tempDevices = new List<LaunchpadDevice>();

      foreach (InputDevice id in DeviceManager.InputDevices)
      {
        foreach (OutputDevice od in DeviceManager.OutputDevices)
        {
          if (id.Name == od.Name)
          {
            if (id.Name.ToLower().Contains("launchpad"))
            {
              tempDevices.Add(new LaunchpadDevice(id.Name));
            }
          }
        }
      }

      return tempDevices.ToArray();
    }


    public bool Connect(LaunchpadDevice device)
    {
      foreach (InputDevice id in DeviceManager.InputDevices)
      {
        if (id.Name.ToLower() == device._midiName.ToLower())
        {
          targetInput = id;
          id.Open();

          targetInput.ControlChange += new ControlChangeHandler(controlPress);
          targetInput.NoteOn += new NoteOnHandler(midiPress);
          targetInput.StartReceiving(null);
        }
      }
      foreach (OutputDevice od in DeviceManager.OutputDevices)
      {
        if (od.Name.ToLower() == device._midiName.ToLower())
        {
          targetOutput = od;
          od.Open();
        }
      }

      return true; // targetInput.IsOpen && targetOutput.IsOpen;
    }


    public bool Disconnect()
    {
      if (targetInput == null && targetOutput == null) return true;
      if (targetInput.IsOpen && targetOutput.IsOpen)
      {
        targetInput.StopReceiving();
        targetInput.Close();
        targetOutput.Close();
      }
      return !targetInput.IsOpen && !targetOutput.IsOpen;
    }

    public class LaunchpadDevice
    {
      public string _midiName;
      //public int _midiDeviceId;

      public LaunchpadDevice(string name)
      {
        _midiName = name;
      }
    }
  }
}
