using NUnit.Framework;
using System;

// ICU4N specific - covers the .NET-only CodePointRuleMatcher (see
// src/ICU4N.TestFramework/Support/CodePointRuleMatcher.cs for context).
// There is no upstream ICU4J equivalent because Java's java.util.regex.Pattern
// already supports the rule grammar used by ICU break-rule files.

namespace ICU4N.Support
{
    public class CodePointRuleMatcherTest
    {
        private static CodePointRuleMatcher.MatchResult LookingAt(string pattern, string input, int start = 0)
            => new CodePointRuleMatcher(pattern).LookingAt(input, start);

        [Test]
        public void Dot_MatchesSingleBmpCodepoint()
        {
            var m = LookingAt(".", "abc");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
            Assert.AreEqual(-1, m.BreakPos);
        }

        [Test]
        public void Dot_MatchesAstralCodepointAsTwoUtf16Units()
        {
            // U+1F600 (😀) is a surrogate pair in UTF-16 but a single codepoint.
            string input = char.ConvertFromUtf32(0x1F600) + "x";
            var m = LookingAt(".", input);
            Assert.IsTrue(m.Success);
            Assert.AreEqual(2, m.End, "Dot should consume both surrogates");
        }

        [Test]
        public void Dot_FailsAtEndOfInput()
        {
            var m = LookingAt(".", "");
            Assert.IsFalse(m.Success);
        }

