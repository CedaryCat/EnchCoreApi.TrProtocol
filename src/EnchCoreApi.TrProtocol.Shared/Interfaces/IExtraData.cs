namespace EnchCoreApi.TrProtocol.Interfaces {
    /// <summary>
    /// This interface is used only for those packets whose data cannot be deserialized directly by the protocol library,
    /// <para>usually because the deserialization of such packets is state-dependent and the protocol library does not store any information</para>
    /// <para>When using this interface, the Generator adds an 'ExtraData' property of type byte[] to the end of the packet that inherits from this interface to hold the remaining data that the protocol library cannot handle.</para>
    /// <para>As a result, this interface is only allowed to be inherited by packets of sealed type</para>
    /// </summary>
    public interface IExtraData {
        public byte[] ExtraData { get; set; }
    }
}
