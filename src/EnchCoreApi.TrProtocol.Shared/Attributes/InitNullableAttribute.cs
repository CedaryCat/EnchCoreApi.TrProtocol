namespace EnchCoreApi.TrProtocol.Attributes {

    /// <summary>
    /// Used on fields or properties that are allowed to remain null after the constructor is executed
    /// <para>The source generator will ignore the members decorated by this attribute when automatically generating the constructor</para>
    /// <para></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InitNullableAttribute : Attribute {
    }
}
