using System.Xml.Serialization;

namespace ClearScriptIssue.Data
{
    public class MajorObject
    {
        #region Properties

        [XmlAttribute]
        public string Payload { get; set; }

        #endregion

        #region Override members

        public override string ToString()
        {
            return $"MajorObject: {Payload ?? "<null>"}";
        }

        #endregion
    }
}