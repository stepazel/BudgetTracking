module BudgetTrackingApp.Repositories.DatabaseModels

open System

type Expense = {Id: System.Nullable<int>; Description: string; Amount: float; Created: DateTime; CategoryId: int; UserId: int}

type Category = {Id: int; Name: string}