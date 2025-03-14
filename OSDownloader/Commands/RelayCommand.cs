using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OSDownloader.Commands
{
    internal class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Action<object> _Execute { get; set; }

        public Predicate<object> _CanExecute { get; set; }


        public RelayCommand(Action<object> ExecuteMethod, Predicate<object> CanExecute )
        {
            _Execute = ExecuteMethod;
            _CanExecute = CanExecute;
            CommandManager.RequerySuggested += CommandManager_RequerySuggested;
        }

        public bool CanExecute(object parameter)
        {
            return _CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _Execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CommandManager_RequerySuggested(object sender, EventArgs e)
        {
            RaiseCanExecuteChanged();
        }
    }
}
