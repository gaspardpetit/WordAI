using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordAI
{
	public static class RangeExtension
	{
		public static Range NewRange(this Range range, int newStart, int newEnd)
		{
			Range newRange = range.Duplicate;
			newRange.SetRange(newStart, newEnd);
			return newRange;
		}
	}
}
