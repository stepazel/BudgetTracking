module BudgetTrackingApp.Repositories.UserRepository

open System
open Microsoft.Extensions.Configuration
open Npgsql

let configuration =
             ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddJsonFile(sprintf "appsettings.%s.json" Environment.MachineName, optional=true) // Load environment-specific file
                .Build()

let databasePassword = if Environment.MachineName = "LT-STZE" then "admin" else Environment.GetEnvironmentVariable("DATABASE_PASSWORD")
let connection = $"""{configuration.GetSection("ConnectionString").Value}Password={databasePassword};""" |> NpgsqlDataSource.Create
                    