using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Input;
using OpenTK.Mathematics;
using Graphics;
using Shaders;
using DeepCopy;

namespace GameObjects
{


    /* THING TO DO:
     * 
     */


    public sealed class Window : GameWindow
    {
        private readonly string TextureRes = "2K planet textures/";
        // Window Variable
        public static Color4 RefreshCol = new Color4(0.01f, 0.01f, 0.1f, 1.0f);
        private RenderList RenderList = new RenderList();
        internal static Camera2D Camera = new Camera2D(NativeWindowSettings.Default.Size, 1, 300, 0.5f);
        internal static DeepCopy<Matrix3> CameraCopy;

        internal static Random RNG = new Random();

        internal static readonly Vertex2D[] SquareMesh = new Vertex2D[6] {
            new Vertex2D(-0.5f, 0.5f, 0, 0, 1, 1, 1, 1),
            new Vertex2D(-0.5f,-0.5f, 0, 1, 1, 1, 1, 1),
            new Vertex2D( 0.5f,-0.5f, 1, 1, 1, 1, 1, 1),

            new Vertex2D( 0.5f,-0.5f, 1, 1, 1, 1, 1, 1),
            new Vertex2D(-0.5f, 0.5f, 0, 0, 1, 1, 1, 1),
            new Vertex2D( 0.5f, 0.5f, 1, 0, 1, 1, 1, 1),
            }; // for convenience
        

        public Window(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            CameraCopy = new DeepCopy<Matrix3>(() => Camera.Transform_Matrix);
            GL.ClearColor(RefreshCol);
            this.VSync = VSyncMode.On;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Size.X, e.Size.Y);
            Camera.UpdateWindowSize(e.Size);
            base.OnResize(e);
        }
        protected override void OnLoad()
        {
            // million km // (696.3f * 2) * 20
            Celestial_Body S1 = new Celestial_Body(new Vector2( 128 /*696.3f * 2*/), new Vector2(128, 128), TextureRes + "earth");
            RenderObject2D B1 = new PressButton(new Vector2(0.9f), new Vector2(0.2f), "Button_home");
            //RenderObject2D B2 = new SliderButton();
            /* 
            Point_Mass P1 = new Point_Mass(new Vector2(2.4f * 2) * 200,  new Vector2(57900f, 0), TextureRes + "mercury");
            Point_Mass P2 = new Point_Mass(new Vector2(6.0f * 2) * 200,  new Vector2(108000f, 0), TextureRes + "venus");
            Point_Mass P3 = new Point_Mass(new Vector2(6.3f * 2) * 200,  new Vector2(150000f, 0), TextureRes + "earth");
            Point_Mass P4 = new Point_Mass(new Vector2(3.4f * 2) * 200,  new Vector2(228000f, 0), TextureRes + "mars");
            Point_Mass P5 = new Point_Mass(new Vector2(69.9f * 2) * 200, new Vector2(778000f, 0), TextureRes + "jupiter");
            Point_Mass P6 = new Point_Mass(new Vector2(58.2f * 2) * 200, new Vector2(1430000f, 0), TextureRes + "saturn");
            Point_Mass P7 = new Point_Mass(new Vector2(25.4f * 2) * 2000, new Vector2(28700000f, 0), TextureRes + "uranus");
            Point_Mass P8 = new Point_Mass(new Vector2(24.6f * 2) * 2000, new Vector2(45000000f, 0), TextureRes + "neptune");
            */

            // remove later
            RenderList.Add("S1", () => S1);
            RenderList.Add("B1", () => B1);
            //RenderList.Add("B2", () => B2);

            /*
            RenderList.Add("P1", () => P1);
            RenderList.Add("P2", () => P2); 
            RenderList.Add("P3", () => P3);
            RenderList.Add("P4", () => P4);
            RenderList.Add("P5", () => P5);
            RenderList.Add("P6", () => P6);
            RenderList.Add("P7", () => P7);
            RenderList.Add("P8", () => P8);
            */


            // allows blending ie semi transparent stuff
            GL.Enable(EnableCap.Blend);
            
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            
        }


        // calls custom event on openGl event -> this is because the event args dont contain very much are all contained in mouse state which
        protected override void OnMouseWheel(MouseWheelEventArgs e) => EventManager.MouseWheel(MouseState, e);
        protected override void OnMouseDown(MouseButtonEventArgs e) => EventManager.MouseDown(MouseState, e);
        protected override void OnMouseUp(MouseButtonEventArgs e) => EventManager.MouseUp(MouseState, e);
        protected override void OnMouseMove(MouseMoveEventArgs e) => EventManager.MouseMove(MouseState, e);


       
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // testing
            //Time += (float)e.Time;

            Title =
                $"CamScreenPos: " +
                $"{MathF.Round(Camera.Position.X, 2)}," +
                $"{MathF.Round(Camera.Position.Y, 2)} " +

                $"CamWorldPos: " +
                $"{MathF.Round(Camera.WorldPosition.X, 2)}," +
                $"{MathF.Round(Camera.WorldPosition.Y, 2)} " +

                $"MousePos: " +
                $"{MathF.Round(MouseState.Position.X, 2)}," +
                $"{MathF.Round(MouseState.Position.Y, 2)} " +

                $"MouseScreenPos: " +
                $"{MathF.Round(MouseToScreen(MouseState.Position).X, 2)}," +
                $"{MathF.Round(MouseToScreen(MouseState.Position).Y, 2)} " +

                $"MouseWorldPos: " +
                $"{MathF.Round(MouseToWorld(MouseState.Position).X, 2)}," +
                $"{MathF.Round(MouseToWorld(MouseState.Position).Y, 2)} ";

                //$"Vsync: { VSync} FPS: { 1f / e.Time: 0}"; // : 0 truncates to 0 decimal places


            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // call Process event
            EventManager.Process((float)e.Time);

            // render in RenderList Order
            foreach (var RO in RenderList)
                RO.Render();
            
            SwapBuffers(); // swap out screen buffer with new one
        }

        //

        /// <summary>
        /// Converts Screen space to world space.
        /// </summary>
        /// <param name="Pos">the pixel position on the screen.</param>
        /// <returns>The position in the world space.</returns>
        public static Vector2 MouseToWorld(Vector2 Pos)
        {
            Vector2 MtS = MouseToScreen(Pos);
            return (CameraCopy.Value.Inverted() * new Vector3(MtS.X, MtS.Y, 1.0f)).Xy;
        }
        public static Vector2 MouseToScreen(Vector2 Pos) => new Vector2((2 * Pos.X / Camera.windowsize.X) - 1, -((2 * Pos.Y / Camera.windowsize.X) - 1));
        public static Vector2 ScreenToWorld(Vector2 Pos) => (CameraCopy.Value.Inverted() * new Vector3(Pos.X, Pos.Y, 1.0f)).Xy;
        public static Vector2 WorldToScreen(Vector2 Pos) => (CameraCopy.Value * new Vector3(Pos.X, Pos.Y, 1.0f)).Xy;
    }
}

