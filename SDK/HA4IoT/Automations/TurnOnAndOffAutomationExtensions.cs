﻿using System;
using System.Linq;
using HA4IoT.Conditions;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components.States;

namespace HA4IoT.Automations
{
    public static class TurnOnAndOffAutomationExtensions
    {
        public static FlipFlopAutomation WithTurnOnIfAllRollerShuttersClosed(this FlipFlopAutomation automation, params IRollerShutter[] rollerShutters)
        {
            if (automation == null) throw new ArgumentNullException(nameof(automation));
            if (rollerShutters == null) throw new ArgumentNullException(nameof(rollerShutters));

            var condition = new Condition().WithExpression(() => rollerShutters.First().GetState().Extract<PositionTrackingState>().IsClosed);
            foreach (var otherRollerShutter in rollerShutters.Skip(1))
            {
                condition.WithRelatedCondition(ConditionRelation.And, new Condition().WithExpression(() => otherRollerShutter.GetState().Extract<PositionTrackingState>().IsClosed));
            }

            return automation.WithEnablingCondition(ConditionRelation.Or, condition);
        }
    }
}
