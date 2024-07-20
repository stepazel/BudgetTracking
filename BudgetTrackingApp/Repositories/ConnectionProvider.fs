module BudgetTrackingApp.Repositories.ConnectionProvider

open System
open System.Data
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
                    
let conn = new NpgsqlConnection($"""{configuration.GetSection("ConnectionString").Value}Password={databasePassword};""") :> IDbConnection