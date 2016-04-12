using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace Id.PowershellExtensions.ZipManipulation
{
    public class ZipFileAugmentor : IZipFileAugmentor
    {
        private readonly IPsCmdletLogger _logger;

        public ZipFileAugmentor(IPsCmdletLogger logger)
        {
            _logger = logger;
        }

        public void AugmentZip(
            string zipFileToAugment,
            string[] filenamesToAdd,
            string baseDirectory,
            string[] directoriesToAdd
            )
        {
            var allArgs = (filenamesToAdd ?? new string[0])
                .Concat(directoriesToAdd ?? new string[0]);

            if (string.IsNullOrEmpty(zipFileToAugment))
            {
                throw new ArgumentException("zipFileToAugment");
            }            

            if (!File.Exists(zipFileToAugment))
            {
                throw new ArgumentException(string.Format("Zip file {0} does not exist", zipFileToAugment),
                    "zipFileToAugment");
            }

            if (!directoriesToAdd.NullOrEmpty() && !Directory.Exists(baseDirectory))
            {
                throw new ArgumentException(string.Format("Base directory {0} does not exist", baseDirectory));
            }

            if (!allArgs.Any())
            {
                throw new ArgumentException("No files or directories to add");
            }

            //Check the files to be added to ensure they all exist
            if (!filenamesToAdd.NullOrEmpty())
            {
                var filesToAddThatDontExist = new HashSet<string>();
                foreach (var fileToAdd in filenamesToAdd.Where(fileToAdd => !File.Exists(fileToAdd)))
                {
                    filesToAddThatDontExist.Add(fileToAdd);
                }

                if (filesToAddThatDontExist.Any())
                {
                    var fileNames = filesToAddThatDontExist.Aggregate();
                    throw new ArgumentException(string.Format("The following file(s) does not exist: {0}", fileNames),
                        "filenamesToAdd");
                }
            }

            _logger.Log("---- Augmenting Zip {0} -----", zipFileToAugment);

            using (var zip = ZipFile.Read(zipFileToAugment))
            {
                //If the file name to add list is not invalid then UpdateFiles overwrites files that already exist, adds any that don't
                if (!filenamesToAdd.NullOrEmpty())
                    zip.UpdateFiles(filenamesToAdd, "");

                if (!directoriesToAdd.NullOrEmpty())
                {
                    foreach (var directory in directoriesToAdd)
                    {
                        var baseDir = directory.ToLower().Replace(baseDirectory.ToLower(), "");

                        zip.AddDirectory(directory, baseDir);
                    }
                }

                zip.Save();
            }

            _logger.Log("Done");
        }

        public void DiminishZip(
            string zipFileToDiminish,
            string[] filenamesToRemove,
            string[] directoriesToRemove
        )
        {
            var allArgs = (filenamesToRemove ?? new string[0])
                .Concat(directoriesToRemove ?? new string[0]);

            if (string.IsNullOrEmpty(zipFileToDiminish))
            {
                throw new ArgumentException("zipFileToDiminish");
            }

            if (!File.Exists(zipFileToDiminish))
            {
                throw new ArgumentException(string.Format("Zip file {0} does not exist", zipFileToDiminish), "zipFileToDiminish");
            }

            if (!allArgs.Any())
            {
                throw new ArgumentException("No files or directories to remove");
            }

            _logger.Log("---- Diminishing Zip {0} -----", zipFileToDiminish);

            using (var zip = ZipFile.Read(zipFileToDiminish))
            {
                //If the file name to remove list is not invalid then remove the files from the zip archive. If the file is the last file in
                //a directory then the directory will be removed.
                if (!filenamesToRemove.NullOrEmpty())
                {
                    //Walk the list of files to remove. If no entry can be retrieved from the archive then it does not exist so can't be removed.
                    //Trying to remove an entry that does not exist will throw exception, this check will avoid that situation.
                    foreach (var fileName in filenamesToRemove)
                    {
                        var entry = zip[fileName];

                        if (entry != null)
                        {
                            zip.RemoveEntry(entry);
                        }
                    }
                }

                //Remove the directories from the archive. If the the parent directory becomes empty if will also be removed. If the directory does not
                //exist no error is thrown, so no checking for existence is required.
                if (!directoriesToRemove.NullOrEmpty())
                {
                    foreach (var directoryName in directoriesToRemove)
                    {
                        var directoryPath = string.Concat(directoryName, "/*");
                        zip.RemoveSelectedEntries(directoryPath);
                    }
                }

                zip.Save();
            }

            _logger.Log("Done");
        }
    }
}
