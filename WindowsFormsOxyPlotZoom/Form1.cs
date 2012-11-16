using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OxyPlot;

namespace WindowsFormsOxyPlotZoom
{
    public partial class Form1 : Form
    {
        class MyValue : IComparable {
            public DateTime Date { get; set; }
            public double Value { get; set; }
            public MyValue(DateTime d) {
                Date = new DateTime(d.Year, d.Month, d.Day);
            }
            public MyValue(DateTime d, double v) {
                Date = new DateTime(d.Year, d.Month, d.Day);
                Value = v;
            }
            int IComparable.CompareTo(object to) {
                return Date.CompareTo(((MyValue)to).Date);
            }
        }
            
        DateTimeAxis xAxisDateTime;
        LinearAxis yAxisValues;
        List<MyValue> myValues;
        PlotModel plotModel;
        LineSeries lineSeries;

        public Form1() {
            InitializeComponent();

            myValues = new List<MyValue>();
            DateTime startDate = new DateTime(2000, 1, 1);
            double startValue = 100;
            Random r = new Random();
            for (int i = 0; i < 1000; i++, startDate = startDate.AddDays(1), startValue += (r.NextDouble() - 0.5)) {
                myValues.Add(new MyValue(startDate, startValue));
            }

            plotModel = new PlotModel("Random chart");

            xAxisDateTime = new DateTimeAxis() { 
                IntervalType = DateTimeIntervalType.Auto, 
                MinorIntervalType = DateTimeIntervalType.Days, 
                MajorGridlineStyle = LineStyle.Solid, 
                MinorGridlineStyle = LineStyle.Dot, 
                CalendarWeekRule = System.Globalization.CalendarWeekRule.FirstFourDayWeek, 
                FirstDayOfWeek = DayOfWeek.Monday, 
                Position = AxisPosition.Bottom 
            };
            //Set the min/max values of the x-axis to my min/max date to prevent zoom/pan beyond data series
            xAxisDateTime.AbsoluteMinimum = myValues[0].Date.ToOADate();
            xAxisDateTime.AbsoluteMaximum = myValues[myValues.Count - 1].Date.ToOADate();
            
            //calling user defined event on change
            xAxisDateTime.AxisChanged += xAxisDateTime_AxisChanged;

            yAxisValues = new LinearAxis() { 
                MajorGridlineStyle = LineStyle.Solid, 
                MinorGridlineStyle = LineStyle.Dot 
            };

            plotModel.Axes.Add(xAxisDateTime);
            plotModel.Axes.Add(yAxisValues);

            lineSeries = new LineSeries {
                Title = "Values",
                Color = OxyColor.FromArgb(255, 78, 154, 6),
                MarkerType = MarkerType.None,
                StrokeThickness = 1,
                DataFieldX = "Date",
                DataFieldY = "Value",
                ItemsSource = myValues
            };

            plotModel.Series.Add(lineSeries);
            plot1.Model = plotModel;

        }

        void xAxisDateTime_AxisChanged(object sender, AxisChangedEventArgs e) {
            DateTimeAxis axis = sender as DateTimeAxis;

            //save the current min/max date values
            DateTime dtMax = DateTime.FromOADate(axis.ActualMaximum);
            DateTime dtMin = DateTime.FromOADate(axis.ActualMinimum);

            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            //BinarySearch, because the xAxisDateTime is sorted
            int idxMin = myValues.BinarySearch(new MyValue(dtMin));
            int idxMax = myValues.BinarySearch(new MyValue(dtMax));

            //if we can not find an exact location (result is negativ) we can use the complement 
            //see BinarySearch help for further explanation
            if (idxMin < 0) idxMin = ~idxMin;
            if (idxMax < 0) idxMax = ~idxMax;

            //find the corresponding min/max values in the selected intervall
            for (int i = idxMin; i < idxMax; i++) {
                minValue = Math.Min(minValue, myValues[i].Value);
                maxValue = Math.Max(maxValue, myValues[i].Value);
            }

            //set y-axis min/max and redraw the chart
            yAxisValues.Zoom(minValue, maxValue);
            plotModel.RefreshPlot(true);
        }
    }
}
   
