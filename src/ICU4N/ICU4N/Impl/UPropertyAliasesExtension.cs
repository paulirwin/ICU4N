﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using ICU4N.Lang;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Text;

namespace ICU4N.Impl
{
    public sealed partial class UPropertyAliases
    {
        private bool ContainsName(BytesTrie trie, string name)
        {
            BytesTrieResult result = BytesTrieResult.NO_VALUE;
            for (int i = 0; i < name.Length; ++i)
            {
                int c = name[i];
                // Ignore delimiters '-', '_', and ASCII White_Space.
                if (c == '-' || c == '_' || c == ' ' || (0x09 <= c && c <= 0x0d))
                {
                    continue;
                }
                if (!result.HasNext())
                {
                    return false;
                }
                c = AsciiToLowercase(c);
                result = trie.Next(c);
            }
            return result.HasValue();
        }

        private bool ContainsName(BytesTrie trie, StringBuilder name)
        {
            BytesTrieResult result = BytesTrieResult.NO_VALUE;
            for (int i = 0; i < name.Length; ++i)
            {
                int c = name[i];
                // Ignore delimiters '-', '_', and ASCII White_Space.
                if (c == '-' || c == '_' || c == ' ' || (0x09 <= c && c <= 0x0d))
                {
                    continue;
                }
                if (!result.HasNext())
                {
                    return false;
                }
                c = AsciiToLowercase(c);
                result = trie.Next(c);
            }
            return result.HasValue();
        }

        private bool ContainsName(BytesTrie trie, char[] name)
        {
            BytesTrieResult result = BytesTrieResult.NO_VALUE;
            for (int i = 0; i < name.Length; ++i)
            {
                int c = name[i];
                // Ignore delimiters '-', '_', and ASCII White_Space.
                if (c == '-' || c == '_' || c == ' ' || (0x09 <= c && c <= 0x0d))
                {
                    continue;
                }
                if (!result.HasNext())
                {
                    return false;
                }
                c = AsciiToLowercase(c);
                result = trie.Next(c);
            }
            return result.HasValue();
        }

        private bool ContainsName(BytesTrie trie, ICharSequence name)
        {
            BytesTrieResult result = BytesTrieResult.NO_VALUE;
            for (int i = 0; i < name.Length; ++i)
            {
                int c = name[i];
                // Ignore delimiters '-', '_', and ASCII White_Space.
                if (c == '-' || c == '_' || c == ' ' || (0x09 <= c && c <= 0x0d))
                {
                    continue;
                }
                if (!result.HasNext())
                {
                    return false;
                }
                c = AsciiToLowercase(c);
                result = trie.Next(c);
            }
            return result.HasValue();
        }

        private int GetPropertyOrValueEnum(int bytesTrieOffset, string alias)
        {
            BytesTrie trie = new BytesTrie(bytesTries, bytesTrieOffset);
            if (ContainsName(trie, alias))
            {
                return trie.GetValue();
            }
            else
            {
#pragma warning disable 612, 618
                return (int)UProperty.UNDEFINED;
#pragma warning restore 612, 618
            }
        }

        private int GetPropertyOrValueEnum(int bytesTrieOffset, StringBuilder alias)
        {
            BytesTrie trie = new BytesTrie(bytesTries, bytesTrieOffset);
            if (ContainsName(trie, alias))
            {
                return trie.GetValue();
            }
            else
            {
#pragma warning disable 612, 618
                return (int)UProperty.UNDEFINED;
#pragma warning restore 612, 618
            }
        }

        private int GetPropertyOrValueEnum(int bytesTrieOffset, char[] alias)
        {
            BytesTrie trie = new BytesTrie(bytesTries, bytesTrieOffset);
            if (ContainsName(trie, alias))
            {
                return trie.GetValue();
            }
            else
            {
#pragma warning disable 612, 618
                return (int)UProperty.UNDEFINED;
#pragma warning restore 612, 618
            }
        }

        private int GetPropertyOrValueEnum(int bytesTrieOffset, ICharSequence alias)
        {
            BytesTrie trie = new BytesTrie(bytesTries, bytesTrieOffset);
            if (ContainsName(trie, alias))
            {
                return trie.GetValue();
            }
            else
            {
#pragma warning disable 612, 618
                return (int)UProperty.UNDEFINED;
#pragma warning restore 612, 618
            }
        }


