﻿using System;
using Windows.Devices.Gpio;
using Wirehome.Contracts.Core;

namespace Wirehome.UWP
{
    public class NativeGpio : INativeGpio
    {
        private readonly GpioPin _gpioPin;
        public event Action ValueChanged;
        public int PinNumber => _gpioPin.PinNumber;
        
        public NativeGpio(GpioPin gpioPin)
        {
            _gpioPin = gpioPin ?? throw new ArgumentNullException(nameof(gpioPin));
            gpioPin.ValueChanged += GpioPin_ValueChanged;
        }

        private void GpioPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            ValueChanged?.Invoke();
        }

        public void SetDriveMode(NativeGpioPinDriveMode pinMode)
        {
            _gpioPin.SetDriveMode((GpioPinDriveMode)pinMode);
        }

        public void Dispose()
        {
            _gpioPin.Dispose();
        }

        public NativeGpioPinValue Read()
        {
            return (NativeGpioPinValue)_gpioPin.Read();
        }

        public void Write(NativeGpioPinValue pinValue)
        {
            _gpioPin.Write((GpioPinValue)pinValue);
        }

    }
}
