using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Duracellko.PlanningPoker.Azure;

/// <summary>
/// Collection of values in queue to initialize.
/// </summary>
public class InitializationList
{
    private readonly StringComparer _comparer = StringComparer.OrdinalIgnoreCase;
    private readonly Lock _listLock = new();
    private List<string>? _list;

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
            lock (_listLock)
            {
                return _list != null && _list.Count == 0;
            }
        }
    }

    /// <summary>
    /// Gets the values in queue for initialization.
    /// </summary>
    /// <value>
    /// The values to initialize.
    /// </value>
    [SuppressMessage("Critical Code Smell", "S2365:Properties should not make collection or array copies", Justification = "Creates copy to be thread-safe.")]
    public IList<string>? Values
    {
        get
        {
            lock (_listLock)
            {
                return _list?.ToList();
            }
        }
    }

    /// <summary>
    /// Determines whether specified value is in queue for initialization or initialization has not started yet.
    /// </summary>
    /// <param name="value">The value to search for.</param>
    /// <returns><c>True</c> if the value is in queue or initialization has not started; otherwise <c>false</c>.</returns>
    public bool ContainsOrNotInit(string value)
    {
        lock (_listLock)
        {
            return _list == null || _list.Contains(value, StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Sets specified values to initialization queue, if the queue is not initialized yet.
    /// </summary>
    /// <param name="values">The values to initialize.</param>
    /// <returns><c>True</c> if queue was setup successfully; otherwise <c>false</c>.</returns>
    public bool Setup(IEnumerable<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        lock (_listLock)
        {
            if (_list == null)
            {
                _list = values.ToList();
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
        lock (_listLock)
        {
            return _list != null && _list.RemoveAll(v => _comparer.Equals(v, value)) != 0;
        }
    }

    /// <summary>
    /// Clears the initialization queue.
    /// </summary>
    public void Clear()
    {
        lock (_listLock)
        {
            _list = new List<string>();
        }
    }
}
