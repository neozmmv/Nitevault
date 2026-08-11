import { API_URL } from "$lib/config";

export async function login(email: string, password: string) {
    const res = await fetch(`${API_URL}/api/auth/login`,
        {
            method: "POST",
            headers: { "Content-Type" : "application/json" },
            body: JSON.stringify({email, password}),
            credentials: "include"
        }
    );

    if (!res.ok) throw new Error("Invalid Credentials");
}

export async function signUp(name: string, email: string, password: string) {
    const res = await fetch(`${API_URL}/api/auth/signUp`,
        {
            method: "POST",
            headers: { "Content-Type" : "application/json" },
            body: JSON.stringify({name, email, password}),
            credentials: "include"
        }
    );

    if (!res.ok) throw new Error("Error while creating account.");
}

export async function logout() {
    const res = await fetch(`${API_URL}/api/auth/logout`, {
        method: "POST",
        headers: { "Content-Type" : "application/json" },
        credentials: "include"
    })

    if (!res.ok) throw new Error("Logout error!")
}