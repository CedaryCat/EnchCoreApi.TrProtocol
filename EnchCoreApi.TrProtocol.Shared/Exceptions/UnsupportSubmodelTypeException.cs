namespace EnchCoreApi.TrProtocol.Exceptions {
    public class UnsupportSubmodelTypeException : Exception {
        public UnsupportSubmodelTypeException(Type packetbase, Enum id, long value) : base($"id '{id}' ({value}) of base packet '{packetbase}' is not defined") {

        }
    }
}
