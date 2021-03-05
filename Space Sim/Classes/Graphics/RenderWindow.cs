using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
namespace Graphics
{    
    public abstract class RenderWindow : GameWindow
    {
        protected Color4 RefreshCol = new Color4(0.01f, 0.01f, 0.1f, 1.0f);
        internal RenderCamera Camera; // static for easy access. -> means only 1 window can exist at 1 time.
        internal RenderList RenderList;
        internal EventManager EventManager;

        public RenderWindow(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            RenderList = new RenderList();
            GL.ClearColor(RefreshCol);
            this.VSync = VSyncMode.On;
        }

        #region Window Events
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Size.X, e.Size.Y); // changes window size
            EventManager.WindowResize(e.Size); // calls window resize event -> most importantly updates camera
        }
        protected override void OnLoad()
        {
            // allows blending ie semi transparent stuff
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            /*
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

                $"FPS: {1 / e.Time : 0} ";
            */
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // clear last frame

            // call Process events
            EventManager.System_Process((float)e.Time); // this is in real time
            
            foreach (RenderObject2D RO in RenderList) RO.Render(); // render in Z index order, must init render list to iterate through

            SwapBuffers(); // swap out old buffer buffer with new buffer
        }
        #endregion



        #region Mouse Events
        // calls custom event on openGl event -> this is because the event args dont contain very much data - added mouse state into the event
        protected override void OnMouseWheel(MouseWheelEventArgs e) => EventManager.MouseWheel(MouseState, e);
        protected override void OnMouseDown(MouseButtonEventArgs e) => EventManager.MouseDown(MouseState, e);
        protected override void OnMouseUp(MouseButtonEventArgs e) => EventManager.MouseUp(MouseState, e);
        protected override void OnMouseMove(MouseMoveEventArgs e) => EventManager.MouseMove(MouseState, e);
        #endregion



        #region Space Converters
        /* Converts between different spaces
         */
        public Vector2 MouseToWorld(Vector2 Pos) => ScreenToWorld(MouseToScreen(Pos));
        public Vector2 MouseToScreen(Vector2 Pos) => new Vector2((2 * Pos.X / Camera.windowsize.X) - 1, -((2 * Pos.Y / Camera.windowsize.Y) - 1));
        public Vector2 ScreenToWorld(Vector2 Pos) => (Camera.Transform_Matrix.Inverted() * new Vector3(Pos.X, Pos.Y, 1.0f)).Xy;
        public Vector2 WorldToScreen(Vector2 Pos) => (Camera.Transform_Matrix * new Vector3(Pos.X, Pos.Y, 1.0f)).Xy;
        #endregion
    }
}

