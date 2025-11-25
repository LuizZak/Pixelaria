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
        private static Vortice.Direct3D11.ID3D11Device _d3DDevice;

        /// <summary>
        /// A global <see cref="SharpDX.Direct2D1.Factory"/> instance to use on all Direct2D-related calls
        /// </summary>
        public static readonly Vortice.Direct2D1.ID2D1Factory D2DFactory = Vortice.Direct2D1.D2D1.D2D1CreateFactory<Vortice.Direct2D1.ID2D1Factory>();

        /// <summary>
        /// A global <see cref="SharpDX.Direct3D11.Device"/> instance to use on all Direct2D-related calls
        /// </summary>
        public static Vortice.Direct3D11.ID3D11Device D3DDevice
        {
            get
            {
                if (_d3DDevice != null)
                    return _d3DDevice;

                var featureLevels = new[]
                {
                    Vortice.Direct3D.FeatureLevel.Level_11_1,
                    Vortice.Direct3D.FeatureLevel.Level_11_0,
                    Vortice.Direct3D.FeatureLevel.Level_10_1,
                    Vortice.Direct3D.FeatureLevel.Level_10_0,
                    Vortice.Direct3D.FeatureLevel.Level_9_3
                };
                var creationFlags = Vortice.Direct3D11.DeviceCreationFlags.BgraSupport;
#if DEBUG
                creationFlags |= Vortice.Direct3D11.DeviceCreationFlags.Debug;
#endif

                Vortice.Direct3D11.D3D11.D3D11CreateDevice(null, Vortice.Direct3D.DriverType.Hardware, creationFlags, featureLevels, out _d3DDevice);
                return _d3DDevice;
            }
        }
    }
}
