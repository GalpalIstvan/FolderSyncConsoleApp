# Folder Sync Console App 
This is a Console app made for Veeam for the interview process.
To use this console app, clone the repository, open the repository folder, navigate to folder-sync-console-app\ConsoleAppFolderSync\bin\Debug\net8.0 and open up a Terminal window in this folder, then proceed to write the following:
./ConsoleAppFolderSync.exe "log_folder" "source_folder" "target_folder" "interval_time"
Where:
log_folder is the path where the log should be saved
source_folder is the path where the source folder is located, whose files should be duplicated to the target folder
target_folder is the patch where the target folder is located, whose contents should be the same as the source folder after the syncing progress
interval_time is a value as float in seconds, amount in which the whole syncing progress should be repeated

e.g. ./ConsoleAppFolderSync.exe "C:\Folder Sync\Log Folder" "C:\Folder Sync\Source Folder" "C:\Folder Sync\Target Folder" "5"

If the log_folder is not given by the user, the log file will be created in the folder in which the exe file is located (folder-sync-console-app\ConsoleAppFolderSync\bin\Debug\net8.0) and the progress will be stopped.
If the source_folder or the target_folder is not given, the progress will be stopped.
If the interval_time is not given, the progress will continue with the interval_time as the default amount, that is in this case 5 seconds.