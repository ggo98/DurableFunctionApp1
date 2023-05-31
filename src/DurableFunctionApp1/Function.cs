using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DurableFunctionApp1
{
    namespace DurableFunctionApp
    {
        public static class Function
        {
            public class Input
            {
                public bool Sleep { get; set; }
            }

            [FunctionName("Function")]
            public static async Task<List<string>> RunOrchestrator(
                [OrchestrationTrigger] IDurableOrchestrationContext context,
                ILogger log)
            {
                var outputs = new List<string>();

                try
                {
                    Input input = context.GetInput<Input>();
                    var json = input.ToJson();

                    Guid uuid = new Guid();

                    outputs.Add("test");

                    log.LogWarning("CreateBlob.");
                    await context.CallActivityAsync<string>(nameof(CreateBlob), uuid.ToString());

                    //// Replace "hello" with the name of your Durable Activity Function.
                    log.LogWarning("SayHello Tokyo.");
                    var s = await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo");
                    outputs.Add(s);
                    log.LogWarning("SayHello Tokyo2.");
                    s = await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo");
                    log.LogWarning("SayHello Seatle.");
                    s = await context.CallActivityAsync<string>(nameof(SayHello), "Seattle");
                    outputs.Add(s);
                    log.LogWarning("SayHello London.");
                    s = await context.CallActivityAsync<string>(nameof(SayHello), "London");
                    outputs.Add(s);
                    //outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
                    //outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));
                }
                catch (Exception ex)
                {
                    outputs.Add(ex.Message);
                }
                // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
                return outputs;
            }

            [FunctionName(nameof(SayHello))]
            public static async Task<string> SayHello([ActivityTrigger] string name, ILogger log)
            {
                OutputDebugString($"SLEEPING FOR {name}");
                log.LogWarning($"SLEEPING FOR {name}");
                //Thread.Sleep(10000);
                await Task.Delay(3000);
                OutputDebugString($"done sleeping for {name}");
                log.LogWarning($"DONE SLEEPING FOR {name}");
                log.LogInformation("Saying hello to {name}.", name);
                return $"Hello {name}!";
            }


            const string connectionString = "UseDevelopmentStorage=true";
            [FunctionName(nameof(CreateBlob))]
            public static async Task CreateBlob([ActivityTrigger] string uuid, ILogger log)
            {
                //CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
                CloudBlockBlob lockBlob = container.GetBlockBlobReference($"{uuid}.lock");

                ///*
                // Check if the lock blob already exists
                if (!await lockBlob.ExistsAsync())
                {
                    // Create the lock blob
                    await lockBlob.UploadTextAsync(string.Empty);
                    log.LogWarning("Lock blob created.");
                }
                else
                {
                    // Lock blob already exists
                    log.LogWarning("Lock blob already exists.");
                }
                //*/
            }

            //private static CloudBlobClient blobClient = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();

            //private static CloudBlobContainer _blobContainer = blobClient.GetContainerReference("mycontainer");

            [FunctionName("Function_HttpStart")]
            public static async Task<HttpResponseMessage> HttpStart(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
                [DurableClient] IDurableOrchestrationClient starter,
                ILogger log)
            {
                int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
                log.LogError($"processid={processId}");
                NameValueCollection nvc = HttpUtility.ParseQueryString(req.RequestUri.Query);
                Input input = new Input()
                {
                    Sleep = nvc["sleep"] is not null,
                };

                // Function input comes from the request content.
                string instanceId = await starter.StartNewAsync<Input>("Function", input);

                log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

                return starter.CreateCheckStatusResponse(req, instanceId);
            }

            [DllImport("kernel32", EntryPoint = "OutputDebugStringW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern void OutputDebugString(string s);

            private static CloudBlobContainer CreateContainer()
            {
                CloudBlobClient blobClient = CloudStorageAccount.Parse(connectionString).CreateCloudBlobClient();
                CloudBlobContainer _blobContainer = blobClient.GetContainerReference("mycontainer");
                return _blobContainer;
            }

            [FunctionName(nameof(CreateLockBlob))]
            public static async Task<CloudBlockBlob> CreateLockBlob([ActivityTrigger] string uuid, ILogger log)
            {
                //var blob = await TestCreateBlob();
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
                CloudBlockBlob lockBlob = container.GetBlockBlobReference($"{uuid}.lock");

                // Check if the lock blob already exists
                if (!await lockBlob.ExistsAsync())
                {
                    // Create the lock blob
                    await lockBlob.UploadTextAsync(string.Empty);
                    log.LogInformation("Lock blob created.");
                }
                else
                {
                    // Lock blob already exists
                    log.LogInformation("Lock blob already exists.");
                }
                return lockBlob;

            }

        }
    }
}


#if GPT_SUCKS
        public static class ParentFunction
        {
            [FunctionName("ParentFunction")]
            public static async Task<List<string>> Run(
                [OrchestrationTrigger] IDurableOrchestrationContext context,
                ILogger log)
            {
                var outputs = new List<string>();

                // Start the child function
                string childInstanceId = await context.CallSubOrchestratorAsync<string>("ChildFunction", null);

                // Wait for the completion of the sub-orchestration
                await context.WaitForCompletionOrCreateCheckStatusResponseAsync(childInstanceId);

                // Retrieve the output of the sub-orchestration
                string childOutput = await context.GetCustomStatusAsync<string>(childInstanceId);

                outputs.Add(childOutput);

                // Continue with other activities or function invocations
                outputs.Add("ParentFunction completed.");

                return outputs;
            }

            [FunctionName("ChildFunction")]
            public static async Task<string> ChildFunction(
                [OrchestrationTrigger] IDurableOrchestrationContext context,
                ILogger log)
            {
                log.LogInformation("ChildFunction started.");

                // Perform some work here...

                log.LogInformation("ChildFunction completed.");

                return "ChildFunction completed.";
            }

            [FunctionName("ParentHttpStart")]
            public static async Task<HttpResponseMessage> HttpStart(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
                [DurableClient] IDurableOrchestrationClient starter,
                ILogger log)
            {
                // Start the orchestration
                string instanceId = await starter.StartNewAsync("ParentFunction", null);

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

                // Return the status endpoint for the client to track the orchestration progress
                return starter.CreateCheckStatusResponse(req, instanceId);
            }
        }
    }
}
#endif


