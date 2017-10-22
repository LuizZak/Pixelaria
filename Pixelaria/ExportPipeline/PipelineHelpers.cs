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

using System.Linq;
using JetBrains.Annotations;

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Helper static extensions for IPipeline* implementers
    /// </summary>
    public static class PipelineHelpers
    {
        /// <summary>
        /// Connects the first Output from <see cref="step"/> that matches the first Input from
        /// <see cref="other"/>.
        /// </summary>
        /// <returns>true if a connection was made; false otherwise.</returns>
        public static bool ConnectTo([NotNull] this IPipelineStep step, IPipelineStep other)
        {
            // Find first matching output from this that matches an input from other
            foreach (var output in step.Output)
            {
                foreach (var input in other.Input)
                {
                    if (input.Connections.Contains(output))
                        continue;

                    if (!input.CanConnect(output)) continue;

                    input.Connect(output);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Connects the first Output from <see cref="step"/> that matches the first Input from
        /// <see cref="other"/>.
        /// </summary>
        /// <returns>true if a connection was made; false otherwise.</returns>
        public static bool ConnectTo([NotNull] this IPipelineStep step, IPipelineEnd other)
        {
            // Find first matching output from this that matches an input from other
            foreach (var output in step.Output)
            {
                foreach (var input in other.Input)
                {
                    if (input.Connections.Contains(output))
                        continue;

                    if (!input.CanConnect(output)) continue;

                    input.Connect(output);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether a pipeline input can be connected to an output.
        /// 
        /// The method looks through the proper data types accepted by the input and the data type
        /// of the output to make the decision.
        /// </summary>
        public static bool CanConnect([NotNull] this IPipelineInput input, IPipelineOutput output)
        {
            return input.DataTypes.Any(type => type.IsAssignableFrom(output.DataType));
        }
    }
}