        //----------------------------------------------------------------
        // Public API

        /// <summary>
        /// Returns a property enum given one of its property names.
        /// If the property name is not known, this method returns
        /// <see cref="UProperty.UNDEFINED"/>.
        /// </summary>
        public int GetPropertyEnum(string alias)
        {
            return GetPropertyOrValueEnum(0, alias);
        }

        /// <summary>
        /// Returns a property enum given one of its property names.
        /// If the property name is not known, this method returns
        /// <see cref="UProperty.UNDEFINED"/>.
        /// </summary>
        public int GetPropertyEnum(StringBuilder alias)
        {
            return GetPropertyOrValueEnum(0, alias);
        }

        /// <summary>
        /// Returns a property enum given one of its property names.
        /// If the property name is not known, this method returns
        /// <see cref="UProperty.UNDEFINED"/>.
        /// </summary>
        public int GetPropertyEnum(char[] alias)
        {
            return GetPropertyOrValueEnum(0, alias);
        }

        /// <summary>
        /// Returns a property enum given one of its property names.
        /// If the property name is not known, this method returns
        /// <see cref="UProperty.UNDEFINED"/>.
        /// </summary>
        internal int GetPropertyEnum(ICharSequence alias)
        {
            return GetPropertyOrValueEnum(0, alias);
        }
		
