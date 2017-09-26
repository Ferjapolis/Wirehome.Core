﻿using System;
using System.Collections.Generic;
using System.Linq;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;

namespace Wirehome.Components
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

        public override IComponentFeatureStateCollection GetState()
        {
            return Components.First().GetState();
        }

        public override IComponentFeatureCollection GetFeatures()
        {
            return Components.First().GetFeatures();
        }

        public override void ExecuteCommand(ICommand command)
        {
            foreach (var component in Components)
            {
                component.ExecuteCommand(command);
            }
        }
    }
}