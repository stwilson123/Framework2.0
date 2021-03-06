﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Autofac.Features.Metadata;
using Framework.Data;
using Framework.Data.Providers;
using Framework.Data.Session;
using Framework.Environment.ShellBuilder.Models;

namespace Framework.Tests.Data
{
   public class ProviderUtilities {

        public static void RunWithSqlServer(IEnumerable<RecordBlueprint> recordDescriptors, Action<ISessionFactory> action,bool isMandleRelease = false) {
            var temporaryPath = Path.GetTempFileName();
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
            Directory.CreateDirectory(temporaryPath);
            var databasePath = Path.Combine(temporaryPath, "SystemTempTest.mdf");
            var databaseName = Path.GetFileNameWithoutExtension(databasePath);
            try
            {
                // create database
//                if (!TryCreateSqlServerDatabase(databasePath, databaseName))
//                    return;

                var meta = new Meta<CreateDataServicesProvider>((dataFolder, connectionString) =>
                        new SqlServerDataServicesProvider(dataFolder, connectionString),
                    new Dictionary<string, object> {{"ProviderName", "SqlServer"}});

                var manager = (IDataServicesProviderFactory) new DataServicesProviderFactory(new[] {meta});

                var parameters = new SessionFactoryParameters
                {
                    Provider = "SqlServer",
                    DataFolder = temporaryPath,
                    ConnectionString = "Data Source=.;Initial Catalog=tempdb;Integrated Security=True;",
                    RecordDescriptors = recordDescriptors,
                };

                var configuration = manager
                    .CreateProvider(parameters)
                    .BuildConfiguration(parameters);

                //TODO
                //new SchemaExport(configuration).Execute(false, true, false);

                var metaSession = new Meta<CreateDbSession>((connectionString) =>
                        new SqlServerSession(connectionString),
                    new Dictionary<string, object> {{"ProviderName", "SqlServer"}});

                TryCreateSqlServerDatabseTable(databasePath, databaseName, @"CREATE TABLE [FooRecord](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Timespan] [DATETIME] NULL,
 CONSTRAINT [PK_FooRecord] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

                TryCreateSqlServerDatabseTable(databasePath, databaseName, @"CREATE TABLE [BigRecord](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Banner] [nvarchar](max) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_BigRecord] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

                using (var sessionFactory = new SessionFactory(new[] {metaSession}, configuration))
                {
                    action(sessionFactory);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally {
                try {
                    if(!isMandleRelease)
                   // Directory.Delete(temporaryPath, true);
                    TryCreateSqlServerDatabseTable(databasePath, databaseName, @"DROP TABLE [FooRecord];DROP TABLE [BigRecord];");
                }
                catch (SqlException) { }
            }
        }

       public static void ReleaseWithSqlServer()
       {
           try
           {
               TryCreateSqlServerDatabseTable(null, null, @"DROP TABLE [FooRecord];DROP TABLE [BigRecord];");

           }
           catch  
           {
               
           }

       }

        private static bool TryCreateSqlServerDatabase(string databasePath, string databaseName) {
            var connection = TryOpenSqlServerConnection();
            if (connection == null)
                return false;

            using (connection) {
                using (var command = connection.CreateCommand()) {
                    command.CommandText =
                        "CREATE DATABASE " + databaseName +
                        " ON PRIMARY (NAME=" + databaseName +
                        ", FILENAME='" + databasePath.Replace("'", "''") + "')";
                    command.ExecuteNonQuery();

                    command.CommandText =
                        "EXEC sp_detach_db '" + databaseName + "', 'true'";
                    command.ExecuteNonQuery();
                }
            }
            return true;
        }

       private static bool TryCreateSqlServerDatabseTable(string databasePath,string databaseName,string tableSql)
       {
           DbConnection connection;
           try {
                connection = new SqlConnection("Data Source=.;Initial Catalog=tempdb;Integrated Security=True;");
               connection.Open();
           }
           catch (SqlException e) {
               Trace.WriteLine(string.Format("Error opening connection to Sql Server ('{0}'). Skipping test.", e.Message));
               return false;
           }
           if (connection == null)
               return false;

           using (connection) {
               using (var command = connection.CreateCommand())
               {
                   command.CommandText = tableSql;
                        
                   command.ExecuteNonQuery();
               }
           }
           return true;
       }
       
        private static SqlConnection TryOpenSqlServerConnection() {
            try {
                var connection = new SqlConnection("Data Source=.;Initial Catalog=tempdb;Integrated Security=true;");
                connection.Open();
                return connection;
            }
            catch (SqlException e) {
                Trace.WriteLine(string.Format("Error opening connection to Sql Server ('{0}'). Skipping test.", e.Message));
                return null;
            }
        }

        public static void RunWithSqlCe(IEnumerable<RecordBlueprint> recordDescriptors, Action<ISessionFactory> action) {
//            var temporaryPath = Path.GetTempFileName();
//            if (File.Exists(temporaryPath))
//                File.Delete(temporaryPath);
//            Directory.CreateDirectory(temporaryPath);
//            var databasePath = Path.Combine(temporaryPath, "Orchard.mdf");
//            var databaseName = Path.GetFileNameWithoutExtension(databasePath);
//            var parameters = new SessionFactoryParameters {
//                Provider = "SqlCe",
//                DataFolder = temporaryPath,
//                RecordDescriptors = recordDescriptors
//            };
//            try {
//                var manager = (IDataServicesProviderFactory)new DataServicesProviderFactory(new[] {
//                new Meta<CreateDataServicesProvider>(
//                    (dataFolder, connectionString) => new SqlCeDataServicesProvider(dataFolder, connectionString),
//                    new Dictionary<string, object> {{"ProviderName", "SqlCe"}})
//            });
//
//                var configuration = manager
//                    .CreateProvider(parameters)
//                    .BuildConfiguration(parameters);
//
//                configuration.SetProperty("connection.release_mode", "on_close");
//
//                new SchemaExport(configuration).Execute(false, true, false);
//
//                using (var sessionFactory = configuration.BuildSessionFactory()) {
//                    action(sessionFactory);
//                }
//
//            }
//            finally {
//                try {
//                    Directory.Delete(temporaryPath, true);
//                }
//                catch (IOException) { }
//            }
        }
    }
}