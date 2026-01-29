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
            const response = await supabase
                .from('expenses')
                .select('id, description, amount, created, categories ( name )')
                .order('created', { ascending: false });
            if (response.status === 200 && response.data !== null) {
                // @ts-ignore
                setExpenses(response.data);
            } else {
                console.error(response.error);
            }
        }
        fetchExpenses();
    }, [])

    // Categories belonging to current user
    const [categories, setCategories] = useState<{ id: number; name: string }[]>([]);
    useEffect(() => {
        async function fetchUserCategories() {
            const response = await supabase.from('categories').select('*');
            if (response.status === 200 && response.data !== null) {
                setCategories(response.data);
            } else {
                console.error(response.error);
            }

        }
        fetchUserCategories();
    }, []);

    const handleSubmit = async (event: any) => {
        event.preventDefault();
        const formData = new FormData(event.target);
        const response = await supabase.auth.getUser();
        const userId = response.data.user?.id;
        const { data, error } = await supabase.from('expenses').insert({
            description: formData.get('description') as string,
            amount: parseFloat(formData.get('amount') as string),
            category_id: Number(formData.get('category_id')),
            created: formData.get('date') as string,
            user_id: userId,
        }).select('id, description, amount, created, categories ( name )');
        
        if (error) {
            alert(error.message);
            return;
        }
        
        if (data && Array.isArray(data) && data.length > 0) {
            // @ts-ignore
            setExpenses([data[0] as Expense, ...expenses]);
        }
    }
    
    const handleDeleteExpense = async (id: number) => {
        const { error } = await supabase
            .from('expenses')
            .delete()
            .eq('id', id);
        if (error) {
            alert(error.message);
            return;
        }
        setExpenses(prev => prev.filter(e => e.id !== id));
    }
    
    return (
        <div className="min-h-dvh flex flex-col bg-background text-foreground">
            <header className="sticky top-0 z-10 border-b bg-card/80 backdrop-blur supports-[backdrop-filter]:bg-card/60">
                <div className="mx-auto w-full max-w-7xl px-4 py-3 flex items-center gap-4">
                    <div className="flex items-center gap-2">
                        <span className="text-base font-semibold">Výdaje</span>
                    </div>

                </div>
            </header>

            {/* App Shell */}
            <div className="flex-1">
                <div className="mx-auto grid w-full max-w-2xl grid-cols-1 gap-6 px-4 py-6">
                    {/* Main content */}
                    <main className="flex flex-col gap-6">

                        <section className="rounded-lg border bg-card">
                            <div className="p-4">
                                <h2 className="text-base font-semibold leading-none mb-1">
                                    Nový výdaj
                                </h2>
                            </div>
                            <Separator />
                            <div className="p-4">
                                <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
                                <FieldSet>
                                    <Field orientation="responsive">
                                        <FieldContent>
                                            <input
                                                type="number"
                                                name="amount"
                                                placeholder="Částka"
                                                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                                            />
                                        </FieldContent>
                                    </Field>
                                    <Field orientation="responsive">
                                        <FieldContent>
                                            <input
                                                type="text"
                                                name="description"
                                                placeholder="Popis výdaje"
                                                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                                            />
                                        </FieldContent>
                                    </Field>
                                    <Field>
                                        <FieldContent>
                                            <select name="category_id" className="h-10 w-full rounded-md border bg-background px-3 text-sm">
                                                {
                                                    categories.map((category) => (
                                                        <option key={category.id} value={category.id}>{category.name}</option>
                                                    ))
                                                }
                                            </select>
                                        </FieldContent>
                                    </Field>
                                    <Field orientation="responsive">
                                        <FieldContent>
                                            <input
                                                type="date"
                                                name="date"
                                                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                                                defaultValue={new Date().toISOString().split("T")[0]}
                                            />
                                        </FieldContent>
                                    </Field>
                                    <div className="flex justify-center gap-2 pt-2">
                                        <button type="submit" className="h-9 w-60 rounded-md bg-primary px-3 text-primary-foreground text-sm">
                                            Přidat
                                        </button>
                                    </div>
                                </FieldSet>
                                </form>
                            </div>
                        </section>
                        <section className="rounded-lg border bg-card">
                            <div className="p-4">
                                <h2 className="text-base font-semibold leading-none mb-1">
                                    Výdaje
                                </h2>
                            </div>
                            <Separator />
                            <div className="p-4 overflow-x-auto">
                                <table className="w-full text-sm">
                                    <thead>
                                    <tr className="text-left border-b">
                                        <th className="py-2 pr-4">Částka</th>
                                        <th className="py-2 pr-4">Popis</th>
                                        <th className="py-2 pr-4">Datum</th>
                                        <th className="py-2 text-right">Akce</th>
                                    </tr>
                                    </thead>
                                    <tbody>
                                    {expenses.map((e) => (
                                        <tr key={e.id} className="border-b last:border-0">
                                            <td className="py-2 pr-4">{e.amount} Kč</td>
                                            <td className="py-2 pr-4">{e.description}</td>
                                            <td className="py-2 pr-4">{new Date(e.created).toLocaleDateString('cs-CZ')}</td>
                                            <td className="py-2 text-right">
                                                <button
                                                    onClick={() => handleDeleteExpense(e.id)}
                                                    className="h-8 px-3 rounded-md border bg-background hover:bg-muted text-xs"
                                                >
                                                    Smazat
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                    </tbody>
                                </table>
                            </div>
                        </section>
                        
                    </main>
                </div>
            </div>

            <footer className="border-t bg-card">
                <div className="mx-auto w-full max-w-7xl px-4 py-6 text-xs text-muted-foreground">
                    © {new Date().getFullYear()} Neutrácej tolik
                </div>
            </footer>
        </div>
        
    )
}