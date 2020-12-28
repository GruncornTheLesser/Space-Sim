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
//using OpenTK.Mathematics;
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

            // remove later
            RenderObjects.Add(new RenderObject<Vertex2D>(0.0f, new Vector2(48f, 48f), new Vector2(0, 0), new Vertex2D[]
            {   
                new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1), // blue
                new Vertex2D(-1,-1, 0, 1, 1, 1, 1, 1), // red
                new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1), // yellow
                
                new Vertex2D( 1,-1, 1, 1, 1, 1, 1, 1), // yellow
                new Vertex2D(-1, 1, 0, 0, 1, 1, 1, 1), // blue
                new Vertex2D( 1, 1, 1, 0, 1, 1, 1, 1), // green
            }
            ));

            /*THING TO DO:
             * setup camera movement and zoom
             */

            Camera = new Camera2D(Size, 1);

            CursorVisible = true;

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 8);
            
            // fixes texture at edges
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // makes pixel perfect
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // allows blending ie semi transparent stuff
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            Closed += OnClosed;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            switch(e.Button)
            {
                case MouseButton.Button1: // left
                    break;
                case MouseButton.Button2: // right
                    break;
                case MouseButton.Button3: // middle
                    break;
                case MouseButton.Button4: // back side thing
                    Camera.ZoomLevel -= 1;
                    break;
                case MouseButton.Button5: // front side thing
                    Camera.ZoomLevel += 1;
                    break;

            }
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (MouseState.ScrollDelta.Y > 0)
            {
                Camera.ZoomLevel -= 1;
            }
            else
            {
                Camera.ZoomLevel += 1;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Time += (float)e.Time;
            Title = //$"MousePos: {MousePosition * Camera.Zoom} Vsync: {VSync} FPS: {1f / e.Time:0} Time: {Time}" + 
                    $"CamID: {Camera.ZoomID} CamZoom: {Camera.Zoom} CamLevel: {Camera.ZoomLevel}";
            
            Camera.Process((float)e.Time);
            
            /* THING TO DO:
             * set up Z index
             * Should be custom list object for render object update on change
             * fancy computer science technique
             */
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            foreach (var R in RenderObjects) R.Process((float)e.Time);
            foreach (var R in RenderObjects) R.Render(Camera);
            
            SwapBuffers();
        }
    }
}

