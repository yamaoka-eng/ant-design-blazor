// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace AntDesign
{
    public partial class TabPane : AntDomComponentBase
    {
        [CascadingParameter(Name = "IsTab")]
        internal bool IsTab { get; set; }

        [CascadingParameter(Name = "IsPane")]
        internal bool IsPane { get; set; }

        [CascadingParameter]
        private Tabs Parent { get; set; }

        /// <summary>
        /// Forced render of content in tabs, not lazy render after clicking on tabs
        /// </summary>
        [Parameter]
        public bool ForceRender { get; set; } = false;

        /// <summary>
        /// TabPane's key
        /// </summary>
        [Parameter]
        public string Key { get; set; }

        /// <summary>
        /// Show text in <see cref="TabPane"/>'s head
        /// </summary>
        [Parameter]
        public string Tab { get; set; }

        [Parameter]
        public RenderFragment TabTemplate { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public RenderFragment TabContextMenu { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public bool Closable { get; set; } = true;

        internal bool IsActive => _isActive;

        private bool HasTabTitle => Tab != null || TabTemplate != null;

        internal ElementReference TabRef => _tabRef;

        private ClassMapper _tabPaneClassMapper = new();

        private const string PrefixCls = "ant-tabs-tab";
        private const string tabPanePrefixCls = "ant-tabs-tabpane";

        private ElementReference _tabRef;
        private bool _isActive;
        private bool _hasClosed;
        private bool _hasRendered;

        private int _hidding = 0;
        private int _showing = 0;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.SetClass();

            Parent?.AddTabPane(this);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (IsTab && HasTabTitle)
            {
                _hasRendered = true;
            }
        }

        private void SetClass()
        {
            ClassMapper
                .Add(PrefixCls)
                .If($"{PrefixCls}-active", () => _isActive)
                .If($"{PrefixCls}-with-remove", () => Closable)
                .If($"{PrefixCls}-disabled", () => Disabled);

            _tabPaneClassMapper
                .Add(tabPanePrefixCls)
                .If($"{tabPanePrefixCls}-active", () => _isActive)
                .If($"{tabPanePrefixCls}-hidden", () => !_isActive)
                .If("ant-tabs-switch-leave ant-tabs-switch-leave-prepare ant-tabs-switch", () => _hidding == 1)
                .If("ant-tabs-switch-leave ant-tabs-switch-leave-start ant-tabs-switch", () => _hidding == 2)
                .If("ant-tabs-switch-leave ant-tabs-switch-leave-active ant-tabs-switch", () => _hidding == 3)
                .If("ant-tabs-switch-enter ant-tabs-switch-enter-prepare ant-tabs-switch", () => _showing == 1)
                .If("ant-tabs-switch-enter ant-tabs-switch-enter-start ant-tabs-switch", () => _showing == 2)
                .If("ant-tabs-switch-enter ant-tabs-switch-enter-active ant-tabs-switch", () => _showing == 3)
                ;
        }

        internal void SetKey(string key)
        {
            Key = key;
        }

        internal async Task SetActive(bool isActive)
        {
            if (_isActive == isActive)
            {
                return;
            }

            if (IsPane && Parent?.Animated == true)
            {
                if (isActive)
                {
                    _showing = 1;

                    StateHasChanged();
                    await Task.Delay(300);

                    _showing = 2;
                    _isActive = true;

                    StateHasChanged();
                    await Task.Delay(300);

                    _showing = 3;
                    StateHasChanged();
                    await Task.Delay(300);
                }
                else
                {
                    _hidding = 1;

                    StateHasChanged();
                    await Task.Delay(300);

                    _hidding = 2;
                    _isActive = false;

                    StateHasChanged();
                    await Task.Delay(300);

                    _showing = 3;
                    StateHasChanged();
                    await Task.Delay(300);
                }
            }

            _showing = 0;
            _hidding = 0;
            _isActive = isActive;
            await InvokeAsync(StateHasChanged);
        }

        internal void Close()
        {
            _hasClosed = true;

            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            Parent?.RemovePane(this);

            base.Dispose(disposing);
        }

        internal void ExchangeWith(TabPane other)
        {
            var temp = other.Clone();
            other.SetPane(this);
            this.SetPane(temp);
        }

        private TabPane Clone()
        {
            return new TabPane
            {
                Key = Key,
                Tab = this.Tab,
                TabTemplate = this.TabTemplate,
                Disabled = this.Disabled,
                Closable = this.Closable,
            };
        }

        private void SetPane(TabPane tabPane)
        {
            Key = tabPane.Key;
            Tab = tabPane.Tab;
            TabTemplate = tabPane.TabTemplate;
            Disabled = tabPane.Disabled;
            Closable = tabPane.Closable;

            StateHasChanged();
        }
    }
}