		/// <summary>
        /// Returns a value enum given a property enum and one of its value names.
        /// </summary>
		/// <seealso cref="TryGetPropertyValueEnum(UProperty, string, out int)"/>
		public int GetPropertyValueEnum(UProperty property, string alias)
        {
            int valueMapIndex = FindProperty((int)property);
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Invalid property enum " + property + " (0x" + string.Format("{0:x2}", (int)property) + ")");
            }
            valueMapIndex = valueMaps[valueMapIndex + 1];
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Property " + property + " (0x" + string.Format("{0:x2}", (int)property) +
                        ") does not have named values");
            }
            // valueMapIndex is the start of the property's valueMap,
            // where the first word is the BytesTrie offset.
            return GetPropertyOrValueEnum(valueMaps[valueMapIndex], alias);
        }
		
		/// <summary>
        /// Returns a value enum given a property enum and one of its value names.
        /// </summary>
		/// <seealso cref="TryGetPropertyValueEnum(UProperty, StringBuilder, out int)"/>
		public int GetPropertyValueEnum(UProperty property, StringBuilder alias)
        {
            int valueMapIndex = FindProperty((int)property);
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Invalid property enum " + property + " (0x" + string.Format("{0:x2}", (int)property) + ")");
            }
            valueMapIndex = valueMaps[valueMapIndex + 1];
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Property " + property + " (0x" + string.Format("{0:x2}", (int)property) +
                        ") does not have named values");
            }
            // valueMapIndex is the start of the property's valueMap,
            // where the first word is the BytesTrie offset.
            return GetPropertyOrValueEnum(valueMaps[valueMapIndex], alias);
        }
		
		/// <summary>
        /// Returns a value enum given a property enum and one of its value names.
        /// </summary>
		/// <seealso cref="TryGetPropertyValueEnum(UProperty, char[], out int)"/>
		public int GetPropertyValueEnum(UProperty property, char[] alias)
        {
            int valueMapIndex = FindProperty((int)property);
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Invalid property enum " + property + " (0x" + string.Format("{0:x2}", (int)property) + ")");
            }
            valueMapIndex = valueMaps[valueMapIndex + 1];
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Property " + property + " (0x" + string.Format("{0:x2}", (int)property) +
                        ") does not have named values");
            }
            // valueMapIndex is the start of the property's valueMap,
            // where the first word is the BytesTrie offset.
            return GetPropertyOrValueEnum(valueMaps[valueMapIndex], alias);
        }
		
		/// <summary>
        /// Returns a value enum given a property enum and one of its value names.
        /// </summary>
		/// <seealso cref="TryGetPropertyValueEnum(UProperty, ICharSequence, out int)"/>
		internal int GetPropertyValueEnum(UProperty property, ICharSequence alias)
        {
            int valueMapIndex = FindProperty((int)property);
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Invalid property enum " + property + " (0x" + string.Format("{0:x2}", (int)property) + ")");
            }
            valueMapIndex = valueMaps[valueMapIndex + 1];
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Property " + property + " (0x" + string.Format("{0:x2}", (int)property) +
                        ") does not have named values");
            }
            // valueMapIndex is the start of the property's valueMap,
            // where the first word is the BytesTrie offset.
            return GetPropertyOrValueEnum(valueMaps[valueMapIndex], alias);
        }

        /// <summary>
        /// Returns a value enum given a property enum and one of its value names.
        /// </summary>
		/// <seealso cref="GetPropertyValueEnum(UProperty, string)"/>
        public bool TryGetPropertyValueEnum(int property, string alias, out int result)
        {
#pragma warning disable 612, 618
            result = (int)UProperty.UNDEFINED;
#pragma warning restore 612, 618
            int valueMapIndex = FindProperty(property);
            if (valueMapIndex == 0)
            {
                return false;
            }
            valueMapIndex = valueMaps[valueMapIndex + 1];
            if (valueMapIndex == 0)
            {
                return false;
            }
            // valueMapIndex is the start of the property's valueMap,
            // where the first word is the BytesTrie offset.
            result = GetPropertyOrValueEnum(valueMaps[valueMapIndex], alias);
#pragma warning disable 612, 618
            return result != (int)UProperty.UNDEFINED;
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Returns a value enum given a property enum and one of its value names.
        /// </summary>
		/// <seealso cref="GetPropertyValueEnum(UProperty, StringBuilder)"/>
        public bool TryGetPropertyValueEnum(int property, StringBuilder alias, out int result)
        {
#pragma warning disable 612, 618
            result = (int)UProperty.UNDEFINED;
#pragma warning restore 612, 618
            int valueMapIndex = FindProperty(property);
            if (valueMapIndex == 0)
            {
                return false;
            }
            valueMapIndex = valueMaps[valueMapIndex + 1];
            if (valueMapIndex == 0)
            {
                return false;
            }
            // valueMapIndex is the start of the property's valueMap,
            // where the first word is the BytesTrie offset.
            result = GetPropertyOrValueEnum(valueMaps[valueMapIndex], alias);
            return true;
        }

        /// <summary>
        /// Returns a value enum given a property enum and one of its value names.
        /// </summary>
		/// <seealso cref="GetPropertyValueEnum(UProperty, char[])"/>
        public bool TryGetPropertyValueEnum(int property, char[] alias, out int result)
        {
#pragma warning disable 612, 618
            result = (int)UProperty.UNDEFINED;
#pragma warning restore 612, 618
            int valueMapIndex = FindProperty(property);
            if (valueMapIndex == 0)
            {
                return false;
            }
            valueMapIndex = valueMaps[valueMapIndex + 1];
            if (valueMapIndex == 0)
            {
                return false;
            }
            // valueMapIndex is the start of the property's valueMap,
            // where the first word is the BytesTrie offset.
            result = GetPropertyOrValueEnum(valueMaps[valueMapIndex], alias);
            return true;
        }

        /// <summary>
        /// Returns a value enum given a property enum and one of its value names.
        /// </summary>
		/// <seealso cref="GetPropertyValueEnum(UProperty, ICharSequence)"/>
        internal bool TryGetPropertyValueEnum(int property, ICharSequence alias, out int result)
        {
#pragma warning disable 612, 618
            result = (int)UProperty.UNDEFINED;
#pragma warning restore 612, 618
            int valueMapIndex = FindProperty(property);
            if (valueMapIndex == 0)
            {
                return false;
            }
            valueMapIndex = valueMaps[valueMapIndex + 1];
            if (valueMapIndex == 0)
            {
                return false;
            }
            // valueMapIndex is the start of the property's valueMap,
            // where the first word is the BytesTrie offset.
            result = GetPropertyOrValueEnum(valueMaps[valueMapIndex], alias);
            return true;
        }
	}
}