using System;

namespace MediaBrowser.Model.Events
{
    /// <summary>
    /// Provides a generic EventArgs subclass that can hold any kind of object.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    public class GenericEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericEventArgs{T}"/> class.
        /// </summary>
        public GenericEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericEventArgs{T}"/> class.
        /// </summary>
        /// <param name="arg">The argument.</param>
        public GenericEventArgs(T arg)
        {
            Argument = arg;
        }

        /// <summary>
        /// Gets or sets the argument.
        /// </summary>
        /// <value>The argument.</value>
        public T Argument { get; set; }
    }
}
