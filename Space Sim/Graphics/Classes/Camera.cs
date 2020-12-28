using OpenTK.Mathematics;
using System;

using System.Threading.Tasks;

namespace Graphics
{
    sealed class Camera2D : Node2D
    {
        private readonly Vector2 BaseScale;
        private float zoom;
        private int zoomlevel;
        public int ZoomID = 0;
        public float Zoom
        {
            set
            {
                zoom = value;
                Scale = BaseScale * zoom;
            }
            get
            {
                return zoom;
            }
        }
        public int ZoomLevel
        {
            set
            {
                ZoomID++;
                if (MathF.Abs(value) < MathF.Abs(zoomlevel))
                {
                    zoomlevel = 0;
                }
                else
                {
                    zoomlevel = value;
                    int localID = ZoomID;
                    Task.Delay(1000).ContinueWith(t => ResetZoom(localID)); // after 1000ms(1 sec) reset zooming
                }
            }
            get 
            { 
                return zoomlevel; 
            }
        }

        public Camera2D(Vector2 WindowSize, float WindowUnit) : base(0, 1f / WindowSize.X / WindowUnit, 1f / WindowSize.Y / WindowUnit, 0, 0)
        {
            zoom = 1;
            zoomlevel = 0;
            BaseScale = Scale;
        }
        private void ResetZoom(int ID) 
        {
            if (ZoomID == ID) zoomlevel = 0;// -= Math.Sign(zoomlevel);
            
        }
        public void ProcessZoom(float delta) => Zoom *= MathF.Pow(MathF.Pow(0.5f, zoomlevel), delta); // decrease by half the size in 1 second
        public void Process(float delta)
        {
            ProcessZoom(delta);
        }
    }
}
