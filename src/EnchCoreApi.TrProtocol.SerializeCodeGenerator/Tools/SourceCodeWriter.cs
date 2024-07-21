using System.Text;

namespace EnchCoreApi.TrProtocol.SerializeCodeGenerator {
    public class SourceCodeWriter {
        private StringBuilder _writer;
        private int _indent;
        private string _singleSpan = "    ";
        private string _space = "";
        private bool _newLine = true;
        private void UpdateSpace() {
            _space = "";
            for (int i = 0; i < _indent; i++) {
                _space += _singleSpan;
            }
        }
        public SourceCodeWriter(int capacity) {
            _writer = new StringBuilder(capacity);
        }
        public SourceCodeWriter WriteLine() {
            if (_newLine) {
                _writer.AppendLine(_space);
            }
            _newLine = true;
            return this;
        }
        public SourceCodeWriter Write(string value) {
            if (_newLine) {
                _newLine = false;
                _writer.Append(_space);
            }
            _writer.Append(value);
            return this;
        }
        public SourceCodeWriter WriteLine(string value) {
            if (_newLine) {
                _newLine = false;
                _writer.Append(_space);
            }
            _writer.AppendLine(value);
            _newLine = true;
            return this;
        }
        public override string ToString() {
            return _writer.ToString();
        }

        public SourceCodeWriter BlockWrite(Action<SourceCodeWriter> write, bool autoBrace = true) {
            if (autoBrace) {
                if (_newLine) {
                    _writer.Append(_space);
                }
                _writer.AppendLine("{");
                _newLine = true;
            }
            _indent += 1;
            UpdateSpace();
            write(this);
            _indent -= 1;
            UpdateSpace();
            if (autoBrace) {
                if (_newLine) {
                    _writer.Append(_space);
                }
                _writer.AppendLine("}");
                _newLine = true;
            }
            return this;
        }
        public SourceCodeWriter BlockWrite<TParams>(Action<SourceCodeWriter, TParams> write, TParams _params, bool autoBrace = true) {
            if (autoBrace) {
                if (_newLine) {
                    _writer.Append(_space);
                }
                _writer.AppendLine("{");
                _newLine = true;
            }
            _indent += 1;
            UpdateSpace();
            write(this, _params);
            _indent -= 1;
            UpdateSpace();
            if (autoBrace) {
                if (_newLine) {
                    _writer.Append(_space);
                }
                _writer.AppendLine("}");
                _newLine = true;
            }
            return this;
        }
        public DeferredWriteAction DeferredBlockWrite<TParams>(DeferredBlockWriteDele<TParams> write, TParams _params, bool autoBrace = true) {
            return new DeferredBlockWriteAction<TParams>(this, write, _params, autoBrace);
        }
        public DeferredWriteAction DeferredWrite<TParams>(DeferredBlockWriteDele<TParams> write, TParams _params) {
            return new DeferredNormalWriteAction<TParams>(this, write, _params);
        }
        public DeferredWriteAction DeferredWrite(params string[] values) {
            return new DeferredNormalWriteAction(this, values);
        }
        sealed class DeferredNormalWriteAction : DeferredWriteAction {
            SourceCodeWriter writer;
            string[] values;
            public DeferredNormalWriteAction(SourceCodeWriter writer, params string[] values) {
                this.writer = writer;
                this.values = values;
            }
            public sealed override SourceCodeWriter Run(Action<SourceCodeWriter>? innermost = null) {
                foreach (var value in values) {
                    writer.Write(value);
                }
                innermost?.Invoke(writer);
                return writer;
            }

            public override SourceCodeWriter WriteToAnother(SourceCodeWriter another, Action<SourceCodeWriter>? innermost = null)
            {
                foreach (var value in values)
                {
                    another.Write(value);
                }
                innermost?.Invoke(another);
                return another;
            }
        }
        sealed class DeferredNormalWriteAction<TParams> : DeferredWriteAction {
            SourceCodeWriter writer;
            DeferredBlockWriteDele<TParams> write;
            TParams p;
            public DeferredNormalWriteAction(SourceCodeWriter writer, DeferredBlockWriteDele<TParams> write, TParams _params) {
                this.writer = writer;
                this.write = write;
                this.p = _params;
            }
            public sealed override SourceCodeWriter Run(Action<SourceCodeWriter>? innermost = null) {
                write(writer, p, innermost);
                return writer;
            }

            public override SourceCodeWriter WriteToAnother(SourceCodeWriter another, Action<SourceCodeWriter>? innermost = null)
            {
                write(another, p, innermost);
                return another;
            }
        }
        sealed class DeferredBlockWriteAction<TParams> : DeferredWriteAction {
            SourceCodeWriter writer;
            DeferredBlockWriteDele<TParams> write;
            TParams p;
            bool autoBrace;
            public DeferredBlockWriteAction(SourceCodeWriter writer, DeferredBlockWriteDele<TParams> write, TParams _params, bool autoBrace = true) {
                this.writer = writer;
                this.write = write;
                this.p = _params;
                this.autoBrace = autoBrace;
            }
            public sealed override SourceCodeWriter Run(Action<SourceCodeWriter>? innermost = null) {
                return writer.BlockWrite((source, p) => {
                    write(writer, p, innermost);
                }, p, autoBrace);
            }

            public override SourceCodeWriter WriteToAnother(SourceCodeWriter another, Action<SourceCodeWriter>? innermost = null)
            {
                return another.BlockWrite((source, p) => {
                    write(another, p, innermost);
                }, p, autoBrace);
            }
        }
        public delegate void DeferredBlockWriteDele<TParams>(SourceCodeWriter source, TParams param, Action<SourceCodeWriter>? innermost);
    }
    public abstract class DeferredWriteAction
    {
        public abstract SourceCodeWriter Run(Action<SourceCodeWriter>? innermost = null);
        public abstract SourceCodeWriter WriteToAnother(SourceCodeWriter another, Action<SourceCodeWriter>? innermost = null);
    }
}
