-- here for remembering, i will add an automated process to remove these invalid tokens
DELETE from "RefreshTokens" rt where rt."ExpiresAt" < NOW() or rt."Revoked" = true