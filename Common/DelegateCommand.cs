using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gmail10.Common
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _executeAsync;
        private readonly Action<object> _executeParamAsync;
        private readonly Func<bool> _canExecuteAsync;

        public DelegateCommand()
        {
            this._executeAsync = delegate { };
            this._canExecuteAsync = () => true;
        }
        public DelegateCommand(Action execute)
        {
            this._executeAsync = execute;
            this._canExecuteAsync = () => true;
        }

        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            this._executeAsync = execute;
            this._canExecuteAsync = canExecute;
        }

        public DelegateCommand(Action<object> executeParam, Func<bool> canExecute)
        {
            this._executeParamAsync = executeParam;
            this._canExecuteAsync = canExecute;
        }


        public event EventHandler CanExecuteChanged;
        public void RaiseExecuteChanged()
        {
            this.CanExecuteChanged(this, EventArgs.Empty);
        }
        public bool CanExecute(object parameter)
        {
            return this._canExecuteAsync();
        }
        public void Execute(object parameter)
        {
            if (this._executeAsync != null)
                this._executeAsync();
            else if (this._executeParamAsync != null)
                this._executeParamAsync(parameter);
            else
                Debug.WriteLine("Action not set");
        }
    }
}
