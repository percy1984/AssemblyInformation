namespace AssemblyInformation.Model
{
    public class DependencyInformationLoader
    {
        public DependencyWalker DependencyWalker { get; }

        public DependencyInformationLoader(DependencyWalker dependencyWalker)
        {
            DependencyWalker = dependencyWalker;
        }
    }
}