﻿using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using J2N.Collections;
using J2N.Text;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using JCG = J2N.Collections.Generic;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Impl
{
    public static partial class Utility // ICU4N specific - made class static because there are no instance members
    {
        private const char APOSTROPHE = '\'';
        private const char BACKSLASH = '\\';
        private const int MAGIC_UNSIGNED = unchecked((int)0x80000000);

        private const int CharStackBufferSize = 32;

        // ICU4N: No need for ArrayEquals overloads because in .NET we have
        // strongly typed generics that aren't just syntactic sugar for object.

        /// <summary>
        /// Convenience utility to compare two <see cref="T:object[]"/>s.
        /// Ought to be in System.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ArrayEquals<T>(T[] source, T[] target)
        {
            // ICU4N: Using generics and a comparer is much faster in .NET
            return ArrayEqualityComparer<T>.OneDimensional.Equals(source, target);
        }

        /// <summary>
        /// Convenience utility to compare two <see cref="T:object[]"/>s.
        /// Ought to be in System.
        /// <para/>
        /// Note that if <typeparamref name="T"/> is a nested collection
        /// type, its contents will not be compared.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceStart"></param>
        /// <param name="target"></param>
        /// <param name="targetStart"></param>
        /// <param name="len">The length to compare. The start indices and start+len must be valid.</param>
        public static bool ArrayRegionMatches<T>(T[] source, int sourceStart,
                T[] target, int targetStart,
                int len)
        {
            int sourceEnd = sourceStart + len;
            int delta = targetStart - sourceStart;
            for (int i = sourceStart; i < sourceEnd; i++)
            {
                if (!JCG.EqualityComparer<T>.Default.Equals(source[i], target[i + delta]))
                    return false;
            }
            return true;
        }

        // ICU4N: No need for ArrayRegionMatches overloads because in .NET we have
        // strongly typed generics that aren't just syntactic sugar for object.

        /// <summary>
        /// Trivial reference equality.
        /// This method should help document that we really want == not <see cref="object.Equals(object, object)"/>
        /// and to have a single place to suppress warnings from static analysis tools.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SameObjects(object a, object b) // ICU4N: Factor out and use object.ReferenceEquals()
        {
            return a == b;
        }

        /// <summary>
        /// Convenience utility. Does null checks on objects, then calls <see cref="object.Equals(object)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ObjectEquals(object a, object b)
        {
            return a == null ?
                    b == null ? true : false :
                        b == null ? false : a.Equals(b);
        }

        // ICU4N specific - overload to ensure culture insensitive comparison when comparing strings
        /// <summary>
        /// Convenience utility. Does null checks on objects, then calls <see cref="J2N.Text.StringExtensions.CompareToOrdinal(string, string)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CheckCompare(string a, string b)
        {
            return a == null ?
                    b == null ? 0 : -1 :
                        b == null ? 1 : a.CompareToOrdinal(b);
        }

        // ICU4N specific - generic overload for comparing objects of known type
        /// <summary>
        /// Convenience utility. Does null checks on objects, then calls <see cref="IComparable{T}.CompareTo(T)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CheckCompare<T>(T a, T b) where T : IComparable<T>
        {
            return a == null ?
                    b == null ? 0 : -1 :
                        b == null ? 1 : a.CompareTo(b);
        }

        /// <summary>
        /// Convenience utility. Does null checks on objects, then calls <see cref="IComparable.CompareTo(object)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CheckCompare(IComparable a, IComparable b)
        {
            return a == null ?
                    b == null ? 0 : -1 :
                        b == null ? 1 : a.CompareTo(b);
        }

        /// <summary>
        /// Convenience utility. Does null checks on object, then calls <see cref="object.GetHashCode()"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CheckHashCode(object a)
        {
            return a == null ? 0 : a.GetHashCode();
        }

        // ICU4N: Factored out Run-Length Encoding (RLE) methods and constants, as they
        // are not in use and the only tests are for CompactByteArray and CompactCharArray,
        // both of which are marked internal and not used internally by anything.
        // If we ever need to resurrect this, we should allow the user to pass in the destination
        // Span<T> so memory can be reused.

        static public string LINE_SEPARATOR = Environment.NewLine;

        /// <summary>
        /// Format a string for representation in a source file.  This includes
        /// breaking it into lines and escaping characters using octal notation
        /// when necessary (control characters and double quotes).
        /// </summary>
        static public string FormatForSource(string s)
        {
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < s.Length;)
            {
                if (i > 0) buffer.Append('+').Append(LINE_SEPARATOR);
                buffer.Append("        \"");
                int count = 11;
                while (i < s.Length && count < 80)
                {
                    char c = s[i++];
                    if (c < '\u0020' || c == '"' || c == '\\')
                    {
                        if (c == '\n')
                        {
                            buffer.Append("\\n");
                            count += 2;
                        }
                        else if (c == '\t')
                        {
                            buffer.Append("\\t");
                            count += 2;
                        }
                        else if (c == '\r')
                        {
                            buffer.Append("\\r");
                            count += 2;
                        }
                        else
                        {
                            // Represent control characters, backslash and double quote
                            // using octal notation; otherwise the string we form
                            // won't compile, since Unicode escape sequences are
                            // processed before tokenization.
                            //buffer.Append('\\');
                            //buffer.Append(HEX_DIGIT[(c & 0700) >> 6]); // HEX_DIGIT works for octal
                            //buffer.Append(HEX_DIGIT[(c & 0070) >> 3]);
                            //buffer.Append(HEX_DIGIT[(c & 0007)]);

                            // ICU4N specific - converted octal literals to decimal literals (.NET has no octal literals)
                            buffer.Append('\\');
                            buffer.Append(HEX_DIGIT[(c & 0448) >> 6]); // HEX_DIGIT works for octal
                            buffer.Append(HEX_DIGIT[(c & 0056) >> 3]);
                            buffer.Append(HEX_DIGIT[(c & 0007)]);

                            count += 4;
                        }
                    }
                    else if (c <= '\u007E')
                    {
                        buffer.Append(c);
                        count += 1;
                    }
                    else
                    {
                        buffer.Append("\\u");
                        buffer.Append(HEX_DIGIT[(c & 0xF000) >> 12]);
                        buffer.Append(HEX_DIGIT[(c & 0x0F00) >> 8]);
                        buffer.Append(HEX_DIGIT[(c & 0x00F0) >> 4]);
                        buffer.Append(HEX_DIGIT[(c & 0x000F)]);
                        count += 6;
                    }
                }
                buffer.Append('"');
            }
            return buffer.ToString();
        }

        internal static readonly char[] HEX_DIGIT = {'0','1','2','3','4','5','6','7',
            '8','9','A','B','C','D','E','F'};

        /// <summary>
        /// Format a string for representation in a source file.  Like
        /// <see cref="FormatForSource(string)"/> but does not do line breaking.
        /// </summary>
        static public string Format1ForSource(string s)
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("\"");
            for (int i = 0; i < s.Length;)
            {
                char c = s[i++];
                if (c < '\u0020' || c == '"' || c == '\\')
                {
                    if (c == '\n')
                    {
                        buffer.Append("\\n");
                    }
                    else if (c == '\t')
                    {
                        buffer.Append("\\t");
                    }
                    else if (c == '\r')
                    {
                        buffer.Append("\\r");
                    }
                    else
                    {
                        // Represent control characters, backslash and double quote
                        // using octal notation; otherwise the string we form
                        // won't compile, since Unicode escape sequences are
                        // processed before tokenization.
                        //buffer.Append('\\');
                        //buffer.Append(HEX_DIGIT[(c & 0700) >> 6]); // HEX_DIGIT works for octal
                        //buffer.Append(HEX_DIGIT[(c & 0070) >> 3]);
                        //buffer.Append(HEX_DIGIT[(c & 0007)]);

                        // ICU4N specific - converted octal literals to decimal literals (.NET has no octal literals)
                        buffer.Append('\\');
                        buffer.Append(HEX_DIGIT[(c & 0448) >> 6]); // HEX_DIGIT works for octal
                        buffer.Append(HEX_DIGIT[(c & 0056) >> 3]);
                        buffer.Append(HEX_DIGIT[(c & 0007)]);
                    }
                }
                else if (c <= '\u007E')
                {
                    buffer.Append(c);
                }
                else
                {
                    buffer.Append("\\u");
                    buffer.Append(HEX_DIGIT[(c & 0xF000) >> 12]);
                    buffer.Append(HEX_DIGIT[(c & 0x0F00) >> 8]);
                    buffer.Append(HEX_DIGIT[(c & 0x00F0) >> 4]);
                    buffer.Append(HEX_DIGIT[(c & 0x000F)]);
                }
            }
            buffer.Append('"');
            return buffer.ToString();
        }

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        /// <summary>
        /// Convert characters outside the range U+0020 to U+007F to
        /// Unicode escapes, and convert backslash to a double backslash.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string Escape(string s)
            => Escape(s.AsSpan());
