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
using System.Drawing;
using System.IO;

namespace Pixelaria.Data.KeyframeMetadataSerializers
{
    public class PointKeyframeSerializer : IKeyframeValueSerializer
    {
        public Type SerializedType => typeof(Point);
        public string SerializedName => "System.Drawing.Point";

        public void Serialize(object value, Stream stream)
        {
            if(!(value is Point p))
                throw new ArgumentException($@"Expected input of type {SerializedType.Name}", nameof(value));

            var writer = new BinaryWriter(stream);
            writer.Write(p.X);
            writer.Write(p.Y);
        }

        public object Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            int x = reader.ReadInt32();
            int y = reader.ReadInt32();

            return new Point(x, y);
        }
    }
}