﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    internal static partial class UnicodeSetExtensions
    {

        /// <seealso cref="UnicodeSet.AddAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // See ticket #11395, this is safe.

        public static UnicodeSet AddAll(this UnicodeSet set, params string[] collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(collection);
        }

        /// <seealso cref="UnicodeSet.AddAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // See ticket #11395, this is safe.

        public static UnicodeSet AddAll(this UnicodeSet set, params StringBuilder[] collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(collection);
        }

        /// <seealso cref="UnicodeSet.AddAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // See ticket #11395, this is safe.
        //[CLSCompliant(false)] // ICU4N: This is only required if exposed publicly (which we don't want to do because of naming)
        public static UnicodeSet AddAll(this UnicodeSet set, params char[][] collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(collection);
        }

        /// <seealso cref="UnicodeSet.AddAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // See ticket #11395, this is safe.

        public static UnicodeSet AddAll(this UnicodeSet set, params ICharSequence[] collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(collection);
        }

        /// <summary>
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet AddAll(this UnicodeSet set, string s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(s);
        }

        /// <summary>
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet AddAll(this UnicodeSet set, StringBuilder s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(s);
        }

        /// <summary>
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet AddAll(this UnicodeSet set, char[] s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(s);
        }

        /// <summary>
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet AddAll(this UnicodeSet set, ICharSequence s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(s);
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Complement(this UnicodeSet set, string s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Complement(s);
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Complement(this UnicodeSet set, StringBuilder s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Complement(s);
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Complement(this UnicodeSet set, char[] s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Complement(s);
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Complement(this UnicodeSet set, ICharSequence s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Complement(s);
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet ComplementAll(this UnicodeSet set, string s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ComplementAll(s);
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet ComplementAll(this UnicodeSet set, StringBuilder s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ComplementAll(s);
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet ComplementAll(this UnicodeSet set, char[] s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ComplementAll(s);
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet ComplementAll(this UnicodeSet set, ICharSequence s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ComplementAll(s);
        }

        /// <seealso cref="UnicodeSet.ContainsAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsAll(this UnicodeSet set, IEnumerable<string> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsAll(collection);
        }

        /// <seealso cref="UnicodeSet.ContainsAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsAll(this UnicodeSet set, IEnumerable<StringBuilder> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsAll(collection);
        }

        /// <seealso cref="UnicodeSet.ContainsAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsAll(this UnicodeSet set, IEnumerable<char[]> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsAll(collection);
        }

        /// <seealso cref="UnicodeSet.ContainsAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsAll<T>(this UnicodeSet set, IEnumerable<T> collection) where T : ICharSequence
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsAll(collection);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// of the given string.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome(this UnicodeSet set, string s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(s);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// of the given string.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome(this UnicodeSet set, StringBuilder s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(s);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// of the given string.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome(this UnicodeSet set, char[] s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(s);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// of the given string.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome(this UnicodeSet set, ICharSequence s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(s);
        }

        /// <seealso cref="UnicodeSet.ContainsSome(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome(this UnicodeSet set, IEnumerable<string> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(collection);
        }

        /// <seealso cref="UnicodeSet.ContainsSome(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome(this UnicodeSet set, IEnumerable<StringBuilder> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(collection);
        }

        /// <seealso cref="UnicodeSet.ContainsSome(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome(this UnicodeSet set, IEnumerable<char[]> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(collection);
        }

        /// <seealso cref="UnicodeSet.ContainsSome(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome<T>(this UnicodeSet set, IEnumerable<T> collection) where T : ICharSequence
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(collection);
        }

        /// <summary>
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAll(this UnicodeSet set, string s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RemoveAll(s);
        }

        /// <summary>
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAll(this UnicodeSet set, StringBuilder s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RemoveAll(s);
        }

        /// <summary>
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAll(this UnicodeSet set, char[] s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RemoveAll(s);
        }

        /// <summary>
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAll(this UnicodeSet set, ICharSequence s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RemoveAll(s);
        }

        /// <seealso cref="UnicodeSet.RemoveAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAll(this UnicodeSet set, IEnumerable<string> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RemoveAll(collection);
        }

        /// <seealso cref="UnicodeSet.RemoveAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAll(this UnicodeSet set, IEnumerable<StringBuilder> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RemoveAll(collection);
        }

        /// <seealso cref="UnicodeSet.RemoveAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAll(this UnicodeSet set, IEnumerable<char[]> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RemoveAll(collection);
        }

        /// <seealso cref="UnicodeSet.RemoveAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAll<T>(this UnicodeSet set, IEnumerable<T> collection) where T : ICharSequence
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RemoveAll(collection);
        }

        /// <summary>
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Retain(this UnicodeSet set, string cs)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Retain(cs);
        }

        /// <summary>
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Retain(this UnicodeSet set, StringBuilder cs)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Retain(cs);
        }

        /// <summary>
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Retain(this UnicodeSet set, char[] cs)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Retain(cs);
        }

        /// <summary>
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Retain(this UnicodeSet set, ICharSequence cs)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Retain(cs);
        }

        /// <seealso cref="UnicodeSet.RetainAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RetainAll(this UnicodeSet set, IEnumerable<string> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RetainAll(collection);
        }

        /// <seealso cref="UnicodeSet.RetainAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RetainAll(this UnicodeSet set, IEnumerable<StringBuilder> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RetainAll(collection);
        }

        /// <seealso cref="UnicodeSet.RetainAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RetainAll(this UnicodeSet set, IEnumerable<char[]> collection)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RetainAll(collection);
        }

        /// <seealso cref="UnicodeSet.RetainAll(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RetainAll<T>(this UnicodeSet set, IEnumerable<T> collection) where T : ICharSequence
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RetainAll(collection);
        }

        /// <summary>
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RetainAll(this UnicodeSet set, string s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RetainAll(s);
        }

        /// <summary>
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RetainAll(this UnicodeSet set, StringBuilder s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RetainAll(s);
        }

        /// <summary>
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RetainAll(this UnicodeSet set, char[] s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RetainAll(s);
        }

        /// <summary>
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RetainAll(this UnicodeSet set, ICharSequence s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RetainAll(s);
        }
    }
}