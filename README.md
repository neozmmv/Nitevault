# Nitevault

`nix develop`
`dotnet ef migrations add name`
`dotnet ef database update`

Telegram based storage

## Routes

Most used backend routes for remembering:
```
GET http://localhost:5172/api/user/{id}
POST http://localhost:5172/api/auth/login {"email", "password"}
POST http://localhost:5172/api/auth/refresh # sends cookies automatically

```

## Bot Routes

```
http://localhost:8081/bot{bot-token}/getMe
http://localhost:8081/bot{bot-token}/getUpdates
http://localhost:8081/bot{bot-token}/sendMessage
```

## Bot Setup

### Step 1

Go to [Telegram](https://my.telegram.org), login with your phone number

Click API Development Tools

Fill the form with your desired info (Nitevault...)

Get the api_id and api_hash

Set them on .env as specified on .env.local

---

### Step 2

Go to @BotFather

/newBot

Put your bot name

BotFather will give you a token

Set the token as specified on .env.local