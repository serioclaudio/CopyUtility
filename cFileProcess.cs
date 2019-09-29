using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

/*
SOURCE_FILE               -- A file to be copied
DESTINATION_FOLDER        -- Folder to copy the Source File  
ARCHIVE_FILE_SUFFIX       -- When a file in the Destination folder is renamed before the Source file is copied, this is the suffix to append
ARCHIVE_FILE_DELETE       -- Indicates whether the Archive file will be deleted
*/
namespace FileCopyUtility_001
{

    class cFileProcess
    {
        private const string COMMENT_DELIMITER = "//";

        private const string CATEGORY_HEADER_SOURCE_FILE = "SOURCE_FILE=";
        private const string CATEGORY_HEADER_DESTINATION_FOLDER = "DESTINATION_FOLDER=";
        private const string CATEGORY_HEADER_ARCHIVE_FILE_SUFFIX = "ARCHIVE_FILE_SUFFIX=";
        private const string CATEGORY_HEADER_ARCHIVE_FILE_DELETE = "ARCHIVE_FILE_DELETE=";

        private string ProcessFile = "";

        private List<string> SourceFiles = new List<string>();
        private List<string> DestinationFolders = new List<string>();

        private string ArchiveFileSuffixString = "ZYX";


        private string CurrentDateTimeStamp = "";

        private string LogFile = ""; 

        
        bool AppendDateToArchiveFileName = true;

        bool DeleteArchiveFile = false;

        private int ErrorCode = 0;
        private string ErrorMessage = "";

        private string Indent = "  ";


        public cFileProcess(string fileToProcess)
        {

            // Set the current date/time stamp
            CurrentDateTimeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Set the Log File
            LogFile = "CopyUtilityLog_" + CurrentDateTimeStamp + ".txt";

            // Set the current Process File
            ProcessFile = fileToProcess;

            // Start Processing
            StartProcessing();
        }

        private void ConsoleAndLog(string strMessage)
        {
            Console.WriteLine(strMessage);
            LogMessage(strMessage);
        }

        private void StartProcessing()
        {
            ConsoleAndLog("Starting to Process");
            ConsoleAndLog("  Process File: " + ProcessFile);
            ConsoleAndLog("  Timestamp Used: " + CurrentDateTimeStamp);
            ConsoleAndLog("  Operation details will be logged to file: " + LogFile);


            LoadProcessFile();
            
            if (ErrorMessage.Length > 0 )
            {
                ConsoleAndLog(Indent + ErrorMessage);
                return;
            }

            
            ValidateProcessFile();

            if (ErrorMessage.Length > 0)
            {
                ConsoleAndLog(Indent + ErrorMessage);
                return;
            }

            ConsoleAndLog("  Source Files to Process: " + SourceFiles.Count.ToString());
            ConsoleAndLog("  Destination Folders to Process: " + DestinationFolders.Count.ToString());


            CopyFiles();

            if (ErrorMessage.Length > 0)
            {
                ConsoleAndLog(Indent + ErrorMessage);
                return;
            }

            ConsoleAndLog("End of Processing.");

        }

