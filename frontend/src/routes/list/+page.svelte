<script lang="ts">
    import * as Breadcrumb from "$lib/components/ui/breadcrumb/index.js";
    import * as Sidebar from "$lib/components/ui/sidebar/index.js";
    import { Separator } from "$lib/components/ui/separator/index.js";
    import AppSidebar from "$lib/components/app-sidebar.svelte";
    import type { PageData } from "./$types";
    import Togglemode from "$lib/components/ui/toggle-mode/togglemode.svelte";
    import * as Table from "$lib/components/ui/table/index.js";
    import * as AlertDialog from "$lib/components/ui/alert-dialog/index.js";
    import type FileItem from "$lib/interfaces/file";
    import { formatFileSize, formatDate } from "$lib/utils/format";
    import { Button } from "$lib/components/ui/button/index.js";
    import Trash2 from "@lucide/svelte/icons/trash-2";
    import Download from "@lucide/svelte/icons/download";
    import { fetchFiles, deleteFile, getDownloadToken } from "$lib/utils/storage";

    let files = $state<FileItem[]>([]);
    let fileToDelete = $state<FileItem | null>(null);
    let isDeleting = $state(false);

    const downloadFile = async (fileId: string) => {
        const token = await getDownloadToken(fileId);
        if (!token) return;

        const url = `http://localhost:5172/api/storage/download/${fileId}?token=${token}`;
        window.location.href = url;
    };

    function requestDelete(file: FileItem) {
        fileToDelete = file;
    }

    async function confirmDelete() {
        if (!fileToDelete) return;

        isDeleting = true;
        const success = await deleteFile(fileToDelete.id);
        isDeleting = false;

        if (success) {
            files = files.filter((f) => f.id !== fileToDelete!.id);
        }

        fileToDelete = null;
    }

    $effect(() => {
        async function load() {
            const result = await fetchFiles();
            if (result) {
                files = result;
            }
        }
        load();
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
                                <Breadcrumb.Page>Manage Files</Breadcrumb.Page>
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
                            <Table.Head class="text-end">Actions</Table.Head>
                        </Table.Row>
                    </Table.Header>
                    <Table.Body>
                        {#each files as file (file.id)}
                            <Table.Row>
                                <Table.Cell class="font-medium">{file.originalFileName}</Table.Cell>
                                <Table.Cell>{file.contentType}</Table.Cell>
                                <Table.Cell>{formatFileSize(file.totalSize)}</Table.Cell>
                                <Table.Cell>{formatDate(file.createdAt)}</Table.Cell>
                                <Table.Cell class="text-end">
                                    <div class="flex justify-end gap-2">
                                        <Button
                                            size="icon"
                                            variant="outline"
                                            class="cursor-pointer"
                                            onclick={() => downloadFile(file.id)}
                                        >
                                            <Download class="size-4" />
                                        </Button>
                                        <Button
                                            size="icon"
                                            variant="destructive"
                                            class="cursor-pointer"
                                            onclick={() => requestDelete(file)}
                                        >
                                            <Trash2 class="size-4" />
                                        </Button>
                                    </div>
                                </Table.Cell>
                            </Table.Row>
                        {/each}
                    </Table.Body>
                </Table.Root>
            </div>
        </div>
    </Sidebar.Inset>
</Sidebar.Provider>

<AlertDialog.Root open={fileToDelete !== null} onOpenChange={(open) => !open && (fileToDelete = null)}>
    <AlertDialog.Content>
        <AlertDialog.Header>
            <AlertDialog.Title>Delete File?</AlertDialog.Title>
            <AlertDialog.Description>
                Are you sure you want to delete <strong>{fileToDelete?.originalFileName}</strong>? This action cannot be undone.
            </AlertDialog.Description>
        </AlertDialog.Header>
        <AlertDialog.Footer>
            <AlertDialog.Cancel disabled={isDeleting} class="cursor-pointer">Cancel</AlertDialog.Cancel>
            <AlertDialog.Action disabled={isDeleting} onclick={confirmDelete} class="bg-red-500 text-white cursor-pointer hover:bg-red-900">
                {isDeleting ? "Deleting..." : "Delete"}
            </AlertDialog.Action>
        </AlertDialog.Footer>
    </AlertDialog.Content>
</AlertDialog.Root>