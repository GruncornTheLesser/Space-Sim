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
using System.Linq;
namespace GameObjects
{


    /* THING TO DO:
     * Don't really like window being static but its awkward accessing camera and base matrices
     */


    public sealed class Window : GameWindow
    {
        private readonly string TextureRes = "2K planet textures/";
        public static Color4 RefreshCol = new Color4(0.01f, 0.01f, 0.1f, 1.0f);
        private RenderList RenderList;
        internal static Camera2D Camera;
        internal static Func<Matrix3> Get_CamMat;
        internal static Func<Matrix3> Get_BaseMat;

        

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
            RenderList = new RenderList();
            Camera = new Camera2D(NWS.Size, 1, 300, 0.5f);

            Get_CamMat = () => Camera.Transform_Matrix;
            Get_BaseMat = () => Camera.BaseMatrix;

            GL.ClearColor(RefreshCol);
            this.VSync = VSyncMode.On;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            
            GL.Viewport(0, 0, e.Size.X, e.Size.Y);
            Camera.UpdateWindowSize(e.Size);
            EventManager.WindowResize(e.Size);
        }
        protected override void OnLoad()
        {
            // 1 = 1 000 000 000m
            // (696.3f * 2) * 20
            Celestial_Body S1 = new Celestial_Body(new Vector2(128 /*696.3f * 2*/), new Vector2(0, 0), "Textures/" + TextureRes + "earth.png");
           
            PressButton B1 = new PressButton(new Vector2(0.9f), new Vector2(0.2f), "Button_home");
            B1.Release += () => Camera.WorldPosition = S1.Position;
            PressButton B2 = new PressButton(new Vector2(0.9f, 0.7f), new Vector2(0.2f), "Button_resize");
            B2.Release += () =>
            {
                Vertex2D[] V = B2.Vertices;
                for (int i = 0; i < B2.Vertices.Length; i++)
                {
                    V[i].VertColour = Color4.BlueViolet;
                }
                B2.Vertices = V;
            };

            SliderButton B3 = new SliderButton(new Vector2(-0.6f, -0.9f), new Vector2(0.8f, 0.2f), "Button_slider");

            // remove later
            RenderList.Add("S1", () => S1);
            RenderList.Add("B1", () => B1);
            RenderList.Add("B2", () => B2);
            RenderList.Add("B3", () => B3);

            



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
            foreach (var RO in RenderList) RO.Render();
            
            SwapBuffers(); // swap out screen buffer with new one
        }

        

        /// <summary>
        /// Converts Screen space to world space.
        /// </summary>
        /// <param name="Pos">the pixel position on the screen.</param>
        /// <returns>The position in the world space.</returns>
        public static Vector2 MouseToWorld(Vector2 Pos) => ScreenToWorld(MouseToScreen(Pos));
        public static Vector2 MouseToScreen(Vector2 Pos) => new Vector2((2 * Pos.X / Camera.windowsize.X) - 1, -((2 * Pos.Y / Camera.windowsize.Y) - 1));
        public static Vector2 ScreenToWorld(Vector2 Pos) => (Get_CamMat().Inverted() * new Vector3(Pos.X, Pos.Y, 1.0f)).Xy;
        public static Vector2 WorldToScreen(Vector2 Pos) => (Get_CamMat() * new Vector3(Pos.X, Pos.Y, 1.0f)).Xy;
    }
}

