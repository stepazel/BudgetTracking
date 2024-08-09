namespace BudgetTrackingApp.Models

type KnownExpense = { Description: string }

type Categories = Map<int, string>

type HomepageModel = { Categories: Categories; Total: float; YearlyTotal: float; MonthlyTotal: float; WeeklyTotal: float }

type TotalsResult = { Total: float; YearlyTotal: float; MonthlyTotal: float; WeeklyTotal: float }
