using System;
using SciChart.Charting.ViewportManagers;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;

namespace GUI
{
    /// <summary>
    /// The following class will apply a scrolling window to the chart unles the user is zooming or panning
    /// </summary>
    public class ScrollingViewportManager : DefaultViewportManager
    {
        private readonly TimeSpan _windowSize;
        public int count;
        public ScrollingViewportManager(TimeSpan windowSize)
        {
            _windowSize = windowSize;
            count = 0;
        }
        public override void AttachSciChartSurface(ISciChartSurface scs)
        {
            base.AttachSciChartSurface(scs);
            this.ParentSurface = scs;
        }
        public ISciChartSurface ParentSurface { get; private set; }
        protected override IRange OnCalculateNewXRange(IAxis xAxis)
        {
            count++;
            // The Current XAxis VisibleRange
            var currentVisibleRange = xAxis.VisibleRange.AsDoubleRange();
            if (ParentSurface.ZoomState == ZoomStates.UserZooming)
                return currentVisibleRange;     // Don't scroll if user is zooming
            // The MaxXRange is the VisibleRange on the XAxis if we were to zoom to fit all data
            var maxXRange = xAxis.GetMaximumRange().AsDoubleRange();
            double xMax = Math.Max(maxXRange.Max, currentVisibleRange.Max);
            // Scroll showing latest window size
            return new DateRange(new DateTime((long)xMax) - _windowSize, new DateTime((long)xMax));
        }
    }
}