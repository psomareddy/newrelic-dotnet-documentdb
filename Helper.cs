using NewRelic.Reflection;
using System;
using NewRelic.Agent.Extensions.Parsing;

namespace Custom.Providers.Wrapper.DocumentDb
{
    public class Helper
    {
        private const string AssemblyName = "Microsoft.Azure.DocumentDB.Core";
        private const string DocumentServiceRequestTypeName = "Microsoft.Azure.Documents.DocumentServiceRequest";
        private const string PropertyOperationType = "OperationType";
        private const string PropertyResourceType = "ResourceType";
        private const string PropertyResourceAddress = "ResourceAddress";
        private const string PropertyDatabaseName = "DatabaseName";

        private const string DocumentClientTypeName = "Microsoft.Azure.Documents.Client.DocumentClient";
        private const string PropertyServiceEndpoint = "ServiceEndpoint";

        private static Func<object, object> _getOperationType;
        private static Func<object, object> _getResourceType;
        private static Func<object, string> _getResourceAddress;
        private static Func<object, Uri> _getServiceEndpoint;
        private static Func<object, string> _getDatabaseName;

        public static object GetOperationType(object owner)
        {
            var idGetter = _getOperationType ?? (_getOperationType = VisibilityBypasser.Instance.GeneratePropertyAccessor<object>(AssemblyName, DocumentServiceRequestTypeName, PropertyOperationType));
            return idGetter(owner);
        }

        public static object GetResourceType(object owner)
        {
            var rtGetter = _getResourceType ?? (_getResourceType = VisibilityBypasser.Instance.GeneratePropertyAccessor<object>(AssemblyName, DocumentServiceRequestTypeName, PropertyResourceType));
            return rtGetter(owner);
        }

        public static string GetResourceAddress(object owner)
        {
            var resAddressGetter = _getResourceAddress ?? (_getResourceAddress = VisibilityBypasser.Instance.GeneratePropertyAccessor<string>(AssemblyName, DocumentServiceRequestTypeName, PropertyResourceAddress));
            return resAddressGetter(owner);
        }

        public static string GetDatabaseName(object owner)
        {
            var dbNameGetter = _getDatabaseName ?? (_getDatabaseName = VisibilityBypasser.Instance.GeneratePropertyAccessor<string>(AssemblyName, DocumentServiceRequestTypeName, PropertyDatabaseName));
            return dbNameGetter(owner);
        }

        public static ConnectionInfo GetConnectionInfo(object owner, string dbName)
        {
            var serviceEndpointGetter = _getServiceEndpoint ?? (_getServiceEndpoint = VisibilityBypasser.Instance.GeneratePropertyAccessor<Uri>(AssemblyName, DocumentClientTypeName, PropertyServiceEndpoint));
            Uri endpoint = serviceEndpointGetter(owner);
            return new ConnectionInfo(endpoint.Host, $"{ endpoint.Port}", dbName);
        }
    }
}