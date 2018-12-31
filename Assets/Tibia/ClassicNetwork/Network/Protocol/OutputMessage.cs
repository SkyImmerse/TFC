using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameClient.Network
{
    public class OutputMessage
    {
        public enum AnonymousEnum
        {
            BufferMaxsize = 65536,
            MaxStringLength = 65536,
            MaxHeaderSize = 8
        }

        public UInt16 MHeaderPos = new UInt16();
        private UInt16 _mWritePos = new UInt16();
        private UInt16 _mMessageSize = new UInt16();
        private byte[] _mBuffer = new byte[(int)AnonymousEnum.BufferMaxsize];

        public OutputMessage(TCPClient client)
        {
            Reset();
        }


        public void Reset()
        {
            _mWritePos = (ushort)AnonymousEnum.MaxHeaderSize;
            MHeaderPos = (ushort)AnonymousEnum.MaxHeaderSize;
            _mMessageSize = 0;
            _mBuffer = new byte[(int)AnonymousEnum.BufferMaxsize];
        }

        public void AddU8(byte value)
        {
            CheckWrite(1);
            //Writer.Write(value);
            _mBuffer[_mWritePos] = value;
            _mWritePos += 1;
            _mMessageSize += 1;
        }

        public void AddU16(UInt16 value)
        {
            CheckWrite(2);
            //Writer.Write(value);
            BitConverter.GetBytes(value).CopyTo(_mBuffer, _mWritePos);
            _mWritePos += 2;
            _mMessageSize += 2;
        }

        public void AddU32(UInt32 value)
        {
            CheckWrite(4);
            //Writer.Write(value);
            BitConverter.GetBytes(value).CopyTo(_mBuffer, _mWritePos);
            _mWritePos += 4;
            _mMessageSize += 4;
        }

        public void AddU64(UInt64 value)
        {
            CheckWrite(8);
            //Writer.Write(value);
            BitConverter.GetBytes(value).CopyTo(_mBuffer, _mWritePos);
            _mWritePos += 8;
            _mMessageSize += 8;
        }

        public void AddString(string buffer)
        {
            ushort len = (ushort)buffer.Length;
            //if (len > (int)AnonymousEnum.MaxStringLength)
            //    throw new Exception(String.Format("string length > {0}", AnonymousEnum.MaxStringLength));
            CheckWrite(len + 2);
            AddU16(len);

            var arr = buffer.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                AddU8((byte)arr[i]);
            }
        }

        public void Flush(TCPClient connection)
        {
            byte[] array = new byte[_mMessageSize];
            Buffer.BlockCopy(_mBuffer, MHeaderPos, array, 0, _mMessageSize);
            connection.Send(array);
        }

        public void AddPaddingBytes(int bytes)
        {
            AddPaddingBytes(bytes, 0);
        }

        public void AddPaddingBytes(int bytes, byte @byte)
        {
            if (bytes <= 0)
                return;
            CheckWrite(bytes);

            for (int i = 0; i < bytes; i++)
            {
                _mBuffer[_mWritePos + i] = @byte;
            }
            _mWritePos += (ushort)bytes;
            _mMessageSize += (ushort)bytes;
        }

        public void EncryptRsa()
        {
            int size = Rsa.RsaGetSize();
            if (_mMessageSize < size)
                throw new Exception("insufficient bytes in buffer to encrypt");

            Rsa.Encrypt(ref _mBuffer, _mWritePos - size);
        }

        public UInt16 GetWritePos()
        {
            return _mWritePos;
        }

        public UInt16 GetMessageSize()
        {
            return _mMessageSize;
        }

        public void SetWritePos(UInt16 writePos)
        {
            _mWritePos = writePos;
        }

        public void SetMessageSize(UInt16 messageSize)
        {
            _mMessageSize = messageSize;
        }

        protected byte[] GetWriteBuffer()
        {
            return _mBuffer;
        }

        protected byte[] GetHeaderBuffer()
        {
            return _mBuffer;
        }

        public byte[] GetDataBuffer()
        {
            return _mBuffer;
        }

        public void WriteChecksum()
        {
            UInt32 checksum = Tools.Adler32(_mBuffer, MHeaderPos, _mMessageSize);
            MHeaderPos -= 4;
            BitConverter.GetBytes(checksum).CopyTo(_mBuffer, MHeaderPos);
            _mMessageSize += 4;
        }

        public void WriteMessageSize()
        {
            MHeaderPos -= 2;
            BitConverter.GetBytes(_mMessageSize).CopyTo(_mBuffer, MHeaderPos);
            _mMessageSize += 2;
        }

        private bool CanWrite(int bytes)
        {
            if (_mWritePos + bytes > (int)AnonymousEnum.BufferMaxsize)
                return false;
            return true;
        }

        private void CheckWrite(int bytes)
        {
            if (!CanWrite(bytes))
                throw new Exception("OutputMessage max buffer size reached");
        }


    }
}
