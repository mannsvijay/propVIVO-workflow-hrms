'use client';

import { FormEvent, Suspense, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useSession } from "../../../context/SessionContext";

export default function LoginPage() {
	return (
		<Suspense fallback={<div className="p-6 text-center">Loading…</div>}>
			<LoginForm />
		</Suspense>
	);
}

function LoginForm() {
	const router = useRouter();
	const searchParams = useSearchParams();
	const { login } = useSession();
	const [email, setEmail] = useState("");
	const [password, setPassword] = useState("");
	const [error, setError] = useState<string | null>(null);
	const [loading, setLoading] = useState(false);

	async function onSubmit(e: FormEvent) {
		e.preventDefault();
		setError(null);
		setLoading(true);
		try {
			await login({ email, password });
			const next = searchParams.get("next") || "/";
			router.replace(next);
		} catch (err: any) {
			setError(err?.message || "Login failed");
		} finally {
			setLoading(false);
		}
	}

	return (
		<div className="flex min-h-screen items-center justify-center p-4">
			<form onSubmit={onSubmit} className="w-full max-w-sm rounded-lg border p-6 space-y-4">
				<h1 className="text-xl font-semibold">Sign in</h1>
				<div className="space-y-2">
					<label className="block text-sm">Email</label>
					<input
						type="email"
						value={email}
						onChange={(e) => setEmail(e.target.value)}
						required
						className="w-full rounded border px-3 py-2"
						placeholder="you@example.com"
					/>
				</div>
				<div className="space-y-2">
					<label className="block text-sm">Password</label>
					<input
						type="password"
						value={password}
						onChange={(e) => setPassword(e.target.value)}
						required
						className="w-full rounded border px-3 py-2"
						placeholder="••••••••"
					/>
				</div>
				{error ? <p className="text-sm text-red-600">{error}</p> : null}
				<button
					type="submit"
					disabled={loading}
					className="w-full rounded bg-black px-3 py-2 text-white disabled:opacity-60"
				>
					{loading ? "Signing in..." : "Sign in"}
				</button>
			</form>
		</div>
	);
}


