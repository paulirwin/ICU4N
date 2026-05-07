using ICU4N.Text;
using J2N;
using J2N.Text;
using System;
using System.Collections.Generic;

namespace ICU4N.Dev.Test.Rbbi
{
    // A small codepoint-level matcher for the rule grammar used by RBBIMonkeyTest.
    //
    // Why this exists: the rule files (break_rules/*.txt) embed UTF-32 codepoint
    // ranges like \U00010000-\U0001FFFF in character classes. .NET's
    // System.Text.RegularExpressions.Regex does not understand that escape syntax,
    // and the prior Utf32Regex shim that rewrites them as UTF-16 surrogate-pair
    // alternations did not work for the full corpus, so the test never matched
    // anything and was disabled.
    //
    // The grammar after BreakRules.AddRule expansion is small and regular:
    //   atom      := '.' | charClass | group
    //   charClass := '[' ... ']'           (parsed by UnicodeSet, which handles \U escapes natively)
    //   group     := '(' alternation ')'   (an empty group "()" is the break marker)
    //   factor    := atom ('*' | '?')?
    //   sequence  := factor*
    //   alternation := sequence ('|' sequence)*
    // Whitespace between tokens is concatenation. '\#' is a literal '#'.
    // The full rule is implicitly anchored at the current position (looking-at).
    //
    // The matcher operates on codepoints, which is what the rules are written in
    // terms of, sidestepping all UTF-16 surrogate handling.
    internal sealed class CodePointRuleMatcher
    {
        internal sealed class MatchResult
        {
            public bool Success;
            public int End;       // exclusive end index in the input string (UTF-16 units).
            public int BreakPos;  // index of the break-marker empty group, or -1.
        }

        private abstract class Node
        {
            // Returns each possible end index (inclusive of the index passed in for empty matches),
            // in order matching greedy regex semantics. For backtracking, the caller iterates
            // the enumerable lazily and tries the rest of the pattern after each candidate end.
            public abstract IEnumerable<int> Match(string input, int pos, MatchState state);
        }

        private sealed class MatchState
        {
            public int BreakPos = -1;
        }

        private sealed class AnyCodePointNode : Node
        {
            public override IEnumerable<int> Match(string input, int pos, MatchState state)
            {
                if (pos >= input.Length) yield break;
                yield return pos + Character.CharCount(input.CodePointAt(pos));
            }
        }

        private sealed class CharClassNode : Node
        {
            private readonly UnicodeSet _set;
            public CharClassNode(UnicodeSet set) { _set = set; }
            public override IEnumerable<int> Match(string input, int pos, MatchState state)
            {
                if (pos >= input.Length) yield break;
                int cp = input.CodePointAt(pos);
                if (_set.Contains(cp))
                    yield return pos + Character.CharCount(cp);
            }
        }

        // The break-marker (an empty capturing group "()" in the rule). Records the
        // current position in MatchState so the outer matcher can report it.
        private sealed class BreakMarkerNode : Node
        {
            public override IEnumerable<int> Match(string input, int pos, MatchState state)
            {
                int prev = state.BreakPos;
                state.BreakPos = pos;
                yield return pos;
                state.BreakPos = prev; // restore on backtrack
            }
        }

        private sealed class ConcatNode : Node
        {
            private readonly Node _left;
            private readonly Node _right;
            public ConcatNode(Node left, Node right) { _left = left; _right = right; }
            public override IEnumerable<int> Match(string input, int pos, MatchState state)
            {
                foreach (int mid in _left.Match(input, pos, state))
                    foreach (int end in _right.Match(input, mid, state))
                        yield return end;
            }
        }

        private sealed class AltNode : Node
        {
            private readonly Node[] _branches;
            public AltNode(Node[] branches) { _branches = branches; }
            public override IEnumerable<int> Match(string input, int pos, MatchState state)
            {
                foreach (Node branch in _branches)
                    foreach (int end in branch.Match(input, pos, state))
                        yield return end;
            }
        }

