using EnchCoreApi.TrProtocol.Attributes;
using Terraria;

namespace EnchCoreApi.TrProtocol.NetPackets;

public partial class WorldData : NetPacket {
    public sealed override MessageID Type => MessageID.WorldData;
    [InitDefaultValue] public int Time;
    [InitDefaultValue] public BitsByte DayAndMoonInfo;
    [InitDefaultValue] public byte MoonPhase;
    public short MaxTileX;
    public short MaxTileY;
    public short SpawnX;
    public short SpawnY;
    [InitDefaultValue] public short WorldSurface;
    [InitDefaultValue] public short RockLayer;
    [InitDefaultValue] public int WorldID;

    public string WorldName;

    [InitDefaultValue] public byte GameMode;

    [ArraySize(16)] public byte[] WorldUniqueIDData;

    [IgnoreSerialize]
    public Guid WorldUniqueID { 
        get => new Guid(WorldUniqueIDData);
        set => WorldUniqueIDData = value.ToByteArray();
    }
    [InitDefaultValue] public ulong WorldGeneratorVersion;
    [InitDefaultValue] public byte MoonType;
    [InitDefaultValue] public byte TreeBackground;
    [InitDefaultValue] public byte CorruptionBackground;
    [InitDefaultValue] public byte JungleBackground;
    [InitDefaultValue] public byte SnowBackground;
    [InitDefaultValue] public byte HallowBackground;
    [InitDefaultValue] public byte CrimsonBackground;
    [InitDefaultValue] public byte DesertBackground;
    [InitDefaultValue] public byte OceanBackground;
    [InitDefaultValue] public byte UnknownBackground1;
    [InitDefaultValue] public byte UnknownBackground2;
    [InitDefaultValue] public byte UnknownBackground3;
    [InitDefaultValue] public byte UnknownBackground4;
    [InitDefaultValue] public byte UnknownBackground5;
    [InitDefaultValue] public byte IceBackStyle;
    [InitDefaultValue] public byte JungleBackStyle;
    [InitDefaultValue] public byte HellBackStyle;
    [InitDefaultValue] public float WindSpeedSet;
    [InitDefaultValue] public byte CloudNumber;
    [InitDefaultValue] public int Tree1;
    [InitDefaultValue] public int Tree2;
    [InitDefaultValue] public int Tree3;
    [InitDefaultValue] public byte TreeStyle1;
    [InitDefaultValue] public byte TreeStyle2;
    [InitDefaultValue] public byte TreeStyle3;
    [InitDefaultValue] public byte TreeStyle4;
    [InitDefaultValue] public int CaveBack1;
    [InitDefaultValue] public int CaveBack2;
    [InitDefaultValue] public int CaveBack3;
    [InitDefaultValue] public byte CaveBackStyle1;
    [InitDefaultValue] public byte CaveBackStyle2;
    [InitDefaultValue] public byte CaveBackStyle3;
    [InitDefaultValue] public byte CaveBackStyle4;
    [InitDefaultValue] public byte ForestTreeTopStyle1;
    [InitDefaultValue] public byte ForestTreeTopStyle2;
    [InitDefaultValue] public byte ForestTreeTopStyle3;
    [InitDefaultValue] public byte ForestTreeTopStyle4;
    [InitDefaultValue] public byte CorruptionTreeTopStyle4;
    [InitDefaultValue] public byte JungleTreeTopStyle4;
    [InitDefaultValue] public byte SnowTreeTopStyle4;
    [InitDefaultValue] public byte HallowTreeTopStyle4;
    [InitDefaultValue] public byte CrimsonTreeTopStyle4;
    [InitDefaultValue] public byte DesertTreeTopStyle4;
    [InitDefaultValue] public byte OceanTreeTopStyle4;
    [InitDefaultValue] public byte GlowingMushroomTreeTopStyle4;
    [InitDefaultValue] public byte UnderworldTreeTopStyle4;
    [InitDefaultValue] public float Rain;
    [InitDefaultValue] public BitsByte EventInfo1;
    [InitDefaultValue] public BitsByte EventInfo2;
    [InitDefaultValue] public BitsByte EventInfo3;
    [InitDefaultValue] public BitsByte EventInfo4;
    [InitDefaultValue] public BitsByte EventInfo5;
    [InitDefaultValue] public BitsByte EventInfo6;
    [InitDefaultValue] public BitsByte EventInfo7;
    [InitDefaultValue] public BitsByte EventInfo8;
    [InitDefaultValue] public BitsByte EventInfo9;
    [InitDefaultValue] public BitsByte EventInfo10;
    [InitDefaultValue] public byte SundialCooldown;
    [InitDefaultValue] public byte MoondialCooldown;
    [InitDefaultValue] public short CopperOreTier;
    [InitDefaultValue] public short IronOreTier;
    [InitDefaultValue] public short SilverOreTier;
    [InitDefaultValue] public short GoldOreTier;
    [InitDefaultValue] public short CobaltOreTier;
    [InitDefaultValue] public short MythrilOreTier;
    [InitDefaultValue] public short AdmantiteOreTier;
    [InitDefaultValue] public sbyte InvasionType;
    [InitDefaultValue] public ulong LobbyID;
    [InitDefaultValue] public float SandstormSeverity;
}
