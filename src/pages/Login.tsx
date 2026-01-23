import {Field, FieldContent, FieldLabel, FieldSet} from "@/components/ui/field"
import {Separator} from "@/components/ui/separator"
import {createClient} from "@supabase/supabase-js";
import {useNavigate} from "react-router";

const supabase = createClient(import.meta.env.VITE_SUPABASE_URL, import.meta.env.VITE_SUPABASE_PUBLISHABLE_DEFAULT_KEY);
export function Login() {
    const navigate = useNavigate();
    
    const handleLogin = async (event: any) => {
        event.preventDefault();
        const formData = new FormData(event.target);
        const {error} = await supabase.auth.signInWithPassword({
            email: formData.get('email') as string,
            password: formData.get('password') as string,
        })

        if (error) {
            alert(error.message);
            return;
        }

        navigate('/');
    }

    return (
        <div className="min-h-dvh flex flex-col bg-background text-foreground">
            <header
                className="sticky top-0 z-10 border-b bg-card/80 backdrop-blur supports-[backdrop-filter]:bg-card/60">
                <div className="mx-auto w-full max-w-7xl px-4 py-3 flex items-center gap-4">
                    <span className="text-base font-semibold">Přihlášení</span>
                </div>
            </header>

            <div className="flex-1 grid place-items-center px-4 py-10">
                <section className="w-full max-w-sm rounded-lg border bg-card">
                    <div className="p-4">
                        <h2 className="text-base font-semibold leading-none mb-1">Vytvoř si účet</h2>
                    </div>
                    <Separator/>
                    <div className="p-4">
                        <form className="flex flex-col gap-4" onSubmit={handleLogin}>
                            <FieldSet>
                                <Field>
                                    <FieldLabel>Email</FieldLabel>
                                    <FieldContent>
                                        <input
                                            type="email"
                                            name="email"
                                            placeholder="jmeno@email.cz"
                                            className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                                        />
                                    </FieldContent>
                                </Field>
                                <Field>
                                    <FieldLabel>Heslo</FieldLabel>
                                    <FieldContent>
                                        <input
                                            type="password"
                                            name="password"
                                            placeholder="••••••••"
                                            className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                                        />
                                    </FieldContent>
                                </Field>
                                <div className="pt-2">
                                    {/*<input type="submit"/>*/}
                                    <button type="submit"
                                            className="h-10 w-full rounded-md bg-primary text-primary-foreground text-sm">
                                        Přihlaš se
                                    </button>
                                </div>
                            </FieldSet>
                        </form>
                    </div>
                </section>
            </div>

            <footer className="border-t bg-card">
                <div className="mx-auto w-full max-w-7xl px-4 py-6 text-xs text-muted-foreground">
                    © {new Date().getFullYear()} Neutrácej tolik
                </div>
            </footer>
        </div>
    )
}
