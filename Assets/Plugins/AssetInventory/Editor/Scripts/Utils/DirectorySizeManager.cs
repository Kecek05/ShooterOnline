using System.IO;
using System;
using System.Threading.Tasks;

namespace AssetInventory
{
    public class DirectorySizeManager
    {
        private const int MIN_ALIVE_TIME = 20; // minutes

        public bool Enabled = true;
        public bool IsRunning;
        public long CurrentSize;
        public DateTime LastCheckTime;

        private string _path;
        private long _byteLimit;
        private bool _isMonitoring;
        private readonly Func<string, bool> _validator;

        public DirectorySizeManager(string path, int gbLimit, Func<string, bool> validator)
        {
            _path = path;
            _validator = validator;

            SetLimit(gbLimit);
        }

        public void SetLimit(int gbLimit)
        {
            _byteLimit = gbLimit * 1024L * 1024L * 1024L;
        }

        public long GetLimit()
        {
            return _byteLimit;
        }

        public async void StartMonitoring(int scanPeriod)
        {
            _isMonitoring = true;
            while (_isMonitoring)
            {
                await Task.Delay(scanPeriod);
                if (!_isMonitoring) break;

                CheckAndClean();
            }
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
        }

        public async void CheckAndClean()
        {
            if (IsRunning || !Enabled) return;
            IsRunning = true;
            try
            {
                CurrentSize = await IOUtils.GetFolderSize(_path);
                if (CurrentSize > _byteLimit)
                {
                    string[] subDirs = Directory.GetDirectories(_path);
                    Array.Sort(subDirs, delegate(string a, string b)
                    {
                        DirectoryInfo aInfo = new DirectoryInfo(a);
                        DirectoryInfo bInfo = new DirectoryInfo(b);
                        return aInfo.CreationTime.CompareTo(bInfo.CreationTime);
                    });
                    int index = 0;
                    while (index < subDirs.Length && CurrentSize > _byteLimit)
                    {
                        // check if folder is older than 10 minutes to ensure just created folders which might still be in use are not deleted
                        if (DateTime.Now - Directory.GetCreationTime(subDirs[index]) < TimeSpan.FromMinutes(MIN_ALIVE_TIME))
                        {
                            index++;
                            continue;
                        }

                        if (!Enabled) break;
                        if (!_validator(subDirs[index]))
                        {
                            index++;
                            continue;
                        }

                        long subDirSize = await IOUtils.GetFolderSize(subDirs[index]);

                        // run non-blocking, no need to wait for deletion
                        int i = index;
                        _ = Task.Run(() => IOUtils.DeleteFileOrDirectory(subDirs[i]));

                        CurrentSize -= subDirSize;
                        index++;
                    }
                }
            }
            finally
            {
                IsRunning = false;
            }
            LastCheckTime = DateTime.Now;
        }
    }
}