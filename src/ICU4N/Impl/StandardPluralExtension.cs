﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using ICU4N.Support.Text;
using J2N.Text;
using System.Text;

namespace ICU4N.Impl
{
    // ICU4N TODO: Refactor this so it is a more sensible API
    public static partial class StandardPluralUtil
    {

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <c>null</c>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or null.</returns>
        public static StandardPlural? OrNullFromString(string keyword)
        {
            switch (keyword.Length)
            {
                case 3:
                    if ("one".ContentEquals(keyword))
                    {
                        return StandardPlural.One;
                    }
                    else if ("two".ContentEquals(keyword))
                    {
                        return StandardPlural.Two;
                    }
                    else if ("few".ContentEquals(keyword))
                    {
                        return StandardPlural.Few;
                    }
                    break;
                case 4:
                    if ("many".ContentEquals(keyword))
                    {
                        return StandardPlural.Many;
                    }
                    else if ("zero".ContentEquals(keyword))
                    {
                        return StandardPlural.Zero;
                    }
                    break;
                case 5:
                    if ("other".ContentEquals(keyword))
                    {
                        return StandardPlural.Other;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <c>null</c>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or null.</returns>
        public static StandardPlural? OrNullFromString(StringBuilder keyword)
        {
            switch (keyword.Length)
            {
                case 3:
                    if ("one".ContentEquals(keyword))
                    {
                        return StandardPlural.One;
                    }
                    else if ("two".ContentEquals(keyword))
                    {
                        return StandardPlural.Two;
                    }
                    else if ("few".ContentEquals(keyword))
                    {
                        return StandardPlural.Few;
                    }
                    break;
                case 4:
                    if ("many".ContentEquals(keyword))
                    {
                        return StandardPlural.Many;
                    }
                    else if ("zero".ContentEquals(keyword))
                    {
                        return StandardPlural.Zero;
                    }
                    break;
                case 5:
                    if ("other".ContentEquals(keyword))
                    {
                        return StandardPlural.Other;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <c>null</c>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or null.</returns>
        public static StandardPlural? OrNullFromString(char[] keyword)
        {
            switch (keyword.Length)
            {
                case 3:
                    if ("one".ContentEquals(keyword))
                    {
                        return StandardPlural.One;
                    }
                    else if ("two".ContentEquals(keyword))
                    {
                        return StandardPlural.Two;
                    }
                    else if ("few".ContentEquals(keyword))
                    {
                        return StandardPlural.Few;
                    }
                    break;
                case 4:
                    if ("many".ContentEquals(keyword))
                    {
                        return StandardPlural.Many;
                    }
                    else if ("zero".ContentEquals(keyword))
                    {
                        return StandardPlural.Zero;
                    }
                    break;
                case 5:
                    if ("other".ContentEquals(keyword))
                    {
                        return StandardPlural.Other;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <c>null</c>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or null.</returns>
        public static StandardPlural? OrNullFromString(ICharSequence keyword)
        {
            switch (keyword.Length)
            {
                case 3:
                    if ("one".ContentEquals(keyword))
                    {
                        return StandardPlural.One;
                    }
                    else if ("two".ContentEquals(keyword))
                    {
                        return StandardPlural.Two;
                    }
                    else if ("few".ContentEquals(keyword))
                    {
                        return StandardPlural.Few;
                    }
                    break;
                case 4:
                    if ("many".ContentEquals(keyword))
                    {
                        return StandardPlural.Many;
                    }
                    else if ("zero".ContentEquals(keyword))
                    {
                        return StandardPlural.Zero;
                    }
                    break;
                case 5:
                    if ("other".ContentEquals(keyword))
                    {
                        return StandardPlural.Other;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static StandardPlural OrOtherFromString(string keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? p.Value : StandardPlural.Other;
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static StandardPlural OrOtherFromString(StringBuilder keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? p.Value : StandardPlural.Other;
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static StandardPlural OrOtherFromString(char[] keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? p.Value : StandardPlural.Other;
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static StandardPlural OrOtherFromString(ICharSequence keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? p.Value : StandardPlural.Other;
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">>When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(StandardPlural)"/>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryFromString(string keyword, out StandardPlural result)
        {
            StandardPlural? p = OrNullFromString(keyword);
            if (p != null)
            {
                result = p.Value;
                return true;
            }
            else
            {
                result = default(StandardPlural);
                return false;
            }
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">>When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(StandardPlural)"/>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryFromString(StringBuilder keyword, out StandardPlural result)
        {
            StandardPlural? p = OrNullFromString(keyword);
            if (p != null)
            {
                result = p.Value;
                return true;
            }
            else
            {
                result = default(StandardPlural);
                return false;
            }
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">>When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(StandardPlural)"/>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryFromString(char[] keyword, out StandardPlural result)
        {
            StandardPlural? p = OrNullFromString(keyword);
            if (p != null)
            {
                result = p.Value;
                return true;
            }
            else
            {
                result = default(StandardPlural);
                return false;
            }
        }

        /// <summary>
        /// Returns the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">>When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(StandardPlural)"/>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryFromString(ICharSequence keyword, out StandardPlural result)
        {
            StandardPlural? p = OrNullFromString(keyword);
            if (p != null)
            {
                result = p.Value;
                return true;
            }
            else
            {
                result = default(StandardPlural);
                return false;
            }
        }

        /// <summary>
        /// Returns the index of the plural form corresponding to the keyword, or a negative value.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The index of the plural form corresponding to the keyword, or a negative value.</returns>
        public static int IndexOrNegativeFromString(string keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? (int)p.Value : -1;
        }

        /// <summary>
        /// Returns the index of the plural form corresponding to the keyword, or a negative value.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The index of the plural form corresponding to the keyword, or a negative value.</returns>
        public static int IndexOrNegativeFromString(StringBuilder keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? (int)p.Value : -1;
        }

        /// <summary>
        /// Returns the index of the plural form corresponding to the keyword, or a negative value.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The index of the plural form corresponding to the keyword, or a negative value.</returns>
        public static int IndexOrNegativeFromString(char[] keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? (int)p.Value : -1;
        }

        /// <summary>
        /// Returns the index of the plural form corresponding to the keyword, or a negative value.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The index of the plural form corresponding to the keyword, or a negative value.</returns>
        public static int IndexOrNegativeFromString(ICharSequence keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? (int)p.Value : -1;
        }

        /// <summary>
        /// Returns the index of the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The index of the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static int IndexOrOtherIndexFromString(string keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? (int)p.Value : (int)StandardPlural.Other;
        }

        /// <summary>
        /// Returns the index of the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The index of the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static int IndexOrOtherIndexFromString(StringBuilder keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? (int)p.Value : (int)StandardPlural.Other;
        }

        /// <summary>
        /// Returns the index of the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The index of the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static int IndexOrOtherIndexFromString(char[] keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? (int)p.Value : (int)StandardPlural.Other;
        }

        /// <summary>
        /// Returns the index of the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <returns>The index of the plural form corresponding to the keyword, or <see cref="StandardPlural.Other"/>.</returns>
        public static int IndexOrOtherIndexFromString(ICharSequence keyword)
        {
            StandardPlural? p = OrNullFromString(keyword);
            return p != null ? (int)p.Value : (int)StandardPlural.Other;
        }

        /// <summary>
        /// Gets the index of the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(int)"/>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryIndexFromString(string keyword, out int result)
        {
            StandardPlural? p = OrNullFromString(keyword);
            if (p != null)
            {
                result = (int)p;
                return true;
            }
            else
            {
                result = default(int);
                return false;
            }
        }

        /// <summary>
        /// Gets the index of the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(int)"/>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryIndexFromString(StringBuilder keyword, out int result)
        {
            StandardPlural? p = OrNullFromString(keyword);
            if (p != null)
            {
                result = (int)p;
                return true;
            }
            else
            {
                result = default(int);
                return false;
            }
        }

        /// <summary>
        /// Gets the index of the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(int)"/>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryIndexFromString(char[] keyword, out int result)
        {
            StandardPlural? p = OrNullFromString(keyword);
            if (p != null)
            {
                result = (int)p;
                return true;
            }
            else
            {
                result = default(int);
                return false;
            }
        }

        /// <summary>
        /// Gets the index of the plural form corresponding to the keyword.
        /// </summary>
        /// <param name="keyword">Keyword for example "few" or "other".</param>
        /// <param name="result">When this method returns, contains the index of the plural form corresponding to the keyword, otherwise
        /// <see cref="T:default(int)"/>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the <paramref name="keyword"/> is valid; otherwise <c>false</c>.</returns>
        public static bool TryIndexFromString(ICharSequence keyword, out int result)
        {
            StandardPlural? p = OrNullFromString(keyword);
            if (p != null)
            {
                result = (int)p;
                return true;
            }
            else
            {
                result = default(int);
                return false;
            }
        }
	}
}