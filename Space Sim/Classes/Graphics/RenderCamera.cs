using OpenTK.Mathematics;
using System;

namespace Graphics
{

    /* THING TO DO:
     * have not implemented the movement while zooming. ie zoom to location - dont know if user wants this. -> they dont care.
     */

    class RenderCamera : Transform
    {

        private Vector2 basescale; // used when changing window size
        private float zoom;

        public Matrix3 BaseMatrix;
        /// <summary>
        /// returns matrix for transforming between window view in pixels for renderobjects fixed to screen
        /// </summary>
        public readonly float windowunit;
        public Vector2 windowsize;


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
        public RenderCamera(EventManager EventManager, Vector2 WindowSize, float WindowUnit) : base(0, 1f / WindowSize.X / WindowUnit, (1f / WindowSize.Y / WindowUnit) * (WindowSize.Y / WindowSize.X), 0, 0)
        {
            EventManager.WindowResize += OnUpdateWindowSize;

            zoom = 1;
            windowunit = WindowUnit;
            basescale = Scale;
            
            float M = MathF.Min(WindowSize.X, WindowSize.Y);
            Vector2 Normalized = new Vector2(M / WindowSize.X, M / WindowSize.Y);
            BaseMatrix = new Matrix3(Normalized.X, 0, 0, 0, Normalized.Y, 0, 0, 0, 1);
        }

        /// <summary>
        /// Changes the camera matrix to reflect the new window size.
        /// </summary>
        /// <param name="WindowSize">New window size.</param>
        public void OnUpdateWindowSize(Vector2 WindowSize)
        {
            windowsize = WindowSize;
            basescale = new Vector2(zoom / WindowSize.X / windowunit, zoom / WindowSize.Y / windowunit);
            
            float M = MathF.Min(WindowSize.X, WindowSize.Y);
            Vector2 Normalized = new Vector2(M / WindowSize.X, M / WindowSize.Y);
            BaseMatrix = new Matrix3(Normalized.X, 0, 0, 0, Normalized.Y, 0, 0, 0, 1);

            Zoom = Zoom; // updates matrices with new basescales 
        }
    }
}
