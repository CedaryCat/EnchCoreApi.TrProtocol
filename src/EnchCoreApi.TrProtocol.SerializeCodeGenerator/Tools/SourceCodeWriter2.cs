using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnchCoreApi.TrProtocol.SerializeCodeGenerator.Tools
{
    public class SourceCodeWriter2
    {
        private int _indent;
        private bool _nextLine;
        private StringBuilder _writer;
        public SourceCodeWriter2(int capacity) {
            _writer = new StringBuilder(capacity);
        }
        public UnitNode CompileUnit = new UnitNode();


        public BlockNode BlockWrite(Action<BlockNode> write) {
            var block = new BlockNode(CompileUnit);
            write(block);
            CompileUnit.Sources.Add(block);
            return block;
        }
        public AppendTextNode Write(string code) {
            var res = new AppendTextNode(code, CompileUnit);
            CompileUnit.Sources.Add(res);
            return res;
        }
        public NewLineTextNode WriteLine(string code) {
            var res = new NewLineTextNode(code, CompileUnit);
            CompileUnit.Sources.Add(res);
            return res;
        }
        public NewLineTextNode WriteLine() {
            var res = new NewLineTextNode("", CompileUnit);
            CompileUnit.Sources.Add(res);
            return res;
        }

        public override string ToString() {
            _writer.Clear();
            WriteNode(CompileUnit);
            return _writer.ToString();
        }
        
        private void WriteNode(SourceNode node) {
            if (node is AppendTextNode append) {
                if (_nextLine) {
                    _writer.Append(new string(' ', _indent * 4) + append.ToString());
                }
                else {
                    _writer.Append(append.ToString() + "\r\n");
                }
                _nextLine = false;
            }
            else if (node is NewLineTextNode newline) {
                if (_nextLine) {
                    _writer.Append(new string(' ', _indent * 4) + newline.ToString() + "\r\n");
                }
                else {
                    _writer.Append(newline.ToString() + "\r\n");
                }
                _nextLine = true;
            }
            else if(node is SourceGroup group) {
                if (group is BlockNode b) {
                    WriteNode(new NewLineTextNode("{", b.Parent));
                    _indent++;
                    foreach (var sub in group.Sources) {
                        WriteNode(sub);
                    }
                    _indent--;
                    WriteNode(new NewLineTextNode("}", b.Parent));
                }
                else {
                    foreach (var sub in group.Sources) {
                        WriteNode(sub);
                    }
                }
            }
        }
    }
    public static class SourceExt
    {
        public static void InsertBefore(this IChildNode target, SourceNode node) {
            var index = target.Parent.Sources.IndexOf((SourceNode)target);
            target.Parent.Sources.Insert(index, node);
        }
        public static void InsertAfter(this IChildNode target, SourceNode node) {
            var index = target.Parent.Sources.IndexOf((SourceNode)target);
            target.Parent.Sources.Insert(index + 1, node);
        }
        public static void AppendTextBefore(this IChildNode target, string text) {
            target.InsertBefore(new AppendTextNode(text, target.Parent));
        }
        public static void NewLineBefore(this IChildNode target, string text) {
            target.InsertBefore(new NewLineTextNode(text, target.Parent));
        }
        public static void AppendTextAfter(this IChildNode target, string text) {
            target.InsertAfter(new AppendTextNode(text, target.Parent));
        }
        public static void NewLineAfter(this IChildNode target, string text) {
            target.InsertAfter(new NewLineTextNode(text, target.Parent));
        }

        public static (BlockNode, BlockNode) BlockWrite(this (BlockNode, BlockNode) targets, Action<BlockNode, BlockNode> write) {
            var group = (new BlockNode(targets.Item1), new BlockNode(targets.Item2));
            targets.Item1.Sources.Add(group.Item1);
            targets.Item2.Sources.Add(group.Item2);
            write(group.Item1, group.Item2);
            return group;
        }
    }
    public interface IChildNode
    {
        public SourceGroup Parent { get; }
    }
    public abstract class SourceNode {
    }
    public class AppendTextNode : SourceNode, IChildNode
    {
        public string Code;
        public AppendTextNode(string code, SourceGroup parent) {
            Code = code;
            Parent = parent;
        }
        public SourceGroup Parent { get; private set; }
        public sealed override string ToString() => Code;
    }
    public class NewLineTextNode : SourceNode, IChildNode
    {
        public string Code;
        public NewLineTextNode(string code, SourceGroup parent) {
            Code = code;
            Parent = parent;
        }
        public SourceGroup Parent { get; private set; }
        public sealed override string ToString() => Code;
    }
    public abstract class SourceGroup : SourceNode
    {
        public readonly List<SourceNode> Sources = new List<SourceNode>();

        public BlockNode BlockWrite(Action<BlockNode> write) {
            var block = new BlockNode(this);
            write(block);
            Sources.Add(block);
            return block;
        }
        public AppendTextNode Write(string code) {
            var res = new AppendTextNode(code, this);
            Sources.Add(res);
            return res;
        }
        public NewLineTextNode WriteLine(string code) {
            var res = new NewLineTextNode(code, this);
            Sources.Add(res);
            return res;
        }
        public NewLineTextNode WriteLine() {
            var res = new NewLineTextNode("", this);
            Sources.Add(res);
            return res;
        }
    }
    public class BlockNode : SourceGroup, IChildNode
    {
        public BlockNode(SourceGroup parent) {
            Parent = parent;
        }
        public BlockNode(Action<BlockNode> write, SourceGroup parent) : this(parent) {
            write(this);
        }

        public SourceGroup Parent { get; private set; }

        public void WarpBlock(int index, params (string text, bool newLine)[] others) {
            int i = 0;
            var inner = new BlockNode(this);
            inner.Sources.AddRange(Sources);
            Sources.Clear();
            foreach (var (text, newline) in others) {
                if (i == index) {
                    Sources.Add(inner);
                    i++;
                }
                if (newline) {
                    Sources.Add(new NewLineTextNode(text, this));
                }
                else {
                    Sources.Add(new AppendTextNode(text, this));
                }
                i++;
            }
        }
        public void WarpBlock(params (string text, bool newLine)[] others) {
            var inner = new BlockNode(this);
            inner.Sources.AddRange(Sources);
            Sources.Clear();
            foreach (var (text, newline) in others) {
                if (newline) {
                    Sources.Add(new NewLineTextNode(text, this));
                }
                else {
                    Sources.Add(new AppendTextNode(text, this));
                }
            }
            Sources.Add(inner);
        }
    }
    public class UnitNode : SourceGroup
    {
    }
}
