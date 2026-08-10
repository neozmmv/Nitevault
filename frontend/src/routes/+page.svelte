<!-- sidebar-07 https://www.shadcn-svelte.com/blocks  -->
<!-- / root -->
<script lang="ts">
	import * as Breadcrumb from "$lib/components/ui/breadcrumb/index.js";
	import * as Sidebar from "$lib/components/ui/sidebar/index.js";
	import { Separator } from "$lib/components/ui/separator/index.js";
	import AppSidebar from "$lib/components/app-sidebar.svelte";
	import type { PageData } from "./$types";
	import Togglemode from "$lib/components/ui/toggle-mode/togglemode.svelte";
	import { fetchFiles } from "$lib/utils/storage";
	import { formatFileSize } from "$lib/utils/format";

    let { data }: {data: PageData} = $props()

	let fileAmount = $state(0);
	let totalBytes = $state(0);

	$effect(() => {
		async function load() {
			const files = await fetchFiles();
			if (files) {
				fileAmount = files.length;
				totalBytes = files.reduce((sum, f) => sum + f.totalSize, 0);
			}
		}
		load();
	});
</script>

<Sidebar.Provider>
	<AppSidebar user={data.userData!} />
	<Sidebar.Inset>
		<header
			class="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12"
		>
			<div class="flex justify-between w-full gap-2 pr-4">
				<div class="flex items-center gap-2 px-4">
					<Sidebar.Trigger class="-ms-1" />
					<Separator orientation="vertical" class="me-2 data-[orientation=vertical]:h-4" />
					<Breadcrumb.Root>
						<Breadcrumb.List>
							<Breadcrumb.Item class="hidden md:block">
								<Breadcrumb.Link>Nitevault</Breadcrumb.Link>
							</Breadcrumb.Item>
							<Breadcrumb.Separator class="hidden md:block" />
							<Breadcrumb.Item>
								<Breadcrumb.Page>Dashboard</Breadcrumb.Page>
							</Breadcrumb.Item>
						</Breadcrumb.List>
					</Breadcrumb.Root>
				</div>
				<div>
					<Togglemode />
				</div>
			</div>
		</header>
		<div class="flex flex-1 flex-col gap-4 p-4 pt-0">
			<div class="grid auto-rows-min gap-4 md:grid-cols-2"> <!-- md:grid-cols-3 if i want to have 3 windows-->
				<div class="aspect-video rounded-xl bg-muted/50 flex items-center justify-center">
					<h1 class="text-7xl"><span class="text-blue-500">{fileAmount}</span> <span class="text-7xl">Files</span></h1>
				</div>
				<div class="aspect-video rounded-xl bg-muted/50 flex items-center justify-center">
					<h1 class="text-5xl"><span class="text-blue-600">{formatFileSize(totalBytes)}</span> stored</h1>
				</div>
				<!-- <div class="aspect-video rounded-xl bg-muted/50"></div> --> <!-- also uncomment this-->
			</div>
			<div class="min-h-screen flex-1 rounded-xl bg-muted/50 md:min-h-min"></div>
		</div>
	</Sidebar.Inset>
</Sidebar.Provider>
