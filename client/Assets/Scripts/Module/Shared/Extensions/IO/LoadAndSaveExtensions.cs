﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; 

namespace Module.Shared
{ 
    public static class LoadAndSaveExtensions {

        public static void SaveStream(this FileInfo self, Stream streamToSave) {
            using (var fileStream = File.Create(self.FullName)) { streamToSave.CopyTo(fileStream); }
        }

        public static FileStream LoadAsStream(this FileInfo self, FileMode fileMode = FileMode.Open,
                               FileAccess fileAccess = FileAccess.Read, FileShare fileShare = FileShare.Read) {
            return File.Open(self.FullPath(), fileMode, fileAccess, fileShare);
        }

        public static T LoadAs<T>(this FileInfo self, FileShare fileShare = FileShare.Read) {
            using (FileStream stream = self.LoadAsStream(fileShare: fileShare)) {
                return stream.LoadAs<T>();
            }
        } 

        public static T LoadAs<T>(this Stream self) {
            using (StreamReader s = new StreamReader(self)) {
                if (typeof(T) == typeof(string)) { return (T)(object)s.ReadToEnd(); }
                return JsonConvert.DeserializeObject<T>(s.ReadToEnd());
            }
        }

        public static object LoadAs(this Stream self, Type t) {
            using (StreamReader s = new StreamReader(self)) {
                if (t == typeof(string)) { return s.ReadToEnd(); }
                return JsonConvert.DeserializeObject(s.ReadToEnd(), t);
            }
        }

        public static void SaveAsJson<T>(this FileInfo self, T objectToSave) {
            using (StreamWriter streamWriter = File.CreateText(self.FullPath())) {
                streamWriter.SaveAsJson(objectToSave);
            }
        }

        public static void SaveAsJson<T>(this FileInfo self, T objectToSave, bool asPrettyString = false) {

            if (asPrettyString) {
                SaveAsText(self, JsonConvert.SerializeObject(objectToSave, Formatting.Indented));
                return;
            }

            using (var stream = self.OpenOrCreateForWrite()) {
                stream.SetLength(0); // Reset the stream in case it was opened
                using (StreamWriter streamWriter = new StreamWriter(stream)) {
                    streamWriter.SaveAsJson(objectToSave);
                }
            }
        }

        public static Stream OpenOrCreateForWrite(this FileInfo self, FileShare share = FileShare.None) {
            return self.Open(FileMode.OpenOrCreate, FileAccess.Write, share);
        }

        public static Stream OpenOrCreateForReadWrite(this FileInfo self, FileShare share = FileShare.Read) {
            return self.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, share);
        }

        public static Stream OpenForRead(this FileInfo self, FileMode fileMode = FileMode.Open,
                               FileAccess fileAccess = FileAccess.Read, FileShare fileShare = FileShare.Read) {
            return self.Open(fileMode, fileAccess, fileShare);
        }

        public static void SaveStream(this FileInfo self, Stream streamToSave, Action<long> onProgress = null, bool resetStreamToStart = true) {
            self.ParentDir().CreateV2();
            if (resetStreamToStart) {
                if (!streamToSave.CanSeek) { throw new InvalidOperationException("Stream not seekable, cant jump back to start of stream, use resetStreamToStart = false or first do stream.CopyToSeekableStreamIfNeeded()"); }
                streamToSave.ResetStreamCurserPositionToBeginning();
            }
            using (var fileStream = self.OpenOrCreateForWrite()) {
                fileStream.SetLength(0); // Reset the stream in case it was opened
                if (onProgress == null) {
                    streamToSave.CopyTo(fileStream);
                } else {
                    streamToSave.CopyTo(fileStream, onProgress);
                }
            }
            if (resetStreamToStart) { streamToSave.ResetStreamCurserPositionToBeginning(); }
        }

        public static async Task SaveStreamAsync(this FileInfo self, Stream streamToSave, Action<long> onProgress = null, bool resetStreamToStart = true) {
            self.ParentDir().CreateV2();
            if (resetStreamToStart) {
                if (!streamToSave.CanSeek) { throw new InvalidOperationException("Stream not seekable, cant jump back to start of stream, use resetStreamToStart = false or first do stream.CopyToSeekableStreamIfNeeded()"); }
                streamToSave.ResetStreamCurserPositionToBeginning();
            }
            using (var fileStream = self.OpenOrCreateForWrite()) {
                fileStream.SetLength(0); // Reset the stream in case it was opened
                if (onProgress == null) {
                    await streamToSave.CopyToAsync(fileStream);
                } else {
                    await streamToSave.CopyToAsync(fileStream, onProgress);
                }
            }
            if (resetStreamToStart) { streamToSave.ResetStreamCurserPositionToBeginning(); }
        }

        public static void SaveAsJson<T>(this StreamWriter self, T objectToSave)
        {
            var json = JsonConvert.SerializeObject(objectToSave);
            self.Write(json);
        }

        /// <summary> This method helps with decrypting the string before parsing it as a json object </summary>
        public static T LoadAsEncyptedJson<T>(this FileInfo self, string jsonEncrKey, Func<T> getDefaultValue) {
            try {
                return JsonConvert.DeserializeObject<T>(self.LoadAs<string>().Decrypt(jsonEncrKey));
            }
            catch (Exception e) { Log.Warning("" + e); return getDefaultValue(); }
        }

        public static void SaveAsEncryptedJson<T>(this FileInfo self, T objectToSave, string jsonEncrKey)
        {
            var json = JsonConvert.SerializeObject(objectToSave); 
            self.SaveAsText(json.Encrypt(jsonEncrKey));
        }

        public static void SaveAsText(this FileInfo self, string text) {
            self.ParentDir().Create();
            File.WriteAllText(self.FullPath(), text, Encoding.UTF8);
        } 

        public static void WriteAsText(this Stream self, string text) {
            self.SetLength(0); // Reset the stream in case it was opened
            using (var w = new StreamWriter(self, Encoding.UTF8)) { w.Write(text); }
        }

    }

} 