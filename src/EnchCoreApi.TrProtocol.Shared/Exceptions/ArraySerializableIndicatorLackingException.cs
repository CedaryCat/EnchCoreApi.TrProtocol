﻿namespace EnchCoreApi.TrProtocol.Exceptions {
    public class ArraySerializableIndicatorLackingException : Exception {

        public ArraySerializableIndicatorLackingException(string memberName) : base($"Array type member '{memberName}' have too few packedValue members to indicate the serializability of their elements") {

        }
    }
}
