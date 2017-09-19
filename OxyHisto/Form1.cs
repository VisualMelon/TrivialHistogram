using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OxyHisto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs eargs)
        {
            var model = new PlotModel { Title = "Continuous Histograms", Subtitle = "Distribution of cos(x) values" };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, Key = "YBottom", StartPosition = 0, EndPosition = 0.48, AbsoluteMinimum = 0, AbsoluteMaximum = 5, Maximum = 5, Title = "Frequency" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, Key = "YTop", StartPosition = 0.52, EndPosition = 1.0, AbsoluteMinimum = 0, AbsoluteMaximum = 5, Maximum = 5, Title = "Frequency" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "(1 + cos(x)) / 2" });

            model.IsLegendVisible = true;
            model.LegendPlacement = LegendPlacement.Outside;
            model.LegendPosition = LegendPosition.RightTop;

            var chs1 = new ContinuousHistogramSeries() { YAxisKey = "YBottom", Title = "Regular Bins" } ;
            chs1.ItemsSource = HistogramHelpers.Collect(RandomSource(10000), 0, 1, 10, true);
            chs1.StrokeThickness = 1;
            chs1.RenderInLegend = true;
            model.Series.Add(chs1);

            var chs2 = new ContinuousHistogramSeries() { YAxisKey = "YTop", Title = "Custom Bins" } ;
            chs2.ItemsSource = HistogramHelpers.Collect(RandomSource(10000), new[] { 0, 0.02, 0.05, 0.1, 0.2, 0.4, 0.6, 0.8, 0.9, 0.95, 0.98, 1.0 }, true);
            chs2.StrokeThickness = 1;
            chs2.RenderInLegend = true;
            model.Series.Add(chs2);

            OxyPlot.WindowsForms.PlotView plotView = new OxyPlot.WindowsForms.PlotView();

            plotView.Model = model;

            this.Controls.Add(plotView);

            model.InvalidatePlot(true);
            plotView.Invalidate();

            plotView.Dock = DockStyle.Fill;

            this.Refresh();
        }

        private static IEnumerable<double> RandomSource(int count)
        {
            Random rnd = new Random();

            for (int i = 0; i < count; i++)
            {
                double r = rnd.NextDouble();
                yield return Math.Cos(r * Math.PI * 2.0) * 0.5 + 0.5;
            }
        }
    }
}

