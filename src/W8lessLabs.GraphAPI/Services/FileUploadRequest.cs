using System;

namespace W8lessLabs.GraphAPI
{
    public class FileUploadRequest
    {
        public FileUploadRequest(string filePath,
            string fileName,
            string description,
            int fileSize,
            bool overwrite,
            SpecialFolder? specialFolder = null)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Description = description;
            FileSize = fileSize;
            Overwrite = overwrite;
            SpecialFolder = specialFolder;
        }

        /// <summary>
        /// Folder path to where the File is to be stored.
        /// Use / for root. Or /SomeFolder/SubFolder to specify a sub folder.
        /// </summary>
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string Description { get; private set; }
        public int FileSize { get; private set; }
        public SpecialFolder? SpecialFolder { get; private set; }
        /// <summary>
        /// Set to True to overwrite an existing file, otherwise, the request will fail and a 
        /// different filename will need to be specified.
        /// </summary>
        public bool Overwrite { get; private set; }

        public string GetFullFilePath()
        {
            string filePath = GetPathNormalized();

            if (!filePath.EndsWith("/"))
                filePath = filePath + "/";

            return filePath + FileName;
        }

        public string GetPathNormalized()
        {
            string filePath = FilePath;
            if (string.IsNullOrEmpty(filePath) || filePath == "/")
                filePath = "/";
            else if (!filePath.StartsWith("/"))
                filePath = "/" + filePath;
            return filePath;
        }
    }
}
