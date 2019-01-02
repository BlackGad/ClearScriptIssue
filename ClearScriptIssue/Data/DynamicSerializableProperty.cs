using System.Linq;
using System.Xml.Serialization;

namespace ClearScriptIssue.Data
{
    public class DynamicSerializableProperty
    {
        #region Properties

        [XmlArray("Values")]
        [XmlArrayItem("Value")]
        public object[] ArrayValue { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement]
        public object Value { get; set; }

        #endregion

        #region Override members

        public override string ToString()
        {
            if (ArrayValue != null)
            {
                return $"{Name}: [ {string.Join(", ", ArrayValue.Select(v => v?.ToString() ?? "<null>"))} ]";
            }

            return $"{Name}: {Value}";
        }

        #endregion
    }
}