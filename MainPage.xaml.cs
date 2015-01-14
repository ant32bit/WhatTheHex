using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WhatTheHex
{
    public partial class MainPage : UserControl
    {
        private const Int32 iMaxPerRow = 5;
        private const Int32 iMaxRows = 4;
        private Duration durNormal = new Duration(TimeSpan.FromSeconds(0.5));
        private Duration durFast = new Duration(TimeSpan.FromSeconds(0.25));
        private Random rRandomNumber = new Random();

        private Color[] acColourSwatches;
        private Boolean[] bActive;

        private Color cLabel;
        private Int32 iCorrectIndex;

        private Int32[] aiDifficulties = new Int32[] { 3, 4, 5, 6, 8, 10, 12, 15, 16, 20 };
        private Int32 iPerRow = 5;
        private Int32 iRows = 1;

        private Boolean bInGame = false;

        public MainPage()
        {
            InitializeComponent();
            sbNewGame.Begin();
        }

        private void PlaySwatchAnimation()
        {
            Storyboard sbColours = new Storyboard();

            ColorAnimation caLabelColor = new ColorAnimation();
            caLabelColor.To = cLabel;
            caLabelColor.Duration = new Duration(TimeSpan.FromSeconds(1));
            sbColours.Children.Add(caLabelColor);

            Storyboard.SetTargetName(caLabelColor, "labelBrush");
            Storyboard.SetTargetProperty(caLabelColor, new PropertyPath("Color"));

            for (Int32 i = 1; i <= iMaxPerRow * iMaxRows; i++)
            {
                Int32 iIndex = BoxToSwatch(i);

                ColorAnimation caBoxColor = new ColorAnimation();
                caBoxColor.To = (iIndex < 0) ? Colors.Black : acColourSwatches[iIndex];
                caBoxColor.Duration = durNormal;
                sbColours.Children.Add(caBoxColor);

                Storyboard.SetTargetName(caBoxColor, "boxBrush" + (i));
                Storyboard.SetTargetProperty(caBoxColor, new PropertyPath("Color"));

                DoubleAnimation daBoxHeight = new DoubleAnimation();
                daBoxHeight.To = (iIndex < 0) ? 0 : 100;
                daBoxHeight.Duration = durNormal;
                sbColours.Children.Add(daBoxHeight);

                Storyboard.SetTargetName(daBoxHeight, "boxColor" + (i));
                Storyboard.SetTargetProperty(daBoxHeight, new PropertyPath("Height"));

                DoubleAnimation daBoxWidth = new DoubleAnimation();
                daBoxWidth.To = (iIndex < 0) ? 0 : 100;
                daBoxWidth.Duration = durNormal;
                sbColours.Children.Add(daBoxWidth);

                Storyboard.SetTargetName(daBoxWidth, "boxColor" + (i));
                Storyboard.SetTargetProperty(daBoxWidth, new PropertyPath("Width"));
            }

            if (spSelection.Resources.Contains("Colours"))
            {
                spSelection.Resources.Remove("Colours");
            }
            spSelection.Resources.Add("Colours", sbColours);

            sbColours.Begin();
        }

        private void PlayRowAnimation()
        {
            Int32 iNewRows = (acColourSwatches.Length / iPerRow) + 1;
            // if (iRows == iNewRows) { return; }

            Storyboard sbRows = new Storyboard();

            for (Int32 i = 1; i <= iMaxRows; i++)
            {
                DoubleAnimation daRowHeight = new DoubleAnimation();
                daRowHeight.To = (i < iNewRows) ? 120 : 0;
                daRowHeight.Duration = durNormal;
                sbRows.Children.Add(daRowHeight);

                Storyboard.SetTargetName(daRowHeight, "spRow" + i);
                Storyboard.SetTargetProperty(daRowHeight, new PropertyPath("Height"));

                DoubleAnimation daRowWidth = new DoubleAnimation();
                daRowWidth.To = 110 * iPerRow;
                daRowWidth.Duration = durNormal;
                sbRows.Children.Add(daRowWidth);

                Storyboard.SetTargetName(daRowWidth, "spRow" + i);
                Storyboard.SetTargetProperty(daRowWidth, new PropertyPath("Width"));
            }

            if (spSelection.Resources.Contains("Rows"))
            {
                spSelection.Resources.Remove("Rows");
            }
            spSelection.Resources.Add("Rows", sbRows);

            sbRows.Begin();

            iRows = iNewRows;
        }

        private Color GetRandomColour()
        {
            Byte[] abRGBColour = new Byte[3];
            rRandomNumber.NextBytes(abRGBColour);

            return Color.FromArgb(255, abRGBColour[0], abRGBColour[1], abRGBColour[2]);
        }

        private String ColourToString(Color cInput)
        {
            return String.Format("#{0:X2}{1:X2}{2:X2}", cInput.R, cInput.G, cInput.B);
        }

        private Int32 BoxToSwatch(Int32 iBox)
        {
            Int32 iSwatch = ((iBox - 1) % iMaxPerRow) + 1;
            if (iSwatch > iPerRow)
            {
                return -1;
            }

            iSwatch += (((iBox - 1) / iMaxPerRow) * iPerRow);
            if (iSwatch > acColourSwatches.Length) { return -1; }

            return iSwatch - 1;
        }

        private Int32 SwatchToBox(Int32 iSwatch)
        {
            Int32 iBox = iSwatch / iPerRow;
            iBox *= iMaxPerRow;
            iBox += (iSwatch % iPerRow) + 1;
            return iBox;
        }

        private void UpdateCursors()
        {
            foreach (UIElement uieCurrRow in spSelection.Children)
            {
                foreach (UIElement uieCurrBox in ((StackPanel)uieCurrRow).Children)
                {
                    Rectangle boxCurrColor = (Rectangle)uieCurrBox;
                    Int32 iCurrBox = Int32.Parse(boxCurrColor.Name.Substring(8));
                    Int32 iCurrSwatch = BoxToSwatch(iCurrBox);

                    if (iCurrSwatch < 0)
                    {
                        boxCurrColor.Cursor = Cursors.Arrow;
                    }
                    else if (!bActive[iCurrSwatch])
                    {
                        boxCurrColor.Cursor = Cursors.Arrow;
                    }
                    else
                    {
                        boxCurrColor.Cursor = Cursors.Hand;
                    }
                }
            }
        }

        private void buttonNewGame_Click(object sender, RoutedEventArgs e)
        {
            Int32 iDifficulty = aiDifficulties[(Int32)sliderDifficulty.Value];

            if (iDifficulty % 5 == 0)
            {
                iPerRow = 5;
            }
            else if (iDifficulty % 4 == 0)
            {
                iPerRow = 4;
            }
            else if (iDifficulty % 3 == 0)
            {
                iPerRow = 3;
            }

            acColourSwatches = new Color[iDifficulty];
            bActive = new Boolean[iDifficulty];

            for (Int32 i = 0; i < acColourSwatches.Length; i++)
            {
                acColourSwatches[i] = GetRandomColour();
                bActive[i] = true;
            }

            cLabel = Colors.White;

            iCorrectIndex = rRandomNumber.Next(1, acColourSwatches.Length) - 1;

            labelColour.Content = ColourToString(acColourSwatches[iCorrectIndex]);

            PlayRowAnimation();
            PlaySwatchAnimation();
            UpdateCursors();

            bInGame = true;
        }

        private void boxColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!bInGame) { return; }

            Rectangle boxSender = (Rectangle)sender;
            Int32 iSelectedBox = Int32.Parse(boxSender.Name.Substring(8));
            Int32 iSelectedSwatch = BoxToSwatch(iSelectedBox);

            if (iCorrectIndex == iSelectedSwatch)
            {
                for (Int32 i = 0; i < acColourSwatches.Length; i++)
                {
                    if (i != iCorrectIndex)
                    {
                        acColourSwatches[i] = Colors.Black;
                    }

                    bActive[i] = false;
                }

                cLabel = acColourSwatches[iCorrectIndex];
                labelClickedColor.Content = "";

                PlaySwatchAnimation();

                boxColor_MouseOff(sender, e);
                bInGame = false;

                UpdateCursors();
            }
            else if (bActive[iSelectedSwatch])
            {
                boxSender.Cursor = Cursors.Arrow;
                labelClickedColor.Content = ColourToString(acColourSwatches[iSelectedSwatch]);
                acColourSwatches[iSelectedSwatch] = Colors.Black;

                PlaySwatchAnimation();
            }
        }

        private void boxColor_MouseOn(object sender, MouseEventArgs e)
        {
            if (!bInGame) { return; }

            Rectangle boxSender = (Rectangle)sender;
            Int32 iSelectedBox = Int32.Parse(boxSender.Name.Substring(8));
            Int32 iSelectedSwatch = BoxToSwatch(iSelectedBox);

            Storyboard sbTilt = new Storyboard();

            DoubleAnimation daBoxRotateX = new DoubleAnimation();
            daBoxRotateX.To = (iSelectedSwatch < 0) ? 0 : -10;
            daBoxRotateX.Duration = durFast;
            sbTilt.Children.Add(daBoxRotateX);

            Storyboard.SetTargetName(daBoxRotateX, "boxPlane" + (iSelectedBox));
            Storyboard.SetTargetProperty(daBoxRotateX, new PropertyPath("RotationX"));

            DoubleAnimation daBoxRotateY = new DoubleAnimation();
            daBoxRotateY.To = (iSelectedSwatch < 0) ? 0 : 20;
            daBoxRotateY.Duration = durFast;
            sbTilt.Children.Add(daBoxRotateY);

            Storyboard.SetTargetName(daBoxRotateY, "boxPlane" + (iSelectedBox));
            Storyboard.SetTargetProperty(daBoxRotateY, new PropertyPath("RotationY"));

            if (spSelection.Resources.Contains("PlanesOn"))
            {
                spSelection.Resources.Remove("PlanesOn");
            }
            spSelection.Resources.Add("PlanesOn", sbTilt);

            sbTilt.Begin();
        }

        private void boxColor_MouseOff(object sender, MouseEventArgs e)
        {
            if (!bInGame) { return; }

            Rectangle boxSender = (Rectangle)sender;
            Int32 iSelectedBox = Int32.Parse(boxSender.Name.Substring(8));
            Int32 iSelectedSwatch = BoxToSwatch(iSelectedBox);

            Storyboard sbTilt = new Storyboard();

            DoubleAnimation daBoxRotateX = new DoubleAnimation();
            daBoxRotateX.To = 0;
            daBoxRotateX.Duration = durFast;
            sbTilt.Children.Add(daBoxRotateX);

            Storyboard.SetTargetName(daBoxRotateX, "boxPlane" + (iSelectedBox));
            Storyboard.SetTargetProperty(daBoxRotateX, new PropertyPath("RotationX"));

            DoubleAnimation daBoxRotateY = new DoubleAnimation();
            daBoxRotateY.To = 0;
            daBoxRotateY.Duration = durFast;
            sbTilt.Children.Add(daBoxRotateY);

            Storyboard.SetTargetName(daBoxRotateY, "boxPlane" + (iSelectedBox));
            Storyboard.SetTargetProperty(daBoxRotateY, new PropertyPath("RotationY"));

            if (spSelection.Resources.Contains("PlanesOff"))
            {
                spSelection.Resources.Remove("PlanesOff");
            }
            spSelection.Resources.Add("PlanesOff", sbTilt);

            sbTilt.Begin();
        }
    }
}
