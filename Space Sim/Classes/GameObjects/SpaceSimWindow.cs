using System;
using System.Collections.Generic;
using System.Text;
using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace GameObjects
{

    /* THING TO DO:
     * Starting Velocities
     * Barnes-Hut algorithm
     * launch rocket on click and drag
     * Real Numbers are messing my calculations up from floating point error or something-> find some better ones
     */
    public class SpaceSimWindow : RenderWindow
    {
        private readonly string TextureRes = "512 planet textures/";
        internal static BarnesHut QuadTree = new BarnesHut(new Vector2(-1e7f), new Vector2(1e7f));

        internal static PressButton HomeButton;
        internal static PressButton ReScaleButton;
        internal static PressButton LightingButton;
        internal static SliderButton TimeSlider;


        public SpaceSimWindow(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS, 1)
        {
            HomeButton = new PressButton(new Vector2(0.95f), new Vector2(0.1f), "Button_home"); // goes to 0, 0
            ReScaleButton = new PressButton(new Vector2(0.95f, 0.85f), new Vector2(0.1f), "Button_resize"); // toggles enlarged or not
            LightingButton = new PressButton(new Vector2(0.95f, 0.75f), new Vector2(0.1f), "Button_Lighting"); // toggles directional lighting
            TimeSlider = new SliderButton(new Vector2(-0.6f, -0.95f), new Vector2(0.8f, 0.1f)); // changes process speed

            EventManager.Program_Process += OnProcess;

            //  sun
            Sphereoid sun = new Sun(Vector2.Zero, Vector2.Zero, TextureRes);

            Sphereoid mercury = new Mercury(  new Vector2(57.910f, 0), Vector2.Zero, TextureRes);
            Sphereoid venus = new Venus(      new Vector2(108.210f, 0), Vector2.Zero, TextureRes);
            Sphereoid earth = new Earth(      new Vector2(149.600f, 0), new Vector2(0, 3f), TextureRes);
            Sphereoid moon = new Moon(        new Vector2(149.985f, 0), Vector2.Zero, TextureRes);
            Sphereoid mars = new Mars(        new Vector2(227.920f, 0), Vector2.Zero, TextureRes);
            Sphereoid jupiter = new Jupiter(  new Vector2(778.570f, 0), Vector2.Zero, TextureRes);
            Sphereoid saturn = new Saturn(    new Vector2(1433.530f, 0), Vector2.Zero, TextureRes);
            Sphereoid uranus = new Uranus(    new Vector2(2872.460f, 0), Vector2.Zero, TextureRes);
            Sphereoid neptune = new Neptune(  new Vector2(4495.060f, 0), Vector2.Zero, TextureRes);

            HomeButton.Release += () => Camera.WorldPosition = Vector2.Zero;
            ReScaleButton.Release += () =>
            {
                sun.Enlarged = !sun.Enlarged;
                mercury.Enlarged = !mercury.Enlarged;
                venus.Enlarged = !venus.Enlarged;
                earth.Enlarged = !earth.Enlarged;
                moon.Enlarged = !moon.Enlarged;
                mars.Enlarged = !mars.Enlarged;
                jupiter.Enlarged = !jupiter.Enlarged;
                saturn.Enlarged = !saturn.Enlarged;
                uranus.Enlarged = !uranus.Enlarged;
                neptune.Enlarged = !neptune.Enlarged;
            };
            LightingButton.Release += () =>
            {
                mercury.DirectionalLighting = !mercury.DirectionalLighting;
                venus.DirectionalLighting = !venus.DirectionalLighting;
                earth.DirectionalLighting = !earth.DirectionalLighting;
                moon.DirectionalLighting = !moon.DirectionalLighting;
                mars.DirectionalLighting = !mars.DirectionalLighting;
                jupiter.DirectionalLighting = !jupiter.DirectionalLighting;
                saturn.DirectionalLighting = !saturn.DirectionalLighting;
                uranus.DirectionalLighting = !uranus.DirectionalLighting;
                neptune.DirectionalLighting = !neptune.DirectionalLighting;
            };
            TimeSlider.Set_Percentage += (new_P) => EventManager.Program_Speed = MathF.Min(MathF.Pow(10, new_P * 5), 1000); 
            TimeSlider.Percentage = 0.0f;
            
        }
        private void OnProcess(float delta)
        {
            Title =
                $"MousePos: " +
                $"{MathF.Round(MouseState.Position.X, 2)}," +
                $"{MathF.Round(MouseState.Position.Y, 2)} " +

                $"MouseScreenPos: " +
                $"{MathF.Round(MouseToScreen(MouseState.Position).X, 2)}," +
                $"{MathF.Round(MouseToScreen(MouseState.Position).Y, 2)} " +

                $"MouseWorldPos: " +
                $"{MathF.Round(MouseToWorld(MouseState.Position).X, 2)}," +
                $"{MathF.Round(MouseToWorld(MouseState.Position).Y, 2)} " +

                $"FPS: {1 / delta * EventManager.Program_Speed : 0} "+
                $"TPS: {delta}"; // : 0 truncates to 0 decimal places

            //QuadTree.CalculateForces(); // Calculate new Force through tree

            // Brute Force
            for (int TimeStep = 0; TimeStep < Math.Ceiling(delta); TimeStep++)
            {
                for (int i = 0; i < QuadTree.MassPool.Count; i++)
                {
                    for (int j = 0; j < QuadTree.MassPool.Count; j++)
                    {
                        if (i != j) QuadTree.MassPool[i].Velocity += CalcVel(QuadTree.MassPool[i], QuadTree.MassPool[j]) * MathF.Min(1, delta - TimeStep);
                    }
                }
            /* Only calculate sun
                QuadTree.MassPool[1].Velocity += CalcVel(QuadTree.MassPool[1], QuadTree.MassPool[0]) * MathF.Min(1, delta - TimeStep);
                QuadTree.MassPool[2].Velocity += CalcVel(QuadTree.MassPool[2], QuadTree.MassPool[0]) * MathF.Min(1, delta - TimeStep);
                QuadTree.MassPool[3].Velocity += CalcVel(QuadTree.MassPool[3], QuadTree.MassPool[0]) * MathF.Min(1, delta - TimeStep);
                QuadTree.MassPool[4].Velocity += CalcVel(QuadTree.MassPool[4], QuadTree.MassPool[0]) * MathF.Min(1, delta - TimeStep);
                QuadTree.MassPool[5].Velocity += CalcVel(QuadTree.MassPool[5], QuadTree.MassPool[0]) * MathF.Min(1, delta - TimeStep);
                QuadTree.MassPool[6].Velocity += CalcVel(QuadTree.MassPool[6], QuadTree.MassPool[0]) * MathF.Min(1, delta - TimeStep);
                QuadTree.MassPool[7].Velocity += CalcVel(QuadTree.MassPool[7], QuadTree.MassPool[0]) * MathF.Min(1, delta - TimeStep);
                QuadTree.MassPool[8].Velocity += CalcVel(QuadTree.MassPool[8], QuadTree.MassPool[0]) * MathF.Min(1, delta - TimeStep);
                QuadTree.MassPool[9].Velocity += CalcVel(QuadTree.MassPool[9], QuadTree.MassPool[0]) * MathF.Min(1, delta - TimeStep);
            */
            }
        }
        private static Vector2d CalcVel(PointMass P, PointMass S)
        {
            Vector2d r = P.Position - S.Position;
            Vector2d a = Vector2d.Zero;
            /*
            if (Math.Abs(r.Y) < S.Scale.Y / 2 && Math.Abs(r.X) < S.Scale.X / 2)
            {
                P.Visible = false; // crash
                return Vector2d.Zero;
            }
            */
            if (Math.Abs(r.X) > S.Scale.X / 2) a.X = S.Mass * 6.67e-23 * r.X / Math.Abs(r.X) / Math.Pow(r.X, 2);
            if (Math.Abs(r.Y) > S.Scale.Y / 2) a.Y = S.Mass * 6.67e-23 * r.Y / Math.Abs(r.Y) / Math.Pow(r.Y, 2);
            
            /*
            // speed limit from unpredictable r calculation ie as r -> 0, a -> infinity
            if (Math.Abs(a.X) > 10 || Math.Abs(a.Y) > 10)
            {
                P.Visible = false;
            }
            */

            return a;
        }
    }
}
