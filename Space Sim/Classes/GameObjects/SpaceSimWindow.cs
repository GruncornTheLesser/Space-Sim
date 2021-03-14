using System;
using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameObjects
{

    public class SpaceSimWindow : RenderWindow
    {
        private string TextureRes = "512 planet textures/";
        internal BarnesHut QuadTree = new BarnesHut(new Vector2(-1e7f), new Vector2(1e7f));

        internal PressButton HomeButton;
        private PressButton ReScaleButton;
        private PressButton LightingButton;
        private SliderButton TimeSlider;
        private SliderButton AccuSlider;

        internal Action<float> UpdatePosition = (delta) => { };

        internal Sphereoid sun;

        private Vector2 RocketStartPosition;

        public SpaceSimWindow(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            EventManager = new EventManager();
            Camera = new Camera(EventManager, NWS.Size, 100, 300, 0.5f);
            Title = "Space Simulation";

            // preload rocket texture
            TextureManager.Get("Textures/rocket.png");

            HomeButton = new PressButton(this, new Vector2(0.95f), new Vector2(0.1f), "Button_home"); // goes to 0, 0
            ReScaleButton = new PressButton(this, new Vector2(0.95f, 0.85f), new Vector2(0.1f), "Button_resize"); // toggles enlarged or not
            LightingButton = new PressButton(this, new Vector2(0.95f, 0.75f), new Vector2(0.1f), "Button_Lighting"); // toggles directional lighting
            TimeSlider = new SliderButton(this, new Vector2(-0.6f, -0.95f), new Vector2(0.8f, 0.1f)); // changes process speed
            AccuSlider = new SliderButton(this, new Vector2(-0.6f, -0.85f), new Vector2(0.8f, 0.1f)); // changes accuracy of simulation
            
            EventManager.Program_Process += EvolveSimulation;
            EventManager.MouseDown += OnMouseDown;
            EventManager.MouseUp += OnMouseRelease;

            sun = new Sun(this, TextureRes);
            Sphereoid mercury = sun.AddSatellite(4.8e0f, 0.33e24, 5.7e4f, TextureRes, "mercury.png");
            Sphereoid venus = sun.AddSatellite(1.21e1f, 4.87e24, 1.08e5f, TextureRes, "venus.png");
            Sphereoid earth = new Earth(this, TextureRes);
            Sphereoid moon = earth.AddSatellite(4.6e0f, 7.3e22, 3.84e2f, TextureRes, "moon.png");
            Sphereoid mars = sun.AddSatellite(6.78e0f, 0.65e24, 2.05e5f, TextureRes, "mars.png");
            Sphereoid jupiter = sun.AddSatellite(1.43e2f, 1.9e27, 7.41e5f, TextureRes, "jupiter.png");
            Sphereoid saturn = sun.AddSatellite(1.21e2f, 5.7e26, 1.35e6f, TextureRes, "saturn.png");
            //saturn.AddRing();
            Sphereoid uranus = sun.AddSatellite(5.07e1f, 8.7e25, 2.75e6f, TextureRes, "uranus.png");
            Sphereoid neptune = sun.AddSatellite(4.92e1f, 1.0e26, 4.45e6f, TextureRes, "neptune.png");

            HomeButton.Release = () => Camera.WorldPosition = Vector2.Zero;
            ReScaleButton.Release = () =>
            {
                sun.Enlarged = !sun.Enlarged;
                mercury.Enlarged = !mercury.Enlarged;
                venus.Enlarged = !venus.Enlarged;
                earth.Enlarged = !earth.Enlarged;
                moon.Visible = !moon.Visible;
                mars.Enlarged = !mars.Enlarged;
                jupiter.Enlarged = !jupiter.Enlarged;
                saturn.Enlarged = !saturn.Enlarged;
                uranus.Enlarged = !uranus.Enlarged;
                neptune.Enlarged = !neptune.Enlarged;
            };
            LightingButton.Release = () =>
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
            TimeSlider.Set_Percentage = (new_P) => EventManager.Program_Speed = new_P * 24 * 3600 * 365.5f; // 1 year per second
            TimeSlider.Percentage = 0.00f;
        }

        
        private void OnMouseDown(MouseState m, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Button2)
            {
                EventManager.Program_Speed = 0;
                RocketStartPosition = MouseToWorld(m.Position);
            }
        }
        private void OnMouseRelease(MouseState m, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Button2)
            {
                PointMass P = new Rocket(this, RocketStartPosition, (RocketStartPosition - MouseToWorld(m.Position)) * 5e-7f);
                EventManager.Program_Speed = TimeSlider.Percentage * 24 * 3600 * 365.25f;
            }
        }

        float deltaOverflow = 0;
        private void EvolveSimulation(float delta)
        {
            delta += deltaOverflow;
            const int MaxStep = 3600 * 24; // 1 day
            // Max step is the maximum length of time passed before an update. if (delta > MaxStep) Evolve the simulation by 

            // if delta > TimeStep, repeat Timesteps until delta time passed
            while (delta > MaxStep)
            {
                delta -= MaxStep;
                QuadTree.Evolve(MaxStep, AccuSlider.Percentage); //BruteForce(MaxStep); //
                UpdatePosition(MaxStep);
            }

            deltaOverflow = delta;
        }
        private void BruteForce(float delta)
        {
            // O(n^2)
            foreach (PointMass P1 in QuadTree)
            {
                Vector2d Acc = Vector2d.Zero;
                foreach (PointMass P2 in QuadTree)
                {
                    if (P1 != P2) Acc += P1.CalcAccFrom(P2, delta);
                }
                P1.Velocity += Acc * delta;
            }
        }
        
    }
}
