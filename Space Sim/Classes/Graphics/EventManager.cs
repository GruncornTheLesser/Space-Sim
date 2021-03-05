using System;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace  Graphics
{
    public class EventManager
    {
        internal float Program_Speed = 0;
        internal float Program_Time = 0;
        internal float System_Time = 0;

        /// <summary>
        /// called when a button is pressed.
        /// </summary>
        internal Action<MouseState, MouseButtonEventArgs> MouseDown;
        /// <summary>
        /// called when a button is released.
        /// </summary>
        internal Action<MouseState, MouseButtonEventArgs> MouseUp;
        /// <summary>
        /// called when the mouse whee scrolls.
        /// </summary>
        internal Action<MouseState, MouseWheelEventArgs> MouseWheel;
        /// <summary>
        /// called when the mouse is moved.
        /// </summary>
        internal Action<MouseState, MouseMoveEventArgs> MouseMove;
        /// <summary>
        /// called when the window size is changed
        /// </summary>
        internal Action<Vector2> WindowResize;
        /// <summary>
        /// Called every frame before rendering. delta is time passed since last process in program time.
        /// </summary>
        internal Action<float> Program_Process;
        /// <summary>
        /// Called every frame before rendering. delta is time passed since last process in system time.
        /// </summary>
        internal Action<float> System_Process;

        public EventManager()
        {
            MouseDown = (M, e) => { };
            MouseUp = (M, e) => { };
            MouseMove = (M, e) => { };
            MouseWheel = (M, e) => { };
            WindowResize = (Size) => { };
            Program_Process = (delta) => { };
            System_Process = (delta) =>
            {
                System_Time += delta;
                Program_Time += delta * Program_Speed;
                Program_Process(delta * Program_Speed);
            };
        }

    }
}
