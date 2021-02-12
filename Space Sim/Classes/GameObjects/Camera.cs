using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using Graphics;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameObjects
{

    /* THING TO DO:
     * have not implemented the movement while zooming. ie zoom to location - dont know if user wants this.
     * Specific Button for movement and controls -> not difficult less convenient right now
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
        private float ZoomIndex; // speed of zooming
        private int zoomID;

        public readonly float windowunit;
        public Vector2 windowsize;

        private readonly int zoomtime;
        private readonly float zoomrate;

        public MouseButton MovementButton = MouseButton.Button1;

        /// <summary>
        /// returns matrix for transforming between window view in pixels for renderobjects fixed to screen
        /// </summary>

        /// <summary>
        /// The center point of the camera in the world space. Different from "position" which is render space
        /// </summary>
        public Vector2 WorldPosition
        {
            get => new Vector2(-Position.X / Scale.X, -Position.Y / Scale.Y);
            set => Position = -value * Scale;
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
            AttachEvents();

            zoomID = 0;
            this.ZoomIndex = 0;
            zoom = 1;

            windowunit = WindowUnit;

            zoomrate = ZoomRate;
            zoomtime = ZoomTime;

            basescale = Scale;
        }

        public void AttachEvents()
        {
            EventManager.MouseDown += OnMouseDown;
            EventManager.MouseUp += OnMouseUp;
            EventManager.MouseWheel += OnMouseWheel;
            EventManager.MouseMove += OnMouseMove;
            EventManager.Process += OnProcess;
        }

        /// <summary>
        /// Sets "zoomrate" to 0 if object ZoomID matches the last time "ZoomTo" was called.
        /// </summary>
        /// <param name="ID">The local ZoomID.</param>
        private void ResetZoomRate(int ID)
        {
            if (zoomID == ID) ZoomIndex = 0;
        }

        // uses a Action delegate to add and subtract from events
        private Action<Vector2> MouseMove = (MousePosition) => { /* Dont Do Anything By Default */ };
        public void OnMouseDown(MouseState MouseState, MouseButtonEventArgs e)
        {
            if (e.Button == MovementButton) MouseMove += MoveCamera;
        }
        public void OnMouseUp(MouseState MouseState, MouseButtonEventArgs e) 
        {
            if (e.Button == MovementButton) MouseMove -= MoveCamera;
        }
        public void OnMouseMove(MouseState MouseState, MouseMoveEventArgs e)
        {
            MouseMove(MouseState.Delta);
        }
        public void OnMouseWheel(MouseState MouseState, MouseWheelEventArgs e) 
        {
            if (MouseState.ScrollDelta.Y > 0) ZoomBy(1); // zoom in
            else ZoomBy(-1); // zoom out
        }

        /// <summary>
        /// moves the camera by the change in mouse position
        /// </summary>
        /// <param name="MouseDelta">the </param>
        private void MoveCamera(Vector2 MouseDelta) => Position += new Vector2(MouseDelta.X / windowsize.X * 2, -MouseDelta.Y / windowsize.Y * 2);



        /// <summary>
        /// Zooms in or out by a step of delta
        /// </summary>
        /// <param name="Step">The time passed since last Processed.</param>
        private void ZoomBy(float Step) 
        {
            zoomID++;
            if (ZoomIndex != 0 && MathF.Sign(Step) != MathF.Sign(ZoomIndex)) // abrupt stop if zooming reversed
            {
                ResetZoomRate(zoomID);
            }
            else
            {
                ZoomIndex += Step;
                int localID = zoomID; // needs to be local here not when ResetZoom is called
                Task.Delay(zoomtime).ContinueWith(ID => ResetZoomRate(localID)); // after zoomtime(ms) stops zooming if local ID matches zoom ID
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
        public void OnProcess(float delta) => Zoom *= MathF.Pow(MathF.Pow(zoomrate, ZoomIndex), delta); // decrease by zoomrate in zoomtime




        
        
    }
}
