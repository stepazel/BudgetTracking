namespace BudgetTrackingApp.Models

open System

type Expense = { Description: string; Amount: int; Created: DateTime }

type HomepageModel = { Expenses: List<Expense> }
