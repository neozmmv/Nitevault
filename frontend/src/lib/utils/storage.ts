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
    try {
        const res = await fetch(`http://localhost:5172/api/storage/file/${fileId}`, { method: "DELETE", credentials: "include"})
        return res.ok;
    } catch (err) {
        console.error("Failed to delete file:", err);
        return false;
    }
}

export async function getDownloadToken(fileId: string): Promise<string | undefined> {
    try {
        const res = await fetch(`http://localhost:5172/api/storage/generateToken/${fileId}`, {
            credentials: "include"
        });

        if (!res.ok) {
            console.error("Failed to generate download token:", res.status);
            return undefined;
        }

        const { token } = await res.json();
        return token;
    } catch (err) {
        console.error("Failed to generate download token:", err);
        return undefined;
    }
}