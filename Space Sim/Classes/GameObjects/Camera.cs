using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using Graphics;
using System.Threading.Tasks;

namespace GameObjects
{

    /* THING TO DO:
     * have not implemented the movement while zooming. 
     * 
     * I think basescale has introduced an extra factor of 2 from somewhere. -> fix that
     */

    sealed class Camera2D : Node2D
    {

        /* ZoomID is the last ID of the change in zoom.
         * The only reason for it not to match is if a newer call exists.
         * 
         * The camera will zoom by "zoomrate" for "zoomtime" since the last change in "ZoomIndex" to change zoom
         * If the ZoomTo is called twice then it zooms at double speed.
         * 
         * If "zoom" = 1, "scale" = "basescale".
         */
        private Vector2 basescale; // used when changing window size
        private float zoom;
        private int ZoomIndex;
        private int zoomID;

        private readonly float windowunit;
        private Vector2 windowsize;

        private readonly int zoomtime;
        private readonly float zoomrate;

        /// <summary>
        /// The center point of the camera in the world space. Different from "position" which is render space
        /// </summary>
        public Vector2 WorldPosition
        {
            set
            {
                Position = value * 2 * -Scale;
            }
            get
            {
                return new Vector2(-Position.X / Scale.X / 2, -Position.Y / Scale.Y / 2);
            }
        }
        
        /// <summary>
        /// Zooms in and out. Preserves Camera World Position.
        /// </summary>
        public float Zoom
        {
            set
            {
                // WP1 = Pos * zoom1 / basescale, WP2 = Pos * zoom2 / basescale
                // WP1 = K * WP2 => K = WP1 / WP2 = zoom1 / zoom2 
                Position = Position * zoom / value; // without this line render position is preserved not world position
                zoom = value;
                Scale = basescale / zoom;
            }
            get
            {
                return zoom;
            }
        }
        
       
        
        
        
        
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
            this.ZoomIndex = 0;
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
            if (zoomID == ID) ZoomIndex = 0;
        }
        


        public void OnMouseDown(MouseButtonEventArgs e)
        {
            
        }
        public void OnMouseUp(MouseButtonEventArgs e)
        {

        }
        public void OnMouseMove(MouseMoveEventArgs e)
        {
            // when mouse moves add the distance its moved to the camera render position
            // dealt with in render space as its tidier
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
            if (ZoomIndex != 0 && MathF.Sign(Delta) != MathF.Sign(ZoomIndex)) // abrupt stop if reverse while zooming
            {
                ResetZoomRate(zoomID);
            }
            else
            {
                ZoomIndex += Delta;
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
            Zoom *= MathF.Pow(MathF.Pow(zoomrate, ZoomIndex), delta); // decrease by zoomrate in zoomtime
        }
    }
}
