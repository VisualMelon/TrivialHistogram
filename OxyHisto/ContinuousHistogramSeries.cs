﻿namespace OxyPlot.Series
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Axes;

    /// <summary>
    /// Represents a series that can be bound to a collection of <see cref="ContinuousHistogramItem"/>.
    /// </summary>
    public class ContinuousHistogramSeries : XYAxisSeries
    {
        /// <summary>
        /// The default fill color.
        /// </summary>
        private OxyColor defaultFillColor;
        
        /// <summary>
        /// Gets or sets the color of the interior of the bars.
        /// </summary>
        /// <value>The color.</value>
        public OxyColor FillColor { get; set; }

        /// <summary>
        /// Gets the actual fill color.
        /// </summary>
        /// <value>The actual color.</value>
        public OxyColor ActualFillColor
        {
            get { return this.FillColor.GetActualColor(this.defaultFillColor); }
        }

        /// <summary>
        /// Gets or sets the color of the interior of the bars when the value is negative.
        /// </summary>
        /// <value>The color.</value>
        public OxyColor NegativeFillColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the border around the bars.
        /// </summary>
        /// <value>The color of the stroke.</value>
        public OxyColor StrokeColor { get; set; }

        /// <summary>
        /// Gets or sets the thickness of the bar border strokes.
        /// </summary>
        /// <value>The stroke thickness.</value>
        public double StrokeThickness { get; set; }

        /// <summary>
        /// The items originating from the items source.
        /// </summary>
        private List<ContinuousHistogramItem> actualItems;

        /// <summary>
        /// Specifies if the <see cref="actualItems" /> list can be modified.
        /// </summary>
        private bool ownsActualItems;

        /// <summary>
        /// The default tracker format string
        /// </summary>
        public new const string DefaultTrackerFormatString = "{0}\n{1}: {2}\n{3}: {4}\n{5}: {6}";

        /// <summary>
        /// Initializes a new instance of the <see cref="HeatMapSeries" /> class.
        /// </summary>
        public ContinuousHistogramSeries()
        {
            this.FillColor = OxyColors.Automatic;
            this.NegativeFillColor = OxyColors.Undefined;
            this.StrokeColor = OxyColors.Black;
            this.StrokeThickness = 0;
            this.TrackerFormatString = DefaultTrackerFormatString;
            this.LabelFormatString = "0.00";
            this.LabelFontSize = 0;
        }

        /// <summary>
        /// Gets the minimum value of the dataset.
        /// </summary>
        public double MinValue { get; private set; }

        /// <summary>
        /// Gets the maximum value of the dataset.
        /// </summary>
        public double MaxValue { get; private set; }

        /// <summary>
        /// Gets or sets the format string for the cell labels. The default value is <c>0.00</c>.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>The label format string is only used when <see cref="LabelFontSize" /> is greater than 0.</remarks>
        public string LabelFormatString { get; set; }

        /// <summary>
        /// Gets or sets the font size of the labels. The default value is <c>0</c> (labels not visible).
        /// </summary>
        /// <value>The font size relative to the cell height.</value>
        public double LabelFontSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tracker can interpolate points.
        /// </summary>
        public bool CanTrackerInterpolatePoints { get; set; }
        
        /// <summary>
        /// Gets or sets the delegate used to map from <see cref="ItemsSeries.ItemsSource" /> to <see cref="ContinuousHistogramSeries" />. The default is <c>null</c>.
        /// </summary>
        /// <value>The mapping.</value>
        /// <remarks>Example: series1.Mapping = item => new ContinuousHistogramItem((double)item.BinStart, (double)item.BinStart + item.BinWidth, (double)item.Count);</remarks>
        public Func<object, ContinuousHistogramItem> Mapping { get; set; }

        /// <summary>
        /// Gets the list of ContinuousHistogramSeries.
        /// </summary>
        /// <value>A list of <see cref="ContinuousHistogramItem" />. This list is used if <see cref="ItemsSeries.ItemsSource" /> is not set.</value>
        public List<ContinuousHistogramItem> Items { get; } = new List<ContinuousHistogramItem>();

        /// <summary>
        /// Gets the list of ContinuousHistogramSeries that should be rendered.
        /// </summary>
        /// <value>A list of <see cref="RectangleItem" />.</value>
        protected List<ContinuousHistogramItem> ActualItems => this.ItemsSource != null ? this.actualItems : this.Items;

        /// <summary>
        /// Transforms data space coordinates to orientated screen space coordinates.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The transformed point.</returns>
        public new ScreenPoint Transform(double x, double y)
        {
            return this.Orientate(base.Transform(x, y));
        }

        /// <summary>
        /// Transforms data space coordinates to orientated screen space coordinates.
        /// </summary>
        /// <param name="point">The point to transform.</param>
        /// <returns>The transformed point.</returns>
        public new ScreenPoint Transform(DataPoint point)
        {
            return this.Orientate(base.Transform(point));
        }

        /// <summary>
        /// Transforms orientated screen space coordinates to data space coordinates.
        /// </summary>
        /// <param name="point">The point to inverse transform.</param>
        /// <returns>The inverse transformed point.</returns>
        public new DataPoint InverseTransform(ScreenPoint point)
        {
            return base.InverseTransform(this.Orientate(point));
        }

        /// <summary>
        /// Renders the series on the specified rendering context.
        /// </summary>
        /// <param name="rc">The rendering context.</param>
        public override void Render(IRenderContext rc)
        {
            var actualBins = this.ActualItems;

            this.VerifyAxes();

            var clippingRect = this.GetClippingRect();
            rc.SetClip(clippingRect);

            this.RenderBins(rc, clippingRect, actualBins);

            rc.ResetClip();
        }

        /// <summary>
        /// Updates the data.
        /// </summary>
        protected override void UpdateData()
        {
            if (this.ItemsSource == null)
            {
                return;
            }

            this.UpdateActualItems();
        }

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <param name="i">The index of the item.</param>
        /// <returns>The item of the index.</returns>
        protected override object GetItem(int i)
        {
            var items = this.ActualItems;
            if (this.ItemsSource == null && items != null && i < items.Count)
            {
                return items[i];
            }

            return base.GetItem(i);
        }

        /// <summary>
        /// Clears or creates the <see cref="actualItems"/> list.
        /// </summary>
        private void ClearActualItems()
        {
            if (!this.ownsActualItems || this.actualItems == null)
            {
                this.actualItems = new List<ContinuousHistogramItem>();
            }
            else
            {
                this.actualItems.Clear();
            }

            this.ownsActualItems = true;
        }

        /// <summary>
        /// Updates the points from the <see cref="ItemsSeries.ItemsSource" />.
        /// </summary>
        private void UpdateActualItems()
        {
            // Use the Mapping property to generate the points
            if (this.Mapping != null)
            {
                this.ClearActualItems();
                foreach (var item in this.ItemsSource)
                {
                    this.actualItems.Add(this.Mapping(item));
                }

                return;
            }

            var sourceAsListOfDataRects = this.ItemsSource as List<ContinuousHistogramItem>;
            if (sourceAsListOfDataRects != null)
            {
                this.actualItems = sourceAsListOfDataRects;
                this.ownsActualItems = false;
                return;
            }

            this.ClearActualItems();

            var sourceAsEnumerableDataRects = this.ItemsSource as IEnumerable<ContinuousHistogramItem>;
            if (sourceAsEnumerableDataRects != null)
            {
                this.actualItems.AddRange(sourceAsEnumerableDataRects);
            }
        }

        /// <summary>
        /// Renders the points as line, broken line and markers.
        /// </summary>
        /// <param name="rc">The rendering context.</param>
        /// <param name="clippingRect">The clipping rectangle.</param>
        /// <param name="items">The Items to render.</param>
        protected void RenderBins(IRenderContext rc, OxyRect clippingRect, ICollection<ContinuousHistogramItem> items)
        {
            foreach (var item in items)
            {
                // Get the color of the item
                var actualFillColor = item.Color;
                if (actualFillColor.IsAutomatic())
                {
                    actualFillColor = this.ActualFillColor;
                    if (item.Value < 0 && !this.NegativeFillColor.IsUndefined())
                    {
                        actualFillColor = this.NegativeFillColor;
                    }
                }

                // transform the data points to screen points
                var s00 = this.Transform(item.RangeStart, 0);
                var s11 = this.Transform(item.RangeEnd, item.Height);

                var pointa = new ScreenPoint(s00.X, s00.Y);
                var pointb = new ScreenPoint(s11.X, s11.Y);
                var rectrect = new OxyRect(pointa, pointb);

                rc.DrawClippedRectangle(clippingRect, rectrect, actualFillColor, StrokeColor, StrokeThickness);
            }
        }

        /// <summary>
        /// Gets the point on the series that is nearest the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="interpolate">Interpolate the series if this flag is set to <c>true</c>.</param>
        /// <returns>A TrackerHitResult for the current hit.</returns>
        public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
        {
            var p = this.InverseTransform(point);

            if (!this.IsPointInRange(p))
            {
                return null;
            }
            
            if (this.ActualItems != null)
            {
                // iterate through the DataRects and return the first one that contains the point
                foreach (var item in this.ActualItems)
                {
                    if (item.Contains(p))
                    {
                        return new TrackerHitResult
                        {
                            Series = this,
                            DataPoint = p,
                            Position = point,
                            Item = null,
                            Index = -1,
                            Text = StringHelper.Format(
                            this.ActualCulture,
                            this.TrackerFormatString,
                            null,
                            this.Title,
                            this.XAxis.Title ?? DefaultXAxisTitle,
                            this.XAxis.GetValue(p.X),
                            this.YAxis.Title ?? DefaultYAxisTitle,
                            this.YAxis.GetValue(p.Y),
                            null,
                            item.Area)
                        };
                    }
                }
            }
            // if no DataRects contain the point, return null
            return null;
        }
        
        /// <summary>
        /// Renders the legend symbol on the specified rendering context.
        /// </summary>
        /// <param name="rc">The rendering context.</param>
        /// <param name="legendBox">The legend rectangle.</param>
        public override void RenderLegend(IRenderContext rc, OxyRect legendBox)
        {
            var xmid = (legendBox.Left + legendBox.Right) / 2;
            var ymid = (legendBox.Top + legendBox.Bottom) / 2;
            var height = (legendBox.Bottom - legendBox.Top) * 0.8;
            var width = height;
            rc.DrawRectangleAsPolygon(
                new OxyRect(xmid - (0.5 * width), ymid - (0.5 * height), width, height),
                this.GetSelectableColor(this.ActualFillColor),
                this.StrokeColor,
                this.StrokeThickness);
        }

        /// <summary>
        /// Sets the default values.
        /// </summary>
        protected override void SetDefaultValues()
        {
            if (this.FillColor.IsAutomatic())
            {
                this.defaultFillColor = this.PlotModel.GetDefaultColor();
            }
        }

        /// <summary>
        /// Ensures that the axes of the series is defined.
        /// </summary>
        protected override void EnsureAxes()
        {
            base.EnsureAxes();
        }

        /// <summary>
        /// Updates the maximum and minimum values of the series for the x and y dimensions only.
        /// </summary>
        protected internal void UpdateMaxMinXY()
        {
            if (this.ActualItems != null && this.ActualItems.Count > 0)
            {
                this.MinX = Math.Min(this.ActualItems.Min(r => r.RangeStart), this.ActualItems.Min(r => r.RangeEnd));
                this.MaxX = Math.Max(this.ActualItems.Max(r => r.RangeStart), this.ActualItems.Max(r => r.RangeEnd));
                this.MinY = Math.Min(this.ActualItems.Min(r => 0), this.ActualItems.Min(r => r.Height));
                this.MaxY = Math.Max(this.ActualItems.Max(r => 0), this.ActualItems.Max(r => r.Height));
            }
        }

        /// <summary>
        /// Updates the maximum and minimum values of the series.
        /// </summary>
        protected override void UpdateMaxMin()
        {
            base.UpdateMaxMin();

            var allDataPoints = new List<DataPoint>();
            allDataPoints.AddRange(this.ActualItems.Select(item => new DataPoint(item.RangeStart, 0.0)));
            allDataPoints.AddRange(this.ActualItems.Select(item => new DataPoint(item.RangeEnd, item.Height)));
            this.InternalUpdateMaxMin(allDataPoints);

            this.UpdateMaxMinXY();

            if (this.ActualItems != null && this.ActualItems.Count > 0)
            {
                this.MinValue = this.ActualItems.Min(r => r.Value);
                this.MaxValue = this.ActualItems.Max(r => r.Value);
            }
        }

        /// <summary>
        /// Updates the axes to include the max and min of this series.
        /// </summary>
        protected override void UpdateAxisMaxMin()
        {
            base.UpdateAxisMaxMin();
        }

        /// <summary>
        /// Gets the label for the specified cell.
        /// </summary>
        /// <param name="v">The value of the cell.</param>
        /// <param name="i">The first index.</param>
        /// <param name="j">The second index.</param>
        /// <returns>The label string.</returns>
        protected virtual string GetLabel(double v, int i, int j)
        {
            return v.ToString(this.LabelFormatString, this.ActualCulture);
        }
        
        /// <summary>
        /// Gets the clipping rectangle, transposed if the X axis is vertically orientated.
        /// </summary>
        /// <returns>The clipping rectangle.</returns>
        protected new OxyRect GetClippingRect()
        {
            double minX = Math.Min(this.XAxis.ScreenMin.X, this.XAxis.ScreenMax.X);
            double minY = Math.Min(this.YAxis.ScreenMin.Y, this.YAxis.ScreenMax.Y);
            double maxX = Math.Max(this.XAxis.ScreenMin.X, this.XAxis.ScreenMax.X);
            double maxY = Math.Max(this.YAxis.ScreenMin.Y, this.YAxis.ScreenMax.Y);

            if (this.XAxis.IsVertical())
            {
                return new OxyRect(minY, minX, maxY - minY, maxX - minX);
            }

            return new OxyRect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Transposes the ScreenPoint if the X axis is vertically orientated
        /// </summary>
        /// <param name="point">The <see cref="ScreenPoint" /> to orientate.</param>
        /// <returns>The oriented point.</returns>
        private ScreenPoint Orientate(ScreenPoint point)
        {
            if (this.XAxis.IsVertical())
            {
                point = new ScreenPoint(point.Y, point.X);
            }

            return point;
        }

        /// <summary>
        /// Tests if a <see cref="DataPoint" /> is inside the heat map
        /// </summary>
        /// <param name="p">The <see cref="DataPoint" /> to test.</param>
        /// <returns><c>True</c> if the point is inside the heat map.</returns>
        private bool IsPointInRange(DataPoint p)
        {
            this.UpdateMaxMinXY();

            return p.X >= this.MinX && p.X <= this.MaxX && p.Y >= this.MinY && p.Y <= this.MaxY;
        }
    }
}