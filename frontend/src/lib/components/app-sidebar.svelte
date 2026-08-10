<script lang="ts" module>
	import FolderOpen from '@lucide/svelte/icons/folder-open';

	// This is sample data.
	/* const data = {
		user: {
			name: "shadcn",
			email: "m@example.com",
			avatar: "/avatars/shadcn.jpg",
		},
	}; */


	const navMain = [
		{
			title: "Files",
			url: "/files",
			icon: FolderOpen,
			isActive: true,
			items: [
				{
					title: "List Files",
					url: "/list"
				},
				{
					title: "Upload Files",
					url: "/upload"
				}
			]
		},
		/* {
			title: "Models",
			url: "#",
			icon: BotIcon,
			items: [
				{
					title: "Genesis",
					url: "#",
				},
				{
					title: "Explorer",
					url: "#",
				},
				{
					title: "Quantum",
					url: "#",
				},
			],
		},
		{
			title: "Documentation",
			url: "#",
			icon: BookOpenIcon,
			items: [
				{
					title: "Introduction",
					url: "#",
				},
				{
					title: "Get Started",
					url: "#",
				},
				{
					title: "Tutorials",
					url: "#",
				},
				{
					title: "Changelog",
					url: "#",
				},
			],
		},
		{
			title: "Settings",
			url: "#",
			icon: Settings2Icon,
			items: [
				{
					title: "General",
					url: "#",
				},
				{
					title: "Team",
					url: "#",
				},
				{
					title: "Billing",
					url: "#",
				},
				{
					title: "Limits",
					url: "#",
				},
			],
		}, */
	]
</script>

<script lang="ts">
	import * as Sidebar from "$lib/components/ui/sidebar/index.js";
	import NavMain from "./nav-main.svelte";
	import NavProjects from "./nav-projects.svelte";
	import NavUser from "./nav-user.svelte";
	import TeamSwitcher from "./team-switcher.svelte";
	import type { ComponentProps } from "svelte";
	import type User from "$lib/interfaces/user";


	let {
		ref = $bindable(null),
		collapsible = "icon",
		user,
		...restProps
	}: ComponentProps<typeof Sidebar.Root> & { user: User } = $props();
</script>

<Sidebar.Root bind:ref {collapsible} {...restProps}>
	<Sidebar.Content>
		<NavMain items={navMain} />
	</Sidebar.Content>
	<Sidebar.Footer>
		<NavUser user={user} />
	</Sidebar.Footer>
	<Sidebar.Rail />
</Sidebar.Root>
