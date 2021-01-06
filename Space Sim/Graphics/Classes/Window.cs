﻿using System;
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
        List<RenderObject2D<Vertex2D>> RenderObjects2D;
        Camera2D Camera;
        
        // Shader Variables
        private float Time;

        public Window(GameWindowSettings GWS, NativeWindowSettings NWS) : base(GWS, NWS)
        {
            RenderObjects2D = new List<RenderObject2D<Vertex2D>>();
            
            
            Camera = new Camera2D(NWS.Size, 1, 300, 0.5f);

            MouseDown += Camera.OnMouseDown;
            MouseUp += Camera.OnMouseUp;

            

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
            RenderObjects2D.Add(new RenderObject2D<Vertex2D>(0.0f, new Vector2(48f, 48f), new Vector2(0, 0), new Vertex2D[]
            {   
                new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1), // blue
                new Vertex2D(-1,-1, 0, 1, 1, 1, 1, 1), // red
                new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1), // yellow
                
                new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1), // yellow
                new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1), // blue
                new Vertex2D( 1, 1, 1, 0, 1, 1, 1, 1), // green
            }));
            // allows blending ie semi transparent stuff
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (MouseState.ScrollDelta.Y > 0) // zoom in
            {
                Camera.ZoomTo(MousePosition, 1);
            }
            else // zoom out
            {
                Camera.ZoomTo(MousePosition, -1);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            MouseMove += Camera.OnMouseMove;
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            MouseMove -= Camera.OnMouseMove;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Time += (float)e.Time;
            //Vsync: {VSync} FPS: {1f / e.Time:0} Time: {Time}
            Title = $"MousePos: {MousePosition} WorldPos:{ScreenToWorld(MousePosition)}, CameraPos: {Camera.Position} Zoom: {Camera.Zoom}";
            
            Camera.Process((float)e.Time);
            
            /* THING TO DO:
             * set up Z index
             * Should be custom list object for render object update on change
             * fancy computer science technique
             */
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            foreach (var R in RenderObjects2D) R.Process((float)e.Time);
            foreach (var R in RenderObjects2D) R.Render(Camera, Time);
            
            SwapBuffers();
        }

        /// <summary>
        /// Converts Screen space to world space.
        /// </summary>
        /// <param name="Pos">the pixel position on the screen.</param>
        /// <returns>The position in the world space.</returns>
        public Vector2 ScreenToWorld(Vector2 Pos) => Camera.Position + new Vector2(((2 * Pos.X / Size.X) - 1) / 2 / Camera.Scale.X, ((2 * Pos.Y / Size.Y) - 1) / 2 / Camera.Scale.Y);
    }
}

