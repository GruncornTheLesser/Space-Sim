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
     * +- problems
     */
    public class SpaceSimWindow : RenderWindow
    {
        private readonly string TextureRes = "512 planet textures/";
        internal static BarnesHut QuadTree = new BarnesHut(new Vector2(-1e7f), new Vector2(1e7f));

        internal static PressButton HomeButton;
        internal static PressButton ReScaleButton;
        internal static PressButton LightingButton;
        internal static SliderButton TimeSlider;

        internal static Action<float> UpdatePosition = (delta) => { };

        internal static Sphereoid sun;

        public SpaceSimWindow(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS, 1)
        {
            HomeButton = new PressButton(new Vector2(0.95f), new Vector2(0.1f), "Button_home"); // goes to 0, 0
            ReScaleButton = new PressButton(new Vector2(0.95f, 0.85f), new Vector2(0.1f), "Button_resize"); // toggles enlarged or not
            LightingButton = new PressButton(new Vector2(0.95f, 0.75f), new Vector2(0.1f), "Button_Lighting"); // toggles directional lighting
            TimeSlider = new SliderButton(new Vector2(-0.6f, -0.95f), new Vector2(0.8f, 0.1f)); // changes process speed

            EventManager.Program_Process += OnProgramProcess;

            //  sun
            sun = new Sun(TextureRes);
            sun.Mass = 0;
            Sphereoid mercury = new Mercury(TextureRes);
            Sphereoid venus = new Venus(TextureRes);
            Sphereoid earth = new Earth(TextureRes);
            earth.Velocity = Vector2d.Zero;
            Sphereoid moon = earth.AddSatellite(3.47e0f, 7.3e22, 3.84e2f, TextureRes, "moon.png");
            //Sphereoid moon = new Moon(TextureRes);
            Sphereoid mars = new Mars(TextureRes);
            Sphereoid jupiter = new Jupiter(TextureRes);
            Sphereoid saturn = new Saturn(TextureRes);
            Sphereoid uranus = new Uranus(TextureRes);
            Sphereoid neptune = new Neptune(TextureRes);

            HomeButton.Release += () => Camera.WorldPosition = Vector2.Zero;
            ReScaleButton.Release += () =>
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
            TimeSlider.Set_Percentage += (new_P) => EventManager.Program_Speed = new_P * 24 * 3600 * 365; // 1 year per second // makes line become unstable passed 1 year
            TimeSlider.Percentage = 0.0f;
            
        }
        private void OnProgramProcess(float delta)
        {
            int MaxStep = 3600 * 24; // 1 day
            // Max step is the maximum length of time passed before an update. if (delta > MaxStep) Evolve the simulation by 

            // if delta > TimeStep, repeat Timesteps until delta time passed
            while (delta > MaxStep)
            {
                delta -= MaxStep;
                BruteForce(MaxStep);//EvolveBarnesHut(MaxStep);//

                Vector2d V = QuadTree.MassPool[0].Velocity;

                //(2.65313411713586E-08, 0) -> BarnesHut θ = 0
                //(2.65313411713586E-08, 0) -> BruteForce 
            }
            if (delta != 0) BruteForce(delta);//EvolveBarnesHut(delta);//


        }
        private void BruteForce(float dt)
        {
            // O(n^2)
            foreach(PointMass P1 in QuadTree)
            {
                Vector2d Acc = Vector2d.Zero;
                foreach (PointMass P2 in QuadTree)
                {
                    if (P1 != P2) Acc += P1.CalcAccFrom(P2, dt);
                }
                P1.Velocity += Acc * dt;
            }
            UpdatePosition(dt);
        }
        private void EvolveBarnesHut(float dt)
        {
            QuadTree.Evolve(dt, 0);
            UpdatePosition(dt);
        }
    }
}
