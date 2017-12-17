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
using PixCore.Geometry;

namespace PixUI.Controls
{
    /// <summary>
    /// A root control view that is used as the base hierarchy of <see cref="Pixelaria.Views.ExportPipeline.ExportPipelineFeatures.ControlViewFeature"/> instances.
    /// 
    /// Exposes methods for working with first responder status of controls.
    /// </summary>
    public class RootControlView : BaseView
    {
        private readonly IFirstResponderDelegate<IEventHandler> _firstResponderDelegate;

        public RootControlView(IFirstResponderDelegate<IEventHandler> firstResponderDelegate)
        {
            _firstResponderDelegate = firstResponderDelegate;
        }

        public bool SetFirstResponder([CanBeNull] IEventHandler handler, bool force = false)
        {
            return _firstResponderDelegate.SetAsFirstResponder(handler, force);
        }

        public void RemoveAsFirstResponder([NotNull] IEventHandler handler)
        {
            if(IsFirstResponder(handler))
                _firstResponderDelegate.RemoveCurrentResponder();
        }

        public bool IsFirstResponder([NotNull] IEventHandler handler)
        {
            return _firstResponderDelegate.IsFirstResponder(handler);
        }
        
        /// <summary>
        /// Returns the first control view under a given point on this root control view.
        /// 
        /// Returns null, if no control was found.
        /// </summary>
        /// <param name="point">Point to hit-test against, in local coordinates of this <see cref="RootControlView"/></param>
        /// <param name="enabledOnly">Whether to only consider views that have interactivity enabled. See <see cref="ControlView.InteractionEnabled"/></param>
        [CanBeNull]
        public ControlView HitTestControl(Vector point, bool enabledOnly = true)
        {
            var views = children.OfType<ControlView>();

            foreach (var controlView in views)
            {
                var control = controlView.HitTestControl(point, enabledOnly);
                if (control != null)
                    return control;
            }

            return null;
        }
    }

    /// <summary>
    /// For objects that manage first responder hierarchies of <see cref="RootControlView"/>.
    /// </summary>
    /// <typeparam name="T">The type of first responder (must be an object derived of <see cref="IEventHandler"/>) to handle.</typeparam>
    public interface IFirstResponderDelegate<in T> where T: IEventHandler
    {
        /// <summary>
        /// Sets a given object as the current first responder.
        /// 
        /// If null, simply resigns first responder status of current first responder.
        /// 
        /// Returns a value saying whether setting the new first responder resulted successfully.
        /// </summary>
        /// <param name="firstResponder">The new first responder to assign. If null, simply resigns current responder without associating a new one</param>
        /// <param name="force">If it should force resigning of any current first responder. Doesn't guarantees that the new first responder will be assigned,
        /// but guarantees that any <i>current</i> will be resigned.</param>
        bool SetAsFirstResponder([CanBeNull] T firstResponder, bool force);

        // TODO: This RemoveCurrentResponder is a hack around nullifying controls when assigning a new first responder.
        // This should be reseen later!

        /// <summary>
        /// For any currently set first responder, removes it as the current first responder.
        /// 
        /// Does not call <see cref="IEventHandler.ResignFirstResponder"/>, it simply nullifies the first
        /// responder straight away without making any checks.
        /// 
        /// Removes even if <see cref="IEventHandler.CanResignFirstResponder"/> currently returns false for the current first responder.
        /// </summary>
        void RemoveCurrentResponder();

        /// <summary>
        /// Queries the first responder delegate if a given event handler is its current
        /// first responder.
        /// </summary>
        bool IsFirstResponder([NotNull] T handler);
    }
}
