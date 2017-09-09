﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2016 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;

namespace DaggerfallWorkshop.Game.UserInterface
{
    /// <summary>
    /// Implements a text list box.
    /// </summary>
    public class ListBox : BaseScreenComponent
    {
        #region Fields

        int maxCharacters = -1;
        PixelFont font;
        int selectedIndex = 0;
        int scrollIndex = 0;
        bool enabledHorizontalScroll = false;
        int horizontalScrollIndex = 0;
        int maxHorizontalScrollIndex = 0;
        bool wrapTextItems = false;
        int rowsDisplayed = 9;
        int rowSpacing = 1;
        HorizontalAlignment rowAlignment = HorizontalAlignment.Left;
        Vector2 shadowPosition = DaggerfallUI.DaggerfallDefaultShadowPos;
        Vector2 selectedShadowPosition = Vector2.zero;
        Color textColor = DaggerfallUI.DaggerfallDefaultTextColor;
        Color selectedTextColor = DaggerfallUI.DaggerfallDefaultSelectedTextColor;
        Color shadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
        Color selectedShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;

        // restricted render area can be used to force list box content rendering inside this rect (used for content rendering in window frames where content is larger than frame)
        bool useRestrictedRenderArea = false;
        Rect rectRestrictedRenderArea;

        public enum VerticalScrollModes
        {
            EntryWise,
            Pixelwise
        }
        VerticalScrollModes verticalScrollMode = VerticalScrollModes.EntryWise;

        // ListItem class allows for each item to have unique text colors if necessary (needed e.g. in talk window for question and answer color flavors)
        public class ListItem
        {
            public TextLabel textLabel;
            public Color textColor = DaggerfallUI.DaggerfallDefaultTextColor;
            public Color selectedTextColor = DaggerfallUI.DaggerfallDefaultSelectedTextColor;
            public Color shadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
            public Color selectedShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;

            public ListItem(TextLabel textLabel)
            {
                this.textLabel = textLabel;
            }
        }
        List<ListItem> listItems = new List<ListItem>();

        #endregion

        #region Properties

        /// <summary>
        /// Maximum length of label string.
        /// Setting to -1 allows for any length.
        /// </summary>
        public int MaxCharacters
        {
            get { return maxCharacters; }
            set { maxCharacters = value; }
        }

        public PixelFont Font
        {
            get { return font; }
            set { font = value; }
        }

        public int ScrollIndex
        {
            get { return scrollIndex; }
            set { scrollIndex = value; }
        }

        public bool EnabledHorizontalScroll
        {
            get { return enabledHorizontalScroll; }
            set { enabledHorizontalScroll = value; }
        }

        public int HorizontalScrollIndex
        {
            get { return horizontalScrollIndex; }
            set
            {
                horizontalScrollIndex = value;
                horizontalScrollIndex = Math.Max(0, Math.Min(maxHorizontalScrollIndex, horizontalScrollIndex));
            }
        }

        public int MaxHorizontalScrollIndex
        {
            get { return maxHorizontalScrollIndex; }
            set { maxHorizontalScrollIndex = value; }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { SelectIndex(value); }
        }

        public string SelectedItem
        {
            get { return listItems[selectedIndex].textLabel.Text; }
        }

        public int Count
        {
            get { return listItems.Count; }
        }

        public bool WrapTextItems
        {
            get { return wrapTextItems; }
            set { wrapTextItems = value; }
        }

        public int RowsDisplayed
        {
            get { return rowsDisplayed; }
            set { rowsDisplayed = value; }
        }

        public int RowSpacing
        {
            get { return rowSpacing; }
            set { rowSpacing = value; }
        }

        public HorizontalAlignment RowAlignment
        {
            get { return rowAlignment; }
            set { rowAlignment = value; }
        }

        public Vector2 ShadowPosition
        {
            get { return shadowPosition; }
            set { shadowPosition = value; }
        }

        public Vector2 SelectedShadowPosition
        {
            get { return selectedShadowPosition; }
            set { selectedShadowPosition = value; }
        }

        public Color TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }

        public Color SelectedTextColor
        {
            get { return selectedTextColor; }
            set { selectedTextColor = value; }
        }

        public Color ShadowColor
        {
            get { return shadowColor; }
            set { shadowColor = value; }
        }

        public Color SelectedShadowColor
        {
            get { return selectedShadowColor; }
            set { selectedShadowColor = value; }
        }

        public Rect RectRestrictedRenderArea
        {
            get { return rectRestrictedRenderArea; }
            set
            {
                rectRestrictedRenderArea = value;
                useRestrictedRenderArea = true;
            }
        }

        public VerticalScrollModes VerticalScrollMode
        {
            get { return verticalScrollMode; }
            set { verticalScrollMode = value; }
        }

        #endregion

        #region Overrides

