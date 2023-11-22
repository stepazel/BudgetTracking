namespace BudgetTrackingApp.Models

open System

type Expense = { Id: int; Description: string; Amount: int; Created: DateTime; Category: string}

type KnownExpense = { Description: string }

type CategoryNames = Map<string, string>

type HomepageModel = { Expenses: Map<string, Expense seq>; CategoryNames: CategoryNames; Total: int }
