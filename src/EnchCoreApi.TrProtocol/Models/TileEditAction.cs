using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnchCoreApi.TrProtocol.Models
{
    public enum TileEditAction : byte
    {
        KillTile = 0,
        PlaceTile,
        KillWall,
        PlaceWall,
        KillTileNoItem,
        PlaceWire,
        KillWire,
        PoundTile,
        PlaceActuator,
        KillActuator,
        PlaceWire2,
        KillWire2,
        PlaceWire3,
        KillWire3,
        SlopeTile,
        FrameTrack,
        PlaceWire4,
        KillWire4,
        PokeLogicGate,
        Acutate,
        TryKillTile,
        ReplaceTile,
        ReplaceWall,
        SlopePoundTile
    }
}
