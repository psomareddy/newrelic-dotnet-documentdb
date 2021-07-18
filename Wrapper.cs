using NewRelic.Agent.Api;
using NewRelic.Agent.Extensions.Parsing;
using NewRelic.Agent.Extensions.Providers.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom.Providers.Wrapper.DocumentDb
{
    public class Wrapper : IWrapper
    {
        private const string AssemblyName = "Microsoft.Azure.DocumentDB.Core";
        private const string TypeName = "Microsoft.Azure.Documents.Client.DocumentClient";
        private string[] MethodNames = new string[]{
                "CreateAsync",
                "UpdateAsync",
                "ReadAsync",
                "ReadFeedAsync",
                "DeleteAsync",
                "ExecuteProcedureAsync",
                "ExecuteQueryAsync",
                "UpsertAsync"
        };
        bool IWrapper.IsTransactionRequired => true;

        AfterWrappedMethodDelegate IWrapper.BeforeWrappedMethod(InstrumentedMethodCall instrumentedMethodCall, IAgent agent, ITransaction transaction)
        {
            if (instrumentedMethodCall.IsAsync)
            {
                transaction.AttachToAsync();
            }
            var invocationTarget = instrumentedMethodCall.MethodCall.InvocationTarget;

            var model = "unknown";
            var operation = instrumentedMethodCall.MethodCall.Method.MethodName;
            string commandText = null;

            ConnectionInfo connectionInfo = null;

            if (instrumentedMethodCall.MethodCall.MethodArguments.Length > 0)
            {
                var parm = instrumentedMethodCall.MethodCall.MethodArguments[0];
                string resourceAddress = Helper.GetResourceAddress(parm);
                object resourceType = Helper.GetResourceType(parm);
                object operationType = Helper.GetOperationType(parm);

                model = resourceAddress.Split('/').Take(4).Last();
                operation = $"{operationType}{resourceType}";
                commandText = $"{operationType} {resourceType} {model}";
                var dbName = Helper.GetDatabaseName(parm);
                connectionInfo = Helper.GetConnectionInfo(invocationTarget, dbName);
            }

            //Console.WriteLine($"Setting model for {operation}: {model}");

            var segment = transaction.StartDatastoreSegment(
                instrumentedMethodCall.MethodCall,
                new ParsedSqlStatement(DatastoreVendor.Other, model, operation),
                connectionInfo: connectionInfo,
                commandText: commandText,
                isLeaf: true);

            if (instrumentedMethodCall.IsAsync)
            {
                return Delegates.GetAsyncDelegateFor<Task>(agent, segment);
            }
            else
            {
                void onComplete()
                {

                }
                return Delegates.GetDelegateFor(onComplete: onComplete);
            }


        }

        CanWrapResponse IWrapper.CanWrap(InstrumentedMethodInfo instrumentedMethodInfo)
        {
            var method = instrumentedMethodInfo.Method;
            var canWrap = method.MatchesAny(
                assemblyNames: new[] { AssemblyName },
                typeNames: new[] { TypeName },
                methodNames: MethodNames
            );

            return new CanWrapResponse(canWrap);
        }
    }
}
