﻿using ICU4N.Impl;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;

namespace ICU4N.Text
{
    internal class BytesDictionaryMatcher : DictionaryMatcher
    {
        private readonly byte[] characters;
        private readonly int transform;

        public BytesDictionaryMatcher(byte[] chars, int transform)
        {
            characters = chars;
            Assert.Assrt((transform & DictionaryData.TRANSFORM_TYPE_MASK) == DictionaryData.TRANSFORM_TYPE_OFFSET);
            // while there is only one transform type so far, save the entire transform constant so that
            // if we add any others, we need only change code in transform() and the assert above rather
            // than adding a "transform type" variable
            this.transform = transform;
        }

        private int Transform(int c)
        {
            if (c == 0x200D)
            {
                return 0xFF;
            }
            else if (c == 0x200C)
            {
                return 0xFE;
            }

            int delta = c - (transform & DictionaryData.TRANSFORM_OFFSET_MASK);
            if (delta < 0 || 0xFD < delta)
            {
                return -1;
            }
            return delta;
        }

        // ICU4N: Changed count parameter from int[] to out int
        public override int Matches(CharacterIterator text_, int maxLength, Span<int> lengths, out int count, int limit, Span<int> values)
        {
            count = 0;
            UCharacterIterator text = UCharacterIterator.GetInstance(text_);
            BytesTrie bt = new BytesTrie(characters, 0);
            int c = text.NextCodePoint();
            if (c == UCharacterIterator.Done)
            {
                return 0;
            }
            Result result = bt.First(Transform(c));
            // TODO: should numChars count Character.charCount() ?
            int numChars = 1;
            for (; ; )
            {
                if (result.HasValue())
                {
                    if (count < limit)
                    {
                        if (!values.IsEmpty)
                        {
                            values[count] = bt.GetValue();
                        }
                        lengths[count] = numChars;
                        count++;
                    }
                    if (result == Result.FinalValue)
                    {
                        break;
                    }
                }
                else if (result == Result.NoMatch)
                {
                    break;
                }

                if (numChars >= maxLength)
                {
                    break;
                }

                c = text.NextCodePoint();
                if (c == UCharacterIterator.Done)
                {
                    break;
                }
                ++numChars;
                result = bt.Next(Transform(c));
            }
            return numChars;
        }

        public override int Type => DictionaryData.TRIE_TYPE_BYTES;
    }
}
