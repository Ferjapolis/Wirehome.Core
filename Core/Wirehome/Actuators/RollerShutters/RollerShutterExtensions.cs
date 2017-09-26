﻿using System;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Areas;

namespace Wirehome.Actuators.RollerShutters
{
    public static class RollerShutterExtensions
    {
        public static IRollerShutter GetRollerShutter(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent<RollerShutter>($"{area.Id}.{id}");
        }
    }
}
