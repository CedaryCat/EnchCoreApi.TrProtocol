
// Terraria.Localization.NetworkText
using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Terraria.Localization;
[TypeConvertion("Terraria.Localization.NetworkText")]
public partial class NetworkTextModel : ISerializableData {
    public enum Mode : byte {
        Literal,
        Formattable,
        LocalizationKey
    }

    public static readonly NetworkTextModel Empty = FromLiteral("");

    [MemberConvertion(
        ConvertionOption.Custom, 
        CustomConvertionFromMethod = "Covert_Array_NetworkTextModel_To_NetworkText",
        CustomConvertionToMethod = "Covert_Array_NetworkText_To_NetworkTextModel")]
    public NetworkTextModel[] _substitutions = Array.Empty<NetworkTextModel>();

    [MemberConvertion]
    public string _text;
    [MemberConvertion]
    public Mode _mode;

    public NetworkTextModel(string text, Mode mode) {
        _text = text;
        _mode = mode;
    }
    public NetworkTextModel()
    {
        _text = string.Empty;
    }

    public static NetworkTextModel[] ConvertSubstitutionsToNetworkText(object[] substitutions) {
        NetworkTextModel[] array = new NetworkTextModel[substitutions.Length];
        for (int i = 0; i < substitutions.Length; i++) {
            NetworkTextModel? networkText = substitutions[i] as NetworkTextModel;
            networkText ??= FromLiteral(substitutions[i].ToString() ?? throw new NullReferenceException());
            array[i] = networkText;
        }
        return array;
    }

    public static NetworkTextModel FromFormattable(string text, params object[] substitutions) {
        return new NetworkTextModel(text, Mode.Formattable) {
            _substitutions = ConvertSubstitutionsToNetworkText(substitutions)
        };
    }

    public static NetworkTextModel FromLiteral(string text) {
        return new NetworkTextModel(text, Mode.Literal);
    }

    public static NetworkTextModel FromKey(string key, params object[] substitutions) {
        return new NetworkTextModel(key, Mode.LocalizationKey) {
            _substitutions = ConvertSubstitutionsToNetworkText(substitutions)
        };
    }

    public int GetMaxSerializedSize() {
        int num = 0;
        num++;
        num += 4 + Encoding.UTF8.GetByteCount(_text);
        if (_mode != 0) {
            num++;
            for (int i = 0; i < _substitutions.Length; i++) {
                num += _substitutions[i].GetMaxSerializedSize();
            }
        }
        return num;
    }

    public void Serialize(BinaryWriter writer) {
        writer.Write((byte)_mode);
        writer.Write(_text);
        SerializeSubstitutionList(writer);
    }

    public void SerializeSubstitutionList(BinaryWriter writer) {
        if (_mode != 0) {
            writer.Write((byte)_substitutions.Length);
            for (int i = 0; i < (_substitutions.Length & 0xFF); i++) {
                _substitutions[i].Serialize(writer);
            }
        }
    }

    public static NetworkTextModel Deserialize(BinaryReader reader) {
        Mode mode = (Mode)reader.ReadByte();
        NetworkTextModel networkText = new NetworkTextModel(reader.ReadString(), mode);
        networkText.DeserializeSubstitutionList(reader);
        return networkText;
    }

    public static NetworkTextModel DeserializeLiteral(BinaryReader reader) {
        Mode mode = (Mode)reader.ReadByte();
        NetworkTextModel networkText = new NetworkTextModel(reader.ReadString(), mode);
        networkText.DeserializeSubstitutionList(reader);
        if (mode != 0)
            networkText.SetToEmptyLiteral();
        return networkText;
    }
    public void DeserializeSubstitutionList(BinaryReader reader) {
        if (_mode != 0) {
            _substitutions = new NetworkTextModel[reader.ReadByte()];
            for (int i = 0; i < _substitutions.Length; i++) {
                _substitutions[i] = Deserialize(reader);
            }
        }
    }

    public void SetToEmptyLiteral() {
        _mode = Mode.Literal;
        _text = string.Empty;
        _substitutions = Array.Empty<NetworkTextModel>();
    }

    // Token: 0x060016E5 RID: 5861 RVA: 0x0046C790 File Offset: 0x0046A990
    public override string ToString() {
        try {
            switch (_mode) {
                case Mode.Literal:
                    return _text;
                case Mode.Formattable: {
                        string text = _text;
                        object[] substitutions = _substitutions;
                        return string.Format(text, substitutions);
                    }
                case Mode.LocalizationKey: {
                        string text2 = _text;
                        return $"lang[{text2}][{string.Join<NetworkTextModel>(',', _substitutions)}]";
                    }
                default:
                    return _text;
            }
        }
        catch {
            SetToEmptyLiteral();
        }
        return _text;
    }

    // Token: 0x060016E6 RID: 5862 RVA: 0x0046C844 File Offset: 0x0046AA44
    public string ToDebugInfoString(string linePrefix = "") {
        string text = string.Format("{0}Mode: {1}\n{0}Text: {2}\n", linePrefix, _mode, _text);
        if (_mode == Mode.LocalizationKey) {
            text += string.Format("{0}Localized Text: {1}\n", linePrefix, $"lang[{_text}]");
        }
        if (_mode != Mode.Literal) {
            for (int i = 0; i < _substitutions.Length; i++) {
                text += string.Format("{0}Substitution {1}:\n", linePrefix, i);
                text += _substitutions[i].ToDebugInfoString(linePrefix + "\t");
            }
        }
        return text;
    }

    [MemberNotNull(nameof(_text))]
    private void DeserializeInstance(BinaryReader reader) {
        _mode = (Mode)reader.ReadByte();
        _text = reader.ReadString();
        DeserializeSubstitutionList(reader);
    }

    [MemberNotNull(nameof(_text))]
    public unsafe void ReadContent(ref void* ptr) {
        using var st = new UnmanagedMemoryStream((byte*)ptr, 1024 * 4, 1024 * 4, FileAccess.Read);
        using var br = new BinaryReader(st);
        DeserializeInstance(br);
        ptr = st.PositionPointer;
    }

    public unsafe void WriteContent(ref void* ptr) {
        using var st = new UnmanagedMemoryStream((byte*)ptr, 1024 * 4, 1024 * 4, FileAccess.Write);
        using var bw = new BinaryWriter(st);
        Serialize(bw);
        ptr = st.PositionPointer;
    }
}
