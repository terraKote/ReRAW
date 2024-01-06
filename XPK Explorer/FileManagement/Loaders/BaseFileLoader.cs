namespace XPK_Explorer.FileManagement.Loaders
{
    public abstract class BaseFileLoader
    {
    }

    public abstract class BaseFileLoader<TFile> : BaseFileLoader
    {
        public abstract TFile Load(string extension, byte[] bytes);
    }
}
