namespace BudgetTrackingApp.Models

open System
open System.Collections.Generic

type KnownExpense = { Description: string }

type Categories = Map<int, string>

type Expense = {Id: int; Description: string; Amount: float; Created: DateTime; CategoryName: string}

type HomepageModel = { Expenses: IEnumerable<Expense>;Categories: Categories; Total: float; YearlyTotal: float; MonthlyTotal: float; WeeklyTotal: float }

type TotalsResult = { Total: float; YearlyTotal: float; MonthlyTotal: float; WeeklyTotal: float }

