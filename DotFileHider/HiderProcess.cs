using System;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace DotFileHider
{
    public class HiderProcess
    {
        private string pathToLogFile;

        public HiderProcess()
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            pathToLogFile = Path
                .GetDirectoryName(System
                                .Reflection
                                .Assembly
                                .GetExecutingAssembly()
                                .Location)
                + "\\log_" + timeStamp + ".txt";
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void run()
        {
            // Set up a FileSystemWatcher that will be notified whenever anything on the C drive is created/modified
            // and handle the case if the content starts with a dot.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = "C:\\";
            watcher.IncludeSubdirectories = true;
            watcher.Filter = "*.*";
            watcher.Renamed += new RenamedEventHandler(onFileRenamed);
            watcher.Created += new FileSystemEventHandler(onWatcherCreated);
            watcher.NotifyFilter = NotifyFilters.LastAccess 
                | NotifyFilters.LastWrite 
                | NotifyFilters.FileName 
                | NotifyFilters.DirectoryName;
            watcher.EnableRaisingEvents = true;

            using (StreamWriter file = new StreamWriter(pathToLogFile))
            {
                string time = DateTime.Now.ToString("yyyy:MM:dd:HH:mm:ss");
                file.WriteLine(time + " - Begun listening for dot files to hide...");
            }

            // This loop is needed in order to keep the program running. The sleep is needed because, if it wasn't
            // present, the program would peg a CPU core at 100%. Sleeping the thread every minute or so keeps the
            // CPU usage low.
            int i = 0;
            while (true)
            {
                Thread.Sleep(1000);
                i++;
                if (i > 10)
                {
                    i = 0;
                }
            }
        }

        private void onWatcherCreated(object sender, FileSystemEventArgs a)
        {
            checkFile(a.FullPath);
        }

        private void onFileRenamed(object source, RenamedEventArgs a)
        {
            checkFile(a.FullPath);
        }

        private void checkFile(string filename)
        {
            string name = Path.GetFileName(filename);
            if (name.StartsWith(".") && !File.GetAttributes(filename).HasFlag(FileAttributes.Hidden))
            {
                // If the filename starts with a dot, and isn't already hidden, go ahead and hide it.
                File.SetAttributes(filename, File.GetAttributes(filename) | FileAttributes.Hidden);
                using (StreamWriter file = new StreamWriter(pathToLogFile, true))
                {
                    string time = DateTime.Now.ToString("yyyy:MM:dd:HH:mm:ss");
                    file.WriteLine(time + " - File hidden at " + filename);
                }
            }
        }
    }
}
