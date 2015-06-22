using System;

namespace Closedown
{
    public class ClosedownException : Exception
    {
        public ClosedownException()
            : base()
        {
        }
        public ClosedownException(long code, String Message)
            : base(Message)
        {
            this._code = code;
        }

        private long _code;

        public long code
        {
            get { return _code; }
        }
        public ClosedownException(Linkhub.LinkhubException le)
            : base(le.Message, le)
        {
            this._code = le.code;
        }

    }
}
