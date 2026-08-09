
// change localhost to container name on dockerized version!

export async function login(email: string, password: string) {
    const res = await fetch("http://localhost:5172/api/auth/login",
        {
            method: "POST",
            headers: { "Content-Type" : "application/json" },
            body: JSON.stringify({email, password}),
            credentials: "include"
        }
    );

    if (!res.ok) throw new Error("Invalid Credentials");
}