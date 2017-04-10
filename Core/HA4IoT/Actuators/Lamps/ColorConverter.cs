﻿using System;

namespace HA4IoT.Actuators.Lamps
{
    public static class ColorConverter
    {
        public static void ConvertHsvToRgb(double h, double s, double v, out double r, out double g, out double b)
        {
            if (h < 0 || h > 360) throw new ArgumentOutOfRangeException(nameof(h));
            if (s < 0 || s > 1) throw new ArgumentOutOfRangeException(nameof(s));
            if (v < 0 || v > 1) throw new ArgumentOutOfRangeException(nameof(v));

            if (s == 0D)
            {
                r = v; b = v; g = v;
                return;
            }

            var hi = Math.Floor(h / 60D);
            var f = (h / 60D) - hi;
            var p = v * (1D - s);
            var q = v * (1D - s * f);
            var t = v * (1D - s * (1D - f));

            switch ((int)hi)
            {
                case 0:
                case 6:
                {
                    r = v; g = t; b = p;
                    break;
                }
                case 1:
                {
                    r = q; g = v; b = p;
                    break;
                }
                case 2:
                {
                    r = p; g = v; b = t;
                    break;
                }
                case 3:
                {
                    r = p; g = q; b = v;
                    break;
                }
                case 4:
                {
                    r = t; g = p; b = v;
                    break;
                }
                case 5:
                {
                    r = v; g = p; b = q;
                    break;
                }

                default: throw new InvalidOperationException();
            }

            if (r < 0) r = 0;
            if (r > 1) r = 1;
            if (g < 0) g = 0;
            if (g > 1) g = 1;
            if (b < 0) b = 0;
            if (b > 1) b = 1;
        }
    }
}
