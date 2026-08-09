export default interface User {
    id: string,
    email: string,
    name: string,
    createdAt: string, // new Date(user.createdAt)
    active: boolean
}