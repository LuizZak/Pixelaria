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
using System.Windows.Forms;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Specifies a view that can store modifiable data, and warns the user
    /// when closing the form without saving the changes
    /// </summary>
    public class ModifiableContentView : Form, IModifiable
    {
        /// <summary>
        /// Whether the data on this view has been modified
        /// </summary>
        protected bool modified;

        /// <summary>
        /// Event raised whenever the state of the Modified property changes
        /// </summary>
        public event EventHandler ModifiedChanged;

        /// <summary>
        /// Gets a boolean value specifying whether the data on this view has been modified, and is pending to be saved on closing
        /// </summary>
        public bool Modified => modified;

        /// <summary>
        /// Called to mark the view as modified
        /// </summary>
        public virtual void MarkModified()
        {
            bool changed = modified == false;

            modified = true;

            if (changed)
            {
                ModifiedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to mark the view as unmodified
        /// </summary>
        public virtual void MarkUnmodified()
        {
            bool changed = modified;

            modified = false;

            if (changed)
            {
                ModifiedChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        /// <summary>
        /// Called to apply the changes made on this view
        /// </summary>
        public virtual void ApplyChanges()
        {
            MarkUnmodified();
        }

        /// <summary>
        /// Applies currently modified changes and closes the form
        /// </summary>
        public virtual void ApplyChangesAndClose()
        {
            ApplyChanges();

            Close();
        }

        /// <summary>
        /// Discard the changes made and closes the form
        /// </summary>
        public virtual void DiscardChangesAndClose()
        {
            modified = false;
            Close();
        }

        /// <summary>
        /// <para>Shows a changes save confirmation to the user and returns the dialog result of the message box that was shown.</para>
        /// <para>If no changes were detected, the method returns DialogResult.Yes by default.</para>
        /// <para>This method calls the ApplyChanges method if the user pesses Yes on the confirmation dialog box</para>
        /// </summary>
        /// <returns>The dialog result of the message box that was shown</returns>
        public virtual DialogResult ConfirmChanges()
        {
            var diag = DialogResult.Yes;

            if (!modified)
                return diag;

            diag = MessageBox.Show(this, @"There are unsaved changes. Do you wish to save before closing?", @"Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (diag == DialogResult.Yes)
            {
                ApplyChanges();
            }
            else if(diag == DialogResult.No)
            {
                modified = false;
            }

            return diag;
        }

        // 
        // Form Closing event handler
        // 
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (!modified)
                return;

            BringToFront();
            var diag = ConfirmChanges();

            if (diag == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }

    /// <summary>
    /// Specifies an object that can be marked as modified through a method
    /// MarkModified
    /// </summary>
    public interface IModifiable
    {
        /// <summary>
        /// Gets a boolean value specifying whether the data on this modifiable object has been modified, and is pending to be saved or applied
        /// </summary>
        bool Modified { get; }

        /// <summary>
        /// Marks this object as modified
        /// </summary>
        void MarkModified();
    }
}