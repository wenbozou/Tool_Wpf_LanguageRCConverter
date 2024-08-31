using System;

namespace LanguageRCConverter.Command
{
    /// <summary>
    /// The command that relays its functionality by invoking delegates.
    /// </summary>
    public class RelayCommand : CommandBase
    {
        private Action<object>      ActonExecute = null;
        private Func<object, bool>  FuncCanExecute = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execute.</param>
        /// <param name="canExecute">The can execute.</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            if (null == execute)
            {
                throw new ArgumentNullException("execute");
            }
            if (null == canExecute)
            {
                // No can execute provided, then always executable
                canExecute = (o) => true;
            }
            this.ActonExecute = execute;
            this.FuncCanExecute = canExecute;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        public override bool CanExecute(object parameter)
        {
            
            return (null == this.FuncCanExecute) ? true : this.FuncCanExecute(parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        protected override void OnExecute(object parameter)
        {
            this.ActonExecute(parameter);
        }

    }

}
