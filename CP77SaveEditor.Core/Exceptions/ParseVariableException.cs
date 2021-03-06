﻿using System;

namespace CP77SaveEditor.Core.Exceptions
{

    [Serializable]
    public class ParseVariableException : Exception
    {
        public ParseVariableException()
        {
        }

        public ParseVariableException(string message) : base(message)
        {
        }

        public ParseVariableException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}