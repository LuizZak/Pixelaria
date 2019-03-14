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

namespace Pixelaria.DXSupport
{
    /// <summary>
    /// Class for storage of static DirectX-related support code
    /// </summary>
    public static class DxSupport
    {
        private static SharpDX.Direct3D11.Device _d3DDevice;

        /// <summary>
        /// A global <see cref="SharpDX.Direct2D1.Factory"/> instance to use on all Direct2D-related calls
        /// </summary>
        public static readonly SharpDX.Direct2D1.Factory D2DFactory = new SharpDX.Direct2D1.Factory();

        /// <summary>
        /// A global <see cref="SharpDX.Direct3D11.Device"/> instance to use on all Direct2D-related calls
        /// </summary>
        public static SharpDX.Direct3D11.Device D3DDevice
        {
            get
            {
                if (_d3DDevice != null)
                    return _d3DDevice;

                var featureLevels = new[]
                {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                    SharpDX.Direct3D.FeatureLevel.Level_10_0,
                    SharpDX.Direct3D.FeatureLevel.Level_9_3
                };
                var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport;
#if DEBUG
                creationFlags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
#endif

                _d3DDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, creationFlags, featureLevels);

                return _d3DDevice;
            }
        }
    }
}
