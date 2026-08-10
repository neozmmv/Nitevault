import { redirect } from "@sveltejs/kit";
import type { LayoutServerLoad } from "./$types";
import type User from "$lib/interfaces/user";
import type { Cookies } from "@sveltejs/kit";

const PUBLIC_ROUTES = ["/login", "/signUp"];

async function tryRefresh(fetch: typeof globalThis.fetch, cookies: Cookies, refreshToken: string) {
    const res = await fetch("http://localhost:5172/api/auth/refresh", {
        headers: { cookie: `refresh-token=${refreshToken}` }
    });

    if (!res.ok) return false;

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

    return true;
}

export const load: LayoutServerLoad = async ({ fetch, cookies, url }) => {
    const isPublicRoute = PUBLIC_ROUTES.includes(url.pathname);
    let sessionCookie = cookies.get("jwt");
    const refreshToken = cookies.get("refresh-token");

    if (!sessionCookie) {
        if (!refreshToken) {
            if (!isPublicRoute) redirect(303, "/login");
            return {};
        }

        const refreshed = await tryRefresh(fetch, cookies, refreshToken);
        if (!refreshed) {
            if (!isPublicRoute) redirect(303, "/login");
            return {};
        }

        sessionCookie = cookies.get("jwt");
    }

    let res = await fetch("http://localhost:5172/api/user/me", {
        headers: { cookie: `jwt=${sessionCookie}` }
    });

    // jwt was present but invalid/expired, fall back to refresh before giving up
    if (!res.ok && refreshToken) {
        const refreshed = await tryRefresh(fetch, cookies, refreshToken);
        if (refreshed) {
            sessionCookie = cookies.get("jwt");
            res = await fetch("http://localhost:5172/api/user/me", {
                headers: { cookie: `jwt=${sessionCookie}` }
            });
        }
    }

    if (!res.ok) {
        if (!isPublicRoute) redirect(303, "/login");
        return {};
    }

    if (isPublicRoute) redirect(303, "/");

    const userData = await res.json() as User;
    return { userData };
};