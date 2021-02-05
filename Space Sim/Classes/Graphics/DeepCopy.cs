using System;
using System.Collections.Generic;
using System.Text;

namespace DeepCopy
{
    // I'm using deep copies to replace value pointers.
    // Ive set up the deep copies using delegates/Func/Action which are to my understanding function pointers

    /// <summary>
    /// A deepCopy interface for passing into functions
    /// </summary>
    interface IDeepCopy { }

    /// <summary>
    /// A deep copy updates the original value when changed and vice versa. This is opposed to a shallow copy that doesnt update the original.
    /// </summary>
    /// <typeparam name="T">The type of the value being copied. T must have the new() constraint.</typeparam>
    class DeepCopy<T> : IDeepCopy where T : new()
    {
        // 2 delegates
        private readonly Func<T> GET;
        private readonly Action<T> SET;

        // create a property from the delegate functions
        public T Value
        {
            get => GET();
            set => SET(value);
        }

        // Default Value constructor. If no delegates are passed in, uses a local value. this doesnt store a value but is useful.
        /// <summary>
        /// Constructs a deepcopy of a new local variable. This is so a default deepcopy value can be constructed before the target value has been constructed.
        /// </summary>
        public DeepCopy()
        {
            T local = new T();
            GET = () => local;
            SET = (value) => local = value;
        }
        /// <summary>
        /// Constructs deep copy from the setter and getter functions. 
        /// </summary>
        /// <param name="getter">The function to get the parameter. The get function must return one value of type T. eg '() => parameter'</param>
        /// <param name="setter">The action to set the parameter. The set action must take one value of type T. eg 'value => parameter = value'. </param>
        public DeepCopy(Func<T> getter, Action<T> setter)
        {
            // pass in a get and set delegates
            GET = getter;
            SET = setter;
        }
    }
}
