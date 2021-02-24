using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using GameObjects;

namespace Space_Sim
{
    static class Program
    {
        static void Main()
        {
            var GWS = new GameWindowSettings();
            var NWS = new NativeWindowSettings();

            NWS.Size = new Vector2i(800, 800);
            using (SpaceSimWindow Sim = new SpaceSimWindow(GWS, NWS))
            {
                Sim.Run();
            }
            
        }
    }

}
