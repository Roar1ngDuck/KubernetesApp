using System;
using customResource;
using k8s.Models;

namespace DummySiteController;

public class DummySite : CustomResource<DummySiteSpec, V1Status>
{
    public override string ToString()
    {
        var labels = Metadata?.Labels != null ? string.Join(", ", Metadata.Labels.Select(kvp => $"{kvp.Key}: {kvp.Value}")) : "No Labels";
        return $"{Metadata?.Name} (Labels: {labels}), Spec: {Spec?.WebsiteUrl}";
    }
}
