# Nitevault

`nix develop`
`dotnet ef migrations add name`
`dotnet ef database update`

Telegram based storage

# Routes

Most used backend routes for remembering:
```
GET http://localhost:5172/api/user/{id}
POST http://localhost:5172/api/auth/login {"email", "password"}
POST http://localhost:5172/api/auth/refresh # sends cookies automatically

```