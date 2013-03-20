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
using Backend.Data;
using Chat.Frontend;
using System;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chat.UI.Flyout
{
    public sealed partial class StatusEdit : UserControl
    {
        private App Frontend { get { return (App)App.Current; } }
        private Flyout flyoutSelf = null;

        private Status CurrentStatus = null;

        public StatusEdit(Flyout self)
        {
            this.InitializeComponent();
            flyoutSelf = self;

            this.DataContext = Frontend.Status;

            CurrentStatus = Frontend.Status;

            if( CurrentStatus != null )
            {
                this.Avatar.AvatarImage = CurrentStatus.ImageData;
                this.Message.Text = CurrentStatus.message;

                foreach (ComboBoxItem status in StatusSelector.Items)
                {
                    if (Convert.ToInt32(status.Tag) == (int)CurrentStatus.status)
                        StatusSelector.SelectedItem = status;
                }

                if (StatusSelector.SelectedItem == null)
                    StatusSelector.SelectedItem = Offline;
            }
        }

        private async void OnSetAvatar(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".bmp");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".gif");

            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var file = await filePicker.PickSingleFileAsync();
            if( file != null )
            {
                var filetype = file.ContentType;

                IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                if( fileStream != null )
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                    if (decoder != null)
                    {
                        InMemoryRandomAccessStream imageStream = new InMemoryRandomAccessStream();
                        BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(imageStream, decoder);
                        if (encoder != null)
                        {
                            encoder.BitmapTransform.ScaledHeight = 64;
                            encoder.BitmapTransform.ScaledWidth = 64;

                            try
                            {
                                await encoder.FlushAsync();
                                
                                // Rewind !
                                imageStream.Seek(0);

                                var reader = new DataReader(imageStream);
                                await reader.LoadAsync((uint)imageStream.Size);
                                if ( imageStream.Size > 0)
                                {
                                    byte[] image = new byte[imageStream.Size];
                                    reader.ReadBytes(image);
                                    CurrentStatus.SetAvatar(image);

                                    XMPPHelper.PublishAvatar(filetype, image);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            flyoutSelf.Hide();
        }

        private async void OnSave(object sender, RoutedEventArgs e)
        {
            await Frontend.RunAsync(() =>
            {
                CurrentStatus.message = Message.Text;

                StatusType lastStatus = CurrentStatus.status;
                ComboBoxItem selectedStatus = StatusSelector.SelectedItem as ComboBoxItem;
                if (selectedStatus != null)
                    CurrentStatus.status = (StatusType)Convert.ToInt32(selectedStatus.Tag);
                else
                    CurrentStatus.status = StatusType.Offline;

                if (lastStatus != StatusType.Offline)
                    Helper.PublishState(CurrentStatus.status, CurrentStatus.message);

                Frontend.Events.StatusChanged();
                Frontend.Backend.UpdateConnections();
            });

            flyoutSelf.Hide();
        }
    }
}
