﻿using ICU4N.Text;
using J2N.Text;
using System;
using System.Text;

namespace ICU4N.Impl.Locale
{
    public sealed class BaseLocale
    {
        private const int CharStackBufferSize = 32;
        public const char Separator = '_';

        private static readonly Cache CACHE = new Cache();
        public static readonly BaseLocale Root = BaseLocale.GetInstance("", "", "", "");

        private readonly string _language = "";
        private readonly string _script = "";
        private readonly string _region = "";
        private readonly string _variant = "";

        private volatile int _hash = 0;

        private BaseLocale(string language, string script, string region, string variant)
        {
            if (language != null)
            {
                _language = AsciiUtil.ToLower(language).Intern();
            }
            if (script != null)
            {
                _script = AsciiUtil.ToTitle(script).Intern();
            }
            if (region != null)
            {
                _region = AsciiUtil.ToUpper(region).Intern();
            }
            if (variant != null)
            {
#if JDKIMPL
                // preserve upper/lower cases
                _variant = variant.Intern();
#else
                _variant = AsciiUtil.ToUpper(variant).Intern();
#endif
            }
        }

        public static BaseLocale GetInstance(string language, string script, string region, string variant)
        {
#if JDKIMPL
            // JDK uses deprecated ISO639.1 language codes for he, yi and id
            if (AsciiUtil.CaseIgnoreMatch(language, "he"))
            {
                language = "iw";
            }
            else if (AsciiUtil.CaseIgnoreMatch(language, "yi"))
            {
                language = "ji";
            }
            else if (AsciiUtil.CaseIgnoreMatch(language, "id"))
            {
                language = "in";
            }
#endif
            Key key = new Key(language, script, region, variant);
            BaseLocale baseLocale = CACHE.Get(key);
            return baseLocale;
        }

        public string Language => _language;

        public string Script => _script;

        public string Region => _region;

        public string Variant => _variant;


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is BaseLocale other)
            {
                return  _language.Equals(other._language) // ICU4N specific - removed GetHashCode() from the comparison, as it is redundant
                    && _script.Equals(other._script)
                    && _region.Equals(other._region)
                    && _variant.Equals(other._variant);
            }
            return false;
        }


        public override string ToString()
        {
            using ValueStringBuilder buf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            if (_language.Length > 0)
            {
                buf.Append("language=");
                buf.Append(_language);
            }
            if (_script.Length > 0)
            {
                if (buf.Length > 0)
                {
                    buf.Append(", ");
                }
                buf.Append("script=");
                buf.Append(_script);
            }
            if (_region.Length > 0)
            {
                if (buf.Length > 0)
                {
                    buf.Append(", ");
                }
                buf.Append("region=");
                buf.Append(_region);
            }
            if (_variant.Length > 0)
            {
                if (buf.Length > 0)
                {
                    buf.Append(", ");
                }
                buf.Append("variant=");
                buf.Append(_variant);
            }
            return buf.ToString();
        }


        public override int GetHashCode()
        {
            int h = _hash;
            if (h == 0)
            {
                // Generating a hash value from language, script, region and variant
                for (int i = 0; i < _language.Length; i++)
                {
                    h = 31 * h + _language[i];
                }
                for (int i = 0; i < _script.Length; i++)
                {
                    h = 31 * h + _script[i];
                }
                for (int i = 0; i < _region.Length; i++)
                {
                    h = 31 * h + _region[i];
                }
                for (int i = 0; i < _variant.Length; i++)
                {
                    h = 31 * h + _variant[i];
                }
                _hash = h;
            }
            return h;
        }

        private class Key : IComparable<Key>
        {
            private readonly string _lang = "";
            private readonly string _scrt = "";
            private readonly string _regn = "";
            private readonly string _vart = "";

            private volatile int _hash; // Default to 0

            internal string Lang => _lang;

            internal string Scrt => _scrt;

            internal string Regn => _regn;

            internal string Vart => _vart;

            public Key(string language, string script, string region, string variant)
            {
                if (language != null)
                {
                    _lang = language;
                }
                if (script != null)
                {
                    _scrt = script;
                }
                if (region != null)
                {
                    _regn = region;
                }
                if (variant != null)
                {
                    _vart = variant;
                }
            }

            public override bool Equals(object obj)
            {
#if JDKIMPL
                return (this == obj) ||
                        (obj is Key key1)
                        && AsciiUtil.CaseIgnoreMatch(key1._lang, this._lang)
                        && AsciiUtil.CaseIgnoreMatch(key1._scrt, this._scrt)
                        && AsciiUtil.CaseIgnoreMatch(key1._regn, this._regn)
                        && key1._vart.Equals(_vart); // variant is case sensitive in JDK!
#else
                return (this == obj) ||
                        (obj is Key key)
                        && AsciiUtil.CaseIgnoreMatch(key._lang, this._lang)
                        && AsciiUtil.CaseIgnoreMatch(key._scrt, this._scrt)
                        && AsciiUtil.CaseIgnoreMatch(key._regn, this._regn)
                        && AsciiUtil.CaseIgnoreMatch(key._vart, this._vart);
#endif
            }

            public virtual int CompareTo(Key other)
            {
                int res = AsciiUtil.CaseIgnoreCompare(this._lang, other._lang);
                if (res == 0)
                {
                    res = AsciiUtil.CaseIgnoreCompare(this._scrt, other._scrt);
                    if (res == 0)
                    {
                        res = AsciiUtil.CaseIgnoreCompare(this._regn, other._regn);
                        if (res == 0)
                        {
#if JDKIMPL
                            res = this._vart.CompareToOrdinal(other._vart);
#else
                            res = AsciiUtil.CaseIgnoreCompare(this._vart, other._vart);
#endif
                        }
                    }
                }
                return res;
            }

            public override int GetHashCode()
            {
                int h = _hash;
                if (h == 0)
                {
                    // Generating a hash value from language, script, region and variant
                    for (int i = 0; i < _lang.Length; i++)
                    {
                        h = 31 * h + AsciiUtil.ToLower(_lang[i]);
                    }
                    for (int i = 0; i < _scrt.Length; i++)
                    {
                        h = 31 * h + AsciiUtil.ToLower(_scrt[i]);
                    }
                    for (int i = 0; i < _regn.Length; i++)
                    {
                        h = 31 * h + AsciiUtil.ToLower(_regn[i]);
                    }
                    for (int i = 0; i < _vart.Length; i++)
                    {
#if JDKIMPL
                        h = 31 * h + _vart[i];
#else
                        h = 31 * h + AsciiUtil.ToLower(_vart[i]);
#endif
                    }
                    _hash = h;
                }
                return h;
            }

            public static Key Normalize(Key key)
            {
                string lang = AsciiUtil.ToLower(key._lang).Intern();
                string scrt = AsciiUtil.ToTitle(key._scrt).Intern();
                string regn = AsciiUtil.ToUpper(key._regn).Intern();
#if JDKIMPL
                // preserve upper/lower cases
                string vart = key._vart.Intern();
#else
                string vart = AsciiUtil.ToUpper(key._vart).Intern();
#endif
                return new Key(lang, scrt, regn, vart);
            }
        }

        private class Cache : LocaleObjectCache<Key, BaseLocale>
        {
            protected override Key NormalizeKey(Key key)
            {
                return Key.Normalize(key);
            }

            protected override BaseLocale CreateObject(Key key)
            {
                return new BaseLocale(key.Lang, key.Scrt, key.Regn, key.Vart);
            }
        }
    }
}
