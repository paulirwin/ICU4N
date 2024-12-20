﻿using ICU4N.Util;
using J2N.IO;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ICU4N.Impl
{
    internal class ICUResourceBundleImpl : ICUResourceBundle
    {
        protected int resource;

        protected ICUResourceBundleImpl(ICUResourceBundleImpl container, string key, int resource)
            : base(container, key)
        {
            this.resource = resource;
        }
        internal ICUResourceBundleImpl(WholeBundle wholeBundle)
            : base(wholeBundle)
        {
            resource = wholeBundle.reader.RootResource;
        }
        public int GetResource()
        {
            return resource;
        }
        protected ICUResourceBundle CreateBundleObject(string _key,
                                                             int _resource,
                                                             IDictionary<string, string> aliasesVisited,
                                                             UResourceBundle requested)
        {
            switch (ICUResourceBundleReader.RES_GET_TYPE(_resource))
            {
                case UResourceType.String:
                case UResourceType.StringV2:
                    return new ICUResourceBundleImpl.ResourceString(this, _key, _resource);
                case UResourceType.Binary:
                    return new ICUResourceBundleImpl.ResourceBinary(this, _key, _resource);
                case UResourceType.Alias:
                    return GetAliasedResource(this, null, 0, _key, _resource, aliasesVisited, requested);
                case UResourceType.Int32:
                    return new ICUResourceBundleImpl.ResourceInt32(this, _key, _resource);
                case UResourceType.Int32Vector:
                    return new ICUResourceBundleImpl.ResourceInt32Vector(this, _key, _resource);
                case UResourceType.Array:
                case UResourceType.Array16:
                    return new ICUResourceBundleImpl.ResourceArray(this, _key, _resource);
                case UResourceType.Table:
                case UResourceType.Table16:
                case UResourceType.Table32:
                    return new ICUResourceBundleImpl.ResourceTable(this, _key, _resource);
                default:
                    throw new InvalidOperationException("The resource type is unknown");
            }
        }

        // Scalar values ------------------------------------------------------- ***

        private sealed class ResourceBinary : ICUResourceBundleImpl
        {
            public override UResourceType Type => UResourceType.Binary;

            public override ByteBuffer GetBinary()
            {
                return wholeBundle.reader.GetBinary(resource);
            }

            public override byte[] GetBinary(byte[] ba)
            {
                return wholeBundle.reader.GetBinary(resource, ba);
            }
            internal ResourceBinary(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
            }
        }
        private sealed class ResourceInt32 : ICUResourceBundleImpl
        {
            public override UResourceType Type => UResourceType.Int32;

            public override int GetInt32()
            {
                return ICUResourceBundleReader.RES_GET_INT(resource);
            }

            public override int GetUInt32() // ICU4N TODO: API - should this actually be uint rather than int?
            {
                return ICUResourceBundleReader.RES_GET_UINT(resource);
            }
            internal ResourceInt32(ICUResourceBundleImpl container, string key, int resource)
                    : base(container, key, resource)
            {
            }
        }
        private sealed class ResourceString : ICUResourceBundleImpl
        {
            private string value;

            public override UResourceType Type => UResourceType.String;

            public override string GetString()
            {
                if (value != null)
                {
                    return value;
                }
                return wholeBundle.reader.GetString(resource);
            }
            internal ResourceString(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
                string s = wholeBundle.reader.GetString(resource);

                // Allow the reader cache's SoftValue to do its job.
                if (s.Length < ICUResourceBundleReader.LARGE_SIZE / 2 || CacheValue<string>.FutureInstancesWillBeStrong)
                {
                    value = s;
                }
            }
        }
        private sealed class ResourceInt32Vector : ICUResourceBundleImpl
        {
            public override UResourceType Type => UResourceType.Int32Vector;

            public override int[] GetInt32Vector()
            {
                return wholeBundle.reader.GetInt32Vector(resource);
            }
            internal ResourceInt32Vector(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
            }
        }

        // Container values ---------------------------------------------------- ***

        internal abstract class ResourceContainer : ICUResourceBundleImpl
        {
            protected internal ICUResourceBundleReader.Container value;

            public override int Length => value.Length;

            public override string GetString(int index)
            {
                int res = value.GetContainerResource(wholeBundle.reader, index);
                if (res == ResBogus)
                {
                    throw new IndexOutOfRangeException();
                }
                string s = wholeBundle.reader.GetString(res);
                if (s != null)
                {
                    return s;
                }
                return base.GetString(index);
            }
            protected virtual int GetContainerResource(int index)
            {
                return value.GetContainerResource(wholeBundle.reader, index);
            }
            protected virtual UResourceBundle CreateBundleObject(int index, string resKey, IDictionary<string, string> aliasesVisited,
                                                         UResourceBundle requested)
            {
                int item = GetContainerResource(index);
                if (item == ResBogus)
                {
                    throw new IndexOutOfRangeException();
                }
                return CreateBundleObject(resKey, item, aliasesVisited, requested);
            }

            internal ResourceContainer(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
            }
            internal ResourceContainer(WholeBundle wholeBundle)
                : base(wholeBundle)
            {
            }
        }
        internal class ResourceArray : ResourceContainer
        {
            public override UResourceType Type => UResourceType.Array;

            protected override string[] HandleGetStringArray()
            {
                ICUResourceBundleReader reader = wholeBundle.reader;
                int length = value.Length;
                string[] strings = new string[length];
                for (int i = 0; i < length; ++i)
                {
                    string s = reader.GetString(value.GetContainerResource(reader, i));
                    if (s == null)
                    {
                        throw new UResourceTypeMismatchException("");
                    }
                    strings[i] = s;
                }
                return strings;
            }

            public override string[] GetStringArray()
            {
                return HandleGetStringArray();
            }

            protected override UResourceBundle HandleGet(string indexStr, IDictionary<string, string> aliasesVisited,
                                                UResourceBundle requested)
            {
                int i = int.Parse(indexStr, CultureInfo.InvariantCulture);
                return CreateBundleObject(i, indexStr, aliasesVisited, requested);
            }

            protected override UResourceBundle HandleGet(int index, IDictionary<string, string> aliasesVisited,
                                                UResourceBundle requested)
            {
                return CreateBundleObject(index, index.ToString(CultureInfo.InvariantCulture), aliasesVisited, requested);
            }
            internal ResourceArray(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
                value = wholeBundle.reader.GetArray(resource);
            }
        }
        internal class ResourceTable : ResourceContainer
        {
            public override UResourceType Type => UResourceType.Table;

            protected virtual string GetKey(int index)
            {
                return ((ICUResourceBundleReader.Table)value).GetKey(wholeBundle.reader, index);
            }
            internal override ISet<string> HandleKeySet() // ICU4N: Marked internal, since base class functionality is obsolete
            {
                ICUResourceBundleReader reader = wholeBundle.reader;
                SortedSet<string> keySet = new SortedSet<string>(StringComparer.Ordinal);
                ICUResourceBundleReader.Table table = (ICUResourceBundleReader.Table)value;
                for (int i = 0; i < table.Length; ++i)
                {
                    keySet.Add(table.GetKey(reader, i));
                }
                return keySet;
            }

            protected override UResourceBundle HandleGet(string resKey, IDictionary<string, string> aliasesVisited,
                                                UResourceBundle requested)
            {
                int i = ((ICUResourceBundleReader.Table)value).FindTableItem(wholeBundle.reader, resKey.AsSpan());
                if (i < 0)
                {
                    return null;
                }
                return CreateBundleObject(resKey, GetContainerResource(i), aliasesVisited, requested);
            }

            protected override UResourceBundle HandleGet(int index, IDictionary<string, string> aliasesVisited,
                                                UResourceBundle requested)
            {
                string itemKey = ((ICUResourceBundleReader.Table)value).GetKey(wholeBundle.reader, index);
                if (itemKey == null)
                {
                    throw new IndexOutOfRangeException();
                }
                return CreateBundleObject(itemKey, GetContainerResource(index), aliasesVisited, requested);
            }

            protected override object HandleGetObject(string key)
            {
                // Fast path for common cases: Avoid creating UResourceBundles if possible.
                // It would be even better if we could override getString(key)/getStringArray(key),
                // so that we know the expected object type,
                // but those are final in java.util.ResourceBundle.
                ICUResourceBundleReader reader = wholeBundle.reader;
                int index = ((ICUResourceBundleReader.Table)value).FindTableItem(reader, key.AsSpan());
                if (index >= 0)
                {
                    int res = value.GetContainerResource(reader, index);
                    // getString(key)
                    string s = reader.GetString(res);
                    if (s != null)
                    {
                        return s;
                    }
                    // getStringArray(key)
                    ICUResourceBundleReader.Container array = reader.GetArray(res);
                    if (array != null)
                    {
                        int length = array.Length;
                        string[] strings = new string[length];
                        for (int j = 0; ; ++j)
                        {
                            if (j == length)
                            {
                                return strings;
                            }
                            s = reader.GetString(array.GetContainerResource(reader, j));
                            if (s == null)
                            {
                                // Equivalent to resolveObject(key, requested):
                                // If this is not a string array,
                                // then build and return a UResourceBundle.
                                break;
                            }
                            strings[j] = s;
                        }
                    }
                }
                return base.HandleGetObject(key);
            }
            /// <summary>
            /// Returns a String if found, or null if not found or if the key item is not a string.
            /// </summary>
            internal virtual string FindString(string key)
            {
                ICUResourceBundleReader reader = wholeBundle.reader;
                int index = ((ICUResourceBundleReader.Table)value).FindTableItem(reader, key.AsSpan());
                if (index < 0)
                {
                    return null;
                }
                return reader.GetString(value.GetContainerResource(reader, index));
            }
            internal ResourceTable(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
                value = wholeBundle.reader.GetTable(resource);
            }
            /// <summary>
            /// Constructor for the root table of a bundle.
            /// </summary>
            internal ResourceTable(WholeBundle wholeBundle, int rootRes)
                : base(wholeBundle)
            {
                value = wholeBundle.reader.GetTable(rootRes);
            }
        }
    }
}