#endif

        /// <summary>
        /// Convert characters outside the range U+0020 to U+007F to
        /// Unicode escapes, and convert backslash to a double backslash.
        /// </summary>
        public static string Escape(ReadOnlySpan<char> s)
        {
            ValueStringBuilder buf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Escape(s, ref buf);
                return buf.ToString();
            }
            finally
            {
                buf.Dispose();
            }
        }

        /// <summary>
        /// Convert characters outside the range U+0020 to U+007F to
        /// Unicode escapes, and convert backslash to a double backslash.
        /// </summary>
        internal static void Escape(ReadOnlySpan<char> s, ref ValueStringBuilder result)
        {
            for (int i = 0; i < s.Length;)
            {
                int c = Character.CodePointAt(s, i);
                i += UTF16.GetCharCount(c);
                if (c >= ' ' && c <= 0x007F)
                {
                    if (c == '\\')
                    {
                        result.Append("\\\\"); // That is, "\\"
                    }
                    else
                    {
                        result.Append((char)c);
                    }
                }
                else
                {
                    bool four = c <= 0xFFFF;
                    result.Append(four ? "\\u" : "\\U");
                    result.AppendFormatHex(c, four ? 4 : 8);
                }
            }
        }

        /* This map must be in ASCENDING ORDER OF THE ESCAPE CODE */
        static private readonly char[] UNESCAPE_MAP = {
            /*"   (char)0x22, (char)0x22 */
            /*'   (char)0x27, (char)0x27 */
            /*?   (char)0x3F, (char)0x3F */
            /*\   (char)0x5C, (char)0x5C */
            /*a*/ (char)0x61, (char)0x07,
            /*b*/ (char)0x62, (char)0x08,
            /*e*/ (char)0x65, (char)0x1b,
            /*f*/ (char)0x66, (char)0x0c,
            /*n*/ (char)0x6E, (char)0x0a,
            /*r*/ (char)0x72, (char)0x0d,
            /*t*/ (char)0x74, (char)0x09,
            /*v*/ (char)0x76, (char)0x0b
        };

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        /// <summary>
        /// Convert an escape to a 32-bit code point value.  We attempt
        /// to parallel the icu4c unescapeAt() function.
        /// </summary>
        /// <param name="s">The character sequence to escape.</param>
        /// <param name="offset16">An offset to the character
        /// <em>after</em> the backslash.  Upon return offset16 will
        /// be updated to point after the escape sequence.</param>
        /// <returns>Character value from 0 to 10FFFF, or -1 on error.</returns>
        // ICU4N: To fix lack of implicit conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int UnescapeAt(string s, ref int offset16)
            => UnescapeAt(s.AsSpan(), ref offset16);
#endif

        /// <summary>
        /// Convert an escape to a 32-bit code point value.  We attempt
        /// to parallel the icu4c unescapeAt() function.
        /// </summary>
        /// <param name="s">The character sequence to escape.</param>
        /// <param name="offset16">An offset to the character
        /// <em>after</em> the backslash.  Upon return offset16 will
        /// be updated to point after the escape sequence.</param>
        /// <returns>Character value from 0 to 10FFFF, or -1 on error.</returns>
        public static int UnescapeAt(ReadOnlySpan<char> s, ref int offset16) // ICU4N: Changed array to ref parameter
        {
            int c;
            int result = 0;
            int n = 0;
            int minDig = 0;
            int maxDig = 0;
            int bitsPerDigit = 4;
            int dig;
            int i;
            bool braces = false;

            /* Check that offset is in range */
            int offset = offset16;
            int length = s.Length;
            if (offset < 0 || offset >= length)
            {
                return -1;
            }

            /* Fetch first UChar after '\\' */
            c = Character.CodePointAt(s, offset);
            offset += UTF16.GetCharCount(c);

            /* Convert hexadecimal and octal escapes */
            switch (c)
            {
                case 'u':
                    minDig = maxDig = 4;
                    break;
                case 'U':
                    minDig = maxDig = 8;
                    break;
                case 'x':
                    minDig = 1;
                    if (offset < length && UTF16.CharAt(s, offset) == 0x7B /*{*/)
                    {
                        ++offset;
                        braces = true;
                        maxDig = 8;
                    }
                    else
                    {
                        maxDig = 2;
                    }
                    break;
                default:
                    dig = UChar.Digit(c, 8);
                    if (dig >= 0)
                    {
                        minDig = 1;
                        maxDig = 3;
                        n = 1; /* Already have first octal digit */
                        bitsPerDigit = 3;
                        result = dig;
                    }
                    break;
            }
            if (minDig != 0)
            {
                while (offset < length && n < maxDig)
                {
                    c = UTF16.CharAt(s, offset);
                    dig = UChar.Digit(c, (bitsPerDigit == 3) ? 8 : 16);
                    if (dig < 0)
                    {
                        break;
                    }
                    result = (result << bitsPerDigit) | dig;
                    offset += UTF16.GetCharCount(c);
                    ++n;
                }
                if (n < minDig)
                {
                    return -1;
                }
                if (braces)
                {
                    if (c != 0x7D /*}*/)
                    {
                        return -1;
                    }
                    ++offset;
                }
                if (result < 0 || result >= 0x110000)
                {
                    return -1;
                }
                // If an escape sequence specifies a lead surrogate, see
                // if there is a trail surrogate after it, either as an
                // escape or as a literal.  If so, join them up into a
                // supplementary.
                if (offset < length &&
                        UTF16.IsLeadSurrogate((char)result))
                {
                    int ahead = offset + 1;
                    c = s[offset]; // [sic] get 16-bit code unit
                    if (c == '\\' && ahead < length)
                    {
                        c = UnescapeAt(s, ref ahead); // ICU4N: Changed array to ref parameter
                    }
                    if (UTF16.IsTrailSurrogate((char)c))
                    {
                        offset = ahead;
                        result = Character.ToCodePoint((char)result, (char)c);
                    }
                }
                offset16 = offset;
                return result;
            }

            /* Convert C-style escapes in table */
            for (i = 0; i < UNESCAPE_MAP.Length; i += 2)
            {
                if (c == UNESCAPE_MAP[i])
                {
                    offset16 = offset;
                    return UNESCAPE_MAP[i + 1];
                }
                else if (c < UNESCAPE_MAP[i])
                {
                    break;
                }
            }

            /* Map \cX to control-X: X & 0x1F */
            if (c == 'c' && offset < length)
            {
                c = UTF16.CharAt(s, offset);
                offset16 = offset + UTF16.GetCharCount(c);
                return 0x1F & c;
            }

            /* If no special forms are recognized, then consider
             * the backslash to generically escape the next character. */
            offset16 = offset;
            return c;
        }

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        /// <summary>
        /// Convert all escapes in a given string using <see cref="UnescapeAt(ReadOnlySpan{char}, ref int)"/>.
        /// </summary>
        /// <exception cref="ArgumentException">If an invalid escape is seen.</exception>
        // ICU4N: To fix lack of implicit conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string Unescape(string s)
            => Unescape(s.AsSpan());
