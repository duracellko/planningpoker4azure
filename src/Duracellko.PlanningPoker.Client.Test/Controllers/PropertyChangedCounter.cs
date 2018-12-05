using System.ComponentModel;

namespace Duracellko.PlanningPoker.Client.Test.Controllers
{
    public class PropertyChangedCounter
    {
        private INotifyPropertyChanged _target;

        public int Count { get; set; }

        public INotifyPropertyChanged Target
        {
            get
            {
                return _target;
            }

            set
            {
                if (_target != null)
                {
                    _target.PropertyChanged -= TargetOnPropertyChanged;
                }

                _target = value;

                if (_target != null)
                {
                    _target.PropertyChanged += TargetOnPropertyChanged;
                }
            }
        }

        private void TargetOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Count++;
        }
    }
}
