﻿using System;
using System.Collections.Generic;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Core.Timer;

namespace HA4IoT.Actuators.Animations
{
    public class DirectionAnimation : Animation
    {
        private LogicalBinaryStateActuator _actuator;
        private bool _isForward = true;
        private StateMachineStateId _targetState;
        private TimeSpan _duration = TimeSpan.FromMilliseconds(250);
        
        public DirectionAnimation(IHomeAutomationTimer timer) : base(timer)
        {
        }

        public DirectionAnimation WithDuration(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }

        public DirectionAnimation WithForwardDirection()
        {
            _isForward = true;
            return this;
        }

        public DirectionAnimation WithReversed()
        {
            _isForward = false;
            return this;
        }

        public DirectionAnimation WithTargetState(StateMachineStateId state)
        {
            _targetState = state;
            return this;
        }

        public DirectionAnimation WithTargetOnState()
        {
            _targetState = DefaultStateIDs.On;
            return this;
        }

        public DirectionAnimation WithTargetOffState()
        {
            _targetState = DefaultStateIDs.Off;
            return this;
        }

        public DirectionAnimation WithActuator(LogicalBinaryStateActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuator = actuator;
            return this;
        }

        public override void Start()
        {
            if (_actuator.Actuators.Count < 2)
            {
                return;
            }

            Frames.Clear();
            
            var orderedActuators = new List<IStateMachine>(_actuator.Actuators);
            if (!_isForward)
            {
                orderedActuators.Reverse();
            }

            double frameLength = _duration.TotalMilliseconds/(orderedActuators.Count - 1);

            for (int i = 0; i < orderedActuators.Count; i++)
            {
                var actuator = orderedActuators[i];
                var offset = TimeSpan.FromMilliseconds(frameLength * i);

                WithFrame(new Frame().WithTargetState(actuator, _targetState).WithStartTime(offset));
            }

            var lastFrame = new Frame().WithStartTime(_duration);
            foreach (var actuator in _actuator.Actuators)
            {
                lastFrame.WithTargetState(actuator, _targetState);
            }

            WithFrame(lastFrame);
            base.Start();
        }
    }
}
