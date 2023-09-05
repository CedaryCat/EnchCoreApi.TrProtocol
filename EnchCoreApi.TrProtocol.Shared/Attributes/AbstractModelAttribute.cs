namespace EnchCoreApi.TrProtocol.Attributes {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AbstractModelAttribute : Attribute {
        public readonly Type EnumIdentity;
        public readonly string IdentityName;
        public AbstractModelAttribute(Type enumIdentity, string identityPropName) {
            EnumIdentity = enumIdentity;
            IdentityName = identityPropName;
        }
    }
}
