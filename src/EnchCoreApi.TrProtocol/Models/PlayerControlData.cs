using EnchCoreApi.TrProtocol.Interfaces;
using Terraria;

namespace EnchCoreApi.TrProtocol.Models
{
    public struct PlayerControlData : ISoildSerializableData
    {
        public BitsByte packedValue;
        public bool ControlUp
        {
            get => packedValue[0];
            set => packedValue[0] = value;
        }
        public bool ControlDown
        {
            get => packedValue[1];
            set => packedValue[1] = value;
        }
        public bool ControlLeft
        {
            get => packedValue[2];
            set => packedValue[2] = value;
        }
        public bool ControlRight
        {
            get => packedValue[3];
            set => packedValue[3] = value;
        }
        public bool ControlJump
        {
            get => packedValue[4];
            set => packedValue[4] = value;
        }
        public bool IsUsingItem
        {
            get => packedValue[5];
            set => packedValue[5] = value;
        }
        public bool FaceDirection
        {
            get => packedValue[6];
            set => packedValue[6] = value;
        }
    }
    public struct PlayerMiscData1 : ISoildSerializableData
    {
        public BitsByte packedValue;

        public bool IsUsingPulley
        {
            get => packedValue[0];
            set => packedValue[0] = value;
        }

        public bool PulleyDirection
        {
            get => packedValue[1];
            set => packedValue[1] = value;
        }

        public bool HasVelocity
        {
            get => packedValue[2];
            set => packedValue[2] = value;
        }

        public bool IsVortexStealthActive
        {
            get => packedValue[3];
            set => packedValue[3] = value;
        }

        public bool GravityDirection
        {
            get => packedValue[4];
            set => packedValue[4] = value;
        }

        public bool IsShieldRaised
        {
            get => packedValue[5];
            set => packedValue[5] = value;
        }

        public bool IsGhosted
        {
            get => packedValue[6];
            set => packedValue[6] = value;
        }
    }
    public struct PlayerMiscData2 : ISoildSerializableData
    {
        public BitsByte packedValue;
        public bool TryHoveringUp
        {
            get => packedValue[0];
            set => packedValue[0] = value;
        }
        public bool IsVoidVaultEnabled
        {
            get => packedValue[1];
            set => packedValue[1] = value;
        }
        public bool IsSitting
        {
            get => packedValue[2];
            set => packedValue[2] = value;
        }
        public bool HasDownedDd2Event
        {
            get => packedValue[3];
            set => packedValue[3] = value;
        }
        public bool IsPettingAnimal
        {
            get => packedValue[4];
            set => packedValue[4] = value;
        }
        public bool IsPettedAnimalSmall
        {
            get => packedValue[5];
            set => packedValue[5] = value;
        }
        public bool CanReturnWithPotionOfReturn
        {
            get => packedValue[6];
            set => packedValue[6] = value;
        }
        public bool TryHoveringDown
        {
            get => packedValue[7];
            set => packedValue[7] = value;
        }
    }
    public struct PlayerMiscData3 : ISoildSerializableData
    {
        public BitsByte packedValue;
        public bool IsSleeping
        {
            get => packedValue[0];
            set => packedValue[0] = value;
        }
    }
}
