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

using System.Windows.Forms;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Specifies a view that can store modifiable data, and warns the user
    /// when closing the form without saving the changes
    /// </summary>
    public class ModifiableContentView : Form, Modifiable
    {
        /// <summary>
        /// Whether the data on this view has been modified
        /// </summary>
        protected bool modified;

        /// <summary>
        /// Called to mark the view as modified
        /// </summary>
        public virtual void MarkModified()
        {
            modified = true;
        }

        /// <summary>
        /// Called to mark the view as unmodified
        /// </summary>
        public virtual void MarkUnmodified()
        {
            modified = false;
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

            this.Close();
        }

        /// <summary>
        /// Discard the changes made and closes the form
        /// </summary>
        public virtual void DiscardChangesAndClose()
        {
            this.modified = false;
            this.Close();
        }

        /// <summary>
        /// Shows a changes save confirmation to the user and returns the dialog result of the message box that was shown.
        /// IF no changes were detected, the method returns DialogResult.Yes by default.
        /// This method calls the ApplyChanges method if the user pesses Yes on the confirmation dialog box
        /// </summary>
        /// <returns>The dialog result of the message box that was shown</returns>
        public virtual DialogResult ConfirmChanges()
        {
            DialogResult diag = DialogResult.Yes;

            if (modified)
            {
                diag = MessageBox.Show(this, "There are unsaved changes. Do you wish to save before closing?", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (diag == DialogResult.Yes)
                {
                    ApplyChanges();
                }
                else if(diag == DialogResult.No)
                {
                    modified = false;
                }
            }

            return diag;
        }

        // 
        // Form Closing event handler
        // 
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (modified)
            {
                this.BringToFront();
                DialogResult diag = ConfirmChanges();

                if (diag == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }

    /// <summary>
    /// Specifies an object that can be marked as modified through a method
    /// SetModified
    /// </summary>
    public interface Modifiable
    {
        /// <summary>
        /// Marks this object as modified
        /// </summary>
        void MarkModified();
    }
}