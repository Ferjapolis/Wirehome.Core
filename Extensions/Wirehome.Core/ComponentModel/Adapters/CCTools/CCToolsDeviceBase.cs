﻿using Quartz;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters.Drivers;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.Extensions;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Extensions;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Quartz;

namespace Wirehome.ComponentModel.Adapters
{
    public abstract class CCToolsBaseAdapter : Adapter
    {
        protected readonly ILogger _log;
        protected readonly II2CBusService _i2CBusService;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly ISchedulerFactory _schedulerFactory;

        private int _poolDurationWarning;

        protected II2CPortExpanderDriver _portExpanderDriver;
        private byte[] _committedState;
        private byte[] _state;

        protected CCToolsBaseAdapter(IAdapterServiceFactory adapterServiceFactory)
        {
            _i2CBusService = adapterServiceFactory.GetI2CService();
            _log = adapterServiceFactory.GetLogger().CreatePublisher($"{nameof(CCToolsBaseAdapter)}_{Uid}");
            _eventAggregator = adapterServiceFactory.GetEventAggregator();
            _schedulerFactory = adapterServiceFactory.GetSchedulerFactory();

            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        public override async Task Initialize()
        {
            var poolInterval = (IntValue)this[AdapterProperties.PoolInterval];
            _poolDurationWarning = (IntValue)this[AdapterProperties.PollDurationWarningThreshold];

            _state = new byte[_portExpanderDriver.StateSize];
            _committedState = new byte[_portExpanderDriver.StateSize];

            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.ScheduleIntervalWithContext<CCToolsSchedulerJob, CCToolsBaseAdapter>(TimeSpan.FromMilliseconds(poolInterval), this, _disposables.Token);

            _disposables.Add(_eventAggregator.SubscribeForDeviceQuery<DeviceCommand>(DeviceCommandHandler, Uid));

            base.Initialize();
        }

        private Task<object> DeviceCommandHandler(IMessageEnvelope<DeviceCommand> messageEnvelope) => ExecuteCommand(messageEnvelope.Message, messageEnvelope.CancellationToken);

        protected Task RefreshCommandHandler(Command message) => FetchState();

        protected Task<object> DiscoverCapabilitiesHandler(Command message) => new DiscoveryResponse(RequierdProperties(), new PowerState()).ToStaticTaskResult();

        protected void UpdateCommandHandler(Command message)
        {
            var state = message[PowerState.StateName] as StringValue;
            var pinNumber = message[AdapterProperties.PinNumber] as IntValue;
            SetPortState(pinNumber.Value, PowerStateValue.ToBinaryState(state), true);
        }

        protected Task<object> QueryCommandHandler(Command message)
        {
            var state = message[PowerState.StateName] as StringValue;
            var pinNumber = message[AdapterProperties.PinNumber] as IntValue;
            return Task.FromResult<object>(GetPortState(pinNumber));
        }

        private async Task FetchState()
        {
            var stopwatch = Stopwatch.StartNew();

            var newState = _portExpanderDriver.Read();

            stopwatch.Stop();

            if (newState.SequenceEqual(_state)) return;

            var oldState = _state.ToArray();

            Buffer.BlockCopy(newState, 0, _state, 0, newState.Length);
            Buffer.BlockCopy(newState, 0, _committedState, 0, newState.Length);

            var oldStateBits = new BitArray(oldState);
            var newStateBits = new BitArray(newState);

            for (int i = 0; i < oldStateBits.Length; i++)
            {
                var oldPinState = oldStateBits.Get(i);
                var newPinState = newStateBits.Get(i);

                if (oldPinState == newPinState) return;

                var properyChangeEvent = new PropertyChangedEvent(Uid, PowerState.StateName, new BooleanValue(oldPinState),
                                            new BooleanValue(newPinState), new Dictionary<string, IValue>() { { AdapterProperties.PinNumber, new IntValue(i) } });

                await _eventAggregator.PublishDeviceEvent(properyChangeEvent, _requierdProperties);

                _log.Info($"'{Uid}' fetched different state ({oldState.ToBitString()}->{newState.ToBitString()})");
            }

            if (stopwatch.ElapsedMilliseconds > _poolDurationWarning)
            {
                _log.Warning($"Polling device '{Uid}' took {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        protected void SetState(byte[] state, bool commit)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            Buffer.BlockCopy(state, 0, _state, 0, state.Length);

            if (commit) CommitChanges();
        }

        private void CommitChanges(bool force = false)
        {
            if (!force && _state.SequenceEqual(_committedState)) return;

            _portExpanderDriver.Write(_state);
            Buffer.BlockCopy(_state, 0, _committedState, 0, _state.Length);

            _log.Verbose("Board '" + Uid + "' committed state '" + BitConverter.ToString(_state) + "'.");
        }

        private BinaryState GetPortState(int id)
        {
            return _state.GetBit(id) ? BinaryState.High : BinaryState.Low;
        }

        private void SetPortState(int pinNumber, BinaryState state, bool commit)
        {
            _state.SetBit(pinNumber, state == BinaryState.High);

            if (commit) CommitChanges();
        }
    }
}