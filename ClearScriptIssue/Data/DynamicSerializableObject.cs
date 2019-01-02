using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ClearScriptIssue.Extensions;

namespace ClearScriptIssue.Data
{
    public class DynamicSerializableObject
    {
        private readonly Dictionary<string, object> _properties;

        #region Constructors

        public DynamicSerializableObject()
        {
            _properties = new Dictionary<string, object>();
        }

        #endregion

        #region Properties

        public object this[string name]
        {
            get
            {
                if (_properties.ContainsKey(name)) return _properties[name];
                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(name)) return;

                if (!_properties.ContainsKey(name)) _properties.Add(name, null);
                _properties[name] = value;
            }
        }

        [XmlElement("Property")]
        public DynamicSerializableProperty[] Properties
        {
            get
            {
                return _properties.Select(p =>
                {
                    var property = new DynamicSerializableProperty
                    {
                        Name = p.Key
                    };
                    if (p.Value is IEnumerable && !(p.Value is string))
                    {
                        property.ArrayValue = p.Value.Enumerate().ToArray();
                    }
                    else
                    {
                        property.Value = p.Value;
                    }

                    return property;
                }).ToArray();
            }
            set
            {
                _properties.Clear();
                foreach (var property in value.Enumerate())
                {
                    if (string.IsNullOrEmpty(property.Name)) continue;
                    _properties[property.Name] = property.ArrayValue ?? property.Value;
                }
            }
        }

        #endregion

        #region Override members

        public override string ToString()
        {
            return $"{{ {string.Join(", ", Properties.Select(p => p.ToString()))} }}";
        }

        #endregion
    }
}