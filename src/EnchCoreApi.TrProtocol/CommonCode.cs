using System.Buffers;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EnchCoreApi.TrProtocol {
    public unsafe static class CommonCode {
        static delegate*<int, string> FastAllocateString;
        static CommonCode() {
            FastAllocateString = (delegate*<int, string>)typeof(string).GetRuntimeMethods().First(m => m.Name == "FastAllocateString").MethodHandle.GetFunctionPointer();
        }

        #region String
        public unsafe static string ReadString(Span<byte> buffer, ref int index) {
            fixed (byte* ptr = buffer) {
                using var s = new UnmanagedMemoryStream(ptr, buffer.Length - index);
                index += (int)s.Position;
                using var r = new BinaryReader(s);
                return r.ReadString();
            }
        }
        public unsafe static void WriteString(Span<byte> buffer, ref int index, ref string str) {
            fixed (byte* ptr = buffer) {
                using var s = new UnmanagedMemoryStream(ptr, buffer.Length - index);
                using var w = new BinaryWriter(s);
                w.Write(str);
                index += (int)s.Position;
            }
        }
        public unsafe static string ReadString(ref void* ptr, int len) {
            using var s = new UnmanagedMemoryStream((byte*)ptr, len);
            using var r = new BinaryReader(s);
            var str = r.ReadString();
            ptr = Unsafe.Add<byte>(ptr, (int)s.Position);
            return str;
        }
        public unsafe static void WriteString(ref void* ptr, int len, ref string str) {
            using var s = new UnmanagedMemoryStream((byte*)ptr, len, len, FileAccess.Write);
            using var w = new BinaryWriter(s);
            w.Write(str);
            ptr = Unsafe.Add<byte>(ptr, (int)s.Position);
        }
        static Encoding encoding = Encoding.UTF8;

        public unsafe static string ReadString_Ex(ref void* ptr)
        {
            int Len = 0;
            var ptr_current = ptr;
            uint stringByteLen_Uint = 0;

            // Read the integer 7 bits at a time. The high bit
            // of the byte when on means to continue reading more bytes.
            //
            // There are two failure cases: we've read more than 5 bytes,
            // or the fifth byte is about to cause integer overflow.
            // This means that we can read the first 4 bytes without
            // worrying about integer overflow.

            const int MaxBytesWithoutOverflow = 4;

            var byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            stringByteLen_Uint = byteReadJustNow & 0x7Fu;

            if (byteReadJustNow == 0)
            {
                ptr = ptr_current;
                return string.Empty;
            }

            if (byteReadJustNow <= 0x7Fu)
            {
                goto getStringLen_finial;
            }


            for (int shift = 7; shift < MaxBytesWithoutOverflow * 7; shift += 7)
            {
                // ReadByte handles end of stream cases for us.
                byteReadJustNow = Unsafe.Read<byte>(ptr_current);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                stringByteLen_Uint |= (byteReadJustNow & 0x7Fu) << shift;

                if (byteReadJustNow <= 0x7Fu)
                {
                    goto getStringLen_finial;
                }
            }

            // Read the 5th byte. Since we already read 28 bits,
            // the value of this byte must fit within 4 bits (32 - 28),
            // and it must not have the high bit set.

            byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            if (byteReadJustNow > 0b_1111u)
            {
                ptr = ptr_current;
                throw new FormatException("");
            }

            stringByteLen_Uint |= (uint)byteReadJustNow << (MaxBytesWithoutOverflow * 7);

        getStringLen_finial:
            if ((Len = (int)stringByteLen_Uint) < 0)
            {
                throw new FormatException();
            }

            ptr = Unsafe.Add<byte>(ptr_current, Len);
            var charBuffer = FastAllocateString(Len); //new char[Len];
            fixed (char* charPtr = charBuffer)
            {
                Len = encoding.GetChars((byte*)ptr_current, Len, charPtr, Len);
            }
            //var str = new string(str);
            //return str;
            return charBuffer;
        }
        public unsafe static string ReadString(ref void* ptr) {
            int Len = 0;
            var ptr_current = ptr;
            uint stringByteLen_Uint = 0;

            // Read the integer 7 bits at a time. The high bit
            // of the byte when on means to continue reading more bytes.
            //
            // There are two failure cases: we've read more than 5 bytes,
            // or the fifth byte is about to cause integer overflow.
            // This means that we can read the first 4 bytes without
            // worrying about integer overflow.

            const int MaxBytesWithoutOverflow = 4;

            var byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            stringByteLen_Uint = byteReadJustNow & 0x7Fu;

            if (byteReadJustNow == 0) {
                ptr = ptr_current;
                return string.Empty;
            }

            if (byteReadJustNow <= 0x7Fu) {
                goto getStringLen_finial;
            }


            byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            stringByteLen_Uint |= (byteReadJustNow & 0x7Fu) << 7;

            if (byteReadJustNow <= 0x7Fu)
            {
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                goto getStringLen_finial;
            }

            byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            stringByteLen_Uint |= (byteReadJustNow & 0x7Fu) << 14;

            if (byteReadJustNow <= 0x7Fu)
            {
                ptr_current = Unsafe.Add<byte>(ptr_current, 2);
                goto getStringLen_finial;
            }

            byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            stringByteLen_Uint |= (byteReadJustNow & 0x7Fu) << 21;

            if (byteReadJustNow <= 0x7Fu)
            {
                ptr_current = Unsafe.Add<byte>(ptr_current, 3);
                goto getStringLen_finial;
            }

            // Read the 5th byte. Since we already read 28 bits,
            // the value of this byte must fit within 4 bits (32 - 28),
            // and it must not have the high bit set.

            byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            if (byteReadJustNow > 0b_1111u) {
                ptr = ptr_current;
                throw new FormatException("");
            }

            stringByteLen_Uint |= (uint)byteReadJustNow << (MaxBytesWithoutOverflow * 7);

        getStringLen_finial:

            if ((Len = (int)stringByteLen_Uint) < 0) {
                throw new FormatException();
            }

            ptr = Unsafe.Add<byte>(ptr_current, Len);
            if (Len <= 1024)
            {
                var charPtr = stackalloc char[Len]; //new char[Len];
                return new string(charPtr, 0, encoding.GetChars((byte*)ptr_current, Len, charPtr, Len));
            }
            else
            {
                fixed (char* charPtr = FastAllocateString(Len))
                {
                    return new string(charPtr, 0, encoding.GetChars((byte*)ptr_current, Len, charPtr, Len));
                }
            }
        }
        public unsafe static string ReadString2(ref void* ptr) {
            int Len = 0;
            var ptr_current = ptr;
            uint stringByteLen_Uint = 0;

            // Read the integer 7 bits at a time. The high bit
            // of the byte when on means to continue reading more bytes.
            //
            // There are two failure cases: we've read more than 5 bytes,
            // or the fifth byte is about to cause integer overflow.
            // This means that we can read the first 4 bytes without
            // worrying about integer overflow.

            const int MaxBytesWithoutOverflow = 4;

            var byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            stringByteLen_Uint = byteReadJustNow & 0x7Fu;

            if (byteReadJustNow == 0) {
                ptr = ptr_current;
                return string.Empty;
            }

            if (byteReadJustNow <= 0x7Fu) {
                goto getStringLen_finial;
            }


            for (int shift = 7; shift < MaxBytesWithoutOverflow * 7; shift += 7) {
                // ReadByte handles end of stream cases for us.
                byteReadJustNow = Unsafe.Read<byte>(ptr_current);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                stringByteLen_Uint |= (byteReadJustNow & 0x7Fu) << shift;

                if (byteReadJustNow <= 0x7Fu) {
                    goto getStringLen_finial;
                }
            }

            // Read the 5th byte. Since we already read 28 bits,
            // the value of this byte must fit within 4 bits (32 - 28),
            // and it must not have the high bit set.

            byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            if (byteReadJustNow > 0b_1111u) {
                ptr = ptr_current;
                throw new FormatException("");
            }

            stringByteLen_Uint |= (uint)byteReadJustNow << (MaxBytesWithoutOverflow * 7);

        getStringLen_finial:
            if ((Len = (int)stringByteLen_Uint) < 0) {
                throw new FormatException();
            }
            string str;
            ptr = Unsafe.Add<byte>(ptr_current, Len);
            //var str = new string(default, Len); //new char[Len];
            var buffer = new char[Len];
            fixed (char* charPtr = buffer) {
                Len = encoding.GetChars((byte*)ptr_current, Len, charPtr, Len);
                str = new string(default, Len);
                fixed (char* charPtrTo = str) {
                    Unsafe.CopyBlock(charPtrTo, charPtr, (uint)(sizeof(char) * Len));
                }
            }
            //var str = new string(str);
            //return str;
            return str;
        }
        public unsafe static string ReadString3(ref void* ptr) {
            int Len = 0;
            var ptr_current = ptr;
            uint stringByteLen_Uint = 0;

            // Read the integer 7 bits at a time. The high bit
            // of the byte when on means to continue reading more bytes.
            //
            // There are two failure cases: we've read more than 5 bytes,
            // or the fifth byte is about to cause integer overflow.
            // This means that we can read the first 4 bytes without
            // worrying about integer overflow.

            const int MaxBytesWithoutOverflow = 4;

            var byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            stringByteLen_Uint = byteReadJustNow & 0x7Fu;

            if (byteReadJustNow == 0) {
                ptr = ptr_current;
                return string.Empty;
            }

            if (byteReadJustNow <= 0x7Fu) {
                goto getStringLen_finial;
            }


            for (int shift = 7; shift < MaxBytesWithoutOverflow * 7; shift += 7) {
                // ReadByte handles end of stream cases for us.
                byteReadJustNow = Unsafe.Read<byte>(ptr_current);
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                stringByteLen_Uint |= (byteReadJustNow & 0x7Fu) << shift;

                if (byteReadJustNow <= 0x7Fu) {
                    goto getStringLen_finial;
                }
            }

            // Read the 5th byte. Since we already read 28 bits,
            // the value of this byte must fit within 4 bits (32 - 28),
            // and it must not have the high bit set.

            byteReadJustNow = Unsafe.Read<byte>(ptr_current);
            ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            if (byteReadJustNow > 0b_1111u) {
                ptr = ptr_current;
                throw new FormatException("");
            }

            stringByteLen_Uint |= (uint)byteReadJustNow << (MaxBytesWithoutOverflow * 7);

        getStringLen_finial:

            if ((Len = (int)stringByteLen_Uint) < 0) {
                throw new FormatException();
            }

            ptr = Unsafe.Add<byte>(ptr_current, Len);
            var str = FastAllocateString(encoding.GetCharCount((byte*)ptr_current, Len));
            fixed (char* charPtr = str) {
                //Unsafe.Write(Unsafe.Subtract<int>(charPtr, 1), encoding.GetChars((byte*)ptr_current, Len, charPtr, Len));
                encoding.GetChars((byte*)ptr_current, Len, charPtr, str.Length);
            }

            //var str = new string(str);
            //return str;
            return str;
        }
        public unsafe static void WriteString(ref void* ptr, string value) {
            if (value.Length == 0)
            {
                Unsafe.Write(ptr, (byte)0);
                ptr = Unsafe.Add<byte>(ptr, 1);
                return;
            }
            var ptr_current = ptr;
            var len = value.Length * sizeof(char) * 2;
            if (len <= 1024)
            {
                fixed (char* char_ptr = value)
                {
                    var byte_ptr = stackalloc byte[len];
                    len = encoding.GetBytes(char_ptr, value.Length, byte_ptr, len);

                    uint len_uint;
                    for (len_uint = (uint)len; len_uint > 127; len_uint >>= 7)
                    {
                        Unsafe.Write(ptr_current, (byte)(len_uint | 0xFFFFFF80u));
                        ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                    }
                    Unsafe.Write(ptr_current, (byte)(len_uint));
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                    Unsafe.CopyBlock(ptr_current, byte_ptr, len_uint);
                }
            }
            else
            {
                len = encoding.GetByteCount(value);

                uint len_uint;
                for (len_uint = (uint)len; len_uint > 127; len_uint >>= 7)
                {
                    Unsafe.Write(ptr_current, (byte)(len_uint | 0xFFFFFF80u));
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                }

                fixed (char* char_ptr = value)
                {
                    Unsafe.Write(ptr_current, (byte)(len_uint));
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                    encoding.GetBytes(char_ptr, value.Length, (byte*)ptr_current, len);
                }
            }

            ptr = Unsafe.Add<byte>(ptr_current, len);
        }
        public unsafe static void WriteString2(ref void* ptr, string value)
        {
            if (value.Length == 0)
            {
                Unsafe.Write(ptr, (byte)0);
                ptr = Unsafe.Add<byte>(ptr, 1);
                return;
            }
            var ptr_current = ptr;

            var len = encoding.GetByteCount(value);

            uint len_uint;
            for (len_uint = (uint)len; len_uint > 127; len_uint >>= 7)
            {
                Unsafe.Write(ptr_current, (byte)(len_uint | 0xFFFFFF80u));
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            }

            fixed (char* char_ptr = value)
            {
                Unsafe.Write(ptr_current, (byte)(len_uint));
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                encoding.GetBytes(char_ptr, value.Length, (byte*)ptr_current, len);
            }

            ptr = Unsafe.Add<byte>(ptr_current, len);
        }
        public unsafe static void WriteString3(ref void* ptr, string value)
        {
            if (value.Length == 0)
            {
                Unsafe.Write(ptr, (byte)0);
                ptr = Unsafe.Add<byte>(ptr, 1);
                return;
            }
            var ptr_current = ptr;
            var len = value.Length * sizeof(char) * 2;
            var buffer = ArrayPool<byte>.Shared.Rent(len);
            fixed (byte* byte_ptr = buffer) {
                fixed (char* char_ptr = value) {
                    len = encoding.GetBytes(char_ptr, value.Length, byte_ptr, len);

                    uint len_uint;
                    for (len_uint = (uint)len; len_uint > 127; len_uint >>= 7) {
                        Unsafe.Write(ptr_current, (byte)(len_uint | 0xFFFFFF80u));
                        ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                    }
                    Unsafe.Write(ptr_current, (byte)(len_uint));
                    ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                    Unsafe.CopyBlock(ptr_current, byte_ptr, len_uint);
                }
            }
            ArrayPool<byte>.Shared.Return(buffer);
            ptr = Unsafe.Add<byte>(ptr_current, len);
        }
        #endregion

        #region Compression
        public unsafe static void ReadDecompressedData(void* source, ref void* destination, int compressedDataLength) {
            using var st = new UnmanagedMemoryStream((byte*)source, compressedDataLength, compressedDataLength, FileAccess.Read);
            using (var dst = new DeflateStream(st, CompressionMode.Decompress, true)) {
                int readed;
                do {
                    readed = dst.Read(new Span<byte>(destination, 1024 * 32));
                    destination = Unsafe.Add<byte>(destination, readed);
                }
                while (readed > 0);
            }
        }
        public unsafe static void WriteCompressedData(void* source, ref void* destination, int rawDataLength, CompressionLevel level) {
            using var st = new UnmanagedMemoryStream((byte*)destination, 1024 * 32, 1024 * 32, FileAccess.Write);
            using (var dst = new DeflateStream(st, level, true)) {
                dst.Write(new Span<byte>(source, rawDataLength));
            }
            destination = st.PositionPointer;
        }
        #endregion
    }
}
