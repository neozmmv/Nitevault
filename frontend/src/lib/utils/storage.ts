import type FileItem from "$lib/interfaces/file";

export const fetchFiles = async (): Promise<FileItem[] | undefined> => {
    try {
        const res = await fetch("http://localhost:5172/api/storage/list", { credentials: "include" });
        if (!res.ok) {
            console.error('Failed to fetch files:', res.status);
            return undefined;
        }
        return await res.json();
    } catch (err) {
        console.error('Failed to fetch files:', err);
        return undefined;
    }
}

export async function deleteFile(fileId: string) {
    const res = await fetch(`http://localhost:5172/api/storage/file/${fileId}`, { method: "DELETE", credentials: "include"})
    if(!res.ok) {
        console.error("Failed to delete file: ", res.status)
    }
    return res.status;
}