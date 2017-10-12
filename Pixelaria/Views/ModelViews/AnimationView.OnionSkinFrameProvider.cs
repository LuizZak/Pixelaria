using System.Drawing;
using Pixelaria.Views.ModelViews.Decorators;

namespace Pixelaria.Views.ModelViews
{
    public partial class AnimationView : IOnionSkinFrameProvider
    {
        public int FrameCount => ViewAnimation.FrameCount;

        public Bitmap GetComposedBitmapForFrame(int index)
        {
            var id = ViewAnimation.GetFrameAtIndex(index);
            var frame = ViewAnimation.GetFrameController(id);

            return frame.GetComposedBitmap();
        }
    }
}
