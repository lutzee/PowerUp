namespace Id.PowershellExtensions.ZipManipulation
{
    public interface IZipFileAugmentor
    {
        void AugmentZip(
            string zipFileToAugment,
            string[] filenamesToAdd,
            string baseDirectory,
            string[] directoriesToAdd
        );

        void DiminishZip(
            string zipFileToDiminish,
            string[] filenamesToRemove,
            string[] directoriesToRemove
        );
    }
}