#endif

        /// <summary>
        /// Convert all escapes in a given string using <see cref="UnescapeAt(ReadOnlySpan{char}, ref int)"/>.
        /// </summary>
        /// <exception cref="ArgumentException">If an invalid escape is seen.</exception>
        public static string Unescape(ReadOnlySpan<char> s)
        {
            ValueStringBuilder buf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Unescape(s, ref buf);
                return buf.ToString();
            }
            finally
            {
                buf.Dispose();
            }
        }

        /// <summary>
        /// Convert all escapes in a given string using <see cref="UnescapeAt(ReadOnlySpan{char}, ref int)"/>.
        /// </summary>
        /// <exception cref="ArgumentException">If an invalid escape is seen.</exception>
        internal static void Unescape(ReadOnlySpan<char> s, ref ValueStringBuilder result)
        {
            int pos;
            for (int i = 0; i < s.Length;)
            {
                char c = s[i++];
                if (c == '\\')
                {
                    pos = i;
                    int e = UnescapeAt(s, ref pos); // ICU4N: Changed array to ref parameter
                    if (e < 0)
                    {
                        throw new ArgumentException(StringHelper.Concat("Invalid escape sequence ".AsSpan(), s.Slice(i - 1, Math.Min(i + 8, s.Length) - (i - 1)))); // ICU4N: Corrected 2nd parameter
                    }
                    result.AppendCodePoint(e);
                    i = pos;
                }
                else
                {
                    result.Append(c);
                }
            }
        }

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        /// <summary>
        /// Convert all escapes in a given string using <see cref="UnescapeAt(ReadOnlySpan{char}, ref int)"/>.
        /// Leave invalid escape sequences unchanged.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string UnescapeLeniently(string s)
            => UnescapeLeniently(s.AsSpan());
#endif
        /// <summary>
        /// Convert all escapes in a given string using <see cref="UnescapeAt(ReadOnlySpan{char}, ref int)"/>.
        /// Leave invalid escape sequences unchanged.
        /// </summary>
        public static string UnescapeLeniently(ReadOnlySpan<char> s)
        {
            ValueStringBuilder buf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                UnescapeLeniently(s, ref buf);
                return buf.ToString();
            }
            finally
            {
                buf.Dispose();
            }
        }

        /// <summary>
        /// Convert all escapes in a given string using <see cref="UnescapeAt(ReadOnlySpan{char}, ref int)"/>.
        /// Leave invalid escape sequences unchanged.
        /// </summary>
        internal static void UnescapeLeniently(ReadOnlySpan<char> s, ref ValueStringBuilder result)
        {
            int pos;
            for (int i = 0; i < s.Length;)
            {
                char c = s[i++];
                if (c == '\\')
                {
                    //pos[0] = i;
                    pos = i;
                    int e = UnescapeAt(s, ref pos); // ICU4N: Changed array to ref parameter
                    if (e < 0)
                    {
                        result.Append(c);
                    }
                    else
                    {
                        result.AppendCodePoint(e);
                        i = pos;
                    }
                }
                else
                {
                    result.Append(c);
                }
            }
        }

        /// <summary>
        /// Convert a char to 4 hex uppercase digits.  E.g., hex('a') =>
        /// "0041".
        /// </summary>
        public static string Hex(long ch)
        {
            return Hex(ch, 4);
        }

