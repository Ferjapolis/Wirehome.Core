﻿using System;
using System.Linq;
using Wirehome.Conditions;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Components.States;

namespace Wirehome.Automations
{
    public static class TurnOnAndOffAutomationExtensions
    {
        public static TurnOnAndOffAutomation WithTurnOnIfAllRollerShuttersClosed(this TurnOnAndOffAutomation automation, params IRollerShutter[] rollerShutters)
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
