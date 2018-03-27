﻿using CSharpFunctionalExtensions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Extensions;
using Wirehome.Core.Services.DependencyInjection;

namespace Wirehome.ComponentModel
{
    public class BaseObject
    {
        private readonly Subject<Event> _events = new Subject<Event>();
        [Map] private Dictionary<string, Property> _properties { get; set; } = new Dictionary<string, Property>();
        protected bool SupressPropertyChangeEvent { get; set; }
        public string Uid { get; protected set; }
        public string Type { get; set; }
        public List<string> Tags { get; private set; } = new List<string>();
        public IObservable<Event> Events => _events.AsObservable();
        public IReadOnlyDictionary<string, Property> Properties => _properties.AsReadOnly();
        
        public IValue this[string propertyName]
        {
            get => GetPropertyValue(propertyName).Value ?? throw new KeyNotFoundException();
            set { SetPropertyValue(propertyName, value); }
        }

        public virtual Maybe<IValue> GetPropertyValue(string propertyName, IValue defaultValue = null)
        {
            var property = _properties[propertyName];
            if (property == null)
            {
                return defaultValue != null ? Maybe<IValue>.From(defaultValue) : Maybe<IValue>.None;
            }
            return Maybe<IValue>.From(property.Value);
        }

        public virtual void SetPropertyValue(string propertyName, IValue value)
        {
            Property property;
            if (!_properties.ContainsKey(propertyName))
            {
                property = new Property
                {
                    Type = propertyName
                };
                _properties.Add(propertyName, property);
            }
            else
            {
                property = _properties[propertyName];
            }
            var oldValue = property.Value;
            property.Value = value;

            if (SupressPropertyChangeEvent || value.Equals(oldValue)) return;

            _events.OnNext(new PropertyChangedEvent(Uid, property.Type, oldValue, value));
        }

        public IDictionary<string, string> GetPropertiesStrings() => _properties.Values.ToDictionary(k => k.Type, v => v.Value.ToString());
        
    }
}