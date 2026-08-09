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
POST http://localhost:8081/bot{bot-token}/sendDocument # (Multi-part form with 'file' key)
GET http://localhost:8081/bot{bot-token}/getFile?file_id={file_id}
```

## Bot Setup (full walkthrough)

### Step 1 — Get api_id / api_hash

Go to [my.telegram.org](https://my.telegram.org), login with your phone number

Click **API Development Tools**

Fill the form with your desired info (app title: `myNitevault`, short name: `nitevault123`, platform: `Other` or `Server`, doesn't matter functionally)

Submit -  you'll get an `api_id` (number) and `api_hash` (string)

Set them in `.env`:
```
TELEGRAM_API_ID=...
TELEGRAM_API_HASH=...
```

---

### Step 2 — Create the bot

Go to `@BotFather` on Telegram

Send `/newbot`

Give it a display name (e.g. `Nitevault Storage`)

Give it a username ending in `bot` (e.g. `nitevault_storage_bot`)

BotFather replies with a token — set it in `.env`:
```
BOT_TOKEN=...
```

---

### Step 3 — Start the self-hosted bot API server

`docker compose up -d telegram-bot-api`

Check logs to confirm it started cleanly (no `check_required_env` errors):
```
docker compose logs telegram-bot-api
```

Confirm the server is reachable and the bot is recognized:
```
curl http://localhost:8081/bot{BOT_TOKEN}/getMe
```
Should return `"ok":true` with the bot's info.

---

### Step 4 — Create the storage channel and add the bot

Create a new Telegram channel (can be private) — this is where all uploaded files will actually live

Add the bot as an **administrator** of the channel (Channel settings → Administrators → Add Admin → search bot username)

This step matters: without admin rights, the bot can't post to the channel, and later can't delete messages older than 48h either

---

### Step 5 — Get the channel's chat_id

Send any message in the channel manually (from your own account)

Call:
```
curl http://localhost:8081/bot{BOT_TOKEN}/getUpdates
```

Look for `"chat":{"id": -100...}` in the response (channel posts appear under `channel_post`, not `message`)

That negative number is the `chat_id`, set it in `.env`:
```
CHAT_ID=-100...
```

Note: `getUpdates` only returns updates it hasn't returned before (long-polling semantics), if the response comes back empty, send a fresh message in the channel and call it again.

---

### Step 6 — Validate the full pipeline

Send a test message:
```
curl -X POST http://localhost:8081/bot{BOT_TOKEN}/sendMessage \
  -H "Content-Type: application/json" \
  -d '{"chat_id": {CHAT_ID}, "text": "Nitevault test"}'
```

Send a test file:
```
curl -X POST http://localhost:8081/bot{BOT_TOKEN}/sendDocument \
  -F "chat_id={CHAT_ID}" \
  -F "document=@/path/to/test/file"
```

If both show up in the channel, the bot setup is fully working end to end.

---

### Step 7 — Bring the whole stack up

`docker compose up -d` (or `docker compose watch` during active development)

This starts `postgres`, `redis`, `telegram-bot-api`, `bot-api-cleanup` (daily cache prune), and the `api` service. The API reads `BOT_TOKEN` / `CHAT_ID` / `TELEGRAM_API_ID` / `TELEGRAM_API_HASH` from the same `.env`.

## ⚠️ Security Notes

- If a bot token is ever exposed, revoke it immediately via @BotFather → /mybots → API Token → Revoke
- The bot's `api_id`/`api_hash` is tied to your personal Telegram account (via my.telegram.org), consider using a secondary phone number for this if running at any real volume, to isolate risk from your main account