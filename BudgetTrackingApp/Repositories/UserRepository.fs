module BudgetTrackingApp.Repositories.UserRepository

open System
open Microsoft.Extensions.Configuration
open Npgsql
let configurationBuilder = new ConfigurationBuilder()

let configuration =
            configurationBuilder
                .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile(sprintf "appsettings.%s.json" Environment.MachineName, optional=true) // Load environment-specific file
                .Build()

let databasePassword = if Environment.MachineName = "LT-STZE" then "admin" else Environment.GetEnvironmentVariable("DATABASE_PASSWORD")
let envName = Environment.MachineName
let connString = configuration.GetSection("ConnectionString").Value
let connectionString = $"{connString}Password={databasePassword};"
let connection = NpgsqlDataSource.Create(connectionString)


