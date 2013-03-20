//###################################################################################################
/*
    Copyright (c) since 2012 - Paul Freund 
    
    Permission is hereby granted, free of charge, to any person
    obtaining a copy of this software and associated documentation
    files (the "Software"), to deal in the Software without
    restriction, including without limitation the rights to use,
    copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the
    Software is furnished to do so, subject to the following
    conditions:
    
    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
    HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
    FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
    OTHER DEALINGS IN THE SOFTWARE.
*/
//###################################################################################################

using Backend.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace Chat.UI.Controls
{
    public sealed partial class ColorEdit : UserControl
    {
        private App Frontend { get { return (App)App.Current; } }


        public static DependencyProperty ColorValueProperty = DependencyProperty.Register("ColorValue", typeof(string), typeof(ColorEdit), new PropertyMetadata(null, new PropertyChangedCallback(OnColorValueChanged)));
        public string ColorValue { get { return (string)GetValue(ColorValueProperty); } set { SetValue(ColorValueProperty, value);} }

        private static void OnColorValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorEdit)
            {
                var control = d as ColorEdit;
                control.ColorEditBox.Text = control.ColorValue;
            }   
        }


        public static DependencyProperty ColorNameProperty = DependencyProperty.Register("ColorName", typeof(string), typeof(ColorEdit), null);
        public string ColorName { get { return (string)GetValue(ColorNameProperty); } set { SetValue(ColorNameProperty, value); } }

        
        public ColorEdit()
        {
            this.InitializeComponent();

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!(sender is TextBox))
                    return;

                var textbox = sender as TextBox;

                if (!string.IsNullOrEmpty(textbox.Text))
                {
                    var hash = textbox.Text[0];
                    var value = textbox.Text.Substring(1);
                    var isHex = System.Text.RegularExpressions.Regex.IsMatch(value, @"\A\b[0-9a-fA-F]+\b\Z");

                    if (hash == '#' && value.Length == 8 && isHex)
                    {
                        ColorValue = textbox.Text;
                        textbox.BorderBrush = Frontend.Resources["TextBoxBorderThemeBrush"] as SolidColorBrush;
                        return;
                    }
                }

                textbox.BorderBrush = new SolidColorBrush(Helper.GetColorFromHexString(Frontend.AppColors.HighlightImportant));
            }
            catch (Exception uiEx) { Frontend.UIError(uiEx); }
        }
    }
}
