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
using System.Collections.Generic;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Backend.Data
{
    public class Avatar
    {
        private static readonly string FolderName = "avatar";
        private static readonly string DefaultAvatarURI = "ms-appx:///Assets/DefaultAvatar.png";

        private static byte[] _defaultAvatarData = null;
        private static byte[] DefaultAvatarData
        {
            get
            {
                if (_defaultAvatarData == null)
                {
                    var uri = new Uri(DefaultAvatarURI);
                    var getTask = Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri).AsTask();
                    getTask.Wait(10000);

                    if (getTask.IsCompleted && getTask.Result != null)
                    {
                        _defaultAvatarData = LoadFile(getTask.Result);
                    }
                }

                return _defaultAvatarData;
            }
        }

        private static StorageFolder Folder 
        {
            get
            {
                var task = ApplicationData.Current.LocalFolder.CreateFolderAsync(FolderName, CreationCollisionOption.OpenIfExists).AsTask();
                task.Wait(10000);
                if (task.IsCompleted && task.Result != null)
                    return task.Result;
                else
                    return null;
            }
        }
        public static string GetFileURI(string jid)
        {
            var file = FindLatestFile(Helper.EncodeBASE64(jid));
            if (file != null)
                return "ms-appdata:///local/" + FolderName + "/" + file.Name;
            else
                return DefaultAvatarURI;
        }

        public static byte[] GetFile(string jid)
        {
            var file = FindLatestFile(Helper.EncodeBASE64(jid));

            if (file != null)
                return LoadFile(file);

            return DefaultAvatarData;
        }

        public static string Set(string jid, byte[] image)
        {
            try
            {
                // Get the sha1 hash of the image for comparing
                var sha1Algorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
                var hashBuffer = sha1Algorithm.HashData(CryptographicBuffer.CreateFromByteArray(image));
                string hash = CryptographicBuffer.EncodeToHexString(hashBuffer);

                // Get Filename
                var filename = Helper.EncodeBASE64(jid) + "_" + Helper.UnixTimestampFromDateTime(DateTime.Now);

                // Create the file ( and overwrite if neccessary )
                var createTask = Folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting).AsTask();
                createTask.Wait(10000);

                // If creation succeeded, write the image
                if (createTask.IsCompleted && createTask.Result != null)
                {
                    var writeTask = FileIO.WriteBytesAsync(createTask.Result, image).AsTask();
                    writeTask.Wait(10000);

                    // return the hash of the new image if everything went well
                    if (writeTask.IsCompleted)
                        return hash;
                }
            } catch {}

            return string.Empty;
        }

        public static void RemoveOld()
        {
            var mapLatest = new Dictionary<string, int>();
            var mapLatestFile = new Dictionary<string, StorageFile>();

            foreach (var file in GetFiles())
            {
                int lastUnderscore = file.Name.LastIndexOf('_');

                string jidhash = file.Name.Substring(0, lastUnderscore);
                int current = Convert.ToInt32(file.Name.Substring(lastUnderscore + 1));

                if (!mapLatest.ContainsKey(jidhash) && !mapLatestFile.ContainsKey(jidhash))
                {
                    mapLatest.Add(jidhash, current);
                    mapLatestFile.Add(jidhash, file);
                }
                else
                {
                    if (current > mapLatest[jidhash])
                    {
                        mapLatest[jidhash] = current;
                        mapLatestFile[jidhash].DeleteAsync().AsTask().Wait(10000);
                        mapLatestFile[jidhash] = file;
                    }
                }
            }
        }

        public static void Remove(string jid)
        {
            try
            {
                foreach (var file in FindFiles(Helper.EncodeBASE64(jid)))
                    file.DeleteAsync().AsTask().Wait(10000);
            }
            catch { }
        }

        public static void Clear()
        {
            try 
            {
                var files = GetFiles();
                if (files != null)
                {
                    foreach (var file in files)
                        file.DeleteAsync().AsTask().Wait(10000);
                }
            }
            catch { }
        }


        public static BitmapImage BitmapFromBytes(byte[] data)
        {
            var imageData = new BitmapImage();

            if (data != null)
            {
                var stream = new InMemoryRandomAccessStream();
                var writer = new DataWriter(stream);
                writer.WriteBytes(data);
                writer.StoreAsync().AsTask().Wait(10000);
                stream.Seek(0);
                imageData.SetSource(stream);
            }

            return imageData;
        }

        private static StorageFile FindLatestFile(string searchterm)
        {
            int latest = 0;
            StorageFile latestFile = null;

            foreach (var file in FindFiles(searchterm))
            {
                int lastUnderscore = file.Name.LastIndexOf('_');

                int current = Convert.ToInt32(file.Name.Substring(lastUnderscore + 1));

                if (current > latest)
                {
                    latestFile = file;
                    latest = current;
                }
            }

            return latestFile;
        }

        private static byte[] LoadFile(StorageFile file)
        {
            try
            {
                if (file != null)
                {
                    // Get the file
                    var openTask = file.OpenAsync(FileAccessMode.Read).AsTask();
                    openTask.Wait(10000);

                    if (openTask.IsCompleted && openTask.Result != null)
                    {
                        IRandomAccessStream fileStream = openTask.Result;

                        var reader = new DataReader(fileStream);
                        var loadTask = reader.LoadAsync((uint)fileStream.Size).AsTask();
                        loadTask.Wait(10000);

                        if (loadTask.IsCompleted)
                        {
                            byte[] image = new byte[fileStream.Size];
                            reader.ReadBytes(image);
                            return image;
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        private static List<StorageFile> FindFiles(string searchterm)
        {
            List<StorageFile> results = new List<StorageFile>();

            var files = GetFiles();
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Name.StartsWith(searchterm))
                        results.Add(file);
                }
            }

            return results;
        }

        private static IReadOnlyList<StorageFile> GetFiles()
        {
            try
            {
                var getFilesTask = Folder.GetFilesAsync().AsTask();
                getFilesTask.Wait(10000);
                if (getFilesTask.IsCompleted && getFilesTask.Result != null)
                    return getFilesTask.Result;
                else
                    return null;
            }
            catch { }

            return null;
        }
    }
}
