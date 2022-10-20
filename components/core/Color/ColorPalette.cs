// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RgbColor = System.Drawing.Color;

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
            var rgb = RgbColor.FromArgb(
                red: (rgb2.R - rgb1.R) * p + rgb1.R,
                green: (rgb2.G - rgb1.G) * p + rgb1.G,
                blue: (rgb2.B - rgb1.B) * p + rgb1.B
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
            HSVColor toReturn = new HSVColor();

            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            toReturn.H = Math.Round(color.GetHue(), 2);
            toReturn.S = ((max == 0) ? 0 : 1d - (1d * min / max)) * 100;
            toReturn.S = Math.Round(toReturn.S, 2);
            toReturn.V = Math.Round(((max / 255d) * 100), 2);

            return toReturn;
        }

        private static string RgbToHex(RgbColor color)
        {
            return $"#{color.Name[2..]}";
        }

        private static RgbColor InputToRgb(string hex)
        {
            return RgbColor.FromArgb(
                 int.Parse(hex[1..3], NumberStyles.AllowHexSpecifier),
                 int.Parse(hex[3..5], NumberStyles.AllowHexSpecifier),
                 int.Parse(hex[5..], NumberStyles.AllowHexSpecifier)
                );
        }

        private static RgbColor InputToRgb(HSVColor hsv)
        {
            static RgbColor Rgb(double r, double g, double b) =>
                RgbColor.FromArgb(Clamp((int)(r * 255.0)), Clamp((int)(g * 255.0)), Clamp((int)(b * 255.0))); ;

            if (hsv.V <= 0)
            {
                return Rgb(0, 0, 0);
            }
            else if (hsv.S <= 0)
            {
                return Rgb(hsv.V, hsv.V, hsv.V);
            }
            else
            {
                double h = hsv.H;
                while (h < 0) { h += 360; };
                while (h >= 360) { h -= 360; };

                double hf = h / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = hsv.V * (1 - hsv.S);
                double qv = hsv.V * (1 - hsv.S * f);
                double tv = hsv.V * (1 - hsv.S * (1 - f));
                return i switch
                {
                    0 => Rgb(hsv.V, tv, pv),
                    1 => Rgb(qv, hsv.V, pv),
                    2 => Rgb(pv, hsv.V, tv),
                    3 => Rgb(pv, qv, hsv.V),
                    4 => Rgb(tv, pv, hsv.V),
                    5 => Rgb(hsv.V, pv, qv),
                    6 => Rgb(hsv.V, tv, pv),
                    -1 => Rgb(hsv.V, pv, qv),
                    _ => Rgb(hsv.V, hsv.V, hsv.V),
                };
            }
        }

        private static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }

    public struct HSVColor
    {
        public double H { get; set; }
        public double S { get; set; }
        public double V { get; set; }
    }

    public enum Theme
    {
        Light,
        Dark,
    }
}
