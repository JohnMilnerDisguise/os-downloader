using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SGet.Commands
{
    internal class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public Action<object> _Execute { get; set; }

        public Predicate<object> _CanExecute { get; set; }


        public RelayCommand(Action<object> ExecuteMethod, Predicate<object> CanExecute )
        {
            _Execute = ExecuteMethod;
            _CanExecute = CanExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _Execute(parameter);
        }
    }
}
