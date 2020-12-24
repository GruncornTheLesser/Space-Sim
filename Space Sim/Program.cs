using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Maths;
using Graphics;
using OpenTK.Windowing.Desktop;
namespace Space_Sim
{
    static class Program
    {
       
        static void Main()
        {
            using (Window Sim = new Window(GameWindowSettings.Default, NativeWindowSettings.Default))
            {
                Sim.Run();
            }
        }
    }

}
