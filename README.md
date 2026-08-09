# Nitevault

## Setup

`nix develop` # enter dev shell

`docker compose up -d` # starts redis, postgres, and telegram-bot-api

`dotnet ef migrations add <name>` # generate migration after model changes

`dotnet ef database update` # apply migrations to db

## Routes

Most used backend routes for remembering:
```
GET http://localhost:5172/api/user/{id}
POST http://localhost:5172/api/auth/login {"email", "password"}
POST http://localhost:5172/api/auth/refresh # sends cookies automatically
GET http://localhost:5172/api/storage/file/{file-id}
GET http://localhost:5172/api/storage/download/{file-id}
POST http://localhost:5172/api/storage/upload # multipart-form {"file"}
```

## Bot Routes

```
GET http://localhost:8081/bot{bot-token}/getMe
GET http://localhost:8081/bot{bot-token}/getUpdates # use to discover chat_id after sending a message manually
POST http://localhost:8081/bot{bot-token}/sendMessage {"chat_id", "text"}
POST http://localhost:8081/bot{bot-token}/sendDocument # (Multi-part form with chat_id and document keys)
GET http://localhost:8081/bot{bot-token}/getFile?file_id={file_id}
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

## ⚠️ Security Notes

If a bot token is ever exposed, revoke it immediately via @BotFather → /mybots → API Token → Revoke