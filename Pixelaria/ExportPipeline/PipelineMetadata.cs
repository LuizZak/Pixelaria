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

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Metadata that can be obtained from pipeline nodes and node links
    /// </summary>
    public interface IPipelineMetadata
    {
        /// <summary>
        /// Returns an object that matches a given key on this pipeline metadata.
        /// 
        /// Returns null, if key is not present.
        /// </summary>
        [CanBeNull]
        object GetValue([NotNull] string key);
        
        /// <summary>
        /// Returns whether a flag with a given key exists on this metadata object.
        /// </summary>
        bool HasFlag([NotNull] string flag);
    }

    /// <summary>
    /// Readable-writeable metadata builder
    /// </summary>
    public class PipelineMetadata : IPipelineMetadata
    {
        /// <summary>
        /// Gets an empty pipeline metadata object
        /// </summary>
        public static readonly IPipelineMetadata Empty = new PipelineMetadata();

        public HashSet<string> Flags { get; set; } = new HashSet<string>();
        public Dictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

        public PipelineMetadata()
        {
            
        }

        public PipelineMetadata(Dictionary<string, object> metadata)
        {
            Metadata = metadata;
        }

        public PipelineMetadata([NotNull] params string[] flags)
        {
            foreach (string flag in flags)
            {
                Flags.Add(flag);
            }
        }
        
        public object GetValue(string key)
        {
            return Metadata.TryGetValue(key, out object value) ? value : null;
        }

        public bool HasFlag(string flag)
        {
            return Flags.Contains(flag);
        }
    }

    /// <summary>
    /// Common static flags for pipeline metadata objects
    /// </summary>
    public static class PipelineMetadataFlags
    {
        /// <summary>
        /// Signals that a pipeline output is static and has its output value
        /// set on creation.
        /// </summary>
        public static readonly string StaticOutput = "StaticOutput";
    }

    /// <summary>
    /// Keys for pipeline metadata objects
    /// </summary>
    public static class PipelineMetadataKeys
    {
        /// <summary>
        /// A short textual description for the object.
        /// </summary>
        public static readonly string ShortDescription = "ShortDescription";

        /// <summary>
        /// A body text for a pipeline node.
        /// 
        /// Used in UI controls.
        /// </summary>
        public static readonly string PipelineStepBodyText = "PipelineStepBodyText";
    }
}