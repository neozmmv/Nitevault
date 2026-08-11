import { redirect } from '@sveltejs/kit';
import type { LayoutServerLoad } from './$types';
import type User from '$lib/interfaces/user';
import type { Cookies } from '@sveltejs/kit';
import { API_URL } from '$lib/config';

const PUBLIC_ROUTES = ['/login', '/signUp'];

async function tryRefresh(fetch: typeof globalThis.fetch, cookies: Cookies, refreshToken: string) {
	const res = await fetch(`${API_URL}/api/auth/refresh`, {
		headers: { cookie: `refresh-token=${refreshToken}` },
		method: 'POST'
	});

	if (!res.ok) return false;

	try {
		const setCookies = res.headers.getSetCookie();

		for (const rawCookie of setCookies) {
			const [nameValue] = rawCookie.split(';');
			const eqIndex = nameValue.indexOf('=');
			const name = nameValue.slice(0, eqIndex).trim();
			const value = nameValue.slice(eqIndex + 1).trim();
			cookies.set(name, value, {
				path: '/',
				httpOnly: true,
				secure: true,
				sameSite: 'strict'
			});
		}
	} catch (err) {
		console.error('ERROR parsing cookies:', err);
		return false;
	}

	return true;
}

export const load: LayoutServerLoad = async ({ fetch, cookies, url }) => {
	const isPublicRoute = PUBLIC_ROUTES.includes(url.pathname);
	let sessionCookie = cookies.get('jwt');
	const refreshToken = cookies.get('refresh-token');

	if (!sessionCookie) {
		if (!refreshToken) {
			if (!isPublicRoute) redirect(303, '/login');
			return {};
		}

		const refreshed = await tryRefresh(fetch, cookies, refreshToken);
		if (!refreshed) {
			if (!isPublicRoute) redirect(303, '/login');
			return {};
		}

		sessionCookie = cookies.get('jwt');
	}

	let res = await fetch(`${API_URL}/api/user/me`, {
		headers: { cookie: `jwt=${sessionCookie}` }
	});

	// jwt was present but invalid/expired, fall back to refresh before giving up
	if (!res.ok && refreshToken) {
		const refreshed = await tryRefresh(fetch, cookies, refreshToken);
		if (refreshed) {
			sessionCookie = cookies.get('jwt');
			res = await fetch(`${API_URL}/api/user/me`, {
				headers: { cookie: `jwt=${sessionCookie}` }
			});
		}
	}

	if (!res.ok) {
		if (!isPublicRoute) redirect(303, '/login');
		return {};
	}

	if (isPublicRoute) redirect(303, '/');

	const userData = (await res.json()) as User;
	return { userData };
};
