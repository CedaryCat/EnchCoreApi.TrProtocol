using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Interfaces;
using EnchCoreApi.TrProtocol.Models;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace EnchCoreApi.TrProtocol.NetPackets.Modules {
    public partial class NetTextModule : NetModulesPacket, ISideDependent {
        public sealed override NetModuleType ModuleType => NetModuleType.NetTextModule;
        [C2SOnly]
        public TextC2S? TextC2S;
        [S2COnly]
        public TextS2C? TextS2C;
        public override string ToString() {
            if (TextC2S is not null) {
                return $"[S2C] {TextS2C}";
            }
            else if (TextC2S is not null) {
                return $"[C2S] {TextC2S}";
            }
            else {
                return "";
            }
        }
    }
    public class TextC2S {
        public string? Command;
        public string? Text;
    }
    public class TextS2C {
        public byte PlayerSlot;
        public NetworkTextModel? Text;
        public Color Color;
    }
}
