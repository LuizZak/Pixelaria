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
    /// Implements a Hue alteration filter
    /// </summary>
    public class SaturationFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying => !(Saturation == 0 && Relative) && !(Saturation == 100 && Multiply);

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name => "Saturation";

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        public int Version => 1;

        /// <summary>
        /// Saturation value ranging from 0 - 100
        /// </summary>
        public int Saturation { get; set; }

        /// <summary>
        /// Gets or sets whether the changes made by this HSL filter are relative to current values
        /// </summary>
        public bool Relative { get; set; }

        /// <summary>
        /// Gets or sets whether to keep the grays
        /// </summary>
        public bool KeepGrays { get; set; }

        /// <summary>
        /// Gets or sets whether to multiply the current values instead of adding to them
        /// </summary>
        public bool Multiply { get; set; }

        /// <summary>
        /// Initializes a new instance of the SaturationFilter class
        /// </summary>
        public SaturationFilter()
        {
            Saturation = 100;
            Relative = false;
            KeepGrays = true;
            Multiply = false;
        }

        /// <summary>
        /// Applies this SaturationFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this SaturationFilter to</param>
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

                float satF = Saturation / 100.0f;

                while (count-- > 0)
                {
                    var ahsl = AhslColor.FromArgb(*scan0);
                    float s = ahsl.FloatSaturation;

                    if (!KeepGrays || ahsl.FloatSaturation > 0)
                    {
                        if (Multiply)
                        {
                            s = ahsl.FloatSaturation * satF;
                        }
                        else
                        {
                            s = Relative ? ahsl.FloatSaturation + satF : satF;
                        }
                    }

                    *scan0++ = new AhslColor(ahsl.FloatAlpha, ahsl.FloatHue, s, ahsl.FloatLightness).ToArgb();
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
                GetType().GetProperty(nameof(Saturation)),
                GetType().GetProperty(nameof(Relative)),
                GetType().GetProperty(nameof(KeepGrays)),
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

            writer.Write(Saturation);
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

            Saturation = reader.ReadInt32();
            Relative = reader.ReadBoolean();
            Multiply = reader.ReadBoolean();
        }

        public bool Equals(IFilter filter)
        {
            return filter is SaturationFilter other && Saturation == other.Saturation && Relative == other.Relative &&
                   KeepGrays == other.KeepGrays && Multiply == other.Multiply && Version == other.Version;
        }
    }
}