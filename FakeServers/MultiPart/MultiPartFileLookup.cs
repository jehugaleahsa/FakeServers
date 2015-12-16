using System;
using System.Collections.Generic;

namespace FakeServers.MultiPart
{
    public class MultiPartFileLookup
    {
        private readonly Dictionary<string, List<MultiPartFile>> fileLookup;
        private readonly List<MultiPartFile> nullFiles;

        public MultiPartFileLookup()
        {
            this.fileLookup = new Dictionary<string, List<MultiPartFile>>();
            this.nullFiles = new List<MultiPartFile>();
        }

        public bool HasName(string name)
        {
            if (name == null)
            {
                return nullFiles.Count > 0;
            }
            else
            {
                return fileLookup.ContainsKey(name);
            }
        }

        public void Add(string name, MultiPartFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            if (name == null)
            {
                nullFiles.Add(file);
            }
            else
            {
                List<MultiPartFile> files;
                if (!fileLookup.TryGetValue(name ?? String.Empty, out files))
                {
                    files = new List<MultiPartFile>();
                    fileLookup.Add(name ?? String.Empty, files);
                }
                files.Add(file);
            }
        }

        public MultiPartFile[] GetFiles(string name)
        {
            if (name == null)
            {
                return nullFiles.ToArray();
            }
            List<MultiPartFile> files;
            if (fileLookup.TryGetValue(name, out files))
            {
                return files.ToArray();
            }
            return null;
        }

        public MultiPartFile[] GetAllFiles()
        {
            List<MultiPartFile> files = new List<MultiPartFile>(nullFiles);
            foreach (List<MultiPartFile> namedFiles in fileLookup.Values)
            {
                files.AddRange(namedFiles);
            }
            return files.ToArray();
        }
    }
}
