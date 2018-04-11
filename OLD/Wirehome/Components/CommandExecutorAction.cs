﻿using System;
using Wirehome.Contracts.Components.Commands;

namespace Wirehome.Components
{
    public class CommandExecutorAction<TCommand> : ICommandExecutorAction where TCommand : ICommand
    {
        private readonly Action<TCommand> _command;

        public CommandExecutorAction(Action<TCommand> command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _command = command;
        }

        public void Execute(ICommand command)
        {
            _command((TCommand)command);
        }
    }
}