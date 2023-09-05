namespace EnchCoreApi.TrProtocol.Attributes {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    public class TypeMigrationTargetAttribute : Attribute {
    }
}