        // Greedy zero-or-more. Yields longest-first match positions.
        private sealed class StarNode : Node
        {
            private readonly Node _inner;
            public StarNode(Node inner) { _inner = inner; }
            public override IEnumerable<int> Match(string input, int pos, MatchState state)
            {
                // Walk the greedy chain forward, then yield positions in reverse (longest first).
                List<int> chain = new List<int> { pos };
                int cur = pos;
                while (true)
                {
                    int next = -1;
                    foreach (int e in _inner.Match(input, cur, state))
                    {
                        next = e;
                        break; // greedy first option
                    }
                    if (next < 0 || next == cur) break;
                    chain.Add(next);
                    cur = next;
                }
                for (int i = chain.Count - 1; i >= 0; --i)
                    yield return chain[i];
            }
        }

        // Greedy zero-or-one. Yields with-match first, then without.
        private sealed class OptionalNode : Node
        {
            private readonly Node _inner;
            public OptionalNode(Node inner) { _inner = inner; }
            public override IEnumerable<int> Match(string input, int pos, MatchState state)
            {
                bool yieldedAny = false;
                foreach (int end in _inner.Match(input, pos, state))
                {
                    yieldedAny = true;
                    yield return end;
                }
                yield return pos;
                _ = yieldedAny;
            }
        }

        private sealed class EmptyNode : Node
        {
            public override IEnumerable<int> Match(string input, int pos, MatchState state)
            {
                yield return pos;
            }
        }

        // Zero-width assertion: positive (=) or negative (!) lookahead.
        private sealed class LookaheadNode : Node
        {
            private readonly Node _inner;
            private readonly bool _negate;
            public LookaheadNode(Node inner, bool negate) { _inner = inner; _negate = negate; }
            public override IEnumerable<int> Match(string input, int pos, MatchState state)
            {
                bool found = false;
                foreach (int _ in _inner.Match(input, pos, state)) { found = true; break; }
                if (found != _negate)
                    yield return pos;
            }
        }

        private readonly Node _root;
        private readonly bool _hasBreakMarker;

        public CodePointRuleMatcher(string pattern)
        {
            var parser = new Parser(pattern);
            _root = parser.ParseAlternation();
            _hasBreakMarker = parser.SawBreakMarker;
            if (parser.Position != pattern.Length)
            {
                throw new ArgumentException(
                    "Unexpected trailing input at position " + parser.Position + " in rule pattern: " + pattern);
            }
        }

        public bool HasBreakMarker => _hasBreakMarker;

        public MatchResult LookingAt(string input, int startIdx)
        {
            var state = new MatchState();
            foreach (int end in _root.Match(input, startIdx, state))
            {
                return new MatchResult { Success = true, End = end, BreakPos = state.BreakPos };
            }
            return new MatchResult { Success = false, End = startIdx, BreakPos = -1 };
        }

        // Recursive-descent parser for the rule grammar.
        private sealed class Parser
        {
            private readonly string _src;
            private int _pos;
            public bool SawBreakMarker;

            public Parser(string src) { _src = src; _pos = 0; }
            public int Position => _pos;

            private void SkipWhitespace()
            {
                while (_pos < _src.Length && (_src[_pos] == ' ' || _src[_pos] == '\t'))
                    _pos++;
            }

            // alternation := sequence ('|' sequence)*
            // Stops at ')' or end-of-input.
            public Node ParseAlternation()
            {
                List<Node> branches = new List<Node>();
                branches.Add(ParseSequence());
                while (true)
                {
                    SkipWhitespace();
                    if (_pos < _src.Length && _src[_pos] == '|')
                    {
                        _pos++;
                        branches.Add(ParseSequence());
                        continue;
                    }
                    break;
                }
                if (branches.Count == 1) return branches[0];
                return new AltNode(branches.ToArray());
            }

            // sequence := factor*
            // Stops at '|', ')', or end-of-input.
            private Node ParseSequence()
            {
                Node result = null;
                while (true)
                {
                    SkipWhitespace();
                    if (_pos >= _src.Length) break;
                    char c = _src[_pos];
                    if (c == '|' || c == ')') break;

                    Node factor = ParseFactor();
                    if (factor == null) break;
                    result = result == null ? factor : new ConcatNode(result, factor);
                }
                return result ?? (Node)new EmptyNode();
            }

            // factor := atom ('*' | '?')?
            // Whitespace between the atom and its quantifier is allowed; some rule files
            // (e.g. sentence.txt SB8) contain "X *" intending "X*".
            private Node ParseFactor()
            {
                Node atom = ParseAtom();
                if (atom == null) return null;
                int savedPos = _pos;
                SkipWhitespace();
                if (_pos < _src.Length)
                {
                    char c = _src[_pos];
                    if (c == '*') { _pos++; return new StarNode(atom); }
                    if (c == '?') { _pos++; return new OptionalNode(atom); }
                }
                _pos = savedPos;
                return atom;
            }

