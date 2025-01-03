using UnityEngine;

namespace USignals
{
    /// <summary>
    /// Utils is a global static class that wraps all the global utilities. 
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// This function will remap a number in range to another.
        /// </summary>
        /// <param name="value"> The incoming value to be converted</param>
        /// <param name="istart">Lower bound of the value's current range</param>
        /// <param name="istop"> Upper bound of the value's current range</param>
        /// <param name="ostart">Lower bound of the value's target range</param>
        /// <param name="ostop"> Upper bound of the value's target range</param>
        /// <returns>The value remapped to the new range</returns>
        static public float MapRange(float value, float istart, float istop, float ostart, float ostop)
        {
            return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
        }

        /// <summary>
        /// Wraps an index value between a specified minimum and maximum index.
        /// If the value is greater or equal than the maximum, it wraps back to the minimum,
        /// and if the value is less than the minimum, it wraps to the maximum - 1.
        /// </summary>
        /// <param name="value">The index value to wrap.</param>
        /// <param name="maxValue">The maximum index value of the range.</param>
        /// <param name="minValue">The minimum index value of the range (default is 0).</param>
        /// <returns>The wrapped index value.</returns>
        public static int WrapIndex(int value, int maxValue, int minValue = 0)
        {
            if (value >= maxValue) return minValue;
            else if (value < minValue) return maxValue - 1;
            return value;
        }

        /// <summary>
        /// Alias for white transparent color
        /// </summary>
        /// <returns>
        /// A white transparent color
        /// </returns>
        public static Color Transparent => new(255, 255, 255, 0);

        // ------------------------------------
        //             Vector3 utils
        // ------------------------------------

        /// <summary>
        /// Creates a copy of the Vector3 with optional new values for each component.
        /// </summary>
        /// <param name="vector">The original Vector3.</param>
        /// <param name="x">The optional new X value. If null, the original X value is used.</param>
        /// <param name="y">The optional new Y value. If null, the original Y value is used.</param>
        /// <param name="z">The optional new Z value. If null, the original Z value is used.</param>
        /// <returns>The copied Vector3 with the specified component values.</returns>
        public static Vector3 CopyWith(this Vector3 vector, float? x = null, float? y = null, float? z = null) => new(x ?? vector.x, y ?? vector.y, z ?? vector.z);

        /// <summary>
        /// Creates a copy of the Color with optional new values for each component.
        /// </summary>
        /// <param name="color">The original Color.</param>
        /// <param name="r">The optional new R value. If null, the original R value is used.</param>
        /// <param name="g">The optional new G value. If null, the original G value is used.</param>
        /// <param name="b">The optional new B value. If null, the original B value is used.</param>
        /// <param name="a">The optional new A value. If null, the original A value is used.</param>
        /// <returns>The copied Vector3 with the specified component values.</returns>
        public static Color CopyWith(this Color color, float? r = null, float? g = null, float? b = null, float? a = null) => new(r ?? color.r, g ?? color.g, b ?? color.b, a ?? color.a);
    }
}
