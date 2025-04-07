using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.SimService.ContentParts;
using OrchardCore.SimService.Indexes;
using YesSql;

namespace OrchardCore.SimService.Services
{
    public class MigrateTableToPostgres
    {
        private readonly string _mssqlConnectionString = "Server=69.57.161.84;User Id=localUserSim;Password=ItsReallyAStrongP@ssword2024!;Database=ChothuesimDatabase;TrustServerCertificate=Yes";
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly IServiceProvider _serviceProvider;
        private const string CheckpointFile = "migration-checkpoint.txt";
        private const int BatchSize = 500; // Increased batch size
        private const int ParallelBatches = 4; // Number of parallel batches

        public MigrateTableToPostgres(
            ISession session,
            IContentManager contentManager,
            IServiceProvider serviceProvider)
        {
            _session = session;
            _contentManager = contentManager;
            _serviceProvider = serviceProvider;
        }

        public async Task MigrateData()
        {
            try
            {
                var lastUserId = GetLastProcessedUserId();
                var totalCount = await GetTotalRecordCount();
                Console.WriteLine($"📊 Total records in MSSQL: {totalCount}");

                // Keep track of processing speed
                var startTime = DateTime.Now;
                var recordsProcessed = 0;

                // Continue until all records are processed
                var hasMore = true;
                while (hasMore)
                {
                    // Load multiple batches of data at once for parallel processing
                    var allBatchData = await LoadMultipleBatches(lastUserId, ParallelBatches);

                    if (allBatchData.Count == 0)
                    {
                        hasMore = false;
                        break;
                    }

                    // Get the highest user ID to update checkpoint later
                    var highestUserId = allBatchData.Max(b => b.Max(u => u.UserId));

                    // Process batches in parallel using multiple sessions
                    var tasks = new List<Task<int>>();
                    for (int i = 0; i < allBatchData.Count; i++)
                    {
                        var batchIndex = i;
                        var batchData = allBatchData[i];
                        tasks.Add(Task.Run(async () => await ProcessBatch(batchData, batchIndex)));
                    }

                    // Wait for all batches to complete
                    var results = await Task.WhenAll(tasks);
                    var batchSuccessCount = results.Sum();

                    // Update progress
                    recordsProcessed += allBatchData.SelectMany(x => x).Count();
                    lastUserId = highestUserId;
                    SaveCheckpoint(lastUserId);

                    // Calculate and display statistics
                    var elapsedMinutes = (DateTime.Now - startTime).TotalMinutes;
                    var recordsPerMinute = recordsProcessed / Math.Max(elapsedMinutes, 0.01);
                    var estimatedTimeLeft = (totalCount - recordsProcessed) / Math.Max(recordsPerMinute, 1) / 60.0;

                    Console.WriteLine($"✅ Migrated {batchSuccessCount} records. Progress: {recordsProcessed}/{totalCount} " +
                                     $"({recordsProcessed * 100.0 / totalCount:F1}%) - " +
                                     $"Speed: {recordsPerMinute:F0} records/min - " +
                                     $"ETA: {estimatedTimeLeft:F1} hours");

                    // If we got fewer records than expected, we're done
                    if (allBatchData.Sum(b => b.Count) < BatchSize * ParallelBatches)
                    {
                        hasMore = false;
                    }
                }

                Console.WriteLine("🎉 Migration completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Migration failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private async Task<List<List<UserProfileData>>> LoadMultipleBatches(long lastUserId, int numberOfBatches)
        {
            var result = new List<List<UserProfileData>>();

            using (var mssqlConnection = new SqlConnection(_mssqlConnectionString))
            {
                await mssqlConnection.OpenAsync();

                // Get multiple batches in one query - more efficient
                var totalBatchSize = BatchSize * numberOfBatches;
                var command = new SqlCommand(@"
                    SELECT TOP (@TotalBatchSize) * 
                    FROM UserProfilePartIndex 
                    WHERE UserId > @LastId 
                    ORDER BY UserId", mssqlConnection);

                command.Parameters.AddWithValue("@TotalBatchSize", totalBatchSize);
                command.Parameters.AddWithValue("@LastId", lastUserId);

                using var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    return result;
                }

                // Read all data from SQL Server
                var allData = new List<UserProfileData>();
                while (await reader.ReadAsync())
                {
                    allData.Add(ReadUserProfileData(reader));
                }

                // Split into batches for parallel processing
                for (int i = 0; i < numberOfBatches; i++)
                {
                    var startIdx = i * BatchSize;
                    if (startIdx >= allData.Count)
                        break;

                    var batchData = allData
                        .Skip(startIdx)
                        .Take(BatchSize)
                        .ToList();

                    result.Add(batchData);
                }
            }

            return result;
        }

        private UserProfileData ReadUserProfileData(SqlDataReader reader)
        {
            return new UserProfileData
            {
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : (int)reader["UserId"],
                ProfileId = reader.IsDBNull(reader.GetOrdinal("ProfileId")) ? 0 : (int)reader["ProfileId"],
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : (string)reader["Email"],
                UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : (string)reader["UserName"],
                ContentItemId = reader.IsDBNull(reader.GetOrdinal("ContentItemId")) ? null : (string)reader["ContentItemId"],
                Vendor = reader.IsDBNull(reader.GetOrdinal("Vendor")) ? null : (string)reader["Vendor"],
                DefaultForwardingNumber = reader.IsDBNull(reader.GetOrdinal("DefaultForwardingNumber")) ? null : (string)reader["DefaultForwardingNumber"],
                Balance = reader.IsDBNull(reader.GetOrdinal("Balance")) ? 0m : (decimal)reader["Balance"],
                OriginalAmount = reader.IsDBNull(reader.GetOrdinal("OriginalAmount")) ? 0m : (decimal)reader["OriginalAmount"],
                Amount = reader.IsDBNull(reader.GetOrdinal("Amount")) ? 0m : (decimal)reader["Amount"],
                GmailMsgId = reader.IsDBNull(reader.GetOrdinal("GmailMsgId")) ? null : (string)reader["GmailMsgId"],
                RateInUsd = reader.IsDBNull(reader.GetOrdinal("RateInUsd")) ? 0m : (decimal)reader["RateInUsd"],
                Rating = reader.IsDBNull(reader.GetOrdinal("Rating")) ? 0 : (int)reader["Rating"],
                DefaultCountryName = reader.IsDBNull(reader.GetOrdinal("DefaultCoutryName")) ? null : (string)reader["DefaultCoutryName"],
                DefaultIso = reader.IsDBNull(reader.GetOrdinal("DefaultIso")) ? null : (string)reader["DefaultIso"],
                DefaultPrefix = reader.IsDBNull(reader.GetOrdinal("DefaultPrefix")) ? null : (string)reader["DefaultPrefix"],
                DefaultOperatorName = reader.IsDBNull(reader.GetOrdinal("DefaultOperatorName")) ? null : (string)reader["DefaultOperatorName"],
                FrozenBalance = reader.IsDBNull(reader.GetOrdinal("FrozenBalance")) ? 0m : (decimal)reader["FrozenBalance"]
            };
        }

        private async Task<int> ProcessBatch(List<UserProfileData> batchData, int batchIndex)
        {
            int successCount = 0;

            // Create a new scope to get fresh services for this batch
            using (var scope = _serviceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

                try
                {
                    // Process in smaller chunks for better transaction management
                    int chunkSize = 25;
                    for (int i = 0; i < batchData.Count; i += chunkSize)
                    {
                        var chunk = batchData.Skip(i).Take(chunkSize).ToList();

                        try
                        {
                            foreach (var userData in chunk)
                            {
                                // Use BulkInsert approach by creating content items efficiently
                                var newContentItem = await contentManager.NewAsync("UserProfile");
                                newContentItem.Owner = userData.UserName ?? "system";
                                newContentItem.Author = userData.UserName ?? "system";

                                // Apply user profile part
                                var profilePart = new UserProfilePart
                                {
                                    ProfileId = userData.ProfileId,
                                    Email = userData.Email,
                                    UserId = userData.UserId,
                                    UserName = userData.UserName,
                                    ContentItemId = userData.ContentItemId,
                                    Vendor = userData.Vendor,
                                    DefaultForwardingNumber = userData.DefaultForwardingNumber,
                                    Balance = userData.Balance,
                                    OriginalAmount = userData.OriginalAmount,
                                    Amount = userData.Amount,
                                    GmailMsgId = userData.GmailMsgId,
                                    RateInUsd = userData.RateInUsd,
                                    Rating = userData.Rating,
                                    DefaultCountryName = userData.DefaultCountryName,
                                    DefaultIso = userData.DefaultIso,
                                    DefaultPrefix = userData.DefaultPrefix,
                                    DefaultOperatorName = userData.DefaultOperatorName,
                                    FrozenBalance = userData.FrozenBalance
                                };

                                newContentItem.Apply(profilePart);
                                await contentManager.CreateAsync(newContentItem, VersionOptions.Published);
                                successCount++;
                            }

                            // Commit after each chunk
                            await session.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Batch {batchIndex}, chunk {i / chunkSize} failed: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Batch {batchIndex} processing failed: {ex.Message}");
                }
            }

            return successCount;
        }

        private static long GetLastProcessedUserId()
        {
            if (!File.Exists(CheckpointFile))
            {
                return 0;
            }

            var content = File.ReadAllText(CheckpointFile);
            return long.TryParse(content, out var id) ? id : 0;
        }

        private static void SaveCheckpoint(long userId)
        {
            File.WriteAllText(CheckpointFile, userId.ToString());
        }

        private async Task<int> GetTotalRecordCount()
        {
            using var connection = new SqlConnection(_mssqlConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("SELECT COUNT(*) FROM UserProfilePartIndex", connection);
            var result = await command.ExecuteScalarAsync();
            return (int)result;
        }
    }

    // Helper class to store user profile data
    public class UserProfileData
    {
        public int UserId { get; set; }
        public int ProfileId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string ContentItemId { get; set; }
        public string Vendor { get; set; }
        public string DefaultForwardingNumber { get; set; }
        public decimal Balance { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal Amount { get; set; }
        public string GmailMsgId { get; set; }
        public decimal RateInUsd { get; set; }
        public int Rating { get; set; }
        public string DefaultCountryName { get; set; }
        public string DefaultIso { get; set; }
        public string DefaultPrefix { get; set; }
        public string DefaultOperatorName { get; set; }
        public decimal FrozenBalance { get; set; }
    }
}
