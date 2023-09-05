using Microsoft.CodeAnalysis;

namespace EnchCoreApi.TrProtocol.SerializeCodeGenerator {

    public class DiagnosticException : Exception {
        public Diagnostic Diagnostic;
        public DiagnosticException(Diagnostic diagnostic) {
            this.Diagnostic = diagnostic;
        }
    }
}