        public override void Update()
        {
            base.Update();

            if (MouseOverComponent)
            {
                if (DaggerfallUI.Instance.LastKeyCode == KeyCode.UpArrow)
                    SelectPrevious();
                else if (DaggerfallUI.Instance.LastKeyCode == KeyCode.DownArrow)
                    SelectNext();
                else if (DaggerfallUI.Instance.LastKeyCode == KeyCode.LeftArrow)
                    HorizontalScrollLeft();
                else if (DaggerfallUI.Instance.LastKeyCode == KeyCode.RightArrow)
                    HorizontalScrollRight();
                else if (DaggerfallUI.Instance.LastKeyCode == KeyCode.Return)
                    UseSelectedItem();
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (verticalScrollMode == VerticalScrollModes.EntryWise)
            {
                float x = 0, y = 0;
                float currentLine = 0;
                for (int i = 0; i < listItems.Count; i++)
                {                    
                    if (currentLine < scrollIndex || currentLine >= scrollIndex + rowsDisplayed)
                    {
                        currentLine += listItems[i].textLabel.NumTextLines;
                        continue;
                    }                    

                    currentLine += listItems[i].textLabel.NumTextLines;

                    TextLabel label = listItems[i].textLabel;
                    label.StartCharacterIndex = horizontalScrollIndex;
                    label.UpdateLabelTexture();
                    if (i == selectedIndex)
                    {
                        label.TextColor = listItems[i].selectedTextColor;
                        label.ShadowPosition = selectedShadowPosition;
                        label.ShadowColor = listItems[i].selectedShadowColor;
                    }
                    else
                    {
                        label.TextColor = listItems[i].textColor;
                        label.ShadowPosition = shadowPosition;
                        label.ShadowColor = listItems[i].shadowColor;
                    }

                    label.Position = new Vector2(x, y);
                    label.HorizontalAlignment = rowAlignment;
                    label.Draw();

                    y += label.TextHeight + rowSpacing;
                }
            }
            else
            {
                int x = 0;
                int y = -scrollIndex;
                for (int i = 0; i < listItems.Count; i++)
                {
                    TextLabel label = listItems[i].textLabel;                  
                    label.StartCharacterIndex = horizontalScrollIndex;
                    label.UpdateLabelTexture();
                    if (i == selectedIndex)
                    {
                        label.TextColor = listItems[i].selectedTextColor;
                        label.ShadowPosition = selectedShadowPosition;
                        label.ShadowColor = listItems[i].selectedShadowColor;
                    }
                    else
                    {
                        label.TextColor = listItems[i].textColor;
                        label.ShadowPosition = shadowPosition;
                        label.ShadowColor = listItems[i].shadowColor;
                    }

                    label.Position = new Vector2(x, y);
                    label.HorizontalAlignment = rowAlignment;
                    label.Draw();

                    y += label.TextHeight + rowSpacing;
                }
            }
        }

        protected override void MouseClick(Vector2 clickPosition)
        {
            base.MouseClick(clickPosition);

            if (listItems.Count == 0)
                return;

            int row = (int)(clickPosition.y / (font.GlyphHeight + rowSpacing));
            int index = scrollIndex + row;
            if (index >= 0 && index < Count)
            {
                selectedIndex = index;
                RaiseOnSelectItemEvent();
            }
        }

        protected override void MouseDoubleClick(Vector2 clickPosition)
        {
            base.MouseDoubleClick(clickPosition);

            UseSelectedItem();
        }

        protected override void MouseScrollUp()
        {
            base.MouseScrollUp();

            ScrollUp();
        }

        protected override void MouseScrollDown()
        {
            base.MouseScrollDown();

            ScrollDown();
        }

        #endregion

        #region Public Methods

        public void ClearItems()
        {
            listItems.Clear();
            scrollIndex = 0;
            SelectNone();
        }

        public void AddItem(string text, out ListItem itemOut, int position = -1)
        {
            if (font == null)
                font = DaggerfallUI.DefaultFont;

            TextLabel textLabel = new TextLabel();
            if (useRestrictedRenderArea)
            {
                textLabel.RectRestrictedRenderArea = this.rectRestrictedRenderArea;
            }
            textLabel.MaxWidth = (int)Size.x;
            textLabel.AutoSize = AutoSizeModes.None;
            textLabel.HorizontalAlignment = rowAlignment;
            textLabel.Font = font;
            textLabel.MaxCharacters = maxCharacters;
            textLabel.Text = text;
            textLabel.Parent = this;
            textLabel.WrapText = wrapTextItems;

            itemOut = new ListItem(textLabel);
            if (position < 0)
                listItems.Add(itemOut);
            else
                listItems.Insert(position, itemOut);
        }

        public void AddItem(string text, int position = -1)
        {
            ListItem itemOut;
            AddItem(text, out itemOut, position);
        }

        public void RemoveItem(int index)
        {
            if (index < 0 || index >= listItems.Count)
                throw new IndexOutOfRangeException("ListBox: RemoveItem index out of range.");

            listItems.RemoveAt(index);
        }

        public void UpdateItem(int index, string label)
        {
            if (index < 0 || index >= listItems.Count)
                throw new IndexOutOfRangeException("ListBox: UpdateItem index out of range.");
            else if (listItems[index] == null)
                throw new IndexOutOfRangeException("ListBox: item at index was null.");
            else
                listItems[index].textLabel.Text = label;
        }

        public void SwapItems(int indexA, int indexB)
        {
            if (indexA < 0 || indexB < 0 || indexA >= listItems.Count || indexB >= listItems.Count)
                throw new IndexOutOfRangeException("ListBox: UpdateItem index out of range.");
            else
            {
                ListItem temp = listItems[indexA];
                listItems[indexA] = listItems[indexB];
                listItems[indexB] = temp;
            }
        }

        public int LengthOfLongestItem()
        {
            int maxLength = 0;
            for (int i = 0; i < listItems.Count; i++)
            {
                maxLength = Math.Max(maxLength, listItems[i].textLabel.Text.Length);
            }
            return maxLength;
        }

        public int HeightContent()
        {
            int sumHeight = 0;
            for (int i = 0; i < listItems.Count; i++)
            {
                if (i > 0)
                    sumHeight += rowSpacing;

                sumHeight += listItems[i].textLabel.TextHeight;
            }
            return sumHeight;
        }

        public void SelectPrevious()
        {
            if (selectedIndex > 0)
            {
                selectedIndex--;
                if (selectedIndex < scrollIndex)
                    scrollIndex = selectedIndex;
            }

            RaiseOnSelectPreviousEvent();
            RaiseOnSelectItemEvent();
            RaiseOnScrollEvent();
        }

        public void SelectNext()
        {
            if (selectedIndex < listItems.Count - 1)
            {
                selectedIndex++;
                if (selectedIndex > scrollIndex + (rowsDisplayed - 1))
                    scrollIndex++;
            }

            RaiseOnSelectNextEvent();
            RaiseOnSelectItemEvent();
            RaiseOnScrollEvent();
        }

        public void HorizontalScrollLeft()
        {
            if (!enabledHorizontalScroll)
                return;

            horizontalScrollIndex--;
            horizontalScrollIndex = Math.Max(0, horizontalScrollIndex);
        }

        public void HorizontalScrollRight()
        {
            if (!enabledHorizontalScroll)
                return;

            horizontalScrollIndex++;
            horizontalScrollIndex = Math.Min(maxHorizontalScrollIndex, horizontalScrollIndex);
        }

        public void SelectIndex(int index)
        {
            if (index < 0 || index >= listItems.Count)
                return;

            selectedIndex = index;
            RaiseOnSelectItemEvent();
        }

        public void SelectNone()
        {
            selectedIndex = -1;
        }

        public void ScrollToSelected()
        {
            scrollIndex = selectedIndex;
            scrollIndex = Mathf.Clamp(scrollIndex, 0, (listItems.Count - 1) - (rowsDisplayed - 1));
            RaiseOnScrollEvent();
        }

        public void UseSelectedItem()
        {
            RaiseOnUseItemEvent();
        }

        public void ScrollUp()
        {
            if (scrollIndex > 0)
                scrollIndex--;

            RaiseOnScrollEvent();
        }

        public void ScrollDown()
        {
            if (scrollIndex < listItems.Count - rowsDisplayed)
                scrollIndex++;

            RaiseOnScrollEvent();
        }

        public void SetRowsDisplayedByHeight()
        {
            if (Count == 0)
                return;

            rowsDisplayed = (int)(Size.y / font.GlyphHeight) - 1;
        }

        public int FindIndex(string text)
        {
            for (int i = 0; i < listItems.Count; i++)
            {
                if (string.Compare(listItems[i].textLabel.Text, text, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion

        #region Event Handlers

        public delegate void OnSelectPreviousEventHandler();
        public event OnSelectPreviousEventHandler OnSelectPrevious;
        void RaiseOnSelectPreviousEvent()
        {
            if (OnSelectPrevious != null)
                OnSelectPrevious();
        }

        public delegate void OnSelectNextEventHandler();
        public event OnSelectNextEventHandler OnSelectNext;
        void RaiseOnSelectNextEvent()
        {
            if (OnSelectNext != null)
                OnSelectNext();
        }

        public delegate void OnSelectItemEventHandler();
        public event OnSelectItemEventHandler OnSelectItem;
        void RaiseOnSelectItemEvent()
        {
            if (selectedIndex < 0 || selectedIndex >= Count)
                return;

            if (OnSelectItem != null)
                OnSelectItem();
        }

        public delegate void OnUseSelectedItemEventHandler();
        public event OnUseSelectedItemEventHandler OnUseSelectedItem;
        void RaiseOnUseItemEvent()
        {
            if (selectedIndex < 0 || selectedIndex >= Count)
                return;

            if (OnUseSelectedItem != null)
                OnUseSelectedItem();
        }

        public delegate void OnScrollHandler();
        public event OnScrollHandler OnScroll;
        void RaiseOnScrollEvent()
        {
            if (OnScroll != null)
                OnScroll();
        }

        #endregion
    }
}