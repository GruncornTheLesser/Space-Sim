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
     * set up Z index
     * Should be custom list thing for render object update on change of Z index
     * Add events to objects
     */
    public sealed class Window : GameWindow
    {
        // Window Variable
        public Color4 RefreshCol = new Color4(0.05f, 0.1f, 0.2f, 1.0f);
        RenderObjectList<Vertex2D> RenderList = new RenderObjectList<Vertex2D>();
        Camera2D Camera;
        
        // Shader Variables
        private float Time;
        DeepCopy<float> TimeCopy;

        // for convenience - doesnt rly belong here
        public static readonly Vertex2D[] SquareMesh = new Vertex2D[6] {
            new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1),
            new Vertex2D(-1,-1, 0, 1, 1, 1, 1, 1),
            new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1),

            new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1),
            new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1),
            new Vertex2D( 1, 1, 1, 0, 1, 1, 1, 1),
            };

        // hides openGL MouseDown - openGl Actions only give limited arguments
        new Action<MouseState> MouseDown;
        new Action<MouseState> MouseUp;
        new Action<MouseState> MouseMove;
        new Action<MouseState> MouseWheel;
        public Window(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            TimeCopy = new DeepCopy<float>(() => Time, value => { Time = value; });
            
            Camera = new Camera2D(NWS.Size, 1, 300, 0.5f);
            
            MouseDown += Camera.OnMouseDown;
            MouseUp += Camera.OnMouseUp;
            MouseWheel += Camera.OnMouseWheel;
            MouseMove += Camera.OnMouseMove;

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
            Planet P1 = new Planet(new Vector2(48f, 48f), new Vector2(48, 0), Camera.TransformCopy, TimeCopy, "4 Mars TS", "Default", "Default");
            Planet P2 = new Planet(new Vector2(48f, 48f), new Vector2(0, 0), Camera.TransformCopy, TimeCopy, "2 Venus TS", "Default", "Default");
            Planet P3 = new Planet(new Vector2(48f, 48f), new Vector2(-48, 0), Camera.TransformCopy, TimeCopy, "3 Earth TS", "Default", "Default");
            
            // remove later
            RenderList.Add(P1);
            RenderList.Add(P2);
            RenderList.Add(P3);

            
            // allows blending ie semi transparent stuff
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        
        protected override void OnMouseWheel(MouseWheelEventArgs e) => MouseWheel(MouseState);
        protected override void OnMouseDown(MouseButtonEventArgs e) => MouseDown(MouseState); //calls custom MouseDown on openGl call
        protected override void OnMouseUp(MouseButtonEventArgs e) => MouseUp(MouseState); //calls custom MouseUp on openGl call
        protected override void OnMouseMove(MouseMoveEventArgs e) => MouseMove(MouseState);




        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // testing
            Time += (float)e.Time;
            Title =
                "WorldPos: " +
                $"{MathF.Round(Camera.ScreenToWorld(MousePosition).X, 2)}," +
                $"{MathF.Round(Camera.ScreenToWorld(MousePosition).Y, 2)} " +

                $"CameraPos: " +
                $"{MathF.Round(Camera.Position.X, 2)}," +
                $"{MathF.Round(Camera.Position.Y, 2)} " +

                $"CameraWorldPos: {Camera.WorldPosition}" +

                $"Vsync: { VSync} FPS: { 1f / e.Time : 0}"; // : 0 truncates to 0 decimal places
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Camera.Process((float)e.Time);

            // process and render objects
            foreach (var R in RenderList) R.Process((float)e.Time);
            foreach (var R in RenderList) R.Render();
            
            SwapBuffers(); // swap out screen buffer with new one
        }

    }
}

