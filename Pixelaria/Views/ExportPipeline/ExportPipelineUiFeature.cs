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
using System.Windows.Forms;
using JetBrains.Annotations;
using PixDirectX.Rendering;
using PixUI;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// Base class for managing UI states/functionalities based on current states of UI and keyboard/mouse.
    /// 
    /// Multiple states can be on simultaneously to satisfy multiple rendering needs.
    /// 
    /// Each state modifies the final rendering state using decorators.
    /// </summary>
    internal abstract class ExportPipelineUiFeature
    {
        /// <summary>
        /// Control to manage
        /// </summary>
        [NotNull]
        protected readonly IExportPipelineControl Control;

        /// <summary>
        /// When an <see cref="ExportPipelineControl"/> calls one of the event handlers
        /// On[...] (like OnMouseDown, OnMouseLeave, OnKeyDown, etc., except <see cref="OnResize"/> and
        /// <see cref="OnFixedFrame"/>) this flag is reset to false on all pipeline UI 
        /// features, and after every feature's event handler call this flag is checked to 
        /// stop calling the event handler on further features.
        /// </summary>
        public bool IsEventConsumed { get; set; }

        /// <summary>
        /// States whether this pipeline feature has exclusive UI control.
        /// 
        /// Exclusive UI control should be requested whenever a UI feature wants to do
        /// work that adds/removes views, modifies the position/scaling of views on 
        /// screen etc., which could otherwise interfere with other features' functionalities
        /// if they happen concurrently.
        /// </summary>
        protected bool hasExclusiveControl => Control.CurrentExclusiveControlFeature() == this;

        protected IPipelineContainer container => Control.PipelineContainer;
        protected BaseView contentsView => container.ContentsView;
        protected BaseView uiContainerView => container.UiContainerView;

        protected ExportPipelineUiFeature([NotNull] IExportPipelineControl control)
        {
            Control = control;
        }

        // TODO: Method bellow is a hack, used to work around the fact control focus works only on
        // ControlViewFeature and its components, but not across all export pipeline features.
        // Maybe someday extend control focusing to allow it to happen between all export pipeline
        // features of an export pipeline control.

        /// <summary>
        /// Called when another feature has consumed the <see cref="OnMouseDown"/> event.
        /// </summary>
        public virtual void OtherFeatureConsumedMouseDown() { }

        /// <summary>
        /// Called on a fixed 8ms (on average) interval across all UI features to perform fixed-update operations.
        /// </summary>
        public virtual void OnFixedFrame([NotNull] EventArgs e) { }

        public virtual void OnResize([NotNull] ResizeEventArgs e) { }

        #region Mouse Events

        public virtual void OnMouseLeave([NotNull] EventArgs e) { }
        public virtual void OnMouseClick([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseDoubleClick([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseDown([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseUp([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseMove([NotNull] MouseEventArgs e) { }
        public virtual void OnMouseEnter([NotNull] EventArgs e) { }
        public virtual void OnMouseWheel([NotNull] MouseEventArgs e) { }

        #endregion

        #region Keyboard Events

        public virtual void OnKeyDown([NotNull] KeyEventArgs e) { }
        public virtual void OnKeyUp([NotNull] KeyEventArgs e) { }
        public virtual void OnKeyPress([NotNull] KeyPressEventArgs e) { }
        public virtual void OnPreviewKeyDown([NotNull] PreviewKeyDownEventArgs e) { }
        #endregion

        /// <summary>
        /// Consumes the current event handler call such that no further UI features
        /// receive it on the current event loop.
        /// 
        /// See <see cref="IsEventConsumed"/> for more info.
        /// </summary>
        protected void ConsumeEvent()
        {
            IsEventConsumed = true;
        }

        /// <summary>
        /// Shortcut for <see cref="ExportPipelineControl.FeatureRequestedExclusiveControl"/>, returning whether
        /// exclusive control was granted.
        /// </summary>
        protected bool RequestExclusiveControl()
        {
            return Control.FeatureRequestedExclusiveControl(this);
        }

        /// <summary>
        /// Shortcut for <see cref="ExportPipelineControl.WaiveExclusiveControl"/>.
        /// 
        /// Returns exclusive control back for new requesters to pick on.
        /// </summary>
        protected void ReleaseExclusiveControl()
        {
            Control.WaiveExclusiveControl(this);
        }

        /// <summary>
        /// Returns true if any feature other than this one currently has exclusive control set.
        /// 
        /// Returns false, if no control currently has exclusive control.
        /// </summary>
        protected bool OtherFeatureHasExclusiveControl()
        {
            var feature = Control.CurrentExclusiveControlFeature();
            return feature != null && feature != this;
        }

        /// <summary>
        /// Executes an action under a temporary exclusive control lock.
        /// 
        /// If no lock could be obtained, the method returns without calling <see cref="action"/>.
        /// </summary>
        protected void ExecuteWithTemporaryExclusiveControl([InstantHandle] Action action)
        {
            if (!RequestExclusiveControl())
                return;

            action();

            ReleaseExclusiveControl();
        }
    }

    /// <summary>
    /// Event args for a resize event
    /// </summary>
    public class ResizeEventArgs : EventArgs
    {
        public Size Size { get; }

        public ResizeEventArgs(Size size)
        {
            Size = size;
        }
    }
}