#nullable enable

        private const string Int64MinHexValue = "-8000000000000000";

        /// <summary>
        /// Supplies a zero-padded hex representation of an integer (without 0x)
        /// </summary>
        public static string Hex(long i, int places) // ICU4N TODO: API - create overload that writes to Span<char> and use throughout ICU4N. Do not throw excpetions.
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                sb.AppendFormatHex(i, places);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Supplies a zero-padded hex representation of an integer (without 0x).
        /// </summary>
        /// <param name="i">The number to convert.</param>
        /// <param name="places">The number of places to pad the hexadecimal number to.</param>
        /// <param name="destination">Upon successful return, will contain the result of the conversion.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters
        /// that are usable in destination; otherwise, this is the length of buffer that will need to be allocated
        /// to succeed in another attempt.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        public static bool TryFormatHex(long i, int places, Span<char> destination, out int charsLength) // ICU4N specific so we don't have to use heap
        {
            ValueStringBuilder sb = new ValueStringBuilder(destination);
            try
            {
                sb.AppendFormatHex(i, places);
                return sb.FitsInitialBuffer(out charsLength);
            }
            finally
            {
                sb.Dispose();
            }
        }

        // ICU4N: Extracted business logic from Hex() so it can be used without allocating strings
        internal static void AppendFormatHex(this ref ValueStringBuilder destination, long i, int places)
        {
            if (i == long.MinValue)
            {
                destination.Append(Int64MinHexValue);
                return;
            }
            bool negative = i < 0;
            if (negative)
            {
                i = -i;
            }
            // ICU4N: Use built-in precision specifier instead of substring
            int length = places + (negative ? 1 : 0) + 16;
            bool usePool = length > CharStackBufferSize;
            char[]? arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(length) : null;
            try
            {
                Span<char> buffer = usePool ? arrayToReturnToPool : stackalloc char[length];
                buffer[0] = '-';

                Span<char> format = stackalloc char[16]; // ICU4N: This is more than enough for the longest positive integer
                format[0] = 'X';
                bool success = J2N.Numerics.Int32.TryFormat(places, format.Slice(1), out int intLength, provider: CultureInfo.InvariantCulture);
                if (!success)
                    throw new InvalidOperationException("Not enough characters in format."); // Unexpected

                success = J2N.Numerics.Int64.TryFormat(i, buffer.Slice(1), out int charsWritten, format.Slice(0, intLength + 1), CultureInfo.InvariantCulture);
                if (!success)
                    throw new InvalidOperationException("Not enough characters in buffer."); // Unexpected

                int start = 1, totalLength = charsWritten;
                if (negative)
                {
                    start -= 1;
                    totalLength += 1;
                }
                destination.Append(buffer.Slice(start, totalLength));
            }
            finally
            {
                ArrayPool<char>.Shared.ReturnIfNotNull(arrayToReturnToPool);
            }
        }

        /// <summary>
        /// Convert a string to comma-separated groups of 4 hex uppercase
        /// digits.  E.g., hex('ab') => "0041,0042".
        /// </summary>
        public static string Hex(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return Hex(s.AsSpan());
        }

        /// <summary>
        /// Convert a string to comma-separated groups of 4 hex uppercase
        /// digits.  E.g., hex('ab') => "0041,0042".
        /// </summary>
        public static string Hex(ReadOnlySpan<char> s)
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Hex(s, 4, ",".AsSpan(), true, ref sb);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Convert a string to separated groups of hex uppercase
        /// digits.  E.g., hex('ab'...) => "0041,0042".  Append the output
        /// to the given <see cref="IAppendable"/>.
        /// </summary>
        public static StringBuilder Hex(ReadOnlySpan<char> s, int width, ReadOnlySpan<char> separator, bool useCodePoints, StringBuilder result) // ICU4N TODO: Factor out and replace with Span<char> overload
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Hex(s, width, separator, useCodePoints, ref sb);
                return result.Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Convert a string to separated groups of hex uppercase
        /// digits.  E.g., hex('ab'...) => "0041,0042".  Append the output
        /// to the given <see cref="IAppendable"/>.
        /// </summary>
        public static T Hex<T>(ReadOnlySpan<char> s, int width, ReadOnlySpan<char> separator, bool useCodePoints, T result) where T : IAppendable // ICU4N TODO: Factor out and replace with Span<char> overload
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Hex(s, width, separator, useCodePoints, ref sb);
                return result.Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Convert a string to separated groups of hex uppercase
        /// digits.  E.g., hex('ab'...) => "0041,0042".  Append the output
        /// to the given <see cref="ValueStringBuilder"/>.
        /// </summary>
        internal static void Hex(ReadOnlySpan<char> s, int width, ReadOnlySpan<char> separator, bool useCodePoints, ref ValueStringBuilder result)
        {
            // ICU4N: Removed unnecessary try/catch
            if (useCodePoints)
            {
                int cp;
                for (int i = 0; i < s.Length; i += UTF16.GetCharCount(cp))
                {
                    cp = Character.CodePointAt(s, i);
                    if (i != 0)
                    {
                        result.Append(separator);
                    }
                    result.AppendFormatHex(cp, width);
                }
            }
            else
            {
                for (int i = 0; i < s.Length; ++i)
                {
                    if (i != 0)
                    {
                        result.Append(separator);
                    }
                    result.AppendFormatHex(s[i], width);
                }
            }
        }

        /// <summary>
        /// Convert a string to comma-separated groups of 4 hex uppercase
        /// digits.  E.g., hex('ab') => "0041,0042".
        /// </summary>
        public static string Hex(ReadOnlySpan<char> s, int width, ReadOnlySpan<char> separator)
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Hex(s, width, separator, true, ref sb);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }


