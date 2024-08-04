using System.Reflection;

class FolderSync
{
    private string logFilePath = string.Empty;
    private List<string> sameItems = new List<string>();
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private Task? timerTask;
    private float timeToRepeat;
    private string sourcePathString = string.Empty;
    private string targetPathString = string.Empty;
    private string logPathString = string.Empty;

    static void Main(string[] args)
    {
        var folderSync = new FolderSync();
        
        if(args == null || args.Count() <= 0)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            folderSync.CreateLogFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
#pragma warning restore CS8604 // Possible null reference argument.
            folderSync.WriteToLogAndConsole("Log Path is empty! Creating Log file to executable location and ending process...");
            Environment.Exit(0);
        }
        else if (args.Count() <= 1)
        {
            folderSync.WriteToLogAndConsole("Source Path is empty! Ending process...");
            Environment.Exit(0);
        }
        else if (args.Count() <= 2)
        {
            folderSync.WriteToLogAndConsole("Target Path is empty! Ending process...");
            Environment.Exit(0);
        }
        else if (args.Count() <= 3)
        {
            folderSync.WriteToLogAndConsole("Timer is not given! Using the default 5 seconds as timer!");
            folderSync.timeToRepeat = 5f;
        }

        string LogPathString = args[0];
        string SourcePathString = args[1];
        string TargetPathString = args[2];
        float TimeToRepeatInSeconds = float.Parse(args[3]);
        
        folderSync.sourcePathString = SourcePathString;
        folderSync.targetPathString = TargetPathString;
        folderSync.logPathString = LogPathString;
        folderSync.timeToRepeat = TimeToRepeatInSeconds;

        folderSync.CreateLogFile(folderSync.logPathString);
        folderSync.StartTimer();
        folderSync.Logic();
        folderSync.WriteToLogAndConsole("Started periodic folder synchronization. Press any key to stop the synchronization process!");

        Console.ReadKey();

        folderSync.StopTimer();

        folderSync.WriteToLogAndConsole("Timer was cancelled!");
    }

    private void Logic()
    {
        WriteToLogAndConsole("Syncing instance started.");
        DeleteFilesNotInSourceFolder(targetPathString, sourcePathString);
        CopyFilesFromSource(sourcePathString, targetPathString);
    }

    private void DeleteFilesNotInSourceFolder(string targetPath, string sourcePath)
    {
        sameItems.Clear();
        List<string> filesToDelete = new List<string>();
        foreach (var itemInTargetPath in Directory.GetFiles(targetPath))
        {
            string filename = Path.GetFileName(itemInTargetPath);
            string newSourcePath = Path.Combine(sourcePath, filename);

            if(FileExists(newSourcePath))
            {
                if (HasSameLength(newSourcePath, itemInTargetPath))
                {
                    if(IsSameFile(newSourcePath, itemInTargetPath))
                    {
                        WriteToLogAndConsole("The file " +  filename + " from " + sourcePath +  " already exists in " + targetPath + ", so it will not be replaced.");
                        sameItems.Add(newSourcePath);
                    }
                    else
                    {
                        filesToDelete.Add(itemInTargetPath);
                    }
                }
                else
                {
                    filesToDelete.Add(itemInTargetPath);
                }
            }
            else
            {
                filesToDelete.Add(itemInTargetPath);
            }
        }
        foreach (var item in filesToDelete)
        {
            WriteToLogAndConsole("Deleted: " + item + " from the target folder.");
            File.Delete(item);
        }
    }

    private bool FileExists(string newPath)
    {
        return File.Exists(newPath);
    }

    private bool HasSameLength(string sourceItem, string targetItem)
    {
        FileInfo sourceFileInfo = new FileInfo(sourceItem);
        FileInfo targetFileInfo = new FileInfo(targetItem);
        return sourceFileInfo.Length == targetFileInfo.Length;
    }

    private bool IsSameFile(string sourceItem, string targetItem)
    {
        return File.ReadLines(sourceItem).SequenceEqual(File.ReadLines(targetItem));
    }

    private void CreateLogFile(string logPath)
    {
        string fileName = "Log date " + DateTime.Now.ToString("MM-dd, h-mm-ss tt") + ".txt";
        string filePath = Path.Combine(logPath, fileName);
        logFilePath = filePath;
        using StreamWriter logStream = new StreamWriter(filePath);
    }

    private void WriteToLogAndConsole(string logAppend)
    {
        using(StreamWriter logFile = new StreamWriter(logFilePath, true))
        {
            logFile.WriteLine(logAppend);
            Console.WriteLine(logAppend);
        }
    }

    private void CopyFilesFromSource(string sourcePath, string targetPath)
    {
        foreach (var item in Directory.GetFiles(sourcePath))
        {
            if (!sameItems.Contains(item))
            {
                string fileName = Path.GetFileName(item);
                string newPath = Path.Combine(targetPath, fileName);
                File.Copy(item, newPath);
                WriteToLogAndConsole("Copied: " + fileName + " from the source folder to the target folder.");
            }
        }
        WriteToLogAndConsole("Syncing instance finished.");
    }

    private void StartTimer()
    {
        timerTask = DoWorkAsync(); 
    }

    private async Task DoWorkAsync()
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(timeToRepeat));
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationTokenSource.Token))
                {
                    Logic();
                }
            }
            catch (OperationCanceledException){}
    }

    private async void StopTimer()
    {
        await AwaitingStopTimer();
    }

    private async Task AwaitingStopTimer()
    {
        if(timerTask == null)
        {
            return;
        }

        cancellationTokenSource.Cancel();
        await timerTask;
        cancellationTokenSource.Dispose();
        WriteToLogAndConsole("Periodic folder syncing has been stopped!");
    }

}
