module BudgetTrackingApp.Models.InsightsModel

type Category = {Name: System.String; YearlyTotal: System.Double; MonthlyTotal: System.Double; WeeklyTotal: System.Double}

type Model = {Categories: List<Category>} 
