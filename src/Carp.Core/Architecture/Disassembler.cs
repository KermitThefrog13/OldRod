using System;
using AsmResolver;

namespace Carp.Core.Architecture
{
    public class Disassembler
    {
        private readonly VMConstants _constants;

        public Disassembler(VMConstants constants, IBinaryStreamReader reader, uint key)
        {
            _constants = constants;
            Reader = reader;
            CurrentKey = key;
        }
        
        public IBinaryStreamReader Reader
        {
            get;
        }

        public uint CurrentKey
        {
            get;
            set;
        }

        public ILInstruction ReadNextInstruction()
        {
            int offset = (int) (Reader.Position - Reader.StartPosition);
            var opcode = ReadNextOpCode();
            var operand = ReadNextOperand(opcode.OperandType);
            return new ILInstruction(offset, opcode, operand);
        }

        private byte ReadByte()
        {
            uint key = CurrentKey;
            byte rawValue = Reader.ReadByte();
            byte b = (byte) (rawValue ^ key);
            key = key * 7 + b;
            CurrentKey = key;
            return b;
        }

        private ILOpCode ReadNextOpCode()
        {
            var opcode = ILOpCodes.All[(int) _constants.OpCodes[ReadByte()]];
            ReadByte();
            return opcode;
        }

        private VMRegisters ReadRegister()
        {
            return _constants.Registers[ReadByte()];
        }

        private uint ReadDword()
        {
            return ReadByte()
                   | ((uint) ReadByte() << 8)
                   | ((uint) ReadByte() << 16)
                   | ((uint) ReadByte() << 24);
        }

        private ulong ReadQword()
        {
            return ReadByte()
                   | ((ulong) ReadByte() << 8)
                   | ((ulong) ReadByte() << 16)
                   | ((ulong) ReadByte() << 24)
                   | ((ulong) ReadByte() << 32)
                   | ((ulong) ReadByte() << 40)
                   | ((ulong) ReadByte() << 48)
                   | ((ulong) ReadByte() << 56);
        }

        private object ReadNextOperand(ILOperandType operandType)
        {
            switch (operandType)
            {
                case ILOperandType.None:
                    return null;
                case ILOperandType.Register:
                    return ReadRegister();
                case ILOperandType.ImmediateDword:
                    return ReadDword();
                case ILOperandType.ImmediateQword:
                    return ReadQword();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}