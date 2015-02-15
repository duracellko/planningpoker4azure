// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Collection of values in queue to initialize.
    /// </summary>
    public class InitializationList
    {
        #region Fields

        private readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        private readonly object listLock = new object();
        private List<string> list;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the initialization queue is empty.
        /// </summary>
        /// <value>
        /// <c>True</c> if the initialization queue is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                lock (this.listLock)
                {
                    return this.list != null && this.list.Count == 0;
                }
            }
        }

        /// <summary>
        /// Gets the values in queue for initialization.
        /// </summary>
        /// <value>
        /// The values to initialize.
        /// </value>
        public IList<string> Values
        {
            get
            {
                lock (this.listLock)
                {
                    return this.list != null ? this.list.ToList() : null;
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether specified value is in queue for initialization or initialization has not started yet.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns><c>True</c> if the value is in queue or initialization has not started; otherwise <c>false</c>.</returns>
        public bool ContainsOrNotInit(string value)
        {
            lock (this.listLock)
            {
                return this.list == null || this.list.Contains(value, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Sets specified values to initialization queue, if the queue is not initialized yet.
        /// </summary>
        /// <param name="values">The values to initialize.</param>
        /// <returns><c>True</c> if queue was setup successfully; otherwise <c>false</c>.</returns>
        public bool Setup(IEnumerable<string> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            lock (this.listLock)
            {
                if (this.list == null)
                {
                    this.list = values.ToList();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes the specified value from initialization queue.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns><c>True</c> if value was removed successfully; otherwise <c>false</c>.</returns>
        public bool Remove(string value)
        {
            lock (this.listLock)
            {
                return this.list != null && this.list.RemoveAll(v => this.comparer.Equals(v, value)) != 0;
            }
        }

        /// <summary>
        /// Clears the initialization queue.
        /// </summary>
        public void Clear()
        {
            lock (this.listLock)
            {
                this.list = new List<string>();
            }
        }

        #endregion
    }
}
