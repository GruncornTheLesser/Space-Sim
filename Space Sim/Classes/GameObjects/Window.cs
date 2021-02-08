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
     * Could Move Shader variables to RenderObjectList
     */

    public sealed class Window : GameWindow
    {
        // Window Variable
        public Color4 RefreshCol = new Color4(0.05f, 0.1f, 0.2f, 1.0f);
        RenderList RenderList = new RenderList();
        Camera2D Camera;

        // Shader Variables
        private static float Time;
        internal static DeepCopy<float> TimeCopy;
        internal static DeepCopy<Matrix3> CameraCopy;

        // for convenience - doesnt rly belong here
        public static readonly Vertex2D[] SquareMesh = new Vertex2D[6] {
            new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1),
            new Vertex2D(-1,-1, 0, 1, 1, 1, 1, 1),
            new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1),

            new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1),
            new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1),
            new Vertex2D( 1, 1, 1, 0, 1, 1, 1, 1),
            };

        // hides openGL MouseDown - openGl events only give limited arguments
        new Action<MouseState> MouseDown = (MouseState) => { };
        new Action<MouseState> MouseUp = (MouseState) => { };
        new Action<MouseState> MouseMove = (MouseState) => { };
        new Action<MouseState> MouseWheel = (MouseState) => { };
        Action<float> Process = (delta) => { };

        public Window(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            Camera = new Camera2D(NWS.Size, 1, 300, 0.5f);
            AttachEvents(Camera);

            CameraCopy = Camera.TransformCopy;
            TimeCopy = new DeepCopy<float>(() => Time, value => { Time = value; });

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
            Planet P1 = new Planet("1 Mercury TS", "Animated", "Default");
            Planet P2 = new Planet("2 Venus TS", "Animated", "Default");
            Planet P3 = new Planet("3 Earth TS", "Animated", "Default");
            Planet P4 = new Planet("4 Mars TS", "Animated", "Default");
            Planet P5 = new Planet("5 Jupiter TS", "Animated", "Default");
            Button P6 = new Button();

            P1.Z_index = 100;
            P2.Z_index = 0;
            P3.Z_index = 25;

            P4.Z_index = 125;
            P5.Z_index = 70;
            P6.Z_index = 97;

            // remove later
            RenderList.Add("P1", P1);
            RenderList.Add("P2", P2);
            RenderList.Add("P3", P3);
            RenderList.Add("P4", P4);
            RenderList.Add("P5", P5);
            RenderList.Add("P6", P6);

            RenderList[4].Z_index = 116;

            

            // allows blending ie semi transparent stuff
            GL.Enable(EnableCap.Blend);
            
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            
        }


        // calls custom event on openGl event -> this is because the event args dont contain very much are all contained in mouse state which
        protected override void OnMouseWheel(MouseWheelEventArgs e) => MouseWheel(MouseState);
        protected override void OnMouseDown(MouseButtonEventArgs e) => MouseDown(MouseState);
        protected override void OnMouseUp(MouseButtonEventArgs e) => MouseUp(MouseState);
        protected override void OnMouseMove(MouseMoveEventArgs e) => MouseMove(MouseState);


        private void AttachEvents(RenderObject2D NewObject)
        {
            MouseDown += NewObject.OnMouseDown;
            MouseUp += NewObject.OnMouseUp;
            MouseWheel += NewObject.OnMouseWheel;
            MouseMove += NewObject.OnMouseMove;
            Process += NewObject.OnProcess;
        }
        private void AttachEvents(Camera2D NewCamera)
        {
            MouseDown += NewCamera.OnMouseDown;
            MouseUp += NewCamera.OnMouseUp;
            MouseWheel += NewCamera.OnMouseWheel;
            MouseMove += NewCamera.OnMouseMove;
            Process += NewCamera.OnProcess;
        }
        
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // testing
            Time += (float)e.Time;
            /*
            Title =
                "WorldPos: " +
                $"{MathF.Round(Camera.ScreenToWorld(MousePosition).X, 2)}," +
                $"{MathF.Round(Camera.ScreenToWorld(MousePosition).Y, 2)} " +

                $"CameraPos: " +
                $"{MathF.Round(Camera.Position.X, 2)}," +
                $"{MathF.Round(Camera.Position.Y, 2)} " +

                $"CameraWorldPos: " +
                $"{MathF.Round(Camera.WorldPosition.X, 2)}," +
                $"{MathF.Round(Camera.WorldPosition.Y, 2)} " +

                $"Vsync: { VSync} FPS: { 1f / e.Time : 0}"; // : 0 truncates to 0 decimal places
            */

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // call Process
            Process((float)e.Time);
            // render in RenderList Order
            foreach (var R in RenderList) 
                R.Render();
            
            SwapBuffers(); // swap out screen buffer with new one
        }

    }
}

