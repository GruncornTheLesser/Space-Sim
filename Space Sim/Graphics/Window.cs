using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Input;
using OpenTK.Mathematics;
//using OpenTK.Mathematics;
namespace Graphics
{
    public sealed class Window : GameWindow
    {
        // Window Variable
        public OpenTK.Mathematics.Color4 RefreshCol = new OpenTK.Mathematics.Color4(0.05f, 0.1f, 0.3f, 1.0f);
        List<RenderObject2D> RenderObjects = new List<RenderObject2D>();

        // Shader Variables
        private float Time;
        
        public Window(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            Size = new Vector2i(800, 800);
            GL.ClearColor(RefreshCol);
            this.VSync = VSyncMode.On;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
        }
        protected override void OnLoad()
        {
            CursorVisible = true;
            RenderObjects.Add(new RenderObject2D(0.0f, new Vector2(1, 1), new Vector2(0, 0), new Vertex[] 
            {   
                new Vertex(new Vector2(-0.4f, 0.7f), OpenTK.Mathematics.Color4.Blue), 
                new Vertex(new Vector2(-0.2f, -0.6f), OpenTK.Mathematics.Color4.Red),
                new Vertex(new Vector2(0.4f, -0.6f), OpenTK.Mathematics.Color4.Yellow),
                new Vertex(new Vector2(0.3f, 0.8f), OpenTK.Mathematics.Color4.Green),
            }
            ));

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);

            Closed += OnClosed;
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e) { }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Time += (float)e.Time;

            Title = $"MousePos: {(MathF.Round(MousePosition.X / Size.X * 2 - 1, 2), MathF.Round(MousePosition.Y / Size.Y * 2 - 1, 2))} Vsync: {VSync} FPS: {1f / e.Time:0} Time: {Time} ";
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (RenderObject2D R in RenderObjects) R.Process((float)e.Time);
            foreach (RenderObject2D R in RenderObjects) R.Render();
            
            SwapBuffers();
        }
        private void OnClosed(object sender, EventArgs eventArgs)
        {
            for (int i = 0; i < RenderObjects.Count; i++)
            {
                RenderObjects[0].Dispose();
                RenderObjects.RemoveAt(0);
            }

            base.Close();

        }
    }
}

