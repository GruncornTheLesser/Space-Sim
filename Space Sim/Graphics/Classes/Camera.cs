using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;

using System.Threading.Tasks;

namespace Graphics
{

    public delegate Vector2 MouseDown(Vector2 Position);
    public delegate Vector2 MouseUp(Vector2 Position);

    sealed class Camera2D : Node2D
    {

        /* ZoomID is the last ID of the change in zoom.
         * The only reason for it not to match is if a newer call exists.
         * 
         * The camera will zoom by "zoomrate" for "zoomtime" since the last change in "zoomlevel" to change zoom
         * If the ZoomTo is called twice then it zooms at double speed.
         * 
         * If "zoom" = 1, "scale" = "basescale".
         */
        private Vector2 basescale; // used when changing window size
        private float zoom;
        private int ZoomRate; 
        private int zoomID;

        private readonly float windowunit;
        private Vector2 windowsize;

        private readonly int zoomtime;
        private readonly float zoomrate;

        private Vector2 dragstart;

        /// <summary>
        /// Sets Zoom and changes scale
        /// </summary>
        public float Zoom
        {
            set
            {
                zoom = value;
                Scale = basescale / zoom;
            }
            get
            {
                return zoom;
            }
        } // this might be pointless. its kinda messy.
        
        /// <summary>
        /// A Camera using a matrix3x3 to control position, zoom and inputs.
        /// </summary>
        /// <param name="WindowSize"> The Size of the window.</param>
        /// <param name="WindowUnit">The size 1 pixel represents.</param>
        /// <param name="ZoomTime">The length of time camera will zoom.</param>
        /// <param name="ZoomRate">The rate at which the camera scales during zoom.</param>
        public Camera2D(Vector2 WindowSize, float WindowUnit, int ZoomTime, float ZoomRate) : base(0, 1f / WindowSize.X / WindowUnit, (1f / WindowSize.Y / WindowUnit) * (WindowSize.Y / WindowSize.X), 0, 0)
        {
            zoomID = 0;
            this.ZoomRate = 0;
            zoom = 1;
            
            windowunit = WindowUnit;

            zoomrate = ZoomRate;
            zoomtime = ZoomTime;
            
            basescale = Scale;
        }
        
        /// <summary>
        /// Sets "zoomrate" to 0 if object ZoomID matches the last time "ZoomTo" was called.
        /// </summary>
        /// <param name="ID">The local ZoomID.</param>
        private void ResetZoomRate(int ID) 
        {
            if (zoomID == ID) ZoomRate = 0;
        }
        

        /* THING TO DO:
         * have not implemented the movement while zooming.
         */

        public void OnMouseDown(MouseButtonEventArgs e)
        {
            
        }
        public void OnMouseUp(MouseButtonEventArgs e)
        {

        }
        public void OnMouseMove(MouseMoveEventArgs e)
        {
            // when mouse moves add the distance its moved to the camera render position
            Position += new Vector2(e.Delta.X / windowsize.X * 2, -e.Delta.Y / windowsize.Y * 2);
        }
        

        /// <summary>
        /// Zooms towards a point.
        /// </summary>
        /// <param name="Position">The position to zoom into.</param>
        /// <param name="Delta">The time passed since last Processed.</param>
        public void ZoomTo(Vector2 Position, int Delta) 
        {
            zoomID++;
            if (ZoomRate != 0 && MathF.Sign(Delta) != MathF.Sign(ZoomRate)) // abrupt stop if reverse while zooming
            {
                ResetZoomRate(zoomID);
            }
            else
            {
                ZoomRate += Delta;
                int localID = zoomID; // needs to be local here not when ResetZoom is called
                Task.Delay(zoomtime).ContinueWith(t => ResetZoomRate(localID)); // after zoomtime(ms) stops zooming if local ID matches zoom ID
            }
        }
        
        /// <summary>
        /// Changes the camera matrix to reflect the new window size.
        /// </summary>
        /// <param name="WindowSize">New window size.</param>
        public void UpdateWindowSize(Vector2 WindowSize)
        {
            windowsize = WindowSize;
            basescale = new Vector2(zoom / WindowSize.X / windowunit, zoom / WindowSize.Y / windowunit);
        }


        /// <summary>
        /// Called on each frame update.
        /// </summary>
        /// <param name="delta">time passed since last processed.</param>
        public void Process(float delta)
        {
            Zoom *= MathF.Pow(MathF.Pow(zoomrate, ZoomRate), delta); // decrease by zoomrate in zoomtime
        }
    }
}
