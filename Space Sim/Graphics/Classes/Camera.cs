using OpenTK.Mathematics;
using System;

using System.Threading.Tasks;

namespace Graphics
{
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
        private int zoomlevel; 
        private int zoomID;

        private Vector2 windowsize; // dont really want stored this -> needed for ScreenToWorld()
        private readonly float windowunit; // nor this

        private readonly int zoomtime;
        private readonly float zoomrate;

        private Vector2 dragstart;
        private bool drag;

        public float Zoom
        {
            set
            {
                zoom = value;
                Scale = basescale * zoom;
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
            zoomlevel = 0;
            zoom = 1;
            
            windowunit = WindowUnit;
            windowsize = WindowSize;

            zoomrate = ZoomRate;
            zoomtime = ZoomTime;
            
            basescale = Scale;
        }
        /// <summary>
        /// Sets "zoomlevel" to 0 if object ZoomID matches the last time "ZoomTo" was called.
        /// </summary>
        /// <param name="ID">The local ZoomID.</param>
        private void ResetZoom(int ID) 
        {
            if (zoomID == ID) zoomlevel = 0;
        }
        /// <summary>
        /// Zooms towards a point.
        /// </summary>
        /// <param name="Position">The position to zoom into.</param>
        /// <param name="Delta">The time passed since last Processed.</param>
        public void ZoomTo(Vector2 Position, int Delta) 
        {
            zoomID++;
            if (zoomlevel != 0 && MathF.Sign(Delta) != MathF.Sign(zoomlevel)) // abrupt stop if reverse while zooming
            {
                ResetZoom(zoomID);
            }
            else
            {
                zoomlevel += Delta;
                int localID = zoomID; // needs to be local here not when ResetZoom is called
                Task.Delay(zoomtime).ContinueWith(t => ResetZoom(localID)); // after zoomtime(ms) stops zooming if local ID matches zoom ID
            }
        }
        /// <summary>
        /// Changes the camera matrix to reflect the new window size.
        /// </summary>
        /// <param name="WindowSize">New window size.</param>
        public void UpdateWindowSize(Vector2 WindowSize)
        {
            windowsize = WindowSize;
            basescale = new Vector2(zoom / windowsize.X / windowunit, (zoom / windowsize.Y / windowunit));
        }
        /// <summary>
        /// Called on each frame update.
        /// </summary>
        /// <param name="delta">time passed since last processed.</param>
        public void Process(float delta)
        {
            Zoom *= MathF.Pow(MathF.Pow(zoomrate, zoomlevel), delta); // decrease by zoomrate in zoomtime
        }
    }
}
