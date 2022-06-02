using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace VidU_Studio.util
{
    internal class SecondsBeatConverter : IValueConverter
    {
        internal static double BPM = -1;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert((double)value);
        }

        internal static string Convert(double seconds)
        {
            if (BPM <= 0) return seconds.ToString("0.######");
            return (seconds * BPM / 60.0).ToString("0.######");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value as string);
        }

        internal static double ConvertBack(string beatsStr)
        {
            double beats = double.Parse(beatsStr);
            if (BPM <= 0) return beats;
            return beats / BPM * 60.0;
        }
    }
}
