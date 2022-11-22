// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AntDesign.core.Color.TinyColor;

namespace AntDesign.Core
{
    /// <summary>
    /// Ant Design Color Palette
    /// port of https://github.com/ant-design/ant-design-colors/blob/v6.0.0/src/generate.ts
    /// </summary>
    public static class ColorPalette
    {
        private static double _hueStep = 2; // 色相阶梯
        private static double _saturationStep = 0.16; // 饱和度阶梯，浅色部分
        private static double _saturationStep2 = 0.05; // 饱和度阶梯，深色部分
        private static double _brightnessStep1 = 0.05; // 亮度阶梯，浅色部分
        private static double _brightnessStep2 = 0.15; // 亮度阶梯，深色部分
        private static int _lightColorCount = 5; // 浅色数量，主色上
        private static int _darkColorCount = 4; // 深色数量，主色下

        // 暗色主题颜色映射关系表
        private static (int index, double opacity)[] _darkColorMap = new[]
        {
            ( index: 7, opacity: 0.15 ),
            ( index: 6, opacity: 0.25 ),
            ( index: 5, opacity: 0.3 ),
            ( index: 5, opacity: 0.45 ),
            ( index: 5, opacity: 0.65 ),
            ( index: 5, opacity: 0.85 ),
            ( index: 4, opacity: 0.9 ),
            ( index: 3, opacity: 0.95 ),
            ( index: 2, opacity: 0.97 ),
            ( index: 1, opacity: 0.98 ),
        };

        public static Dictionary<string, string> PresetPrimaryColors = new()
        {
            ["red"] = "#F5222D",
            ["volcano"] = "#FA541C",
            ["orange"] = "#FA8C16",
            ["gold"] = "#FAAD14",
            ["yellow"] = "#FADB14",
            ["lime"] = "#A0D911",
            ["green"] = "#52C41A",
            ["cyan"] = "#13C2C2",
            ["blue"] = "#1890FF",
            ["geekblue"] = "#2F54EB",
            ["purple"] = "#722ED1",
            ["magenta"] = "#EB2F96",
            ["grey"] = "#666666",
        };

        public static Dictionary<string, string[]> PresetPalettes = new();
        public static Dictionary<string, string[]> PresetDarkPalettes = new();

        static ColorPalette()
        {
            foreach (var color in PresetPrimaryColors)
            {
                PresetPalettes.Add(color.Key, Generate(color.Value));
                PresetDarkPalettes.Add(color.Key, Generate(color.Value));
            }
        }

        public static string[] Generate(string color, Theme? theme = null, string background = null)
        {
            var patternList = new List<string>();
            var pColor = InputToRgb(color);

            for (int i = _lightColorCount; i > 0; i -= 1)
            {
                var hsv = RgbToHsv(pColor);
                var colorString = RgbToHex(
                    InputToRgb(new HSVColor
                    {
                        H = GetHue(hsv, i, true),
                        S = GetSaturation(hsv, i, true),
                        V = GetValue(hsv, i, true)
                    }));

                patternList.Add(colorString);
            }

            patternList.Add(RgbToHex(pColor));

            for (var i = 1; i <= _darkColorCount; i += 1)
            {
                var hsv = RgbToHsv(pColor);
                var colorString = RgbToHex(
                    InputToRgb(new HSVColor
                    {
                        H = GetHue(hsv, i),
                        S = GetSaturation(hsv, i),
                        V = GetValue(hsv, i)
                    }));

                patternList.Add(colorString);
            }

            // dark theme patterns
            var patterns = patternList.ToArray();

            if (theme == Theme.Dark)
            {
                return _darkColorMap.Select(color =>
                {
                    var darkColorString = RgbToHex(
                        Mix(InputToRgb(background ?? "#141414"), InputToRgb(patterns[color.index]), (int)(color.opacity * 100)));
                    return darkColorString;
                }).ToArray();
            }

            return patterns;
        }

        public static string GetPrimaryColor(string colorName)
        {
            return PresetPalettes[colorName][5];
        }

        public static string GetPrimaryDarkColor(string colorName)
        {
            return PresetDarkPalettes[colorName][5];
        }

        private static double GetHue(HSVColor hsv, int i, bool light = false)
        {
            double hue;

            if (Math.Round(hsv.H) is >= 60 and <= 240)
            {
                hue = light ? Math.Round(hsv.H) - _hueStep * i : Math.Round(hsv.H) + _hueStep * i;
            }
            else
            {
                hue = light ? Math.Round(hsv.H) + _hueStep * i : Math.Round(hsv.H) - _hueStep * i;
            }

            if (hue < 0)
            {
                hue += 360;
            }
            else if (hue >= 360)
            {
                hue -= 360;
            }

            return hue;
        }

        private static RgbColor Mix(RgbColor rgb1, RgbColor rgb2, int amount)
        {
            var p = amount / 100;
            var rgb = new RgbColor(
                R: (rgb2.R - rgb1.R) * p + rgb1.R,
                G: (rgb2.G - rgb1.G) * p + rgb1.G,
                B: (rgb2.B - rgb1.B) * p + rgb1.B
               );

            return rgb;
        }

        private static double GetSaturation(HSVColor hsv, int i, bool light = false)
        {
            if (hsv.H == 0 && hsv.S == 0)
            {
                return hsv.S;
            }

            double saturation;

            if (light)
            {
                saturation = hsv.S - _saturationStep * i;
            }
            else if (i == _darkColorCount)
            {
                saturation = hsv.S + _saturationStep;
            }
            else
            {
                saturation = hsv.S + _saturationStep2 * i;
            }

            if (saturation > 1)
            {
                saturation = 1;
            }

            if (light && i == _lightColorCount && saturation > 0.1)
            {
                saturation = 0.1;
            }
            if (saturation < 0.06)
            {
                saturation = 0.06;
            }

            return saturation;
        }

