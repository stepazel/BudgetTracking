module BudgetTrackingApp.Models.InsightsModel

type Category = {
    Name: string
    Total: float
    Count: int64
    Median: float
    Max: float
    Min: float
    YearTotal: float
    YearCount: int64
    MonthTotal: float
    MonthCount: int64
    WeekTotal: float
    WeekCount: int64
}

type Model = {Categories: List<Category>} 