#nullable restore

        /// <summary>
        /// Split a string into pieces based on the given <paramref name="divider"/> character
        /// </summary>
        /// <param name="s">The string to split.</param>
        /// <param name="divider">The character on which to split.  Occurrences of
        /// this character are not included in the output.</param>
        /// <param name="output">An array to receive the substrings between
        /// instances of divider.  It must be large enough on entry to
        /// accomodate all output.  Adjacent instances of the <paramref name="divider"/>
        /// character will place empty strings into output.  Before
        /// returning, output is padded out with empty strings.</param>
        public static void Split(string s, char divider, string[] output) // ICU4N TODO: API - factor out (we don't want to use string[])
        {
            int last = 0;
            int current = 0;
            int i;
            for (i = 0; i < s.Length; ++i)
            {
                if (s[i] == divider)
                {
                    output[current++] = s.Substring(last, i - last); // ICU4N: Corrected 2nd parameter
                    last = i + 1;
                }
            }
            output[current++] = s.Substring(last, i - last); // ICU4N: Corrected 2nd parameter
            while (current < output.Length)
            {
                output[current++] = "";
            }
        }

        /// <summary>
        /// Split a string into pieces based on the given <paramref name="divider"/> character.
        /// </summary>
        /// <param name="s">The string to split.</param>
        /// <param name="divider">The character on which to split.  Occurrences of
        /// this character are not included in the output.</param>
        /// <returns>An array of the substrings between
        /// instances of <paramref name="divider"/>. Adjacent instances of the <paramref name="divider"/>
        /// character will place empty strings into output.</returns>
        public static string[] Split(string s, char divider) // ICU4N TODO: API - factor out (we don't want to use string[])
        {
            int last = 0;
            int i;
            List<string> output = new List<string>();
            for (i = 0; i < s.Length; ++i)
            {
                if (s[i] == divider)
                {
                    output.Add(s.Substring(last, i - last)); // ICU4N: Corrected 2nd parameter
                    last = i + 1;
                }
            }
            output.Add(s.Substring(last, i - last)); // ICU4N: Corrected 2nd parameter
            return output.ToArray();
        }

        /// <summary>
        /// Look up a given string in a string array.  Returns the index at
        /// which the first occurrence of the string was found in the
        /// array, or -1 if it was not found.
        /// </summary>
        /// <param name="source">The string to search for.</param>
        /// <param name="target">The array of zero or more strings in which to
        /// look for source.</param>
        /// <returns>The index of target at which source first occurs, or -1
        /// if not found.</returns>
        public static int Lookup(string source, string[] target) // ICU4N TODO: API - factor out (we don't want to use string[])
        {
            for (int i = 0; i < target.Length; ++i)
            {
                if (source.Equals(target[i])) return i;
            }
            return -1;
        }

        /// <summary>
        /// Parse a single non-whitespace character '<paramref name="ch"/>', optionally
        /// preceded by whitespace.
        /// </summary>
        /// <param name="id">The string to be parsed.</param>
        /// <param name="pos">INPUT-OUTPUT parameter.  On input, <paramref name="pos"/> is the
        /// offset of the first character to be parsed.  On output, <paramref name="pos"/>
        /// is the index after the last parsed character.  If the parse
        /// fails, <paramref name="pos"/> will be unchanged.</param>
        /// <param name="ch">The non-whitespace character to be parsed.</param>
        /// <returns>true if '<paramref name="ch"/>' is seen preceded by zero or more
        /// whitespace characters.</returns>
        public static bool ParseChar(string id, ref int pos, char ch) // ICU4N: Changed pos from int[] to ref int
        {
            int start = pos;
            pos = PatternProps.SkipWhiteSpace(id, pos);
            if (pos == id.Length ||
                    id[pos] != ch)
            {
                pos = start;
                return false;
            }
            ++pos;
            return true;
        }

        /// <summary>
        /// Parse a pattern string starting at offset pos.  Keywords are
        /// matched case-insensitively.  Spaces may be skipped and may be
        /// optional or required.  Integer values may be parsed, and if
        /// they are, they will be returned in the given array.  If
        /// successful, the offset of the next non-space character is
        /// returned.  On failure, -1 is returned.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="pos"></param>
        /// <param name="limit"></param>
        /// <param name="pattern">Must only contain lowercase characters, which
        /// will match their uppercase equivalents as well.  A space
        /// character matches one or more required spaces.  A '~' character
        /// matches zero or more optional spaces.  A '#' character matches
        /// an integer and stores it in <paramref name="parsedInts"/>, which the caller must
        /// ensure has enough capacity.</param>
        /// <param name="parsedInts">Array to receive parsed integers.  Caller
        /// must ensure that parsedInts.Length is >= the number of '#'
        /// signs in 'pattern'.</param>
        /// <returns>The position after the last character parsed, or -1 if
        /// the parse failed.</returns>
        public static int ParsePattern(string rule, int pos, int limit, // ICU4N TODO: API Make limit into length, like in .NET ?
                string pattern, int[] parsedInts)
        {
            // TODO Update this to handle surrogates
            int p; // ICU4N: Converted from array to int
            int intCount = 0; // number of integers parsed
            for (int i = 0; i < pattern.Length; ++i)
            {
                char cpat = pattern[i];
                char c;
                switch (cpat)
                {
                    case ' ':
                        if (pos >= limit)
                        {
                            return -1;
                        }
                        c = rule[pos++];
                        if (!PatternProps.IsWhiteSpace(c))
                        {
                            return -1;
                        }
                        // FALL THROUGH to skipWhitespace
                        pos = PatternProps.SkipWhiteSpace(rule, pos);
                        break;
                    case '~':
                        pos = PatternProps.SkipWhiteSpace(rule, pos);
                        break;
                    case '#':
                        p = pos;
                        parsedInts[intCount++] = ParseInteger(rule, ref p, limit);
                        if (p == pos)
                        {
                            // Syntax error; failed to parse integer
                            return -1;
                        }
                        pos = p;
                        break;
                    default:
                        if (pos >= limit)
                        {
                            return -1;
                        }
                        c = (char)UChar.ToLower(rule[pos++]);
                        if (c != cpat)
                        {
                            return -1;
                        }
                        break;
                }
            }
            return pos;
        }

        /// <summary>
        /// Parse a pattern string within the given <see cref="IReplaceable"/> and a parsing
        /// pattern.  Characters are matched literally and case-sensitively
        /// except for the following special characters:
        /// <code>
        /// ~  zero or more Pattern_White_Space chars
        /// </code>
        /// If end of pattern is reached with all matches along the way,
        /// pos is advanced to the first unparsed index and returned.
        /// Otherwise -1 is returned.
        /// </summary>
        /// <param name="pat">Pattern that controls parsing.</param>
        /// <param name="text">Text to be parsed, starting at index.</param>
        /// <param name="index">Offset to first character to parse.</param>
        /// <param name="limit">Offset after last character to parse.</param>
        /// <returns>Index after last parsed character, or -1 on parse failure.</returns>
        public static int ParsePattern(string pat,
                IReplaceable text,
                int index,
                int limit) // ICU4N TODO: API Make limit into length, like in .NET ?
        {
            int ipat = 0;

            // empty pattern matches immediately
            if (ipat == pat.Length)
            {
                return index;
            }

            int cpat = Character.CodePointAt(pat, ipat);

            while (index < limit)
            {
                int c = text.Char32At(index);

                // parse \s*
                if (cpat == '~')
                {
                    if (PatternProps.IsWhiteSpace(c))
                    {
                        index += UTF16.GetCharCount(c);
                        continue;
                    }
                    else
                    {
                        if (++ipat == pat.Length)
                        {
                            return index; // success; c unparsed
                        }
                        // fall thru; process c again with next cpat
                    }
                }

                // parse literal
                else if (c == cpat)
                {
                    int n = UTF16.GetCharCount(c);
                    index += n;
                    ipat += n;
                    if (ipat == pat.Length)
                    {
                        return index; // success; c parsed
                    }
                    // fall thru; get next cpat
                }

                // match failure of literal
                else
                {
                    return -1;
                }

                cpat = UTF16.CharAt(pat, ipat);
            }

            return -1; // text ended before end of pat
        }

        /// <summary>
        /// Parse an integer at pos, either of the form \d+ or of the form
        /// 0x[0-9A-Fa-f]+ or 0[0-7]+, that is, in standard decimal, hex,
        /// or octal format.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="pos">INPUT-OUTPUT parameter.  On input, the first
        /// character to parse.  On output, the character after the last
        /// parsed character.</param>
        /// <param name="limit"></param>
        // ICU4N: Converted pos from array to ref int
        public static int ParseInteger(string rule, ref int pos, int limit) // ICU4N TODO: API Make limit into length, like in .NET ?
        {
            int count = 0;
            int value = 0;
            int p = pos;
            int radix = 10;

            //if (rule.RegionMatches(/*true,*/ p, "0x", 0, 2, StringComparison.OrdinalIgnoreCase))
            if (rule.Length >= p + 1 && rule[p] == '0' && (rule[p + 1] == 'x' || rule[p + 1] == 'X'))
            {
                p += 2;
                radix = 16;
            }
            else if (p < limit && rule[p] == '0')
            {
                p++;
                count = 1;
                radix = 8;
            }

            while (p < limit)
            {
                int d = UChar.Digit(rule[p++], radix);
                if (d < 0)
                {
                    --p;
                    break;
                }
                ++count;
                int v = (value * radix) + d;
                if (v <= value)
                {
                    // If there are too many input digits, at some point
                    // the value will go negative, e.g., if we have seen
                    // "0x8000000" already and there is another '0', when
                    // we parse the next 0 the value will go negative.
                    return 0;
                }
                value = v;
            }
            if (count > 0)
            {
                pos = p;
            }
            return value;
        }

        /// <summary>
        /// Parse a Unicode identifier from the given string at the given
        /// position.  Return the identifier, or null if there is no
        /// identifier.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="pos">INPUT-OUPUT parameter.  On INPUT, <paramref name="pos"/> is the
        /// first character to examine.  It must be less than str.Length,
        /// and it must not point to a whitespace character.  That is, must
        /// have <paramref name="pos"/> &lt; str.Length.  On
        /// OUTPUT, the position after the last parsed character.</param>
        /// <returns>The Unicode identifier, or null if there is no valid
        /// identifier at <paramref name="pos"/>.</returns>
        // ICU4N: Converted pos from int[] to ref int
        public static string ParseUnicodeIdentifier(string str, ref int pos)
        {
            // assert(pos[0] < str.length());
            StringBuilder buf = new StringBuilder();
            int p = pos;
            while (p < str.Length)
            {
                int ch = Character.CodePointAt(str, p);
                if (buf.Length == 0)
                {
                    if (UChar.IsUnicodeIdentifierStart(ch))
                    {
                        buf.AppendCodePoint(ch);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (UChar.IsUnicodeIdentifierPart(ch))
                    {
                        buf.AppendCodePoint(ch);
                    }
                    else
                    {
                        break;
                    }
                }
                p += UTF16.GetCharCount(ch);
            }
            pos = p;
            return buf.ToString();
        }

        private static readonly char[] DIGITS = new char[] {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        // ICU4N: Factored out RecursiveAppendNumber(IAppendable result, int n,
        //    int radix, int minDigits) and loop inside AppendNumber() instead

        /// <summary>
        /// Append a number to the given <see cref="StringBuilder"/> in the given radix.
        /// Standard digits '0'-'9' are used and letters 'A'-'Z' for
        /// radices 11 through 36.
        /// </summary>
        /// <param name="result">The digits of the number are appended here.</param>
        /// <param name="n">The number to be converted to digits; may be negative. If negative, a '-' is prepended to the digits.</param>
        /// <param name="radix">A radix from 2 to 36 inclusive.</param>
        /// <param name="minDigits">
        /// The minimum number of digits, not including
        /// any '-', to produce.  Values less than 2 have no effect.  One
        /// digit is always emitted regardless of this parameter.
        /// </param>
        /// <returns>A reference to result.</returns>
        // ICU4N: Refactored to eliminate recursion to keep the stack size small
        internal static void AppendNumber(this ref ValueStringBuilder result, int n,
            int radix, int minDigits)
        {
            if (radix < 2 || radix > 36)
            {
                throw new ArgumentException("Illegal radix " + radix);
            }
            int abs = n;
            if (n < 0)
            {
                abs = -n;
                result.Append('-');
            }
            // Pre-count the amount to allocate
            int count = 1;
            while ((n /= radix) != 0)
                count++;
            if (count < minDigits)
                count = minDigits;

            Span<char> buffer = result.AppendSpan(count);

            // Append the actual number
            do
            {
                int digit = abs % radix;
                buffer[--count] = DIGITS[digit];

            } while ((abs /= radix) != 0);

            // Fill any remaining space with zeros
            while (--count >= 0)
                buffer[count] = '0';
        }

        /// <summary>
        /// Parse an unsigned 31-bit integer at the given offset.  Use
        /// <see cref="UChar.Digit(int, int)"/> to parse individual characters into digits.
        /// </summary>
        /// <param name="text">The text to be parsed.</param>
        /// <param name="pos">INPUT-OUTPUT parameter.  On entry, <paramref name="pos"/> is the
        /// offset within text at which to start parsing; it should point
        /// to a valid digit.  On exit, <paramref name="pos"/> is the offset after the last
        /// parsed character.  If the parse failed, it will be unchanged on
        /// exit.  Must be >= 0 on entry.</param>
        /// <param name="radix">The radix in which to parse; must be >= 2 and &lt;= 36.</param>
        /// <returns>A non-negative parsed number, or -1 upon parse failure.
        /// Parse fails if there are no digits, that is, if <paramref name="pos"/> does not
        /// point to a valid digit on entry, or if the number to be parsed
        /// does not fit into a 31-bit unsigned integer.</returns>
        // ICU4N: Converted pos from int[] to ref int
        public static int ParseNumber(string text, ref int pos, int radix)
        {
            // assert(pos[0] >= 0);
            // assert(radix >= 2);
            // assert(radix <= 36);
            int n = 0;
            int p = pos;
            while (p < text.Length)
            {
                int ch = Character.CodePointAt(text, p);
                int d = UChar.Digit(ch, radix);
                if (d < 0)
                {
                    break;
                }
                n = radix * n + d;
                // ASSUME that when a 32-bit integer overflows it becomes
                // negative.  E.g., 214748364 * 10 + 8 => negative value.
                if (n < 0)
                {
                    return -1;
                }
                ++p;
            }
            if (p == pos)
            {
                return -1;
            }
            pos = p;
            return n;
        }

        /// <summary>
        /// Return true if the character is NOT printable ASCII.  The tab,
        /// newline and linefeed characters are considered unprintable.
        /// </summary>
        public static bool IsUnprintable(int c)
        {
            //0x20 = 32 and 0x7E = 126
            return !(c >= 0x20 && c <= 0x7E);
        }

        /// <summary>
        /// Escape unprintable characters using \uxxxx notation
        /// for U+0000 to U+FFFF and \Uxxxxxxxx for U+10000 and
        /// above. If the character is printable ASCII, then do nothing
        /// and return FALSE. Otherwise, append the escaped notation and
        /// return TRUE.
        /// </summary>
        public static bool EscapeUnprintable(StringBuilder result, int c) // ICU4N TODO: API - Rename TryEscapeUnprintable, since this does nothing if it fails
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                bool success = EscapeUnprintable(ref sb, c);
                result.Append(sb.AsSpan());
                return success;
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Escape unprintable characters using \uxxxx notation
        /// for U+0000 to U+FFFF and \Uxxxxxxxx for U+10000 and
        /// above. If the character is printable ASCII, then do nothing
        /// and return FALSE. Otherwise, append the escaped notation and
        /// return TRUE.
        /// </summary>
        public static bool EscapeUnprintable(IAppendable result, int c) // ICU4N TODO: API - Rename TryEscapeUnprintable, since this does nothing if it fails
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                bool success = EscapeUnprintable(ref sb, c);
                result.Append(sb.AsSpan());
                return success;
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Escape unprintable characters using \uxxxx notation
        /// for U+0000 to U+FFFF and \Uxxxxxxxx for U+10000 and
        /// above. If the character is printable ASCII, then do nothing
        /// and return FALSE. Otherwise, append the escaped notation and
        /// return TRUE.
        /// </summary>
        internal static bool EscapeUnprintable(ref ValueStringBuilder result, int c) // ICU4N TODO: API - Rename TryEscapeUnprintable, since this does nothing if it fails
        {
            // ICU4N TODO: API - Make an overload named Escape that writes to a Span<char> (6 chars).
            // ICU4N: Removed unnecessary try/catch
            if (IsUnprintable(c))
            {
                result.Append('\\');
                if ((c & ~0xFFFF) != 0)
                {
                    result.Append('U');
                    result.Append(DIGITS[0xF & (c >> 28)]);
                    result.Append(DIGITS[0xF & (c >> 24)]);
                    result.Append(DIGITS[0xF & (c >> 20)]);
                    result.Append(DIGITS[0xF & (c >> 16)]);
                }
                else
                {
                    result.Append('u');
                }
                result.Append(DIGITS[0xF & (c >> 12)]);
                result.Append(DIGITS[0xF & (c >> 8)]);
                result.Append(DIGITS[0xF & (c >> 4)]);
                result.Append(DIGITS[0xF & c]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the index of the first character in a set, ignoring quoted text.
        /// For example, in the string "abc'hide'h", the 'h' in "hide" will not be
        /// found by a search for "h".  Unlike <see cref="string.IndexOf(char, int, int)"/>, this method searches
        /// not for a single character, but for any character of the string <paramref name="setOfChars"/>.
        /// </summary>
        /// <param name="text">Text to be searched.</param>
        /// <param name="setOfChars">String with one or more distinct characters.</param>
        /// <returns>Offset of the first character in <paramref name="setOfChars"/>
        /// found, or -1 if not found.</returns>
        /// <seealso cref="string.IndexOf(char, int, int)"/>
        public static int QuotedIndexOf(ReadOnlySpan<char> text, ReadOnlySpan<char> setOfChars)
        {
            int limit = text.Length;
            for (int i = 0; i < limit; ++i)
            {
                char c = text[i];
                if (c == BACKSLASH)
                {
                    ++i;
                }
                else if (c == APOSTROPHE)
                {
                    while (++i < limit
                            && text[i] != APOSTROPHE) { }
                }
                else if (setOfChars.IndexOf(c) >= 0)
                {
                    return i;
                }
            }
            return -1;
        }

#nullable enable

        /// <summary>
        /// Append a character to a rule that is being built up.  To flush
        /// the <paramref name="quoteBuf"/> to <paramref name="rule"/>, make one final call with <paramref name="isLiteral"/> == true.
        /// If there is no final character, pass in (int)-1 as <paramref name="c"/>.
        /// </summary>
        /// <param name="rule">The string to append the character to.</param>
        /// <param name="c">The character to append, or (int)-1 if none.</param>
        /// <param name="isLiteral">If true, then the given character should not be
        /// quoted or escaped.  Usually this means it is a syntactic element
        /// such as > or $.</param>
        /// <param name="escapeUnprintable">If true, then unprintable characters
        /// should be escaped using <c>EscapeUnprintable(ref ValueStringBuilder, int)</c>. These escapes will
        /// appear outside of quotes.</param>
        /// <param name="quoteBuf">A buffer which is used to build up quoted
        /// substrings.  The caller should initially supply an empty buffer,
        /// and thereafter should not modify the buffer.  The buffer should be
        /// cleared out by, at the end, calling this method with a literal
        /// character (which may be -1).</param>
        internal static void AppendToRule(ref ValueStringBuilder rule,
                int c,
                bool isLiteral,
                bool escapeUnprintable,
                ref ValueStringBuilder quoteBuf)
        {
            // If we are escaping unprintables, then escape them outside
            // quotes.  \\u and \\U are not recognized within quotes.  The same
            // logic applies to literals, but literals are never escaped.
            if (isLiteral ||
                    (escapeUnprintable && Utility.IsUnprintable(c)))
            {
                if (quoteBuf.Length > 0)
                {
                    // We prefer backslash APOSTROPHE to double APOSTROPHE
                    // (more readable, less similar to ") so if there are
                    // double APOSTROPHEs at the ends, we pull them outside
                    // of the quote.

                    // If the first thing in the quoteBuf is APOSTROPHE
                    // (doubled) then pull it out.
                    while (quoteBuf.Length >= 2 &&
                            quoteBuf[0] == APOSTROPHE &&
                            quoteBuf[1] == APOSTROPHE)
                    {
                        rule.Append(BACKSLASH);
                        rule.Append(APOSTROPHE);
                        quoteBuf.Delete(0, 2 - 0); // ICU4N: Corrected 2nd parameter
                    }
                    // If the last thing in the quoteBuf is APOSTROPHE
                    // (doubled) then remove and count it and add it after.
                    int trailingCount = 0;
                    while (quoteBuf.Length >= 2 &&
                            quoteBuf[quoteBuf.Length - 2] == APOSTROPHE &&
                            quoteBuf[quoteBuf.Length - 1] == APOSTROPHE)
                    {
                        quoteBuf.Length = quoteBuf.Length - 2;
                        ++trailingCount;
                    }
                    if (quoteBuf.Length > 0)
                    {
                        rule.Append(APOSTROPHE);
                        rule.Append(quoteBuf.AsSpan());
                        rule.Append(APOSTROPHE);
                        quoteBuf.Length = 0;
                    }
                    while (trailingCount-- > 0)
                    {
                        rule.Append(BACKSLASH);
                        rule.Append(APOSTROPHE);
                    }
                }
                if (c != -1)
                {
                    /* Since spaces are ignored during parsing, they are
                     * emitted only for readability.  We emit one here
                     * only if there isn't already one at the end of the
                     * rule.
                     */
                    if (c == ' ')
                    {
                        int len = rule.Length;
                        if (len > 0 && rule[len - 1] != ' ')
                        {
                            rule.Append(' ');
                        }
                    }
                    else if (!escapeUnprintable || !Utility.EscapeUnprintable(ref rule, c))
                    {
                        rule.AppendCodePoint(c);
                    }
                }
            }

            // Escape ' and '\' and don't begin a quote just for them
            else if (quoteBuf.Length == 0 &&
                    (c == APOSTROPHE || c == BACKSLASH))
            {
                rule.Append(BACKSLASH);
                rule.Append((char)c);
            }

            // Specials (printable ascii that isn't [0-9a-zA-Z]) and
            // whitespace need quoting.  Also append stuff to quotes if we are
            // building up a quoted substring already.
            else if (quoteBuf.Length > 0 ||
                    (c >= 0x0021 && c <= 0x007E &&
                            !((c >= 0x0030/*'0'*/ && c <= 0x0039/*'9'*/) ||
                                    (c >= 0x0041/*'A'*/ && c <= 0x005A/*'Z'*/) ||
                                    (c >= 0x0061/*'a'*/ && c <= 0x007A/*'z'*/))) ||
                                    PatternProps.IsWhiteSpace(c))
            {
                quoteBuf.AppendCodePoint(c);
                // Double ' within a quote
                if (c == APOSTROPHE)
                {
                    quoteBuf.Append((char)c);
                }
            }

            // Otherwise just append
            else
            {
                rule.AppendCodePoint(c);
            }
        }

        /// <summary>
        /// Append the given string to the rule.  Calls the single-character
        /// version of <c>AppendToRule(ref ValueStringBuilder, int, bool, bool, ref ValueStringBuilder)</c> for each character.
        /// </summary>
        internal static void AppendToRule(ref ValueStringBuilder rule,
                scoped ReadOnlySpan<char> text,
                bool isLiteral,
                bool escapeUnprintable,
                ref ValueStringBuilder quoteBuf)
        {
            for (int i = 0; i < text.Length; ++i)
            {
                // Okay to process in 16-bit code units here
                AppendToRule(ref rule, text[i], isLiteral, escapeUnprintable, ref quoteBuf);
            }
        }

        /// <summary>
        /// Given a matcher reference, which may be null, append its
        /// pattern as a literal to the given rule.
        /// </summary>
        internal static void AppendToRule(ref ValueStringBuilder rule,
                IUnicodeMatcher? matcher,
                bool escapeUnprintable,
                ref ValueStringBuilder quoteBuf)
        {
            if (matcher != null)
            {
                char[]? matcherPatternArray = null;
                try
                {
                    Span<char> matcherPattern = stackalloc char[CharStackBufferSize];
                    if (!matcher.TryToPattern(escapeUnprintable, matcherPattern, out int matcherPatternLength))
                    {
                        // Not enough buffer, use the array pool
                        matcherPattern = matcherPatternArray = ArrayPool<char>.Shared.Rent(matcherPatternLength);
                        bool success = matcher.TryToPattern(escapeUnprintable, matcherPattern, out matcherPatternLength);
                        Debug.Assert(success); // Unexpected
                    }
                    AppendToRule(ref rule, matcherPattern.Slice(0, matcherPatternLength),
                        true, escapeUnprintable, ref quoteBuf);
                }
                finally
                {
                    ArrayPool<char>.Shared.ReturnIfNotNull(matcherPatternArray);
                }
            }
        }

#nullable restore


        /// <summary>
        /// Compares 2 unsigned integers.
        /// </summary>
        /// <param name="source">32 bit unsigned integer.</param>
        /// <param name="target">32 bit unsigned integer.</param>
        /// <returns>0 if equals, 1 if source is greater than target and -1 otherwise.</returns>
        public static int CompareUnsigned(int source, int target)
        {
            source += MAGIC_UNSIGNED;
            target += MAGIC_UNSIGNED;
            if (source < target)
            {
                return -1;
            }
            else if (source > target)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Find the highest bit in a positive integer. This is done
        /// by doing a binary search through the bits.
        /// </summary>
        /// <remarks>
        /// Return type changed from byte in ICU4J to short in ICU4N because it can
        /// have negative values, which <see cref="byte"/> doesn't support in .NET, and 
        /// <see cref="sbyte"/> is not CLS compliant.
        /// </remarks>
        /// <param name="n">The integer.</param>
        /// <returns>the bit number of the highest bit, with 0 being
        /// the low order bit, or -1 if <paramref name="n"/> is not positive.</returns>
        public static short HighBit(int n)
        {
            if (n <= 0)
            {
                return -1;
            }

            byte bit = 0;

            if (n >= 1 << 16)
            {
                n >>= 16;
                bit += 16;
            }

            if (n >= 1 << 8)
            {
                n >>= 8;
                bit += 8;
            }

            if (n >= 1 << 4)
            {
                n >>= 4;
                bit += 4;
            }

            if (n >= 1 << 2)
            {
                n >>= 2;
                bit += 2;
            }

            if (n >= 1 << 1)
            {
                n >>= 1;
                bit += 1;
            }

            return bit;
        }

        /// <summary>
        /// Utility method to take a <see cref="T:int[]"/> containing codepoints and return
        /// a string representation with code units.
        /// </summary>
        public static string ValueOf(int[] source)
        {
            // TODO: Investigate why this method is not on UTF16 class
            return Character.ToString(source);
        }

        /// <summary>
        /// Utility to duplicate a string count times.
        /// </summary>
        /// <param name="s">String to be duplicated.</param>
        /// <param name="count">Number of times to duplicate a string.</param>
        public static string Repeat(string s, int count)
        {
            if (count <= 0) return "";
            if (count == 1) return s;
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < count; ++i)
            {
                result.Append(s);
            }
            return result.ToString();
        }

        public static string[] SplitString(string src, string target)
        {
            return Regex.Split(src, "\\Q" + target + "\\E"); // ICU4N TODO: This is broken on .NET (\Q and \E are not supported)
        }

        /// <summary>
        /// Split the string at runs of ascii whitespace characters.
        /// </summary>
        public static string[] SplitWhitespace(string src)
        {
            return Regex.Split(src, "\\s+");
        }

        /// <summary>
        /// Parse a list of hex numbers and return a string.
        /// </summary>
        /// <param name="str">String of hex numbers.</param>
        /// <param name="minLength">Minimal length.</param>
        /// <param name="separator">Separator.</param>
        /// <returns>A string from hex numbers.</returns>
        public static string FromHex(string str, int minLength, string separator)
        {
            return FromHex(str, minLength, new Regex(separator ?? "\\s+"));
        }

        /// <summary>
        /// Parse a list of hex numbers and return a string.
        /// </summary>
        /// <param name="str">String of hex numbers.</param>
        /// <param name="minLength">Minimal length.</param>
        /// <param name="separator">Separator.</param>
        /// <returns>A string from hex numbers.</returns>
        public static string FromHex(string str, int minLength, Regex separator)
        {
            StringBuilder buffer = new StringBuilder();
            string[] parts = separator.Split(str);
            foreach (string part in parts)
            {
                if (part.Length < minLength)
                {
                    throw new ArgumentException("code point too short: " + part);
                }
                int cp = int.Parse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                buffer.AppendCodePoint(cp);
            }
            return buffer.ToString();
        }

        ///// <summary>
        ///// This implementation is equivalent to Java 7+ Objects.equals(Object a, Object b).
        ///// Note this compares the values in any nested collections.
        ///// </summary>
        ///// <param name="a">An object.</param>
        ///// <param name="b">An object to be compared with a for equality.</param>
        ///// <returns>true if the arguments are equal to each other and false otherwise.</returns>
        //new public static bool Equals(object a, object b) // ICU4N TODO: Fix every collection so we don't need aggressive mode.
        //{
        //    return (a == b)
        //            || (a != null && b != null && StructuralEqualityComparer.Aggressive.Equals(a, b));
        //}

        ///// <summary>
        ///// This implementation is equivalent to Java 7+ Objects.hash(Object... values).
        ///// Note this takes into consideration the values in any nested collections.
        ///// </summary>
        ///// <param name="values">The values to be hashed.</param>
        ///// <returns>A hash value of the sequence of input values.</returns>
        //public static int Hash(params object[] values) // ICU4N TODO: Fix every collection so we don't need aggressive mode.
        //{
        //    //return Arrays.hashCode(values);
        //    if (values == null)
        //    {
        //        return 0;
        //    }
        //    int hashCode = 1;
        //    foreach (object element in values)
        //    {
        //        int elementHashCode;

        //        if (element == null)
        //        {
        //            elementHashCode = 0;
        //        }
        //        else
        //        {
        //            elementHashCode = StructuralEqualityComparer.Aggressive.GetHashCode(element);
        //        }
        //        hashCode = 31 * hashCode + elementHashCode;
        //    }
        //    return hashCode;
        //}

        //    /// <summary>
        //    /// This implementation is equivalent to Java 7+ Objects.hashCode(Object o).
        //    /// Note this takes into consideration the values in any nested collections.
        //    /// </summary>
        //    /// <param name="o">An object.</param>
        //    /// <returns>A hash value of a non-null argument and 0 for null argument.</returns>
        //    public static int GetHashCode(object o) // ICU4N TODO: Fix every collection so we don't need aggressive mode (or eliminate this method)
        //    {
        //        return o == null ? 0 : StructuralEqualityComparer.Aggressive.GetHashCode(o);
        //    }

        //    /// <summary>
        //    /// This implementation is equivalent to Java 7+ Objects.toString(Object o).
        //    /// Note this takes into consideration the values in any nested collections.
        //    /// </summary>
        //    /// <param name="o">An object.</param>
        //    /// <returns>the result of calling <see cref="Support.Collections.CollectionUtil.ToString(object)"/> for a non-null argument and "null" for a
        //    /// null argument.</returns>
        //    public static string ToString(object o)
        //    {
        //        return o == null ? "null" : string.Format(StringFormatter.CurrentCulture, "{0}", o);
        //    }
    }
}
