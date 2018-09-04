namespace ZoomIn.Utility
{
	using System;
	using System.Collections.Generic;

	public static class ArrayExtensions
	{
		/// <summary>
		/// Creates an <see cref="IEnumerable{T}"/> over the elements in a specified section of the array.
		/// </summary>
		/// <param name="array">The array on which to operate.</param>
		/// <param name="startIndex">The array startIndex from which to begin.</param>
		/// <param name="count">The number of array elements to include.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> over the elements in a specified section of the array.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is outside the range of valid indexes for <paramref name="array"/>
		/// -or- count is less than zero. -or- startIndex and count do not specify a valid section in
		/// <paramref name="array"/>.
		/// </exception>
		public static IEnumerable<T> AsEnumerable<T>(this T[] array, int startIndex, int count)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (0 > startIndex || startIndex >= array.Length)
				throw new ArgumentOutOfRangeException("startIndex");
			if (0 >= count || count > array.Length)
				throw new ArgumentOutOfRangeException("count");

			var endIndex = startIndex + count;
			if (endIndex > array.Length)
				throw new ArgumentOutOfRangeException();

			for (var index = startIndex; index < endIndex; ++index)
				yield return array[index];
		}
	}
}
