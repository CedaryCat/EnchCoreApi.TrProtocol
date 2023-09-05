using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnchCoreApi.TrProtocol.Patcher {
    public abstract class Logger {
        public abstract void WriteLine();
        protected abstract void WriteLine(string message);
        protected abstract void WriteLineError(string message);
        protected abstract void WriteLineWarning(string message);
        protected abstract void WriteLineSuccess(string message);

        public LogData WriteLineError(string message, LogData parent = default) {
            var data = new LogData(message, LogMode.Error, parent);
            var leadingTrival = new string(' ', data.LeadingTrival * 4);
            WriteLineError(leadingTrival + message);
            return data;
        }
        public LogData WriteLineWarning(string message, LogData parent = default) {
            var data = new LogData(message, LogMode.Warning, parent);
            var leadingTrival = new string(' ', data.LeadingTrival * 4);
            WriteLineWarning(leadingTrival + message);
            return data;
        }
        public LogData WriteLineSuccess(string message, LogData parent = default) {
            var data = new LogData(message, LogMode.Success, parent);
            var leadingTrival = new string(' ', data.LeadingTrival * 4);
            WriteLineSuccess(leadingTrival + message);
            return data;
        }
        public LogData WriteLine(string message, LogData parent = default) {
            var data = new LogData(message, LogMode.Info, parent);
            var leadingTrival = new string(' ', data.LeadingTrival * 4);
            WriteLine(leadingTrival + message);
            return data;
        }
    }
    public enum LogMode {
        Info,
        Warning, 
        Error,
        Success,
    }
    public readonly struct LogData {
        public LogData(string text, LogMode mode, LogData parent = default) {
            LogText = text;
            Mode = mode;
            IsDefault = false;
            if (!parent.IsDefault) {
                LeadingTrival = parent.LeadingTrival + 1;
            }
        }
        public readonly bool IsDefault = true;
        public readonly string LogText = string.Empty;
        public readonly LogMode Mode;
        public readonly int LeadingTrival = 0;
    }
}
