using System;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace  Graphics
{
    public static class EventManager
    {
        internal static float Program_Speed = 0;
        internal static float Program_Time = 0;
        internal static float System_Time = 0;

        /// <summary>
        /// called when a button is pressed.
        /// </summary>
        internal static Action<MouseState, MouseButtonEventArgs> MouseDown = (M, e) => { };
        /// <summary>
        /// called when a button is released.
        /// </summary>
        internal static Action<MouseState, MouseButtonEventArgs> MouseUp = (M, e) => { };
        /// <summary>
        /// called when the mouse whee scrolls.
        /// </summary>
        internal static Action<MouseState, MouseWheelEventArgs> MouseWheel = (M, e) => { };
        /// <summary>
        /// called when the mouse is moved.
        /// </summary>
        internal static Action<MouseState, MouseMoveEventArgs> MouseMove = (M, e) => { };
        /// <summary>
        /// called when the window size is changed
        /// </summary>
        internal static Action<Vector2> WindowResize = (Size) => { };
        /// <summary>
        /// Called every frame before rendering. delta is time passed since last process in program time.
        /// </summary>
        internal static Action<float> Program_Process = (delta) => { };
        /// <summary>
        /// Called every frame before rendering. delta is time passed since last process in system time.
        /// </summary>
        internal static Action<float> System_Process = (delta) =>
        {
            System_Time += delta;
            Program_Time += delta * Program_Speed;
            Program_Process(delta * Program_Speed);
        };

    }
}