        [Test]
        public void Dot_MatchesLineTerminators()
        {
            // CodePointRuleMatcher always matches "any codepoint" (java DOTALL semantics).
            var m = LookingAt(".", "\n");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void CharClass_BmpRange()
        {
            var m = LookingAt("[a-z]", "qx");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void CharClass_BmpRange_NoMatch()
        {
            var m = LookingAt("[a-z]", "Q");
            Assert.IsFalse(m.Success);
        }

        [Test]
        public void CharClass_NegatedClass()
        {
            var m = LookingAt("[^a-z]", "Q");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void CharClass_Utf32Range_AstralStartIsMatched()
        {
            // The whole point of this matcher: \U-escaped astral codepoints/ranges
            // must work the same way Java's regex does. .NET Regex does not support \U.
            string input = char.ConvertFromUtf32(0x10000); // first astral codepoint
            var m = LookingAt(@"[\U00010000-\U0001FFFF]", input);
            Assert.IsTrue(m.Success);
            Assert.AreEqual(2, m.End, "Astral codepoint occupies two UTF-16 units");
        }

        [Test]
        public void CharClass_Utf32Range_AstralEndIsMatched()
        {
            string input = char.ConvertFromUtf32(0x1FFFF);
            var m = LookingAt(@"[\U00010000-\U0001FFFF]", input);
            Assert.IsTrue(m.Success);
            Assert.AreEqual(2, m.End);
        }

        [Test]
        public void CharClass_Utf32Range_OutsideRangeRejected()
        {
            string input = char.ConvertFromUtf32(0x20000);
            var m = LookingAt(@"[\U00010000-\U0001FFFF]", input);
            Assert.IsFalse(m.Success);
        }

        [Test]
        public void CharClass_UnicodeProperty()
        {
            // UnicodeSet handles the full \p{...} / [:...:] property syntax.
            var m = LookingAt(@"[\p{L}]", "X");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void CharClass_UnicodeProperty_NoMatch()
        {
            var m = LookingAt(@"[\p{L}]", "1");
            Assert.IsFalse(m.Success);
        }

        [Test]
        public void Concatenation()
        {
            var m = LookingAt("[a-z] [0-9]", "a1Z");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(2, m.End);
        }

        [Test]
        public void Concatenation_FailsIfSecondAtomDoesnt()
        {
            var m = LookingAt("[a-z] [0-9]", "ab");
            Assert.IsFalse(m.Success);
        }

        [Test]
        public void Alternation_FirstBranch()
        {
            var m = LookingAt("([a-z] | [0-9])", "x");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void Alternation_SecondBranch()
        {
            var m = LookingAt("([a-z] | [0-9])", "5");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void Star_ZeroOccurrences()
        {
            var m = LookingAt("[a-z]*", "123");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(0, m.End);
        }

        [Test]
        public void Star_GreedyConsumesAll()
        {
            var m = LookingAt("[a-z]*", "abc1");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(3, m.End);
        }

        [Test]
        public void Star_BacktracksToAllowSuffixToMatch()
        {
            // [a-z]* is greedy but must give back enough chars so [0-9] can match.
            var m = LookingAt("[a-z]* [0-9]", "abc1");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(4, m.End);
        }

        [Test]
        public void Optional_Present()
        {
            var m = LookingAt("[a-z]? [0-9]", "a1");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(2, m.End);
        }

        [Test]
        public void Optional_Absent()
        {
            var m = LookingAt("[a-z]? [0-9]", "1");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void Quantifier_AllowsWhitespaceBeforeQuantifier()
        {
            // sentence.txt SB8 contains "ExtFmt *" (with a space) intending "ExtFmt*".
            var m = LookingAt("[a-z] *", "abc1");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(3, m.End);
        }

        [Test]
        public void NonCapturingGroup()
        {
            var m = LookingAt("(?:[a-z] [0-9])", "a1");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(2, m.End);
        }

        [Test]
        public void PositiveLookahead_Succeeds()
        {
            // "a" followed by "b" (consuming only the "a").
            var m = LookingAt("[a] (?=[b])", "ab");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void PositiveLookahead_FailsWhenNotFollowed()
        {
            var m = LookingAt("[a] (?=[b])", "ax");
            Assert.IsFalse(m.Success);
        }

        [Test]
        public void NegativeLookahead_EndOfInput()
        {
            // sentence.txt SB2: ". ÷ (?!.)" - the negative lookahead asserts end of input.
            var m = LookingAt(". (?!.)", "x");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void NegativeLookahead_NotEndOfInput()
        {
            var m = LookingAt(". (?!.)", "xy");
            Assert.IsFalse(m.Success);
        }

        [Test]
        public void BreakMarker_RecordsPosition()
        {
            // The empty group "()" in compiled rules is a zero-width break marker.
            var m = LookingAt("[a-z] () [0-9]", "a1");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(2, m.End);
            Assert.AreEqual(1, m.BreakPos);
        }

        [Test]
        public void BreakMarker_AtStart()
        {
            var m = LookingAt("() [a-z]", "a");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(0, m.BreakPos);
        }

        [Test]
        public void HasBreakMarker_TrueWhenPresent()
        {
            Assert.IsTrue(new CodePointRuleMatcher("[a-z] ()").HasBreakMarker);
        }

        [Test]
        public void HasBreakMarker_FalseWhenAbsent()
        {
            Assert.IsFalse(new CodePointRuleMatcher("[a-z]").HasBreakMarker);
        }

        [Test]
        public void LookingAt_StartsAtGivenIndex()
        {
            var m = LookingAt("[a-z]", "12abc", start: 2);
            Assert.IsTrue(m.Success);
            Assert.AreEqual(3, m.End);
        }

        [Test]
        public void LookingAt_FailsWhenStartIsAtEnd()
        {
            var m = LookingAt("[a-z]", "abc", start: 3);
            Assert.IsFalse(m.Success);
        }

        [Test]
        public void EscapedHashLiteral()
        {
            // BreakRules.AddRule escapes literal '#' to '\#' to avoid UnicodeSet's
            // comment syntax. The matcher must accept top-level escapes as literals.
            var m = LookingAt(@"\#", "#");
            Assert.IsTrue(m.Success);
            Assert.AreEqual(1, m.End);
        }

        [Test]
        public void EmptyCharClass_NeverMatches()
        {
            // UAX rules can contain empty character classes "[]". UnicodeSet handles this
            // directly (unlike java.util.regex.Pattern), so no substitution is needed.
            var m = LookingAt("[]", "a");
            Assert.IsFalse(m.Success);
        }

        [Test]
        public void MalformedPattern_UnclosedGroup_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CodePointRuleMatcher("([a-z]"));
        }

        [Test]
        public void MalformedPattern_TrailingBackslash_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CodePointRuleMatcher(@"\"));
        }

        [Test]
        public void MalformedPattern_UnclosedCharClass_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CodePointRuleMatcher("[a-z"));
        }

        [Test]
        public void MalformedPattern_UnsupportedGroupConstruct_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CodePointRuleMatcher("(?<foo>bar)"));
        }
    }
}
