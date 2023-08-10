using System;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Reference data to call back the application that started the Planning Poker after estimation is finished.
    /// </summary>
    public class ApplicationCallbackReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationCallbackReference"/> class.
        /// </summary>
        /// <param name="url">The url of the application to send the callback message to.</param>
        /// <param name="reference">The reference that the external application can use to identify an estimation callback.</param>
        public ApplicationCallbackReference(Uri url, string reference)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        }

        /// <summary>
        /// Gets a url of the application to send the callback message to.
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// Gets a reference that the external application can use to identify an estimation callback.
        /// </summary>
        public string Reference { get; }
    }
}
