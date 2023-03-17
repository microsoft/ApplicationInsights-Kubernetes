using static System.FormattableString;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    internal static class TelemetryProperty
    {
        private static class Constant
        {
            public const string Container = "Container";
            public const string Image = "Image";
            public const string Deployment = "Deployment";
            public const string K8s = "Kubernetes";
            public const string Node = "Node";
            public const string Pod = "Pod";
            public const string ReplicaSet = "ReplicaSet";
            public const string ProcessString = "Process";

            public const string ID = "ID";
            public const string Name = "Name";
            public const string Labels = "Labels";
            public const string Namespace = "Namespace";
            public const string CPU = "CPU";
            public const string Memory = "Memory";
        }


        public static class Container
        {
            public static readonly string ID = Invariant($"{Constant.K8s}.{Constant.Container}.{Constant.ID}");
            public static readonly string Name = Invariant($"{Constant.K8s}.{Constant.Container}.{Constant.Name}");
            public static readonly string ImageName = Invariant($"{Constant.K8s}.{Constant.Container}.{Constant.Image}.{Constant.Name}");
        }

        public static class Pod
        {
            public static readonly string ID = Invariant($"{Constant.K8s}.{Constant.Pod}.{Constant.ID}");
            public static readonly string Name = Invariant($"{Constant.K8s}.{Constant.Pod}.{Constant.Name}");
            public static readonly string Labels = Invariant($"{Constant.K8s}.{Constant.Pod}.{Constant.Labels}");
            public static readonly string Namespace = Invariant($"{Constant.K8s}.{Constant.Pod}.{Constant.Namespace}");
        }

        public static class ReplicaSet
        {
            public static readonly string Name = Invariant($"{Constant.K8s}.{Constant.ReplicaSet}.{Constant.Name}");
        }

        public static class Deployment
        {
            public static readonly string Name = Invariant($"{Constant.K8s}.{Constant.Deployment}.{Constant.Name}");
        }

        public static class Node
        {
            public static readonly string ID = Invariant($"{Constant.K8s}.{Constant.Node}.{Constant.ID}");
            public static readonly string Name = Invariant($"{Constant.K8s}.{Constant.Node}.{Constant.Name}");
        }
    }
}
