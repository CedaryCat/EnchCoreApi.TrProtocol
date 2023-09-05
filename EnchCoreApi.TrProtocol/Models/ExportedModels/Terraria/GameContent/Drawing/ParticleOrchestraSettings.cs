using EnchCoreApi.TrProtocol.Attributes;
using Microsoft.Xna.Framework;

namespace Terraria.GameContent.Drawing;
[TypeForward]
public struct ParticleOrchestraSettings {
    public Vector2 PositionInWorld;

    public Vector2 MovementVector;

    public int UniqueInfoPiece;

    public byte IndexOfPlayerWhoInvokedThis;

    public const int SerializationSize = 21;

    [Obsolete]
    public void Serialize(BinaryWriter writer) {
        writer.Write(PositionInWorld.X);
        writer.Write(PositionInWorld.Y);
        writer.Write(MovementVector.X);
        writer.Write(MovementVector.Y);
        writer.Write(UniqueInfoPiece);
        writer.Write(IndexOfPlayerWhoInvokedThis);
    }

    [Obsolete]
    public void DeserializeFrom(BinaryReader reader) {
        PositionInWorld = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        MovementVector = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        UniqueInfoPiece = reader.ReadInt32();
        IndexOfPlayerWhoInvokedThis = reader.ReadByte();
    }
}
