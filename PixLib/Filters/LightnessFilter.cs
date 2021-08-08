using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using FastBitmapLib;
using JetBrains.Annotations;
using PixCore.Colors;

namespace PixLib.Filters
{
    /// <summary>
    /// Implements a Lightness alteration filter
    /// </summary>
    public class LightnessFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying => !(Lightness == 0 && Relative) && !(Lightness == 100 && Multiply);

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name => "Lightness";

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        public int Version => 1;

        /// <summary>
        /// Lightness value ranging from 0 - 100
        /// </summary>
        public int Lightness { get; set; }

        /// <summary>
        /// Gets or sets whether the changes made by this HSL filter are relative to current values
        /// </summary>
        public bool Relative { get; set; }

        /// <summary>
        /// Gets or sets whether to multiply the current values instead of adding to them
        /// </summary>
        public bool Multiply { get; set; }

        /// <summary>
        /// Initializes a new instance of the LightnessFilter class
        /// </summary>
        public LightnessFilter()
        {
            Lightness = 100;
            Relative = false;
            Multiply = false;
        }

        /// <summary>
        /// Applies this LightnessFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this LightnessFilter to</param>
        public unsafe void ApplyToBitmap(Bitmap bitmap)
        {
            // 
            // !!!   ATENTION: UNSAFE POINTER HANDLING    !!!
            // !!! WATCH IT WHEN MESSING WITH THIS METHOD !!!
            // 

            if (!Modifying || bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            // Lock the bitmap
            using (var fastBitmap = bitmap.FastLock())
            {
                int* scan0 = (int*) fastBitmap.Scan0;
                int count = bitmap.Width * bitmap.Height;

                float lightF = Lightness / 100.0f;

                while (count-- > 0)
                {
                    var ahsl = AhslColor.FromArgb(*scan0);
                    float l;

                    if (Multiply)
                    {
                        l = ahsl.FloatLightness * lightF;
                    }
                    else
                    {
                        l = Relative ? ahsl.FloatLightness + lightF : lightF;
                    }

                    *scan0++ = new AhslColor(ahsl.FloatAlpha, ahsl.FloatHue, ahsl.FloatSaturation, l).ToArgb();
                }
            }
        }

        /// <summary>
        /// Array of property infos from this <see cref="IFilter"/> that can be inspected and set using reflection.
        /// 
        /// Used by export pipeline UI to streamlining process of creating pipeline nodes based off of filters.
        /// </summary>
        public PropertyInfo[] InspectableProperties()
        {
            return new[]
            {
                GetType().GetProperty(nameof(Lightness)),
                GetType().GetProperty(nameof(Relative)),
                GetType().GetProperty(nameof(Multiply))
            };
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream([NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(Lightness);
            writer.Write(Relative);
            writer.Write(Multiply);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        /// <param name="version">The version of the filter data that is stored on the stream</param>
        public void LoadFromStream([NotNull] Stream stream, int version)
        {
            var reader = new BinaryReader(stream);

            Lightness = reader.ReadInt32();
            Relative = reader.ReadBoolean();
            Multiply = reader.ReadBoolean();
        }

        public bool Equals(IFilter filter)
        {
            return filter is LightnessFilter other && Lightness == other.Lightness && Relative == other.Relative &&
                   Multiply == other.Multiply && Version == other.Version;
        }
    }
}