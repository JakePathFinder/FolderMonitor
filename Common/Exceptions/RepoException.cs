using Common.Constants;
using Microsoft.Extensions.Logging;
using static System.String;

namespace Common.Exceptions
{
    public class RepoException : Exception
    {
        public RepoException()
        {
        }

        public RepoException(string message)
            : base(message)
        {
        }

        public RepoException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public RepoException(Exception inner, string opName)
            : base(Format(Const.DbExceptionMessageTemplate, opName, inner.Message), inner)
        {
        }
    }
}
