using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Azure.Cosmos;
using CosmosDb = Microsoft.Azure.Cosmos;

namespace CosmosDBInsertJsonExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
             // Check if command line arguments were provided
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the number of documents to insert as a command line argument.");
                return;
            }

            // Cosmos DB connection string
            string connectionString = "";

            // Cosmos DB client instance
            using CosmosClient cosmosClient = new CosmosClient(connectionString);

            // Database and container names
            string databaseName = "db";
            string containerName = "keys";

                       // Create or get the database
            DatabaseResponse databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            CosmosDb.Database database = databaseResponse.Database;

            // Create or get the container
            ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync(containerName, "/id");
            Container container = containerResponse.Container;

            // Use Bogus to generate fake data for the JSON object
            var faker = new Faker();
            int numDocsToInsert = int.Parse(args[0]); // Get number of documents to insert from command line argument

            // Track total request unit charge and latency for all inserts
            double totalRU = 0;
            TimeSpan totalLatency = new TimeSpan(0);

            for (int i = 0; i < numDocsToInsert; i++)
            {
                dynamic jsonObject = new
                {
                    id = faker.Random.Guid(),
                    name = faker.Name.FullName(),
                    age = faker.Random.Number(18, 65),
                    email = faker.Internet.Email(),
                    address = new
                    {
                        street = faker.Address.StreetAddress(),
                        city = faker.Address.City(),
                        state = faker.Address.State(),
                        zip = faker.Address.ZipCode()
                    }
                };

                // Insert the JSON object into the container and measure request unit charge and latency
                Stopwatch stopwatch = Stopwatch.StartNew();
                ItemResponse<object> response = await container.CreateItemAsync<object>((object)jsonObject);
                stopwatch.Stop();

                double requestCharge = response.RequestCharge;
                TimeSpan latency = stopwatch.Elapsed;

                // Update total request unit charge and latency
                totalRU += requestCharge;
                totalLatency += latency;

                // Print the status code, request unit charge, and latency of the response
                Console.WriteLine($"Status code for document {i + 1}: {response.StatusCode}, RU: {requestCharge}, Latency: {latency}");
            }

            // Print total request unit charge and latency for all inserts
            Console.WriteLine($"Total RU: {totalRU}, Total Latency: {totalLatency}");
        }
    }
}