export type Expense = {
    id: number;
    description: string;
    amount: number;
    created: string; // ISO date string from DB
    categories: { name: string } | null;
}