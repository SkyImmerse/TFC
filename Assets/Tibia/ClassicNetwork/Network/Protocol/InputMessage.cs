using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Network.Protocol
{
    public class InputMessage
    {
        public enum InputMsgDefinitions
        {
            BufferMaxsize = 65536,
            MaxHeaderSize = 8
        }

        private ushort _mHeaderPos;
        private ushort _mReadPos;
        private ushort _mMessageSize;
        private byte[] _mBuffer = new byte[(int)InputMsgDefinitions.BufferMaxsize];

        public InputMessage()
        {
            Reset();
        }

        public void SetBuffer(string buffer)
        {
            var len = buffer.Length;
            CheckWrite((int)InputMsgDefinitions.MaxHeaderSize + len);
            _mReadPos = ((int)InputMsgDefinitions.MaxHeaderSize);
            _mHeaderPos = ((int)InputMsgDefinitions.MaxHeaderSize);
            _mMessageSize = (ushort)len;
        }

        public void SkipBytes(ushort bytes)
        {
            _mReadPos += bytes;
        }

        public void SetReadPos(ushort readPos)
        {
            _mReadPos = readPos;
        }

        public byte GetU8()
        {
            CheckRead(1);
            var v = _mBuffer[_mReadPos];
            _mReadPos += 1;
            return v;
        }

        public ushort GetU16()
        {
            CheckRead(2);

            var result = BitConverter.ToUInt16(_mBuffer, _mReadPos);
            _mReadPos += 2;

            return result;
        }

        public uint GetU32()
        {
            CheckRead(4);
            var result = BitConverter.ToUInt32(_mBuffer, _mReadPos);

            _mReadPos += 4;
            return result;
        }

        public ulong GetU64()
        {
            CheckRead(8);
            var result = BitConverter.ToUInt64(_mBuffer, _mReadPos);

            _mReadPos += 8;
            return result;
        }

        public string GetString()
        {
            var stringLength = GetU16();
            CheckRead(stringLength);
            var charArray = new char[stringLength];
            for (var i = 0; i < stringLength; i++)
            {
                charArray[i] = (char)GetU8();
            }
            var result = new string(charArray);

            return result;
        }

        public double GetDouble()
        {
            var precision = GetU8();
            var v = (int)GetU32() - int.MaxValue;
            return (v / Math.Pow((float)10, precision));
        }

        public byte PeekU8()
        {
            var v = GetU8();
            _mReadPos -= 1;
            return v;
        }

        public ushort PeekU16()
        {
            var v = GetU16();
            _mReadPos -= 2;
            return v;
        }

        public uint PeekU32()
        {
            var v = GetU32();
            _mReadPos -= 4;
            return v;
        }

        public ulong PeekU64()
        {
            var v = GetU64();
            _mReadPos -= 8;
            return v;
        }

        public bool DecryptRsa(int size)
        {
            CheckRead(size);
            return (GetU8() == 0x00);
        }

        public int GetReadSize()
        {
            return _mReadPos - _mHeaderPos;
        }

        public int GetReadPos()
        {
            return _mReadPos;
        }

        public int GetUnreadSize()
        {
            return _mMessageSize - (_mReadPos - _mHeaderPos);
        }

        public ushort GetMessageSize()
        {
            return _mMessageSize;
        }

        public bool Eof()
        {
            return (_mReadPos - _mHeaderPos) >= _mMessageSize;
        }

        public void Reset()
        {
            _mMessageSize = 0;
            _mReadPos = (int)InputMsgDefinitions.MaxHeaderSize;
            _mHeaderPos = (int)InputMsgDefinitions.MaxHeaderSize;
        }

        public void FillBuffer(ref byte[] buffer, ushort size)
        {
            CheckWrite(_mReadPos + size);
            buffer.CopyTo(_mBuffer, _mReadPos);
            _mMessageSize += size;
        }

        public void SetHeaderSize(ushort size)
        {
            _mHeaderPos = (ushort)((int)InputMsgDefinitions.MaxHeaderSize - (int)size);
            _mReadPos = _mHeaderPos;
        }

        public void SetMessageSize(ushort size)
        {
            _mMessageSize = size;
        }

        public byte[] GetReadBuffer()
        {
            return _mBuffer;
        }

        protected byte[] GetHeaderBuffer()
        {
            return _mBuffer;
        }

        protected byte[] GetDataBuffer()
        {
            return _mBuffer;
        }

        public ushort ReadSize()
        {
            return GetU16();
        }

        public bool ReadChecksum()
        {
            var receivedCheck = GetU32();
            var checksum = Tools.Adler32(_mBuffer /*+ m_readPos*/, 6, GetUnreadSize());
            return receivedCheck == checksum;
        }

        private bool CanRead(int bytes)
        {
            return (_mReadPos - _mHeaderPos + bytes <= _mMessageSize) && (_mReadPos + bytes <= (int)InputMsgDefinitions.BufferMaxsize);
        }

        private void CheckRead(int bytes)
        {
            if (!CanRead(bytes))
                throw new Exception("InputMessage eof reached");
        }

        private static void CheckWrite(int bytes)
        {
            if (bytes > (int)InputMsgDefinitions.BufferMaxsize)
                throw new Exception("InputMessage max buffer size reached");
        }


    }
}
