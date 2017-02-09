﻿using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.StateMachines
{
    public static class StateMachineExtensions
    {
        public static IStateMachine GetStateMachine(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<IStateMachine>(ComponentIdGenerator.Generate(area.Id, id));
        }

        public static bool GetSupportsOffState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.SupportsState(BinaryStateId.Off);
        }

        public static bool GetSupportsOnState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.SupportsState(BinaryStateId.On);
        }

        public static StateMachineState AddOffState(this StateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.AddState(BinaryStateId.Off);
        }

        public static StateMachineState AddOnState(this StateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return stateMachine.AddState(BinaryStateId.On);
        }

        public static StateMachineState AddState(this StateMachine stateMachine, GenericComponentState id)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));
            if (id == null) throw new ArgumentNullException(nameof(id));

            var state = new StateMachineState(id);
            stateMachine.AddState(state);
            return state;
        }

        public static void SetNextState(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            var activeStateId = stateMachine.GetState();
            var nextStateId = stateMachine.GetNextState(activeStateId.Get<GenericComponentState>());

            stateMachine.ChangeState(nextStateId);
        }

        public static bool TryTurnOff(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            if (!stateMachine.SupportsState(BinaryStateId.Off))
            {
                return false;
            }

            stateMachine.ChangeState(BinaryStateId.Off);
            return true;
        }

        public static bool TryTurnOn(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            if (!stateMachine.SupportsState(BinaryStateId.On))
            {
                return false;
            }

            stateMachine.ChangeState(BinaryStateId.On);
            return true;
        }

        public static Action GetSetStateAction(this IStateMachine stateStateMachine, GenericComponentState stateId)
        {
            if (stateStateMachine == null) throw new ArgumentNullException(nameof(stateStateMachine));
            if (stateId == null) throw new ArgumentNullException(nameof(stateId));

            return () => stateStateMachine.ChangeState(stateId);
        }

        public static Action GetTurnOnAction(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return () => stateMachine.ChangeState(BinaryStateId.On);
        }

        public static Action GetTurnOffAction(this IStateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            return () => stateMachine.ChangeState(BinaryStateId.Off);
        }

        public static Action GetSetNextStateAction(this IStateMachine stateStateMachine)
        {
            if (stateStateMachine == null) throw new ArgumentNullException(nameof(stateStateMachine));

            return stateStateMachine.SetNextState;
        }
    }
}