#if OLDDDDDDDD
    public static class Function
    {
        public class Input
        {
            public bool Sleep { get; set; }
        }

        [FunctionName("Function")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var outputs = new List<string>();

            try
            {
                Guid uuid = Guid.NewGuid();

                // Use the Development Storage Account for Azurite
                //CloudStorageAccount storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");

                // Create the container if it doesn't exist
                await container.CreateIfNotExistsAsync();

                CloudBlockBlob lockBlob = container.GetBlockBlobReference($"{uuid}.lock");

                // Check if the lock blob already exists
                if (!await lockBlob.ExistsAsync())
                {
                    // Create the lock blob
                    await lockBlob.UploadTextAsync(string.Empty);
                    log.LogInformation("Lock blob created.");
                }
                else
                {
                    // Lock blob already exists
                    log.LogInformation("Lock blob already exists.");
                }

                /*
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
                CloudBlockBlob lockBlob = container.GetBlockBlobReference($"{uuid}.lock");

                // Check if the lock blob already exists
                if (!await lockBlob.ExistsAsync())
                {
                    // Create the lock blob
                    await lockBlob.UploadTextAsync(string.Empty);
                    log.LogInformation("Lock blob created.");
                }
                else
                {
                    // Lock blob already exists
                    log.LogInformation("Lock blob already exists.");
                }*/

                //Input input = context.GetInput<Input>();
                //var json = input.ToJson();  

                // Replace "hello" with the name of your Durable Activity Function.
                // PAS de await car SayHello() est devenue async, mais il faut le .Result à la fin à la place....
                //outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
                var s = await SayHelloTask("Tokyo",log);

                outputs.Add(s);


                //outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
                //outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

                // Rest of your code...
                log.LogCritical("1");
                outputs.Add("test");
                log.LogCritical("2");
            }
            catch (Exception ex)
            {
                outputs.Add(ex.Message);
            }

            log.LogCritical("FINI");

            // Orchestration completed successfully, set the output
            context.SetOutput(outputs);

            return outputs;
        }

        [FunctionName(nameof(SayHelloTask))]
        public static async Task<string> SayHelloTask([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}...");
            await Task.Delay(10000); // Simulating some work or delay
            log.LogInformation($"Hello {name}!");
            return $"Hello {name}!";

            //OutputDebugString($"SLEEPING FOR {name}");
            //log.LogWarning($"SLEEPING FOR {name}");

            ////Thread.Sleep(10000); // <<<=== FOUT LA MERDE.
            ////await Task.Delay(10000);

            //OutputDebugString($"done sleeping for {name}");
            //log.LogWarning($"DONE SLEEPING FOR {name}");
            //log.LogInformation("Saying hello to {name}.", name);
            //return await Task.FromResult<string>($"Hello {name}!");
        }


        [FunctionName(nameof(SayHello))]
        public static async Task<string> SayHello([ActivityTrigger] string name, ILogger log)
        {
            OutputDebugString($"SLEEPING FOR {name}");
            log.LogWarning($"SLEEPING FOR {name}");
            
            //Thread.Sleep(10000); // <<<=== FOUT LA MERDE.
            //await Task.Delay(10000);

            OutputDebugString($"done sleeping for {name}");
            log.LogWarning($"DONE SLEEPING FOR {name}");
            log.LogInformation("Saying hello to {name}.", name);
            return $"Hello {name}!";
        }


        [FunctionName("Function_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            log.LogError($"processid={processId}");
            NameValueCollection nvc = HttpUtility.ParseQueryString(req.RequestUri.Query);
            Input input = new Input()
            {
                Sleep = nvc["sleep"] is not null,
            };

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync<Input>("Function", input);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [DllImport("kernel32", EntryPoint = "OutputDebugStringW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern void OutputDebugString(string s);

    }
}
#endif

