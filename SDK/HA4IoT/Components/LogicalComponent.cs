﻿using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Components
{
    public class LogicalComponent : ComponentBase
    {
        public LogicalComponent(string id) : base(id)
        {
        }

        public IList<IComponent> Components { get; } = new List<IComponent>();

        public LogicalComponent WithComponent(IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            Components.Add(component);
            return this;
        }

        public override ComponentFeatureStateCollection GetState()
        {
            return Components.First().GetState();
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return Components.First().GetFeatures();
        }

        public override void InvokeCommand(ICommand command)
        {
            foreach (var component in Components)
            {
                component.InvokeCommand(command);
            }
        }
    }
}