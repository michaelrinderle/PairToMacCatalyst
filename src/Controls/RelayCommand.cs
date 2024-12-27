﻿/*
    MIT License

    michael rinderle 2024
    written by michael rinderle <michael@sofdigital.net>

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

*/

using System;
using System.Windows.Input;

namespace Ptm.Controls;

/// <summary>
///     Represents a command that can be executed.
/// </summary>
/// <remarks>
///     This command can be used to bind actions to UI elements.
/// </remarks>
public class RelayCommand
    : ICommand
{
    private readonly Predicate<object> _canExecute;
    private readonly Action<object> _execute;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">The predicate to determine if the action can be executed.</param>
    /// <exception cref="ArgumentNullException">Thrown when the execute action is null.</exception>
    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    ///     Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">
    ///     Data used by the command. If the command does not require data to be passed, this object can be
    ///     set to null.
    /// </param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name="parameter">
    ///     Data used by the command. If the command does not require data to be passed, this object can be
    ///     set to null.
    /// </param>
    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    /// <summary>
    ///     Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

public class RelayCommand<T>
    : ICommand
{
    private readonly Predicate<T> _canExecute;
    private readonly Action<T> _execute;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelayCommand{T}" /> class.
    /// </summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">The predicate to determine if the action can be executed.</param>
    /// <exception cref="ArgumentNullException">Thrown when the execute action is null.</exception>
    public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    ///     Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">
    ///     Data used by the command. If the command does not require data to be passed, this object can be
    ///     set to null.
    /// </param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke((T)parameter) ?? true;
    }

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name="parameter">
    ///     Data used by the command. If the command does not require data to be passed, this object can be
    ///     set to null.
    /// </param>
    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }

    /// <summary>
    ///     Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}