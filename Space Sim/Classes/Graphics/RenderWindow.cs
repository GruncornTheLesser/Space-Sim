using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
namespace Graphics
{


    /* THING TO DO:
     * Don't really like having static window fields but its awkward accessing camera and base matrices
     * Could pass in object reference
     */


    public class RenderWindow : GameWindow
    {
        protected Color4 RefreshCol = new Color4(0.01f, 0.01f, 0.1f, 1.0f);
        internal static Camera2D Camera;
      
        public RenderWindow(GameWindowSettings GWS, NativeWindowSettings NWS, float WindowUnit) : base(GWS, NWS)
        {
            Camera = new Camera2D(NWS.Size, WindowUnit, 300, 0.5f);

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

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // clear last frame

            // call Process events
            EventManager.System_Process((float)e.Time); // this is in real time
            
            foreach (var RO in new RenderList()) RO.Render(); // render in Z index order, must create new object to iterate through

            SwapBuffers(); // swap out old buffer buffer with new buffer
        }
        #endregion
        #region Mouse Events
        // calls custom event on openGl event -> this is because the event args dont contain very much are all contained in mouse state which
        protected override void OnMouseWheel(MouseWheelEventArgs e) => EventManager.MouseWheel(MouseState, e);
        protected override void OnMouseDown(MouseButtonEventArgs e) => EventManager.MouseDown(MouseState, e);
        protected override void OnMouseUp(MouseButtonEventArgs e) => EventManager.MouseUp(MouseState, e);
        protected override void OnMouseMove(MouseMoveEventArgs e) => EventManager.MouseMove(MouseState, e);
        #endregion
        #region Space Converters
        /* Converts between different spaces
         */
        public static Vector2 MouseToWorld(Vector2 Pos) => ScreenToWorld(MouseToScreen(Pos));
        public static Vector2 MouseToScreen(Vector2 Pos) => new Vector2((2 * Pos.X / Camera.windowsize.X) - 1, -((2 * Pos.Y / Camera.windowsize.Y) - 1));
        public static Vector2 ScreenToWorld(Vector2 Pos) => (Camera.Transform_Matrix.Inverted() * new Vector3(Pos.X, Pos.Y, 1.0f)).Xy;
        public static Vector2 WorldToScreen(Vector2 Pos) => (Camera.Transform_Matrix * new Vector3(Pos.X, Pos.Y, 1.0f)).Xy;
        #endregion
    }
}

