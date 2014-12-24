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

namespace Pixelaria.Algorithms.PaintOperations.Abstracts
{
    /// <summary>
    /// Specifies a paint operation that requires a StartOperation() and FinishOperation() calls before being able to perform and task
    /// </summary>
    public class BasicContinuousPaintOperation
    {
        /// <summary>
        /// The target bitmap for the operation
        /// </summary>
        protected Bitmap targetBitmap;

        /// <summary>
        /// Whether this operation has been started by calling the StartOperation() method
        /// </summary>
        protected bool operationStarted;

        /// <summary>
        /// Gets or sets the target bitmap for this operation.
        /// If the operation is currently being performed, a call to the setter will trigger an InvalidOperationException
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The target bitmap is modified while the operation is already started</exception>
        public virtual Bitmap TargetBitmap
        {
            get { return targetBitmap; }
            set
            {
                if (operationStarted)
                {
                    throw new InvalidOperationException("The target bitmap cannot be modified while the operation is being performed");
                }

                targetBitmap = value;
            }
        }

        /// <summary>
        /// Gets whether this operation has been started by calling the StartOperation() method
        /// </summary>
        public bool OperationStarted
        {
            get { return operationStarted; }
        }

        /// <summary>
        /// Intiailzies a new instance of the BasicContinuousPaintOperation class, with a target bitmap for the operation
        /// </summary>
        /// <param name="targetBitmap">The bitmap to perform the operations on</param>
        public BasicContinuousPaintOperation(Bitmap targetBitmap)
        {
            this.targetBitmap = targetBitmap;
        }

        /// <summary>
        /// Starts this continuous paint operation
        /// </summary>
        public virtual void StartOpertaion()
        {
            operationStarted = true;
        }

        /// <summary>
        /// Finishes this continuous paint operation
        /// </summary>
        public virtual void FinishOperation()
        {
            operationStarted = false;
        }
    }
}