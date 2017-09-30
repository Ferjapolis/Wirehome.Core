﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Wirehome.Contracts.Components.Adapters;

namespace Wirehome.Simulator.Controls
{
    public class UIButtonAdapter : IButtonAdapter
    {
        private readonly Button _button;

        public UIButtonAdapter(Button button)
        {
            _button = button ?? throw new ArgumentNullException(nameof(button));
        }

        public event EventHandler<ButtonAdapterStateChangedEventArgs> StateChanged;

        public void Connect()
        {
            _button.Click += (s, e) => PressAndRelease();
            _button.PointerPressed += (s, e) => Press();
            _button.PointerReleased += (s, e) => Release();
        }

        private void Press()
        {
            Task.Run(() => StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Pressed)));
        }

        private void Release()
        {
            Task.Run(() => StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Released)));
        }

        private void PressAndRelease()
        {
            Task.Run(() =>
            {
                StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Pressed));
                StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Released));
            });
        }
    }
}