        private static double GetValue(HSVColor hsv, int i, bool light = false)
        {
            double value;
            if (light)
            {
                value = hsv.V + _brightnessStep1 * i;
            }
            else
            {
                value = hsv.V - _brightnessStep2 * i;
            }

            if (value > 1)
            {
                value = 1;
            }
            return value;
        }

        private static HSVColor RgbToHsv(RgbColor color)
        {
            return HSVColor.FromRgb(color);
        }

        private static string RgbToHex(RgbColor color, bool allow3Char = false)
        {
            return color.ToHexString(allow3Char);
        }

        private static RgbColor InputToRgb(string hex)
        {
            return RgbColor.FromHex(hex);
        }

        private static RgbColor InputToRgb(HSVColor hsv)
        {
            return hsv.ToRgb();
        }
    }

    public record struct RgbColor(double R, double G, double B)
    {
        public static RgbColor FromHex(string hex)
        {
            return new(
                int.Parse(hex[1..3], NumberStyles.AllowHexSpecifier),
                int.Parse(hex[3..5], NumberStyles.AllowHexSpecifier),
                int.Parse(hex[5..], NumberStyles.AllowHexSpecifier)
               );
        }

        public string ToHex(bool allow3Char = false)
        {
            var hex = new[]
            {
                ((int)Math.Round(R)).ToString("X").PadLeft(2,'0'),
                ((int)Math.Round(G)).ToString("X").PadLeft(2,'0'),
                ((int)Math.Round(B)).ToString("X").PadLeft(2,'0'),
            };

            // Return a 3 character hex if possible
            if (allow3Char && hex[0][0] == hex[0][1] && hex[1][0] == hex[1][1] && hex[2][0] == hex[2][1])
            {
                return $"{hex[0][0]}{hex[1][0]}{hex[2][0]}".ToLowerInvariant();
            }

            return $"{hex[0]}{hex[1]}{hex[2]}".ToLowerInvariant();
        }

        public string ToHexString(bool allow3Char = false)
        {
            return $"#{ToHex(allow3Char)}";
        }
    }

    public record struct HSVColor(double H, double S, double V)
    {
        public RgbColor ToRgb()
        {
            double h = TinyColor.Bound01(H, 360) * 6;
            double s = TinyColor.Bound01(S, 100);
            double v = TinyColor.Bound01(V, 100);

            double i = Math.Floor(h),
            f = h - i,
            p = v * (1 - s),
            q = v * (1 - f * s),
            t = v * (1 - (1 - f) * s),
            mod = i % 6,
            r = new[] { v, q, p, p, t, v }[(int)mod],
            g = new[] { t, v, v, q, p, p }[(int)mod],
            b = new[] { p, p, t, v, v, q }[(int)mod];

            return new(r * 255, g * 255, b * 255);
        }

        public static HSVColor FromRgb(RgbColor color)
        {
            double r = TinyColor.Bound01(color.R, 255);
            double g = TinyColor.Bound01(color.G, 255);
            double b = TinyColor.Bound01(color.B, 255);

            var max = new[] { r, g, b }.Max();
            var min = new[] { r, g, b }.Min();
            double h, s, v = max;

            var d = max - min;
            s = max == 0 ? 0 : d / max;

            if (max == min)
            {
                h = 0; // achromatic
            }
            else
            {
                h = max switch
                {
                    _ when max == r => (g - b) / d + (g < b ? 6 : 0),
                    _ when max == g => (b - r) / d + 2,
                    _ when max == b => (r - g) / d + 4,
                };
                h /= 6;
            }
            return new(h, s, v);
        }
    }

    public record struct HslColor(double H, double S, double L, double A = 0.0)
    {
        public override string ToString()
        {
            return $"HSL(h: {H}, s: {S}, l: {L}, a: {A})";
        }

        RgbColor ToRgb()
        {
            double r;
            double g;
            double b;

            double h = TinyColor.Bound01(H, 360.0);
            double s = TinyColor.Bound01(S * 100, 100.0);
            double l = TinyColor.Bound01(L * 100, 100.0);

            static double Hue2rgb(double p, double q, double t)
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1 / 6) return p + (q - p) * 6 * t;
                if (t < 1 / 2) return q;
                if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;

                return p;
            }

            if (S == 0)
                r = g = b = l;
            else
            {
                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = Hue2rgb(p, q, h + 1 / 3);
                g = Hue2rgb(p, q, h);
                b = Hue2rgb(p, q, h - 1 / 3);
            }

            return new(r * 255, g * 255, b * 255);
        }

        static HslColor FromRgb(RgbColor color)
        {
            double r = TinyColor.Bound01(color.R, 255);
            double g = TinyColor.Bound01(color.G, 255);
            double b = TinyColor.Bound01(color.B, 255);

            double max = new[] { r, g, b }.Max(), min = new[] { r, g, b }.Min();
            double h, s, l = (max + min) / 2;

            if (max == min)
            {
                h = s = 0; // achromatic
            }
            else
            {
                var d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
                h = max switch
                {
                    _ when max == r => (g - b) / d + (g < b ? 6 : 0),
                    _ when max == g => (b - r) / d + 2,
                    _ when max == b => (r - g) / d + 4,
                };

                h /= 6;
            }

            return new(h, s, l);
        }
    }

    public enum Theme
    {
        Light,
        Dark,
    }
}
