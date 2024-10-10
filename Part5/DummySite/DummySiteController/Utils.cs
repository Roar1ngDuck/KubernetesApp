using customResource;
using k8s.Models;

public static class Utils
{
    public static CustomResourceDefinition MakeDummySiteCRD()
    {
        return new CustomResourceDefinition
        {
            Kind = "DummySite",
            Group = "stable.dwk",
            Version = "v1",
            PluralName = "dummysites"
        };
    }
}
