/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Pixelaria.Data.KeyframeMetadataSerializers;

namespace Pixelaria.Data
{
    public sealed class KeyframeMetadata
    {
        private static readonly List<IKeyframeValueSerializer> Serializers;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        static KeyframeMetadata()
        {
            Serializers = new List<IKeyframeValueSerializer>
            {
                new PointKeyframeSerializer()
            };
        }

        [CanBeNull]
        public object this[[NotNull] string key]
        {
            get => _values.TryGetValue(key, out var obj) ? obj : null;
            set
            {
                if(value != null && SerializerForValue(value) == null)
                    throw new InvalidOperationException($"No serializer available for value type {value.GetType().Name}. Ensure proper serializers are registered within this {nameof(KeyframeMetadata)} class during its initialization.");

                _values[key] = value;
            }
        }

        internal void CopyFrom([NotNull] KeyframeMetadata metadata)
        {
            _values.Clear();
            foreach (var keyValuePair in metadata._values)
            {
                _values[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        public IReadOnlyDictionary<string, object> GetDictionary()
        {
            return _values;
        }

        [CanBeNull]
        public static IKeyframeValueSerializer SerializerForValue(object value)
        {
            return Serializers.FirstOrDefault(s => s.SerializedType == value.GetType());
        }

        [CanBeNull]
        public static IKeyframeValueSerializer SerializerForName(string name)
        {
            return Serializers.FirstOrDefault(s => s.SerializedName == name);
        }
    }

    /// <summary>
    /// Class used to serialize keyframe values within a <see cref="KeyframeMetadata"/>
    /// </summary>
    public interface IKeyframeValueSerializer
    {
        /// <summary>
        /// Gets the type that this serializer operates upon
        /// </summary>
        Type SerializedType { get; }

        /// <summary>
        /// Gets the serialized name for identifying this serializer
        /// </summary>
        string SerializedName { get; }

        void Serialize([NotNull] object value, [NotNull] Stream stream);
        object Deserialize([NotNull] Stream stream);
    }

    /// <summary>
    /// Contains common metadata keys to use in a <see cref="KeyframeMetadata"/> object.
    /// </summary>
    public static class FrameMetadataKeys
    {
        /// <summary>
        /// A frame origin, whose value is of <see cref="Point"/> type.
        /// </summary>
        public const string FrameOrigin = "frameOrigin";
    }
}