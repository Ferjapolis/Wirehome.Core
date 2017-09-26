﻿using System;
using Windows.Devices.Gpio;
using Windows.System.Threading;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.RaspberryPi;
using Wirehome.Contracts.Logging;

namespace Wirehome.Hardware.Drivers.RaspberryPi
{
    public sealed class GpioInputPort : IBinaryInput, IDisposable
    {
        private const int PollInterval = 15; // TODO: Set from constructor. Consider two classes with "IGpioMonitoringStrategy".

        private readonly GpioPin _pin;
        // ReSharper disable once NotAccessedField.Local
        //private readonly Timer _timer;

        private BinaryState _latestState;

        public GpioInputPort(GpioPin pin, GpioInputMonitoringMode mode, GpioPullMode pullMode)
        {
            _pin = pin ?? throw new ArgumentNullException(nameof(pin));
            if (pullMode == GpioPullMode.High)
            {
                _pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            }
            else if (pullMode == GpioPullMode.Low)
            {
                _pin.SetDriveMode(GpioPinDriveMode.InputPullDown);
            }
            else
            {
                _pin.SetDriveMode(GpioPinDriveMode.Input);
            }
            
            if (mode == GpioInputMonitoringMode.Polling)
            {
                ThreadPoolTimer.CreatePeriodicTimer(PollState, TimeSpan.FromMilliseconds(PollInterval));
            }
            else if (mode == GpioInputMonitoringMode.Interrupt)
            {
                //_pin.DebounceTimeout = TimeSpan.FromTicks(DebounceTimeoutTicks); // TODO: Set from constructor.
                _pin.ValueChanged += HandleInterrupt;
            }

            _latestState = ReadAndConvert();
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public BinaryState Read()
        {
            return Update(ReadAndConvert());
        }

        public void Dispose()
        {
            _pin.ValueChanged -= HandleInterrupt;
            _pin?.Dispose();
        }

        private void HandleInterrupt(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            var newState = ReadAndConvert();

            Log.Default.Verbose("Interrupt raised for GPIO" + _pin.PinNumber + ".");
            Update(newState);
        }

        private void PollState(object state)
        {
            try
            {
                Update(ReadAndConvert());
            }
            catch (Exception exception)
            {
                Log.Default.Error(exception, $"Error while polling input state of GPIO{_pin.PinNumber}.");
            }
        }

        private BinaryState ReadAndConvert()
        {
            return _pin.Read() == GpioPinValue.High ? BinaryState.High : BinaryState.Low;
        }

        private BinaryState Update(BinaryState newState)
        {
            var oldState = _latestState;

            if (oldState == newState)
            {
                return oldState;
            }

            _latestState = newState;

            try
            {
                StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, newState));
            }
            catch (Exception exception)
            {
                Log.Default.Error(exception, $"Error while reading input state of GPIO{_pin.PinNumber}.");
            }
            
            return newState;
        }
    }
}
