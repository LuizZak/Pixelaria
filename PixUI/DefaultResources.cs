﻿/*
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

using JetBrains.Annotations;
using PixRendering;
using PixUI.Properties;

namespace PixUI
{
    public class DefaultResources
    {
        /// <summary>
        /// Loads the default image resources into a given resource manager.
        /// </summary>
        public static void LoadDefaultResources([NotNull] IImageResourceManager imageResourceManager)
        {
            Images.CancelButton = imageResourceManager.CreateManagedImageResource(Resources.cancel_button);
            Images.CancelButtonDark = imageResourceManager.CreateManagedImageResource(Resources.cancel_button_dark);
        }

        public static class Images
        {
            public static IManagedImageResource CancelButton;
            public static IManagedImageResource CancelButtonDark;
        }
    }
}
