module BudgetTrackingApp.Models.UserpageModel

open System.Collections.Generic

type User = { Id: int; Password: string }

type Category = { Id: int; Name: string }

type ManageCategoriesModel = { ExistingCategories: IEnumerable<Category>; UserCategories: IEnumerable<Category>  }
