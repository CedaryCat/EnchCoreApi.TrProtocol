
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EnchCoreApi.TrProtocol {
    public static class CommonCode {
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


            for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7) {
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
            var charBuffer = new string(default, Len); //new char[Len];
            fixed (char* charPtr = charBuffer) {
                Len = encoding.GetChars((byte*)ptr_current, Len, charPtr, Len);
            }
            //var str = new string(charBuffer);
            //return str;
            return charBuffer[..Len];
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


            for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7) {
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
            //var charBuffer = new string(default, Len); //new char[Len];
            var buffer = new char[Len];
            fixed (char* charPtr = buffer) {
                Len = encoding.GetChars((byte*)ptr_current, Len, charPtr, Len);
                str = new string(default, Len);
                fixed (char* charPtrTo = str) {
                    Unsafe.CopyBlock(charPtrTo, charPtr, (uint)(sizeof(char) * Len));
                }
            }
            //var str = new string(charBuffer);
            //return str;
            return str;
        }
        public unsafe static void WriteString(ref void* ptr, string value) {
            var ptr_current = ptr;
            var len = encoding.GetByteCount(value);

            uint len_uint;
            for (len_uint = (uint)len; len_uint > 127; len_uint >>= 7) {
                Unsafe.Write(ptr_current, (byte)(len_uint | 0xFFFFFF80u));
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
            }

            fixed (char* char_ptr = value) {
                Unsafe.Write(ptr_current, (byte)(len_uint));
                ptr_current = Unsafe.Add<byte>(ptr_current, 1);
                encoding.GetBytes(char_ptr, value.Length, (byte*)ptr_current, len);
            }

            ptr = Unsafe.Add<byte>(ptr_current, len);
        }

        public unsafe static void WriteString2(ref void* ptr, string value) {
            var ptr_current = ptr;
            var len = value.Length * 4;
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
    }
}
