<script lang="ts">
	import * as Card from "$lib/components/ui/card/index.js";
	import * as Field from "$lib/components/ui/field/index.js";
	import { Button } from "$lib/components/ui/button/index.js";
	import { Input } from "$lib/components/ui/input/index.js";
	import type { ComponentProps } from "svelte";
	import { signUp } from "$lib/api/auth";
	import Page from "../../routes/+page.svelte";
	import { goto } from "$app/navigation";

	let { ...restProps }: ComponentProps<typeof Card.Root> = $props();

	let error = $state("");

	async function handleSubmit(e: SubmitEvent){
		e.preventDefault();
		const form = e.target as HTMLFormElement;
		const data = new FormData(form);
		const name = data.get("name") as string;
		const email = data.get("email") as string;
		const password = data.get("password") as string;
		try {
			await signUp(name, email, password);
			await goto("/");
		} catch {
			error = "Something went wrong while creating your account!";
		}
	}
</script>

<Card.Root {...restProps}>
	<Card.Header>
		<Card.Title>Create an account</Card.Title>
		<Card.Description>Enter your information below to create your account</Card.Description>
	</Card.Header>
	{#if error}
		<p class="text-red-500 px-(--card-spacing)">{error}</p>
	{/if}
	<Card.Content>
		<form method="POST" onsubmit={handleSubmit}>
			<Field.Group>
				<Field.Field>
					<Field.Label for="name">Full Name</Field.Label>
					<Input id="name" type="text" placeholder="John Doe" name="name" required />
				</Field.Field>
				<Field.Field>
					<Field.Label for="email">Email</Field.Label>
					<Input id="email" type="email" placeholder="me@example.com" name="email" required />
				</Field.Field>
				<Field.Field>
					<Field.Label for="password">Password</Field.Label>
					<Input id="password" type="password" name="password" required />
					<Field.Description>Must be at least 8 characters long.</Field.Description>
				</Field.Field>
				<Field.Field>
					<Field.Label for="confirm-password">Confirm Password</Field.Label>
					<Input id="confirm-password" type="password" required />
					<Field.Description>Please confirm your password.</Field.Description>
				</Field.Field>
				<Field.Group>
					<Field.Field>
						<Button type="submit">Create Account</Button>
						<Field.Description class="px-6 text-center">
							Already have an account? <a href="/login">Sign in</a>
						</Field.Description>
					</Field.Field>
				</Field.Group>
			</Field.Group>
		</form>
	</Card.Content>
</Card.Root>
