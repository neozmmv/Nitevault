import { redirect } from "@sveltejs/kit";
import type { LayoutServerLoad } from "./$types";

const PUBLIC_ROUTES = ["/login", "/signUp"];

export const load: LayoutServerLoad = async ({ fetch, cookies, url }) => {
    const sessionCookie = cookies.get("jwt");
    const refreshToken = cookies.get("refresh-token");
    const isPublicRoute = PUBLIC_ROUTES.includes(url.pathname);

    if (!sessionCookie) {
        if (refreshToken) {
            const res = await fetch("http://localhost:5172/api/auth/refresh", {
                headers: { cookie: `refresh-token=${refreshToken}` }
            });

            if (!res.ok) redirect(303, "/login");

            const setCookies = res.headers.getSetCookie();
            for (const rawCookie of setCookies) {
                const [nameValue] = rawCookie.split(';');
                const [name, value] = nameValue.split('=');
                cookies.set(name.trim(), value.trim(), {
                    path: '/',
                    httpOnly: true,
                    secure: true,
                    sameSite: 'strict'
                });
            }

            if (isPublicRoute) redirect(303, "/");
            return {};
        }

        if (!isPublicRoute) redirect(303, "/login");
        return {};
    }

    // maybe /user/me instead...

    const res = await fetch("http://localhost:5172/api/auth/checkAuth", {
        headers: { cookie: `jwt=${sessionCookie}` }
    });

    if (!res.ok) redirect(303, "/login");
    if (isPublicRoute) redirect(303, "/");

    return {};
};