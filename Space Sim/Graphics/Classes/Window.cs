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


namespace Graphics
{
    public sealed class Window : GameWindow
    {
        // Window Variable
        public OpenTK.Mathematics.Color4 RefreshCol = new OpenTK.Mathematics.Color4(0.05f, 0.1f, 0.2f, 1.0f);
        List<RenderObject<Vertex2D>> RenderObjects;
        Camera2D Camera;
        
        
        // Shader Variables
        private float Time;
        
        public Window(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            RenderObjects = new List<RenderObject<Vertex2D>>();
            Camera = new Camera2D(NWS.Size, 1, 300, 0.5f);
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

            // remove later
            RenderObjects.Add(new RenderObject<Vertex2D>(0.0f, new Vector2(48f, 48f), new Vector2(0, 0), new Vertex2D[]
            {   
                new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1), // blue
                new Vertex2D(-1,-1, 0, 1, 1, 1, 1, 1), // red
                new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1), // yellow
                
                new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1), // yellow
                new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1), // blue
                new Vertex2D( 1, 1, 1, 0, 1, 1, 1, 1), // green
            }));

            CursorVisible = true;

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 8);

            // allows blending ie semi transparent stuff
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }


        // should remove later
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (MouseState.ScrollDelta.Y > 0)
            {
                Camera.ZoomTo(MousePosition, -1);
            }
            else
            {
                Camera.ZoomTo(MousePosition, 1);
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Time += (float)e.Time;
            //Vsync: {VSync} FPS: {1f / e.Time:0} Time: {Time} 
            Title = $"MousePos: {MousePosition} WorldPos:{ScreenToWorld(MousePosition)}";
            
            Camera.Process((float)e.Time);
            
            /* THING TO DO:
             * set up Z index
             * Should be custom list object for render object update on change
             * fancy computer science technique
             */
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            foreach (var R in RenderObjects) R.Process((float)e.Time);
            foreach (var R in RenderObjects) R.Render(Camera, Time);
            
            SwapBuffers();
        }


        /// <summary>
        /// Converts Screen space to world space.
        /// </summary>
        /// <param name="Pos">The pixel position to be found in the world</param>
        /// <returns>The Position is in the world.</returns>
        public Vector2 ScreenToWorld(Vector2 Pos) => Camera.Position + new Vector2(((2 * Pos.X / Size.X) - 1) / Camera.Scale.X, ((2 * Pos.Y / Size.Y) - 1) / Camera.Scale.Y);
    }
}

