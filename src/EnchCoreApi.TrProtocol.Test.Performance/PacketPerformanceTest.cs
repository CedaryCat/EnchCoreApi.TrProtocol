using BenchmarkDotNet.Attributes;
using EnchCoreApi.TrProtocol.NetPackets;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EnchCoreApi.TrProtocol.Test.Performance
{

    public class PacketPerformanceTest
    {
        readonly static WorldData worldData = new WorldData(2, 4, 6, 8, "world", new Guid().ToByteArray());
        readonly static byte[] buffer = new byte[1024];
        readonly static MemoryStream st = new MemoryStream(buffer);
        readonly static BinaryReader br = new BinaryReader(st);
        readonly static BinaryWriter bw = new BinaryWriter(st);
        static unsafe PacketPerformanceTest()
        {
            fixed (void* ptr = buffer)
            {
                var p = ptr;
                worldData.WriteContent(ref p);   
            }
        }
        [MemoryDiagnoser, RankColumn]
        public class Read
        {
            [Benchmark]
            public unsafe void Test_Unsafe()
            {
                fixed (void* ptr = buffer)
                {
                    var p = Unsafe.Add<byte>(ptr, 1);
                    worldData.ReadContent(ref p);
                }
            }
            [Benchmark]
            public void Test_BinaryReader()
            {
                var br = new BinaryReader(new MemoryStream(buffer));
                br.BaseStream.Position = 1;
                worldData.Time = br.ReadInt32();
                worldData.DayAndMoonInfo = br.ReadByte();
                worldData.MoonPhase = br.ReadByte();
                worldData.MaxTileX = br.ReadInt16();
                worldData.MaxTileY = br.ReadInt16();
                worldData.SpawnX = br.ReadInt16();
                worldData.SpawnY = br.ReadInt16();
                worldData.WorldSurface = br.ReadInt16();
                worldData.RockLayer = br.ReadInt16();
                worldData.WorldID = br.ReadInt32();
                worldData.WorldName = br.ReadString();
                worldData.GameMode = br.ReadByte();
                worldData.WorldUniqueIDData = br.ReadBytes(16);
                worldData.WorldGeneratorVersion = br.ReadUInt64();
                worldData.MoonType = br.ReadByte();
                worldData.TreeBackground = br.ReadByte();
                worldData.CorruptionBackground = br.ReadByte();
                worldData.JungleBackground = br.ReadByte();
                worldData.SnowBackground = br.ReadByte();
                worldData.HallowBackground = br.ReadByte();
                worldData.CrimsonBackground = br.ReadByte();
                worldData.DesertBackground = br.ReadByte();
                worldData.OceanBackground = br.ReadByte();
                worldData.UnknownBackground1 = br.ReadByte();
                worldData.UnknownBackground2 = br.ReadByte();
                worldData.UnknownBackground3 = br.ReadByte();
                worldData.UnknownBackground4 = br.ReadByte();
                worldData.UnknownBackground5 = br.ReadByte();
                worldData.IceBackStyle = br.ReadByte();
                worldData.JungleBackStyle = br.ReadByte();
                worldData.HellBackStyle = br.ReadByte();
                worldData.WindSpeedSet = br.ReadSingle();
                worldData.CloudNumber = br.ReadByte();
                worldData.Tree1 = br.ReadInt32();
                worldData.Tree2 = br.ReadInt32();
                worldData.Tree3 = br.ReadInt32();
                worldData.TreeStyle1 = br.ReadByte();
                worldData.TreeStyle2 = br.ReadByte();
                worldData.TreeStyle3 = br.ReadByte();
                worldData.TreeStyle4 = br.ReadByte();
                worldData.CaveBack1 = br.ReadInt32();
                worldData.CaveBack2 = br.ReadInt32();
                worldData.CaveBack3 = br.ReadInt32();
                worldData.CaveBackStyle1 = br.ReadByte();
                worldData.CaveBackStyle2 = br.ReadByte();
                worldData.CaveBackStyle3 = br.ReadByte();
                worldData.CaveBackStyle4 = br.ReadByte();
                worldData.ForestTreeTopStyle1 = br.ReadByte();
                worldData.ForestTreeTopStyle2 = br.ReadByte();
                worldData.ForestTreeTopStyle3 = br.ReadByte();
                worldData.ForestTreeTopStyle4 = br.ReadByte();
                worldData.CorruptionTreeTopStyle4 = br.ReadByte();
                worldData.JungleTreeTopStyle4 = br.ReadByte();
                worldData.SnowTreeTopStyle4 = br.ReadByte();
                worldData.HallowTreeTopStyle4 = br.ReadByte();
                worldData.CrimsonTreeTopStyle4 = br.ReadByte();
                worldData.DesertTreeTopStyle4 = br.ReadByte();
                worldData.OceanTreeTopStyle4 = br.ReadByte();
                worldData.GlowingMushroomTreeTopStyle4 = br.ReadByte();
                worldData.UnderworldTreeTopStyle4 = br.ReadByte();
                worldData.Rain = br.ReadSingle();
                worldData.EventInfo1 = br.ReadByte();
                worldData.EventInfo2 = br.ReadByte();
                worldData.EventInfo3 = br.ReadByte();
                worldData.EventInfo4 = br.ReadByte();
                worldData.EventInfo5 = br.ReadByte();
                worldData.EventInfo6 = br.ReadByte();
                worldData.EventInfo7 = br.ReadByte();
                worldData.EventInfo8 = br.ReadByte();
                worldData.EventInfo9 = br.ReadByte();
                worldData.EventInfo10 = br.ReadByte();
                worldData.SundialCooldown = br.ReadByte();
                worldData.MoondialCooldown = br.ReadByte();
                worldData.CopperOreTier = br.ReadInt16();
                worldData.IronOreTier = br.ReadInt16();
                worldData.SilverOreTier = br.ReadInt16();
                worldData.GoldOreTier = br.ReadInt16();
                worldData.CobaltOreTier = br.ReadInt16();
                worldData.MythrilOreTier = br.ReadInt16();
                worldData.AdmantiteOreTier = br.ReadInt16();
                worldData.InvasionType = br.ReadSByte();
                worldData.LobbyID = br.ReadUInt64();
                worldData.SandstormSeverity = br.ReadSingle();
            }
            [Benchmark]
            public void Test_ReuseBinaryReader()
            {
                br.BaseStream.Position = 1;
                worldData.Time = br.ReadInt32();
                worldData.DayAndMoonInfo = br.ReadByte();
                worldData.MoonPhase = br.ReadByte();
                worldData.MaxTileX = br.ReadInt16();
                worldData.MaxTileY = br.ReadInt16();
                worldData.SpawnX = br.ReadInt16();
                worldData.SpawnY = br.ReadInt16();
                worldData.WorldSurface = br.ReadInt16();
                worldData.RockLayer = br.ReadInt16();
                worldData.WorldID = br.ReadInt32();
                worldData.WorldName = br.ReadString();
                worldData.GameMode = br.ReadByte();
                worldData.WorldUniqueIDData = br.ReadBytes(16);
                worldData.WorldGeneratorVersion = br.ReadUInt64();
                worldData.MoonType = br.ReadByte();
                worldData.TreeBackground = br.ReadByte();
                worldData.CorruptionBackground = br.ReadByte();
                worldData.JungleBackground = br.ReadByte();
                worldData.SnowBackground = br.ReadByte();
                worldData.HallowBackground = br.ReadByte();
                worldData.CrimsonBackground = br.ReadByte();
                worldData.DesertBackground = br.ReadByte();
                worldData.OceanBackground = br.ReadByte();
                worldData.UnknownBackground1 = br.ReadByte();
                worldData.UnknownBackground2 = br.ReadByte();
                worldData.UnknownBackground3 = br.ReadByte();
                worldData.UnknownBackground4 = br.ReadByte();
                worldData.UnknownBackground5 = br.ReadByte();
                worldData.IceBackStyle = br.ReadByte();
                worldData.JungleBackStyle = br.ReadByte();
                worldData.HellBackStyle = br.ReadByte();
                worldData.WindSpeedSet = br.ReadSingle();
                worldData.CloudNumber = br.ReadByte();
                worldData.Tree1 = br.ReadInt32();
                worldData.Tree2 = br.ReadInt32();
                worldData.Tree3 = br.ReadInt32();
                worldData.TreeStyle1 = br.ReadByte();
                worldData.TreeStyle2 = br.ReadByte();
                worldData.TreeStyle3 = br.ReadByte();
                worldData.TreeStyle4 = br.ReadByte();
                worldData.CaveBack1 = br.ReadInt32();
                worldData.CaveBack2 = br.ReadInt32();
                worldData.CaveBack3 = br.ReadInt32();
                worldData.CaveBackStyle1 = br.ReadByte();
                worldData.CaveBackStyle2 = br.ReadByte();
                worldData.CaveBackStyle3 = br.ReadByte();
                worldData.CaveBackStyle4 = br.ReadByte();
                worldData.ForestTreeTopStyle1 = br.ReadByte();
                worldData.ForestTreeTopStyle2 = br.ReadByte();
                worldData.ForestTreeTopStyle3 = br.ReadByte();
                worldData.ForestTreeTopStyle4 = br.ReadByte();
                worldData.CorruptionTreeTopStyle4 = br.ReadByte();
                worldData.JungleTreeTopStyle4 = br.ReadByte();
                worldData.SnowTreeTopStyle4 = br.ReadByte();
                worldData.HallowTreeTopStyle4 = br.ReadByte();
                worldData.CrimsonTreeTopStyle4 = br.ReadByte();
                worldData.DesertTreeTopStyle4 = br.ReadByte();
                worldData.OceanTreeTopStyle4 = br.ReadByte();
                worldData.GlowingMushroomTreeTopStyle4 = br.ReadByte();
                worldData.UnderworldTreeTopStyle4 = br.ReadByte();
                worldData.Rain = br.ReadSingle();
                worldData.EventInfo1 = br.ReadByte();
                worldData.EventInfo2 = br.ReadByte();
                worldData.EventInfo3 = br.ReadByte();
                worldData.EventInfo4 = br.ReadByte();
                worldData.EventInfo5 = br.ReadByte();
                worldData.EventInfo6 = br.ReadByte();
                worldData.EventInfo7 = br.ReadByte();
                worldData.EventInfo8 = br.ReadByte();
                worldData.EventInfo9 = br.ReadByte();
                worldData.EventInfo10 = br.ReadByte();
                worldData.SundialCooldown = br.ReadByte();
                worldData.MoondialCooldown = br.ReadByte();
                worldData.CopperOreTier = br.ReadInt16();
                worldData.IronOreTier = br.ReadInt16();
                worldData.SilverOreTier = br.ReadInt16();
                worldData.GoldOreTier = br.ReadInt16();
                worldData.CobaltOreTier = br.ReadInt16();
                worldData.MythrilOreTier = br.ReadInt16();
                worldData.AdmantiteOreTier = br.ReadInt16();
                worldData.InvasionType = br.ReadSByte();
                worldData.LobbyID = br.ReadUInt64();
                worldData.SandstormSeverity = br.ReadSingle();
            }
        }
        [MemoryDiagnoser, RankColumn]
        public class Write
        {
            [Benchmark]
            public unsafe void Test_Unsafe()
            {
                fixed (void* ptr = buffer)
                {
                    var p = Unsafe.Add<byte>(ptr, 1);
                    worldData.ReadContent(ref p);
                }
            }
            [Benchmark]
            public void Test_BinaryWriter()
            {
                var bw = new BinaryWriter(new MemoryStream(buffer));
                bw.BaseStream.Position = 1;
                bw.Write(worldData.Time);
                bw.Write(worldData.DayAndMoonInfo);
                bw.Write(worldData.MoonPhase);
                bw.Write(worldData.MaxTileX);
                bw.Write(worldData.MaxTileY);
                bw.Write(worldData.SpawnX);
                bw.Write(worldData.SpawnY);
                bw.Write(worldData.WorldSurface);
                bw.Write(worldData.RockLayer);
                bw.Write(worldData.WorldID);
                bw.Write(worldData.WorldName);
                bw.Write(worldData.GameMode);
                bw.Write(worldData.WorldUniqueIDData);
                bw.Write(worldData.WorldGeneratorVersion);
                bw.Write(worldData.MoonType);
                bw.Write(worldData.TreeBackground);
                bw.Write(worldData.CorruptionBackground);
                bw.Write(worldData.JungleBackground);
                bw.Write(worldData.SnowBackground);
                bw.Write(worldData.HallowBackground);
                bw.Write(worldData.CrimsonBackground);
                bw.Write(worldData.DesertBackground);
                bw.Write(worldData.OceanBackground);
                bw.Write(worldData.UnknownBackground1);
                bw.Write(worldData.UnknownBackground2);
                bw.Write(worldData.UnknownBackground3);
                bw.Write(worldData.UnknownBackground4);
                bw.Write(worldData.UnknownBackground5);
                bw.Write(worldData.IceBackStyle);
                bw.Write(worldData.JungleBackStyle);
                bw.Write(worldData.HellBackStyle);
                bw.Write(worldData.WindSpeedSet);
                bw.Write(worldData.CloudNumber);
                bw.Write(worldData.Tree1);
                bw.Write(worldData.Tree2);
                bw.Write(worldData.Tree3);
                bw.Write(worldData.TreeStyle1);
                bw.Write(worldData.TreeStyle2);
                bw.Write(worldData.TreeStyle3);
                bw.Write(worldData.TreeStyle4);
                bw.Write(worldData.CaveBack1);
                bw.Write(worldData.CaveBack2);
                bw.Write(worldData.CaveBack3);
                bw.Write(worldData.CaveBackStyle1);
                bw.Write(worldData.CaveBackStyle2);
                bw.Write(worldData.CaveBackStyle3);
                bw.Write(worldData.CaveBackStyle4);
                bw.Write(worldData.ForestTreeTopStyle1);
                bw.Write(worldData.ForestTreeTopStyle2);
                bw.Write(worldData.ForestTreeTopStyle3);
                bw.Write(worldData.ForestTreeTopStyle4);
                bw.Write(worldData.CorruptionTreeTopStyle4);
                bw.Write(worldData.JungleTreeTopStyle4);
                bw.Write(worldData.SnowTreeTopStyle4);
                bw.Write(worldData.HallowTreeTopStyle4);
                bw.Write(worldData.CrimsonTreeTopStyle4);
                bw.Write(worldData.DesertTreeTopStyle4);
                bw.Write(worldData.OceanTreeTopStyle4);
                bw.Write(worldData.GlowingMushroomTreeTopStyle4);
                bw.Write(worldData.UnderworldTreeTopStyle4);
                bw.Write(worldData.Rain);
                bw.Write(worldData.EventInfo1);
                bw.Write(worldData.EventInfo2);
                bw.Write(worldData.EventInfo3);
                bw.Write(worldData.EventInfo4);
                bw.Write(worldData.EventInfo5);
                bw.Write(worldData.EventInfo6);
                bw.Write(worldData.EventInfo7);
                bw.Write(worldData.EventInfo8);
                bw.Write(worldData.EventInfo9);
                bw.Write(worldData.EventInfo10);
                bw.Write(worldData.SundialCooldown);
                bw.Write(worldData.MoondialCooldown);
                bw.Write(worldData.CopperOreTier);
                bw.Write(worldData.IronOreTier);
                bw.Write(worldData.SilverOreTier);
                bw.Write(worldData.GoldOreTier);
                bw.Write(worldData.CobaltOreTier);
                bw.Write(worldData.MythrilOreTier);
                bw.Write(worldData.AdmantiteOreTier);
                bw.Write(worldData.InvasionType);
                bw.Write(worldData.LobbyID);
                bw.Write(worldData.SandstormSeverity);
            }
            [Benchmark]
            public void Test_ReuseBinaryWriter()
            {
                bw.BaseStream.Position = 1;
                bw.Write(worldData.Time);
                bw.Write(worldData.DayAndMoonInfo);
                bw.Write(worldData.MoonPhase);
                bw.Write(worldData.MaxTileX);
                bw.Write(worldData.MaxTileY);
                bw.Write(worldData.SpawnX);
                bw.Write(worldData.SpawnY);
                bw.Write(worldData.WorldSurface);
                bw.Write(worldData.RockLayer);
                bw.Write(worldData.WorldID);
                bw.Write(worldData.WorldName);
                bw.Write(worldData.GameMode);
                bw.Write(worldData.WorldUniqueIDData);
                bw.Write(worldData.WorldGeneratorVersion);
                bw.Write(worldData.MoonType);
                bw.Write(worldData.TreeBackground);
                bw.Write(worldData.CorruptionBackground);
                bw.Write(worldData.JungleBackground);
                bw.Write(worldData.SnowBackground);
                bw.Write(worldData.HallowBackground);
                bw.Write(worldData.CrimsonBackground);
                bw.Write(worldData.DesertBackground);
                bw.Write(worldData.OceanBackground);
                bw.Write(worldData.UnknownBackground1);
                bw.Write(worldData.UnknownBackground2);
                bw.Write(worldData.UnknownBackground3);
                bw.Write(worldData.UnknownBackground4);
                bw.Write(worldData.UnknownBackground5);
                bw.Write(worldData.IceBackStyle);
                bw.Write(worldData.JungleBackStyle);
                bw.Write(worldData.HellBackStyle);
                bw.Write(worldData.WindSpeedSet);
                bw.Write(worldData.CloudNumber);
                bw.Write(worldData.Tree1);
                bw.Write(worldData.Tree2);
                bw.Write(worldData.Tree3);
                bw.Write(worldData.TreeStyle1);
                bw.Write(worldData.TreeStyle2);
                bw.Write(worldData.TreeStyle3);
                bw.Write(worldData.TreeStyle4);
                bw.Write(worldData.CaveBack1);
                bw.Write(worldData.CaveBack2);
                bw.Write(worldData.CaveBack3);
                bw.Write(worldData.CaveBackStyle1);
                bw.Write(worldData.CaveBackStyle2);
                bw.Write(worldData.CaveBackStyle3);
                bw.Write(worldData.CaveBackStyle4);
                bw.Write(worldData.ForestTreeTopStyle1);
                bw.Write(worldData.ForestTreeTopStyle2);
                bw.Write(worldData.ForestTreeTopStyle3);
                bw.Write(worldData.ForestTreeTopStyle4);
                bw.Write(worldData.CorruptionTreeTopStyle4);
                bw.Write(worldData.JungleTreeTopStyle4);
                bw.Write(worldData.SnowTreeTopStyle4);
                bw.Write(worldData.HallowTreeTopStyle4);
                bw.Write(worldData.CrimsonTreeTopStyle4);
                bw.Write(worldData.DesertTreeTopStyle4);
                bw.Write(worldData.OceanTreeTopStyle4);
                bw.Write(worldData.GlowingMushroomTreeTopStyle4);
                bw.Write(worldData.UnderworldTreeTopStyle4);
                bw.Write(worldData.Rain);
                bw.Write(worldData.EventInfo1);
                bw.Write(worldData.EventInfo2);
                bw.Write(worldData.EventInfo3);
                bw.Write(worldData.EventInfo4);
                bw.Write(worldData.EventInfo5);
                bw.Write(worldData.EventInfo6);
                bw.Write(worldData.EventInfo7);
                bw.Write(worldData.EventInfo8);
                bw.Write(worldData.EventInfo9);
                bw.Write(worldData.EventInfo10);
                bw.Write(worldData.SundialCooldown);
                bw.Write(worldData.MoondialCooldown);
                bw.Write(worldData.CopperOreTier);
                bw.Write(worldData.IronOreTier);
                bw.Write(worldData.SilverOreTier);
                bw.Write(worldData.GoldOreTier);
                bw.Write(worldData.CobaltOreTier);
                bw.Write(worldData.MythrilOreTier);
                bw.Write(worldData.AdmantiteOreTier);
                bw.Write(worldData.InvasionType);
                bw.Write(worldData.LobbyID);
                bw.Write(worldData.SandstormSeverity);
            }
        }
    }
}
