// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core;
using Stride.UI.Attributes;

namespace Stride.UI.Controls
{
    /// <summary>
    /// Represents a drop-down list control that allows users to select an item from a list.
    /// </summary>
    [DataContract(nameof(DropDown))]
    [DataContractMetadataType(typeof(DropDownMetadata))]
    [DebuggerDisplay("Dropdown - Name={Name}")]
    public class DropDown
    {
        /// <summary>
        /// Creates a new instance of <see cref="DropDown"/>.
        /// </summary>
        public DropDown()
        {
            _items = new();
            SelectedIndex = 0;

            _text = new EditText
            {
                IsReadOnly = true,
                Padding = new Thickness(5, 3, 5, 3),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 20, 0), // Leave space for the dropdown arrow
            };
        }

        List<string> _items;

        /// <summary>
        /// Gets or sets the collection of items contained in the list.
        /// </summary>
        public List<string> Items
        {
            get => _items;
        }

        /// <summary>
        /// Gets or sets the index of the currently selected item in the list.
        /// </summary>
        public int SelectedIndex { get; set; }

        EditText _text;
        Button _arrow;


        private class DropDownMetadata
        {
            [DefaultThicknessValue(10, 5, 10, 7)]
            public Thickness Padding { get; }
        }
    }
}
