using System.ComponentModel;

namespace Duracellko.PlanningPoker.Client.Test.Controllers;

public class PropertyChangedCounter
{
    public int Count { get; set; }

    public INotifyPropertyChanged? Target
    {
        get;
        set
        {
            if (field != null)
            {
                field.PropertyChanged -= TargetOnPropertyChanged;
            }

            field = value;

            if (field != null)
            {
                field.PropertyChanged += TargetOnPropertyChanged;
            }
        }
    }

    private void TargetOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Count++;
    }
}
