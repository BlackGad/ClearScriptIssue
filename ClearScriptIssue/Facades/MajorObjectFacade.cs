using ClearScriptIssue.Data;

// ReSharper disable InconsistentNaming

namespace ClearScriptIssue.Facades
{
    public class MajorObjectFacade : IOrigin
    {
        private readonly MajorObject _origin;

        #region Constructors

        public MajorObjectFacade()
        {
            _origin = new MajorObject();
        }

        #endregion

        #region Properties

        public string payload
        {
            get { return _origin.Payload; }
            set { _origin.Payload = value; }
        }

        #endregion

        #region IOrigin Members

        public object QueryOrigin()
        {
            return _origin;
        }

        #endregion
    }
}