/* import { fail, redirect } from '@sveltejs/kit';
import type { Actions } from './$types';

// for dockerized version, localhost should be the container name

export const actions: Actions = {
    default : async ({request, fetch, cookies}) => {
        const data = await request.formData();
        const email = data.get("email") as string;
        const password = data.get("password") as string;

        const res = await fetch("http://localhost:5172/api/auth/login", {
            method: "POST",
            headers: {
                "Content-Type" : "application/json"
            },
            body: JSON.stringify({email, password})
        });

        if (!res.ok) {
            return fail(401, { error: "Invalid Credentials!" });
        }

        redirect(303, "/");
    }
} */