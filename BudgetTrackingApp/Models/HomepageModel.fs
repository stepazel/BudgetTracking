namespace BudgetTrackingApp.Models

open System

type Expense = { Description: string; Amount: int; Created: DateTime }

type KnownExpense = { Description: string }

type HomepageModel = { Expenses: List<Expense>; KnownExpenses: List<KnownExpense> }
