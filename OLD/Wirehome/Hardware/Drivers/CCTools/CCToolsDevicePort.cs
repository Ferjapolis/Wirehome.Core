﻿using System;
using System.Collections;
using System.Threading.Tasks;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Hardware.Drivers.CCTools
{
    public sealed class CCToolsDevicePort : IBinaryOutput
    {
        private readonly int _id;
        private readonly CCToolsDeviceBase _board;
        
        public CCToolsDevicePort(int id, CCToolsDeviceBase board)
        {
            _id = id;
            _board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;
        
        public BinaryState Read()
        {
            return _board.GetPortState(_id);
        }

        public void Write(BinaryState state, WriteBinaryStateMode mode)
        {
            _board.SetPortState(_id, state, mode == WriteBinaryStateMode.Commit);
        }

        internal void OnBoardStateChanged(BitArray oldState, BitArray newState)
        {
            var stateChangedEvent = StateChanged;
            if (stateChangedEvent == null)
            {
                return;
            }

            var oldPinState = oldState.Get(_id);
            var newPinState = newState.Get(_id);

            if (oldPinState == newPinState)
            {
                return;
            }

            var newBinaryState = newPinState ? BinaryState.High : BinaryState.Low;
            var oldBinaryState = oldPinState ? BinaryState.High : BinaryState.Low;
            
            // TODO: Create a message bus message.
            Task.Run(() => stateChangedEvent.Invoke(this, new BinaryStateChangedEventArgs(oldBinaryState, newBinaryState)));
        }
    }
}