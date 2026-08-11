<script lang="ts">
    import UploadIcon from "@lucide/svelte/icons/upload";
    import FileIcon from "@lucide/svelte/icons/file";
    import XIcon from "@lucide/svelte/icons/x";
    import { Button } from "$lib/components/ui/button/index.js";
    import { formatFileSize } from "$lib/utils/format";

    let { onUploadComplete }: { onUploadComplete: () => void } = $props();

    let isDragging = $state(false);
    let dragCounter = 0; // avoids flicker when dragging over child elements
    let fileInput: HTMLInputElement;

    type PendingFile = {
        file: File;
        progress: number; // 0-100
        status: "pending" | "uploading" | "done" | "error";
    };

    let pendingFiles = $state<PendingFile[]>([]);
    let isUploading = $state(false);

    function addFiles(newFiles: File[]) {
        pendingFiles = [
            ...pendingFiles,
            ...newFiles.map((file) => ({ file, progress: 0, status: "pending" as const }))
        ];
    }

    function removeFile(index: number) {
        pendingFiles = pendingFiles.filter((_, i) => i !== index);
    }

    function handleDrop(e: DragEvent) {
        e.preventDefault();
        isDragging = false;
        dragCounter = 0;
        if (e.dataTransfer?.files?.length) {
            addFiles(Array.from(e.dataTransfer.files));
        }
    }

    function handleDragEnter(e: DragEvent) {
        e.preventDefault();
        dragCounter++;
        isDragging = true;
    }

    function handleDragOver(e: DragEvent) {
        e.preventDefault(); // required on dragover too, or drop never fires
    }

    function handleDragLeave(e: DragEvent) {
        e.preventDefault();
        dragCounter--;
        if (dragCounter <= 0) {
            isDragging = false;
            dragCounter = 0;
        }
    }

    function handleFileInput(e: Event) {
        const target = e.target as HTMLInputElement;
        if (target.files?.length) {
            addFiles(Array.from(target.files));
            target.value = "";
        }
    }

    function uploadOne(pending: PendingFile): Promise<void> {
        return new Promise((resolve) => {
            const formData = new FormData();
            formData.append("file", pending.file);

            const xhr = new XMLHttpRequest();
            xhr.open("POST", "http://localhost:5172/api/storage/upload");
            xhr.withCredentials = true; // sends cookies, equivalent to credentials: 'include'

            xhr.upload.onprogress = (e) => {
                if (e.lengthComputable) {
                    pending.progress = Math.round((e.loaded / e.total) * 100);
                }
            };

            xhr.onload = () => {
                pending.status = xhr.status >= 200 && xhr.status < 300 ? "done" : "error";
                resolve();
            };

            xhr.onerror = () => {
                pending.status = "error";
                resolve();
            };

            pending.status = "uploading";
            xhr.send(formData);
        });
    }

    async function confirmUpload() {
        isUploading = true;

        // sequential to avoid overwhelming the connection; switch to Promise.all for parallel
        for (const pending of pendingFiles) {
            if (pending.status === "pending") {
                await uploadOne(pending);
            }
        }

        isUploading = false;

        const allSucceeded = pendingFiles.every((p) => p.status === "done");
        if (allSucceeded) {
            pendingFiles = [];
        }

        onUploadComplete();
    }

    function cancelAll() {
        pendingFiles = [];
    }
</script>

<div class="flex h-full w-full flex-col gap-3 p-4">
    <div
        role="button"
        tabindex="0"
        class="flex flex-1 flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed transition-colors {isDragging ? 'border-primary bg-muted' : 'border-transparent bg-muted/50'}"
        ondragenter={handleDragEnter}
        ondragover={handleDragOver}
        ondragleave={handleDragLeave}
        ondrop={handleDrop}
        onclick={() => fileInput.click()}
        onkeydown={(e) => e.key === "Enter" && fileInput.click()}
    >
        <UploadIcon class="size-10 text-muted-foreground" />
        <p class="text-sm text-muted-foreground">
            <span class="font-medium text-foreground">Click to select</span> or drop file here
        </p>

        <input bind:this={fileInput} type="file" multiple class="hidden" onchange={handleFileInput} />
    </div>

    {#if pendingFiles.length > 0}
        <div class="flex flex-col gap-2 rounded-xl bg-muted/50 p-3">
            {#each pendingFiles as pending, i (pending.file.name + i)}
                <div class="flex items-center gap-3 rounded-lg bg-background px-3 py-2 text-sm">
                    <FileIcon class="size-4 shrink-0 text-muted-foreground" />
                    <div class="flex-1 overflow-hidden">
                        <p class="truncate font-medium">{pending.file.name}</p>
                        <p class="text-xs text-muted-foreground">{formatFileSize(pending.file.size)}</p>
                        {#if pending.status === "uploading"}
                            <div class="mt-1 h-1.5 w-full overflow-hidden rounded-full bg-muted">
                                <div
                                    class="h-full bg-primary transition-all"
                                    style="width: {pending.progress}%"
                                ></div>
                            </div>
                        {/if}
                    </div>

                    {#if pending.status === "done"}
                        <span class="text-xs text-green-500">Uploaded</span>
                    {:else if pending.status === "error"}
                        <span class="text-xs text-red-500">Error</span>
                    {:else if pending.status === "pending"}
                        <Button size="icon" variant="ghost" class="size-6" onclick={() => removeFile(i)}>
                            <XIcon class="size-3.5" />
                        </Button>
                    {/if}
                </div>
            {/each}

            <div class="flex justify-end gap-2 pt-1">
                <Button variant="outline" size="sm" disabled={isUploading} onclick={cancelAll}>
                    Cancel
                </Button>
                <Button size="sm" disabled={isUploading} onclick={confirmUpload}>
                    {isUploading ? "Uploading..." : `Upload ${pendingFiles.length} file(s)`}
                </Button>
            </div>
        </div>
    {/if}
</div>