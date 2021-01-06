using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Graphics;
using OpenTK.Windowing.Desktop;
namespace Space_Sim
{
    static class Program
    {
       
        static void Main()
        {
            var GWS = new GameWindowSettings();
            var NWS = new NativeWindowSettings();
            NWS.Size = new OpenTK.Mathematics.Vector2i(800, 800);
            using (Window Sim = new Window(GWS, NWS))
            {
                Sim.Run();
            }
        }
    }

}
