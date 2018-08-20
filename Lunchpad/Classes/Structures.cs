using System.Collections.Generic;
using System.Linq;

using NAudio.Wave;
using NAudio.Vorbis;

using System.Xml.Serialization;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Lunchpad
{
  public enum PushToTalkButton {
    MIDDLE,
    MOUSE4,
    MOUSE5
  }

  public class LPPushToTalk {
    public bool Enabled { get; set; }
    public PushToTalkButton MouseButton { get; set; }
  }

  public class LPConfig {
    public List<LPPage> Pages { get; set; }

    public int Launchpad { get; set; }
    public int Output { get; set; }
    public LPPushToTalk PushToTalk { get; set; }

    public LPConfig() {
      Pages = new List<LPPage>();
    }
    
    public void Initialize() {
      Pages.Clear();
      var Page = new LPPage(0);
      Page.Initialize();
      Pages.Add(Page);
    }

    public LPPage GetPage(int number) {
      if (number >= Pages.Count)
      {
        return null;
      }
      else
      {
        return Pages[number];
      }
    }

    public void Save(string filename) {
      using (var writer = new System.IO.StreamWriter(filename))
      {
        var serializer = new XmlSerializer(this.GetType());
        serializer.Serialize(writer, this);
        writer.Flush();
      }
    }

    public static LPConfig Load(string filename) {
      using (var stream = System.IO.File.OpenRead(filename))
      {
        var serializer = new XmlSerializer(typeof(LPConfig));
        return serializer.Deserialize(stream) as LPConfig;
      }
    }
  }

  public class LPPage {
    public int Number { get; set; }
    public List<LPButton> Buttons { get; set;  }
   
    public LPPage() {
      Buttons = new List<LPButton>();
      /*for(int x = 0; x < 8; x++) {
        for(int y = 0; y < 8; y++) {
          Buttons.Add(new LPButton(x, y));
        }
      }*/
    }

    public void Initialize() {
      Buttons.Clear();
      for(int x = 0; x < 8; x++) {
        for(int y = 0; y < 8; y++) {
          Buttons.Add(new LPButton(x, y));
        }
      }
    }

    public LPPage(int number) : this() {
      Number = number;
    }

    public LPButton GetButton(int index)
    {
      if (index >= Buttons.Count ) {
        return null;
      } else {
        return Buttons[index];
      }
    }

    public LPButton GetButton(int x, int y) {
      return Buttons.Where(e => e.X == x && e.Y == y).FirstOrDefault();
    }
  }
  
  public class LPButton {
    

    public int X { get; set; }
    public int Y { get; set; }
    
    public string Caption { get; set; }

    public int NormalColor { get; set; }
    public int PressedColor { get; set; }

    public LPMacro Macro { get; set; }
    
    public LPButton() {
      Caption = "";
    }
    public LPButton(int x, int y) : this() {
      X = x;
      Y = y;
    }
  }

  [XmlInclude(typeof(LPSoundMacro))]
  public class LPMacro {
    public LPMacroType MacroType { get; set; }

    public LPMacro() {
      MacroType = LPMacroType.None;
    }
  }

  public class LPSoundMacro : LPMacro {
    

    public string Soundfile { get; set;}
    public bool Loop { get; set; }

    public LPSoundMacro() {

    }

    public LPSoundMacro(string soundfile, bool loop = false) : this() {
      MacroType = LPMacroType.Sound;
      Soundfile = soundfile;
      Loop = loop;      
    }
  }
  
  public enum LPMacroType {
    None,   
    Sound
  }
}