        private void CopyFiles()
        {
            string SourceCopyFile = "";
            string DestinationCopyFile = "";
            string ArchiveFile = "";

            int iSourceLoop = 0;
            int iDestinationLoop = 0;

            for (iDestinationLoop = 0; iDestinationLoop < DestinationFolders.Count; iDestinationLoop++)
            {
                for ( iSourceLoop = 0; iSourceLoop < SourceFiles.Count; iSourceLoop++)
                {
                    SourceCopyFile = SourceFiles[iSourceLoop];
                    DestinationCopyFile = DestinationFolders[iDestinationLoop];

                    if (DestinationCopyFile.Substring(DestinationCopyFile.Length - 1) != @"\")
                    {
                        DestinationCopyFile += @"\";
                    }

                    DestinationCopyFile += Path.GetFileName(SourceCopyFile);
                    ArchiveFile = DestinationCopyFile + ArchiveFileSuffixString;

                    if (AppendDateToArchiveFileName)
                    {
                        ArchiveFile += CurrentDateTimeStamp;
                    }

                    // Rename the Destination File to Archive file
                    if (File.Exists(DestinationCopyFile))
                    {
                        try
                        {
                            File.Move(DestinationCopyFile, ArchiveFile);
                        }
                        catch(Exception ex)
                        {
                            ErrorCode = -90;
                            ErrorMessage = "Error archiving file: " + DestinationCopyFile + "   to Archive file: " + ArchiveFile + "   . Error received: " + ex.Message;
                            return;
                        }
                    }

                    LogMessage(Indent + "Archived Destination File: " + DestinationCopyFile + " to " + ArchiveFile);

                    // Copy the Source file to the destination file
                    try
                    {
                        File.Copy(SourceCopyFile, DestinationCopyFile);
                    }
                    catch (Exception ex)
                    {
                        ErrorCode = -91;
                        ErrorMessage = "Error copying file: " + SourceCopyFile + "   to Destination file: " + DestinationCopyFile + "   . Error received: " + ex.Message;
                        return;
                    }

                    LogMessage(Indent + "Copied Source File: " + SourceCopyFile + " to " + DestinationCopyFile);

                    if (DeleteArchiveFile == true)
                    {
                        try
                        {
                            File.Delete(ArchiveFile);
                        }
                        catch (Exception ex)
                        {
                            ErrorCode = -92;
                            ErrorMessage = "Error deleting temporary Archive file: " + ArchiveFile + "   . Error received: " + ex.Message;
                            return;
                        }

                        LogMessage(Indent + "Deleted Archive File: " + ArchiveFile);

                    }
                }
            }
        }

        private void ValidateProcessFile()
        {
            // Verify thet we have at least one source file
            if (SourceFiles.Count == 0 )
            {
                ErrorCode = -200;
                ErrorMessage = "  Error.  Process File does not contain any Source Files.  Terminating execution.";
                return;
            }

            // Verify that we have at least one destination folder
            if (DestinationFolders.Count == 0)
            {
                ErrorCode = -201;
                ErrorMessage = "  Error.  Process File does not contain any Destnation Folders.  Terminating execution.";
                return;
            }

            // Verify that all Source Files exist
            foreach( string CurrentFile in SourceFiles)
            {
                if (!File.Exists(CurrentFile) )
                {
                    ErrorCode = -202;
                    ErrorMessage = "  Error.  Specified Source File: " + CurrentFile + " does not exist.  Terminating execution.";
                    return;
                }
            }

            // Verify that all Destination Folders exist
            foreach (string CurrentFolder in DestinationFolders)
            {
                if (!Directory.Exists(CurrentFolder) )
                {
                    ErrorCode = -203;
                    ErrorMessage = "  Error.  Specified Destination Folder: " + CurrentFolder + " does not exist.  Terminating execution.";
                    return;
                }
            }
        }



        private void LoadProcessFile()
        {
            string strFileContents = "";

            if (!File.Exists(ProcessFile) )
            {
                ErrorCode = -1;
                ErrorMessage = "Error.  Process file: " + ProcessFile + " does not exist.";
                return;
            }

            try
            {
                strFileContents = File.ReadAllText(ProcessFile);
            }
            catch (Exception Ex)
            {
                ErrorCode = -2;
                ErrorMessage = "Error Reading Process file: " + ProcessFile + ". " + Ex.Message;
                return;
            }

            string[] ProcessFileLines = strFileContents.Split("\r\n");
            
            int CharPos = -1;
            int iLoop = 0;

            string LineContent = "";


            for ( iLoop = 0; iLoop < ProcessFileLines.Length; iLoop++ )
            {
                LineContent = ProcessFileLines[iLoop];

                // Strip out Comments
                CharPos = LineContent.IndexOf(COMMENT_DELIMITER);

                if (CharPos >= 0)
                {
                    LineContent = LineContent.Substring(0, CharPos);
                }

                LoadProcessFileLine(LineContent);
            }
            
        }

        private void LoadProcessFileLine(string LineContent)
        {
            string LineCategory = "";
            string LineValue = "";

          
            LineContent = LineContent.Trim();

            if (LineContent.Length == 0)
            {
                return;
            }

            // SOURCE_FILE
            LineCategory = CATEGORY_HEADER_SOURCE_FILE;

            if (LineContent.ToUpper().Substring(0, LineCategory.Length) == LineCategory)
            {
                LineValue = LineContent.Substring(LineCategory.Length).Trim();

                if (LineValue.Length > 0)
                {
                    SourceFiles.Add(LineValue);
                }
            }

            // DESTINATION_FOLDER
            LineCategory = CATEGORY_HEADER_DESTINATION_FOLDER;

            if (LineContent.ToUpper().Substring(0, LineCategory.Length) == LineCategory)
            {
                LineValue = LineContent.Substring(LineCategory.Length).Trim();

                if (LineValue.Length > 0)
                {
                    DestinationFolders.Add(LineValue);
                }
            }

            // ARCHIVE_FILE_SUFFIX
            LineCategory = CATEGORY_HEADER_ARCHIVE_FILE_SUFFIX;

            if (LineContent.ToUpper().Substring(0, LineCategory.Length) == LineCategory)
            {
                LineValue = LineContent.Substring(LineCategory.Length).Trim();

                if (LineValue.Length > 0 )
                {
                    ArchiveFileSuffixString = LineValue;
                }
            }

            // ARCHIVE_FILE_DELETE
            LineCategory = CATEGORY_HEADER_ARCHIVE_FILE_DELETE;

            if (LineContent.ToUpper().Substring(0, LineCategory.Length) == LineCategory)
            {
                LineValue = LineContent.Substring(LineCategory.Length).Trim().ToUpper();

                if ((LineValue == "TRUE") || (LineValue == "YES"))
                {
                    DeleteArchiveFile = true;
                }
            }
        }

        private void LogMessage(string MessageToLog, bool PrefixTimeStamp = true)
        {
            StringBuilder WriteMessage = new StringBuilder("");

            if (PrefixTimeStamp == true)
            {
                WriteMessage.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss "));
            }

            WriteMessage.Append(MessageToLog);

            try
            {
                StreamWriter sw = File.AppendText(LogFile);
                sw.WriteLine(WriteMessage);
                sw.Close();
            }
            catch (Exception ex)
            {
                ErrorCode = -100;
                ErrorMessage = "Error writing to log file: " + LogFile + "   . Error received: " + ex.Message;
            }
        }
    }
}
