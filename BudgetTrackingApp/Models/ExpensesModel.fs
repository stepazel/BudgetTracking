namespace BudgetTrackingApp.Models.Expenses

open System
open System.Collections.Generic


type Expense =
    { Id: int
      Description: string
      Amount: float
      Created: DateTime
      CategoryName: string }

type Model = {Expenses: IEnumerable<Expense>}

