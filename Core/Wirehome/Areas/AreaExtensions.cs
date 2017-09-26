﻿using System;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Components;

namespace Wirehome.Areas
{
    public static class AreaExtensions
    {
        public static IComponent GetComponent(this IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            return area.GetComponent(area.Id + "." + id);
        }
    }
}
