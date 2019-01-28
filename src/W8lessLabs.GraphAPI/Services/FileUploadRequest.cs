namespace W8lessLabs.GraphAPI
{
    public class FileUploadRequest
    {
        /// <summary>
        /// Folder path to where the File is to be stored.
        /// Use / for root. Or /SomeFolder/SubFolder to specify a sub folder.
        /// </summary>
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public int FileSize { get; set; }
        /// <summary>
        /// Set to True to overwrite an existing file, otherwise, the request will fail and a 
        /// different filename will need to be specified.
        /// </summary>
        public bool Overwrite { get; set; }

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
