<!-- sidebar-07 https://www.shadcn-svelte.com/blocks -->
<script lang="ts">
    import * as Breadcrumb from "$lib/components/ui/breadcrumb/index.js";
    import * as Sidebar from "$lib/components/ui/sidebar/index.js";
    import { Separator } from "$lib/components/ui/separator/index.js";
    import AppSidebar from "$lib/components/app-sidebar.svelte";
    import type { PageData } from "./$types";
    import Togglemode from "$lib/components/ui/toggle-mode/togglemode.svelte";
    import * as Table from "$lib/components/ui/table/index.js";
    import type FileItem from "$lib/interfaces/file";
    import { formatFileSize, formatDate } from "$lib/utils/format";
    import { Button } from "$lib/components/ui/button/index.js";

    let files = $state<FileItem[]>([]);

    const downloadFile = async (fileId: string, fileName: string) => {
        try {
            const res = await fetch(`http://localhost:5172/api/storage/download/${fileId}`, { credentials: "include" });
            if(!res.ok) {
                console.error("Error while downloading file ", fileId);
                return;
            }

            const blob = await res.blob();
            const url = URL.createObjectURL(blob);

            const a = document.createElement("a");
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);

            URL.revokeObjectURL(url);
        } catch (err) {
            console.error("Failed to download file: ", err);
        }
    }

    const fetchFiles = async () => {
        try {
            // change localhost to container name on dockerized
            const res = await fetch("http://localhost:5172/api/storage/list", { credentials: "include" });
            if (!res.ok) {
                console.error('Failed to fetch files:', res.status);
                return;
            }
            files = await res.json();
        } catch (err) {
            console.error('Failed to fetch files:', err);
        }
    }

    $effect(() => {
        fetchFiles();
    });

    let { data }: { data: PageData } = $props();
</script>

<Sidebar.Provider>
    <AppSidebar user={data.userData!} />
    <Sidebar.Inset>
        <header class="flex h-16 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12">
            <div class="flex justify-between w-full gap-2 pr-4">
                <div class="flex items-center gap-2 px-4">
                    <Sidebar.Trigger class="-ms-1" />
                    <Separator orientation="vertical" class="me-2 data-[orientation=vertical]:h-4" />
                    <Breadcrumb.Root>
                        <Breadcrumb.List>
                            <Breadcrumb.Item class="hidden md:block">
                                <Breadcrumb.Link>Files</Breadcrumb.Link>
                            </Breadcrumb.Item>
                            <Breadcrumb.Separator class="hidden md:block" />
                            <Breadcrumb.Item>
                                <Breadcrumb.Page>List Files</Breadcrumb.Page>
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
            <div class="grid auto-rows-min gap-4 md:grid-cols-3"></div>
            <div class="min-h-screen flex-1 rounded-xl bg-muted/50 md:min-h-min px-6 py-6">
                <Table.Root>
                    <Table.Caption>{files.length} files(s)</Table.Caption>
                    <Table.Header>
                        <Table.Row>
                            <Table.Head>Name</Table.Head>
                            <Table.Head>Type</Table.Head>
                            <Table.Head>Size</Table.Head>
                            <Table.Head>Created At</Table.Head>
                            <Table.Head class="text-end">Download</Table.Head>
                        </Table.Row>
                    </Table.Header>
                    <Table.Body>
                        {#each files as file (file.id)}
                            <Table.Row>
                                <Table.Cell class="font-medium">{file.originalFileName}</Table.Cell>
                                <Table.Cell>{file.contentType}</Table.Cell>
                                <Table.Cell>{formatFileSize(file.totalSize)}</Table.Cell>
                                <Table.Cell>{formatDate(file.createdAt)}</Table.Cell>
                                <Table.Cell class="text-end"><Button class="cursor-pointer" onclick={() => downloadFile(file.id, file.originalFileName)}>Download</Button></Table.Cell>
                            </Table.Row>
                        {/each}
                    </Table.Body>
                </Table.Root>
            </div>
        </div>
    </Sidebar.Inset>
</Sidebar.Provider>