import {createClient} from "@supabase/supabase-js";
import {Separator} from "@radix-ui/react-separator";
import {Field, FieldContent, FieldSet} from "@/components/ui/field.tsx";
import type {Expense} from "@/types/expense.ts";
import {useEffect, useState} from "react";
import {useNavigate} from "react-router";

const supabase = createClient(import.meta.env.VITE_SUPABASE_URL, import.meta.env.VITE_SUPABASE_PUBLISHABLE_DEFAULT_KEY);

export default function Homepage() {
    const navigate = useNavigate();

    useEffect(() => {
        async function isSignedIn() {
            const { data } = await supabase.auth.getSession();
            if (data.session === null) {
                navigate('/login');
            }
        }
        isSignedIn();
    }, []);
    
    const [expenses, setExpenses] = useState<Expense[]>([]);
    useEffect( () => {
        async function fetchExpenses() {
            
            const response = await supabase.from('expenses').select('*');
            if (response.status === 200 && response.data !== null) {
                setExpenses(response.data);
            } else {
                console.error(response.error);
            }
        }
        fetchExpenses();
    })
    
    if (expenses.length === 0) return <div>Loading...</div>;

    return (
        <div className="min-h-dvh flex flex-col bg-background text-foreground">
            {/* Header */}
            <header className="sticky top-0 z-10 border-b bg-card/80 backdrop-blur supports-[backdrop-filter]:bg-card/60">
                <div className="mx-auto w-full max-w-7xl px-4 py-3 flex items-center gap-4">
                    <div className="flex items-center gap-2">
                        {/*<div className="size-8 rounded-lg bg-primary/10 grid place-items-center text-primary font-bold">*/}
                        {/*  BT*/}
                        {/*</div>*/}
                        <span className="text-base font-semibold">Výdaje</span>
                    </div>

                    <div className="ml-auto flex items-center gap-2">
                        {/* Theme toggle placeholder */}
                        <button
                            tabIndex={-1}
                            className="h-9 px-3 rounded-md border bg-background hover:bg-muted text-sm"
                            aria-label="Toggle theme"
                        >
                            Dark mode
                        </button>
                    </div>
                </div>
            </header>

            {/* App Shell */}
            <div className="flex-1">
                <div className="mx-auto grid w-full max-w-2xl grid-cols-1 md:grid-cols-[240px_1fr] gap-6 px-4 py-6">
                    {/* Sidebar (hidden on mobile) */}
                    <aside className="hidden md:block">
                        <section className="rounded-lg border bg-card">
                            <div className="p-4">
                                <h2 className="text-base font-semibold leading-none mb-1">
                                    Poslední výdaje
                                </h2>
                            </div>
                            <Separator />
                            <div className="p-4">
                                <nav className="flex flex-col gap-1">
                                    {expenses.map((expense) => (
                                        <div key={expense.description} className="text-left rounded-md px-3 py-2">
                                            {expense.description} - {expense.amount}
                                        </div>
                                    ))}
                                </nav>
                            </div>
                        </section>
                    </aside>

                    {/* Main content */}
                    <main className="flex flex-col gap-6">
                        {/* Summary cards */}
                        {/*<section className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">*/}
                        {/*  {["Balance", "Income", "Expenses", "Savings"].map((title) => (*/}
                        {/*    <div*/}
                        {/*      key={title}*/}
                        {/*      className="rounded-lg border bg-card p-4 flex flex-col gap-2"*/}
                        {/*    >*/}
                        {/*      <div className="text-sm text-muted-foreground">{title}</div>*/}
                        {/*      <div className="text-2xl font-semibold tracking-tight">$0.00</div>*/}
                        {/*    </div>*/}
                        {/*  ))}*/}
                        {/*</section>*/}

                        <section className="rounded-lg border bg-card">
                            <div className="p-4">
                                <h2 className="text-base font-semibold leading-none mb-1">
                                    Nový výdaj
                                </h2>
                            </div>
                            <Separator />
                            <div className="p-4">
                                <FieldSet>
                                    <Field orientation="responsive">
                                        <FieldContent>
                                            <input
                                                type="text"
                                                placeholder="Popis výdaje"
                                                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                                            />
                                        </FieldContent>
                                    </Field>
                                    <Field orientation="responsive">
                                        <FieldContent>
                                            <input
                                                type="number"
                                                placeholder="Částka"
                                                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                                            />
                                        </FieldContent>
                                    </Field>
                                    <Field>
                                        <FieldContent>
                                            <select className="h-10 w-full rounded-md border bg-background px-3 text-sm">
                                                <option defaultValue="neco">Potraviny</option>
                                                <option defaultValue="dalsi">Pití</option>
                                            </select>
                                        </FieldContent>
                                    </Field>
                                    <Field orientation="responsive">
                                        <FieldContent>
                                            <input
                                                type="date"
                                                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                                                defaultValue={new Date().toISOString().split("T")[0]}
                                            />
                                        </FieldContent>
                                    </Field>
                                    <div className="flex justify-center gap-2 pt-2">
                                        <button className="h-9 w-60 rounded-md bg-primary px-3 text-primary-foreground text-sm">
                                            Přidat
                                        </button>
                                    </div>
                                </FieldSet>
                            </div>
                        </section>
                    </main>
                </div>
            </div>

            {/* Mobile bottom nav */}
            {/*<div className="md:hidden border-t bg-card/80 backdrop-blur supports-[backdrop-filter]:bg-card/60">*/}
            {/*  <div className="mx-auto max-w-7xl px-4 py-2 grid grid-cols-4 gap-2">*/}
            {/*    {["Home", "Add", "Budgets", "More"].map((t) => (*/}
            {/*      <button key={t} className="py-2 text-sm rounded-md hover:bg-muted">*/}
            {/*        {t}*/}
            {/*      </button>*/}
            {/*    ))}*/}
            {/*  </div>*/}
            {/*</div>*/}

            {/* Footer */}
            <footer className="border-t bg-card">
                <div className="mx-auto w-full max-w-7xl px-4 py-6 text-xs text-muted-foreground">
                    © {new Date().getFullYear()} Neutrácej tolik
                </div>
            </footer>
        </div>
        
    )
}