using EnchCoreApi.TrProtocol.Attributes;
using EnchCoreApi.TrProtocol.Models.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class PlayerHealth : NetPacket, IPlayerSlot {
    public sealed override MessageID Type => MessageID.PlayerHealth;
    public byte PlayerSlot { get; set; }
    public short StatLife;
    public short StatLifeMax;
}
