using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Graphics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameObjects
{
    class Camera : RenderCamera
    {
        /* ZoomID is the last ID of the change in zoom.
         * The only reason for it not to match is if a newer call exists.
         * 
         * The camera will zoom by "zoomrate" for "zoomtime" since the last change in "ZoomIndex" to change zoom
         * If the ZoomTo is called twice then it zooms at double speed.
         * 
         * If "zoom" = 1, "scale" = "basescale".
         */
        private float ZoomIndex; // speed of zooming
        private int zoomID;

        private readonly int zoomtime;
        private readonly float zoomrate;
        
        public MouseButton MovementButton = MouseButton.Button3;
        EventManager EventManager;
        public Camera(EventManager EventManager, Vector2 WindowSize, float WindowUnit, int ZoomTime, float ZoomRate) : base(EventManager, WindowSize, WindowUnit)
        {
            this.EventManager = EventManager;
            EventManager.MouseDown += OnMouseDown;
            EventManager.MouseUp += OnMouseUp;
            EventManager.MouseWheel += OnMouseWheel;
            


            zoomID = 0;
            this.ZoomIndex = 0;
            zoomrate = ZoomRate;
            zoomtime = ZoomTime;
        }
        /// <summary>
        /// Sets "zoomrate" to 0 if object ZoomID matches the last time "ZoomTo" was called.
        /// </summary>
        /// <param name="ID">The local ZoomID.</param>
        /// <summary>
        /// Sets "zoomrate" to 0 if object ZoomID matches the last time "ZoomTo" was called.
        /// </summary>
        /// <param name="ID">The local ZoomID.</param>
        private void ResetZoomRate(int ID) 
        {
            if (zoomID == ID)
            {
                ZoomIndex = 0;
                EventManager.System_Process -= ZoomCamera;
            }
        }


        public void OnMouseDown(MouseState MouseState, MouseButtonEventArgs e)
        {
            if (e.Button == MovementButton) EventManager.MouseMove += MoveCamera;
        }
        public void OnMouseUp(MouseState MouseState, MouseButtonEventArgs e)
        {
            if (e.Button == MovementButton) EventManager.MouseMove -= MoveCamera;
        }
        public void OnMouseWheel(MouseState MouseState, MouseWheelEventArgs e)
        {
            if (MouseState.ScrollDelta.Y > 0) ZoomBy(1); // zoom in
            else ZoomBy(-1); // zoom out
        }


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
                if (ZoomIndex == 0) // if begin to zoom
                {
                    EventManager.System_Process += ZoomCamera;
                }
                ZoomIndex += Step;
                int localID = zoomID; // needs to be local here not when ResetZoom is called
                
                Task.Delay(zoomtime).ContinueWith(ID => ResetZoomRate(localID)); // after zoomtime(ms) stops zooming if local ID matches zoom ID
            }
        }

        /// <summary>
        /// zooms the camera by the zoomrate ^ zoom index in delta time
        /// </summary>
        /// <param name="delta">time passed since last processed.</param>
        public void ZoomCamera(float delta) => Zoom *= MathF.Pow(MathF.Pow(zoomrate, ZoomIndex), delta); // decrease by zoomrate in zoomtime

        /// <summary>
        /// moves the camera by the change in mouse position
        /// </summary>
        /// <param name="MouseDelta">the </param>
        private void MoveCamera(MouseState M, MouseMoveEventArgs e) => Position += new Vector2(e.Delta.X / windowsize.X * 2, -e.Delta.Y / windowsize.Y * 2);
    }
}
