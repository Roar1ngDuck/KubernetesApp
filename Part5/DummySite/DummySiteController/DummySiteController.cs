using k8s;
using k8s.Models;
using customResource;

namespace DummySiteController;

public class DummySiteController
{
    private readonly IKubernetes client;

    public DummySiteController()
    {
        var config = KubernetesClientConfiguration.BuildDefaultConfig();
        client = new Kubernetes(config);
    }

    public async Task WatchDummySiteAsync()
    {
        try
        {
            Console.WriteLine($"Starting to watch DummySite resources");

            var @namespace = "default";

            var myCRD = Utils.MakeDummySiteCRD();
            var genericClient = new GenericClient(client, myCRD.Group, myCRD.Version, myCRD.PluralName);

            var dummySiteList = await genericClient.ListNamespacedAsync<CustomResourceList<DummySite>>(@namespace).ConfigureAwait(false);
            foreach (var dummySite in dummySiteList.Items)
            {
                Console.WriteLine($"- Existing DummySite: {dummySite.Metadata.Name}");
            }

            using (genericClient.WatchNamespaced<DummySite>(@namespace, (type, dummySite) =>
                {
                    Console.WriteLine($"Received watch event: {type}");
                    switch (type)
                    {
                        case WatchEventType.Added:
                            Console.WriteLine($"DummySite added: {dummySite.Metadata.Name}");
                            CreateResourcesAsync(dummySite).Wait();
                            break;
                    }
                }))
            {
                var ctrlc = new ManualResetEventSlim(false);
                Console.CancelKeyPress += (sender, eventArgs) => ctrlc.Set();
                ctrlc.Wait();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while watching DummySite resources: {ex.Message}");
        }
    }

    private async Task CreateResourcesAsync(DummySite dummySite)
    {
        try
        {
            var websiteUrl = dummySite.Spec?.WebsiteUrl;
            var siteName = dummySite.Metadata?.Name;

            if (string.IsNullOrEmpty(websiteUrl) || string.IsNullOrEmpty(siteName))
            {
                Console.WriteLine("Error: Missing required fields 'spec.website_url' or 'metadata.name'.");
                return;
            }

            Console.WriteLine($"Creating Deployment for site '{siteName}' with URL '{websiteUrl}'...");

            var deployment = new V1Deployment
            {
                Metadata = new V1ObjectMeta { Name = siteName },
                Spec = new V1DeploymentSpec
                {
                    Replicas = 1,
                    Selector = new V1LabelSelector
                    {
                        MatchLabels = new Dictionary<string, string> { { "app", siteName } }
                    },
                    Template = new V1PodTemplateSpec
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Labels = new Dictionary<string, string> { { "app", siteName } }
                        },
                        Spec = new V1PodSpec
                        {
                            Containers = new List<V1Container>
                            {
                                new V1Container
                                {
                                    Name = "websitecopy",
                                    Image = "nginx",
                                    Command = new List<string>
                                    {
                                        "/bin/sh",
                                        "-c",
                                        $"apt-get update && apt-get install -y wget && wget {websiteUrl} -O /usr/share/nginx/html/index.html && nginx -g 'daemon off;'"
                                    },
                                    Ports = new List<V1ContainerPort> { new V1ContainerPort { ContainerPort = 80 } }
                                }
                            }
                        }
                    }
                }
            };
            var deploymentResponse = await client.AppsV1.CreateNamespacedDeploymentAsync(deployment, "default");
            Console.WriteLine($"Deployment created: {deploymentResponse.Metadata.Name}");

            var service = new V1Service
            {
                Metadata = new V1ObjectMeta { Name = siteName },
                Spec = new V1ServiceSpec
                {
                    Selector = new Dictionary<string, string> { { "app", siteName } },
                    Ports = new List<V1ServicePort> { new V1ServicePort { Port = 80, TargetPort = 80 } },
                    Type = "NodePort"
                }
            };
            var serviceResponse = await client.CoreV1.CreateNamespacedServiceAsync(service, "default");
            Console.WriteLine($"Service created: {serviceResponse.Metadata.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while creating resources: {ex.Message}");
        }
    }
}