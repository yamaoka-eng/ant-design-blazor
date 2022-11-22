// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using RgbColor = System.Drawing.Color;

namespace AntDesign.core.Color.TinyColor
{
    public class TinyColor
    {
        private RgbColor _originalColor;
        private RgbColor _color;

        private TinyColor(RgbColor color)
        {
            this._originalColor = RgbColor.FromArgb(color.A, color.R, color.G, color.B);
            this._color = RgbColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static TinyColor FromRGB(int r, int g, int b, int a = 100)
        {
            return new TinyColor(RgbColor.FromArgb(a, r, g, b));
        }

        //public static fromHSL(hslclor)
        //{
        //}

        internal static double Bound01(double n, double max)
        {
            n = max == 360.0 ? n : Math.Min(max, Math.Max(0.0, n));
            double absDifference = n - max;
            if (Math.Abs(absDifference) < 0.000001)
            {
                return 1.0;
            }
            if (max == 360)
            {
                n = (n < 0 ? n % max + max : n % max) / max;
            }
            else
            {
                n = (n % max) / max;
            }
            return n;
        }

        internal static double Clamp01(double val)
        {
            return Math.Min(1.0, Math.Max(0.0, val));
        }
    }
}
