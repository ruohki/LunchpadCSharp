using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Lunchpad
{

  public static class LaunchpadColors
  {
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
    private const int MOUSEEVENTF_RIGHTUP = 0x10;

    public static String[] Colors = new String[] {
      "#000000",
      "#1c1c1c",
      "#7c7c7c",
      "#fcfcfc",
      "#ff4e48",
      "#fe0a00",
      "#5a0000",
      "#180002",

      "#ffbc61",
      "#ff5700",
      "#5a1d00",
      "#241802",
      "#fdfd21",
      "#fdfd00",
      "#585800",
      "#181800",

      "#81fd2b",
      "#40fd01",
      "#165800",
      "#132801",
      "#35fd2b",
      "#00fe00",
      "#005801",
      "#001800",

      "#2fff40",
      "#00fe00",
      "#005801",
      "#001800",
      "#32fd7f",
      "#00fd3a",
      "#015814",
      "#001c0e",

      "#2ffcb1",
      "#00fb91",
      "#015732",
      "#011810",
      "#39beff",
      "#00a7ff",
      "#014051",
      "#001018",

      "#4186ff",
      "#0050ff",
      "#011a5a",
      "#010619",
      "#4747ff",
      "#0000fe",
      "#00005a",
      "#000116",

      "#8347ff",
      "#5000ff",
      "#160067",
      "#0a0032",
      "#ff48fe",
      "#ff00fc",
      "#5a005a",
      "#180018",

      "#fc4e81",
      "#ff0753",
      "#5a021b",
      "#210110",
      "#ff1901",
      "#9a3500",
      "#7a5101",
      "#3c6401",

      "#013800",
      "#005432",
      "#00537f",
      "#0000fe",
      "#00444d",
      "#1a00d1",
      "#7c7c7c",
      "#202020",

      "#ff0705",
      "#bafd00",
      "#acec00",
      "#56fd00",
      "#008800",
      "#01fc7b",
      "#00a7ff",
      "#001cfb",

      "#3500ff",
      "#7800ff",
      "#b4177e",
      "#412000",
      "#ff4a01",
      "#82e100",
      "#66fd00",
      "#00fe00",

      "#00ff04",
      "#45fd61",
      "#01fbcb",
      "#5086ff",
      "#274dc8",
      "#847aed",
      "#d30cff",
      "#ff065a",

      "#ff7d01",
      "#b8b100",
      "#8afd00",
      "#815d00",
      "#3a2802",
      "#0d4c05",
      "#005037",
      "#131429",

      "#121f53",
      "#6a3c18",
      "#ac0401",
      "#e15136",
      "#dc6900",
      "#fee100",
      "#99e101",
      "#60b500",

      "#1b1c31",
      "#dcfd54",
      "#76fbb9",
      "#9698ff",
      "#8b62ff",
      "#404040",
      "#747474",
      "#defcfc",

      "#9f0601",
      "#340100",
      "#00d201",
      "#004101",
      "#b8b100",
      "#3c3000",
      "#b45d00",
      "#491404",

    };
  }
}
