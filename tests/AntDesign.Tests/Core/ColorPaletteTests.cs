// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using AntDesign.Core;
using FluentAssertions;
using Xunit;

namespace AntDesign.Tests.Core
{
    public class ColorPaletteTests
    {
        [Fact]
        public void Generate_palettes_from_a_given_color()
        {
            ColorPalette.Generate("#1890ff").Should().Equal(_blueColors, (a, b) => a.Equals(b, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public void Generate_dark_palettes_from_a_given_color()
        {
            ColorPalette.Generate("#1890ff", Theme.Dark, "#141414").Should().Equal(_blueDarkColors);
        }

        [Fact]
        public void Generate_primary_color()
        {
            ColorPalette.GetPrimaryColor("blue").Should().Be("#1890ff");
        }

        private string[] _blueColors =
        {
            "#E6F7FF",
            "#BAE7FF",
            "#91D5FF",
            "#69C0FF",
            "#40A9FF",
            "#1890FF",
            "#096DD9",
            "#0050B3",
            "#003A8C",
            "#002766",
        };

        private string[] _blueDarkColors =
        {
            "#111d2c",
            "#112a45",
            "#15395b",
            "#164c7e",
            "#1765ad",
            "#177ddc",
            "#3c9ae8",
            "#65b7f3",
            "#8dcff8",
            "#b7e3fa",
        };
    }
}
