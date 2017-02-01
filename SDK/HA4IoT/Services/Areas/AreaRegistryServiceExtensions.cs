﻿using System;
using HA4IoT.Contracts.Areas;

namespace HA4IoT.Services.Areas
{
    public static class AreaRegistryServiceExtensions
    {
        public static IArea RegisterArea(this IAreaRegistryService areaService, Enum id)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));
            return areaService.RegisterArea(new AreaId(id));
        }

        public static IArea GetArea(this IAreaRegistryService areaService, Enum id)
        {
            if (areaService == null) throw new ArgumentNullException(nameof(areaService));

            return areaService.GetArea(new AreaId(id));
        }
    }
}
