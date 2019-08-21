using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend2DCS
{
    /// <summary>
    /// Geometry type.
    ///
    /// Geometry describes a shape or path that can be either rendered or added to
    /// a <see cref="BLPath"/> container. Both <see cref="BLPath"/> and <see cref="BLContext"/>
    /// provide functionality to work with all geometry types. Please note that
    /// each type provided here requires to pass a matching struct or class to
    /// the function that consumes a `geometryType` and `geometryData` arguments.
    /// </summary>
    public enum BLGeometryType : uint
    {
        /// <summary>
        /// No geometry provided.
        /// </summary>
        None = 0,
        /// <summary>
        /// BLBoxI struct.
        /// </summary>
        BoxI = 1,
        /// <summary>
        /// BLBox struct.
        /// </summary>
        BoxD = 2,
        /// <summary>
        /// BLRectI struct.
        /// </summary>
        RectI = 3,
        /// <summary>
        /// BLRect struct.
        /// </summary>
        RectD = 4,
        /// <summary>
        /// BLCircle struct.
        /// </summary>
        Circle = 5,
        /// <summary>
        /// BLEllipse struct.
        /// </summary>
        Ellipse = 6,
        /// <summary>
        /// BLRoundRect struct.
        /// </summary>
        RoundRect = 7,
        /// <summary>
        /// BLArc struct.
        /// </summary>
        Arc = 8,
        /// <summary>
        /// BLArc struct representing chord.
        /// </summary>
        Chord = 9,
        /// <summary>
        /// BLArc struct representing pie.
        /// </summary>
        Pie = 10,
        /// <summary>
        /// BLLine struct.
        /// </summary>
        Line = 11,
        /// <summary>
        /// BLTriangle struct.
        /// </summary>
        Triangle = 12,
        /// <summary>
        /// BLArrayView&lt;BLPointI&gt; representing a polyline.
        /// </summary>
        PolylineI = 13,
        /// <summary>
        /// BLArrayView&lt;BLPoint&gt; representing a polyline.
        /// </summary>
        PolylineD = 14,
        /// <summary>
        /// BLArrayView&lt;BLPointI&gt; representing a polygon.
        /// </summary>
        PolygonI = 15,
        /// <summary>
        /// BLArrayView&lt;BLPoint&gt; representing a polygon.
        /// </summary>
        PolygonD = 16,
        /// <summary>
        /// BLArrayView&lt;BLBoxI&gt; struct.
        /// </summary>
        ArrayViewBoxI = 17,
        /// <summary>
        /// BLArrayView&lt;BLBox&gt; struct.
        /// </summary>
        ArrayViewBoxD = 18,
        /// <summary>
        /// BLArrayView&lt;BLRectI&gt; struct.
        /// </summary>
        ArrayViewRectI = 19,
        /// <summary>
        /// BLArrayView&lt;BLRect&gt; struct.
        /// </summary>
        ArrayViewRectD = 20,
        /// <summary>
        /// BLPath (or BLPathCore).
        /// </summary>
        Path = 21,
        /// <summary>
        /// BLRegion (or BLRegionCore).
        /// </summary>
        Region = 22
    }
}
