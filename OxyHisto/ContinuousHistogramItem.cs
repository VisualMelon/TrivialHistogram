
namespace OxyPlot.Series
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an item in a <see cref="ContinuousHistogramSeries" />, a bin (range) and its area.
    /// </summary>
    public class ContinuousHistogramItem
    {
        /// <summary>
        /// A range in a histogram, and it's area
        /// </summary>
        /// <param name="rangeStart">The range start.</param>
        /// <param name="rangeEnd">The range end.</param>
        /// <param name="area">The area.</param>
        /// <param name="color">The color.</param>
        public ContinuousHistogramItem(double rangeStart, double rangeEnd, double area, OxyColor color)
        {
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
            Area = area;
            this.Color = color;
        }
        
        /// <summary>
        /// A range in a histogram, and it's area
        /// </summary>
        /// <param name="rangeStart">The range start.</param>
        /// <param name="rangeEnd">The range end.</param>
        /// <param name="area">The area.</param>
        public ContinuousHistogramItem(double rangeStart, double rangeEnd, double area)
        {
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
            Area = area;

            this.Color = OxyColors.Automatic;
        }

        /// <summary>
        /// Gets or sets the color of the item.
        /// </summary>
        /// <remarks>If the color is not specified (default), the color of the series will be used.</remarks>
        public OxyColor Color { get; set; }

        /// <summary>
        /// Gets or sets the range start.
        /// </summary>
        /// <value>The range start.</value>
        public double RangeStart { get; set; }
        
        /// <summary>
        /// Gets or sets the range end.
        /// </summary>
        /// <value>The range end.</value>
        public double RangeEnd { get; set; }
        
        /// <summary>
        /// Gets or sets the area.
        /// </summary>
        /// <value>The area.</value>
        public double Area { get; set; }

        /// <summary>
        /// Gest the computed width of the item.
        /// </summary>
        public double Width => this.RangeEnd - this.RangeStart;
        
        /// <summary>
        /// Gest the computed height of the item.
        /// </summary>
        public double Height => this.Area / this.Width;
        
        /// <summary>
        /// Gets the value of the item. Equivalent to the Height;
        /// </summary>
        /// <value>The value can be used to color-code the rectangle.</value>
        public double Value => Height;

        /// <summary>
        /// Determines whether the specified point lies within the boundary of the rectangle.
        /// </summary>
        /// <returns><c>true</c> if the value of the <param name="p"/> parameter is inside the bounds of this instance.</returns>
        public bool Contains(DataPoint p)
        {
            return (p.X <= this.RangeEnd && p.X >= this.RangeStart && p.Y <= this.Height && p.Y >= 0) ||
                   (p.X <= this.RangeStart && p.X >= this.RangeEnd && p.Y <= this.Height && p.Y >= 0);
        }
        
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format(
                "{0} {1} {2}",
                this.RangeStart,
                this.RangeEnd,
                this.Area);
        }
    }

    public static class HistogramHelpers
    {
        public static IEnumerable<ContinuousHistogramItem> Collect(IEnumerable<double> samples, double start, double end, int binCount, bool countUnplaced)
        {
            List<double> binBreaks = new List<double>(binCount);

            for (int i = 0; i <= binCount; i++)
            {
                binBreaks.Add(start + ((end - start) / binCount) * i);
            }

            return Collect(samples, binBreaks, countUnplaced);
        }

        public static IEnumerable<ContinuousHistogramItem> Collect(IEnumerable<double> samples, IReadOnlyList<double> binBreaks, bool countUnplaced)
        {
            // determin ranges
            double[] orderedBreaks = binBreaks.Distinct().OrderBy(b => b).ToArray(); // TODO: resolve distinct

            // count samples
            List<int> counts = new List<int>();
            long total = 0;
            
            for (int i = 0; i < binBreaks.Count - 1; i++)
            {
                counts.Add(0);
            }

            foreach (double sample in samples)
            {
                int idx = System.Array.BinarySearch(orderedBreaks, sample);

                bool placed = false;
                
                if (idx >= 0)
                {
                    // exact match, place in the corresponding bin (exclude last bin)
                    if (idx < counts.Count)
                    {
                        counts[idx] += 1;
                        placed = true;
                    }
                }
                else
                {
                    // inexact match, place in lower bin
                    idx = ~idx - 1;
                    
                    if (idx >= 0 && idx < counts.Count)
                    {
                        counts[idx] += 1;
                        placed = true;
                    }
                }

                if (placed || countUnplaced)
                {
                    total++;
                }
            }

            // create items
            List<ContinuousHistogramItem> items = new List<ContinuousHistogramItem>(counts.Count);
            
            for (int i = 0; i < binBreaks.Count - 1; i++)
            {
                items.Add(new ContinuousHistogramItem(binBreaks[i], binBreaks[i + 1], (double)counts[i] / total));
            }

            return items;
        }
    }
}