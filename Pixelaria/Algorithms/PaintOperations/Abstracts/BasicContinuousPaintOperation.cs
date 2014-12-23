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
        /// Gets whether this operation has been started by calling the StartOperation() method
        /// </summary>
        protected bool OperationStarted
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