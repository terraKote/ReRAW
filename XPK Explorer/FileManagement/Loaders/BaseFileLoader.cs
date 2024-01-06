namespace XPK_Explorer.FileManagement.Loaders
{
    /// <summary>
    /// Base class for file providers. Inherit from this class, to implement custom file provider.
    /// </summary>
    public abstract class BaseFileLoader
    {
    }

    /// <summary>
    /// Generic version of <see cref="BaseFileLoader"/>.
    /// </summary>
    /// <typeparam name="TFile">Generic file type.</typeparam>
    public abstract class BaseFileLoader<TFile> : BaseFileLoader
    {
        /// <summary>
        /// Method that returns the file from bytes.
        /// </summary>
        /// <param name="extension">File extension.</param>
        /// <param name="bytes">Bytes to get data from.</param>
        /// <returns>File, from provided bytes.</returns>
        public abstract TFile Load(string extension, byte[] bytes);
    }
}
