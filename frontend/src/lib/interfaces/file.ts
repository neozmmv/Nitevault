export default interface FileItem {
    id: string;
    userId: string;
    folderId: string | null;
    originalFileName: string;
    contentType: string;
    totalSize: number;
    createdAt: string;
    parts: unknown[];
}