            // atom := '.' | charClass | group | escapedLiteral
            private Node ParseAtom()
            {
                if (_pos >= _src.Length) return null;
                char c = _src[_pos];

                if (c == '.')
                {
                    _pos++;
                    return new AnyCodePointNode();
                }
                if (c == '[')
                {
                    return ParseCharClass();
                }
                if (c == '(')
                {
                    return ParseGroup();
                }
                if (c == '\\')
                {
                    return ParseEscapedLiteral();
                }
                // Unexpected token at this level. Could be a stray bracket-close,
                // alternation, or end-of-group; the caller will stop here.
                return null;
            }

            // charClass := '[' (anything, with nesting and \-escapes) ']'
            // The full bracketed expression (including outer brackets) is fed to UnicodeSet,
            // which natively understands \U-escapes, ranges, set arithmetic, and properties.
            private Node ParseCharClass()
            {
                int start = _pos;
                int depth = 0;
                while (_pos < _src.Length)
                {
                    char c = _src[_pos];
                    if (c == '\\')
                    {
                        // skip the escape sequence; treat \U escapes (10 chars total) and shorter ones uniformly
                        // by skipping just the backslash and the next character. UnicodeSet will handle
                        // the rest of multi-char escapes via its own parser.
                        _pos += 2;
                        continue;
                    }
                    if (c == '[') depth++;
                    else if (c == ']')
                    {
                        depth--;
                        _pos++;
                        if (depth == 0) break;
                        continue;
                    }
                    _pos++;
                }
                if (depth != 0)
                    throw new ArgumentException("Unclosed character class at position " + start);

                string raw = _src.Substring(start, _pos - start);
                UnicodeSet set;
                try
                {
                    set = new UnicodeSet(raw, UnicodeSet.IgnoreSpace);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException("Invalid character class \"" + raw + "\": " + e.Message, e);
                }
                return new CharClassNode(set);
            }

            // group := '(' alternation ')'
            //        | '(?:' alternation ')'   (non-capturing group)
            //        | '(?=' alternation ')'   (positive lookahead)
            //        | '(?!' alternation ')'   (negative lookahead)
            // An empty group "()" is the break marker.
            private Node ParseGroup()
            {
                _pos++; // skip '('
                bool isLookahead = false;
                bool negate = false;
                if (_pos + 1 < _src.Length && _src[_pos] == '?')
                {
                    char next = _src[_pos + 1];
                    if (next == ':')
                    {
                        _pos += 2; // skip "?:"
                    }
                    else if (next == '=' || next == '!')
                    {
                        isLookahead = true;
                        negate = (next == '!');
                        _pos += 2; // skip "?=" or "?!"
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported group construct '(?" + next + "' at position " + (_pos - 1));
                    }
                }
                SkipWhitespace();
                if (!isLookahead && _pos < _src.Length && _src[_pos] == ')')
                {
                    _pos++;
                    SawBreakMarker = true;
                    return new BreakMarkerNode();
                }
                Node inner = ParseAlternation();
                SkipWhitespace();
                if (_pos >= _src.Length || _src[_pos] != ')')
                    throw new ArgumentException("Expected ')' at position " + _pos);
                _pos++;
                return isLookahead ? new LookaheadNode(inner, negate) : inner;
            }

            // Handles '\#' (literal '#') and \-escaped single chars at top level.
            private Node ParseEscapedLiteral()
            {
                // \# is the only top-level escape we expect post-expansion (BreakRules.AddRule
                // escapes literal '#' to avoid UnicodeSet's comment syntax). Other escapes are
                // confined to within character classes. Treat a top-level escape as a literal
                // single-codepoint match.
                if (_pos + 1 >= _src.Length)
                    throw new ArgumentException("Trailing backslash at position " + _pos);
                _pos++; // skip backslash
                int cp = _src.CodePointAt(_pos);
                _pos += Character.CharCount(cp);
                var set = new UnicodeSet();
                set.Add(cp);
                return new CharClassNode(set);
            }
        }
    }
}
