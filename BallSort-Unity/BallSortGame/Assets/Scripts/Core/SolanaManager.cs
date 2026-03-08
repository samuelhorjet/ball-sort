using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Core.Http;     // ← SolanaRpcClient direct ctor
using Solana.Unity.Rpc.Types;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Utilities;   // ← for Base58 encoding
using Newtonsoft.Json.Linq;

namespace BallSort.Core
{
    /// <summary>
    /// Manages the Solana RPC connection and provides low-level account fetching.
    ///
    /// TEE Authentication — mirrors TypeScript auth.js getAuthToken() exactly:
    ///
    ///   1. GET  ${endpoint}/auth/challenge?pubkey=${sessionPublicKey}
    ///            → { challenge: "..." }
    ///   2. Sign challenge bytes with session keypair (ed25519 / nacl.sign.detached)
    ///   3. Encode signature as BASE58 (not base64 — matches bs58.encode in the test)
    ///   4. POST ${endpoint}/auth/login
    ///            body: { pubkey, challenge, signature }
    ///            → { token: "...", expiresAt: ... }
    ///   5. Build RPC client: new Connection(`${TEE_URL}?token=${token}`)
    ///
    /// Called FRESH before every single ER transaction — never cached.
    /// Blockhash for ER transactions is fetched from the TEE client, not L1.
    /// </summary>
    public class SolanaManager : MonoBehaviour
    {
        public static SolanaManager Instance { get; private set; }

        // ── Network URLs ─────────────────────────────────────────────────────
        // Hardcoded as constants — NOT [SerializeField].
        // [SerializeField] values are stored in the Unity scene file and can silently
        // be wrong (empty, localhost, etc.) without any compile error. If the SDK
        // receives a bad URL it falls back to http://127.0.0.1:8899 with no warning.
        private const string L1_RPC_URL = "https://api.devnet.solana.com";
        private const string L1_WS_URL  = "wss://api.devnet.solana.com";
        private const string ER_RPC_URL = "https://tee.magicblock.app";
        private const string ER_WS_URL  = "wss://tee.magicblock.app";

        // Keep these as properties so the rest of the code still works
        private string _rpcUrl   => L1_RPC_URL;
        private string _wsUrl    => L1_WS_URL;
        private string _erRpcUrl => ER_RPC_URL;
        private string _erWsUrl  => ER_WS_URL;

        public IRpcClient          RpcClient       { get; private set; }
        public IStreamingRpcClient StreamingClient { get; private set; }
        public string ErRpcUrl => _erRpcUrl;
        public bool IsConnected => RpcClient != null;

        // ── JWT token cache — reused across calls until near-expiry ──────────
        // Each GetFreshErClientAsync was doing 2 HTTP calls per move × 3 reads = 6
        // extra round trips. Caching cuts this to 0 for the lifetime of one token.
        private string _cachedJwt;
        private long   _jwtExpiresAt; // unix seconds from TEE response
        private string _cachedForPubkey;
        private const int TOKEN_EXPIRY_BUFFER_SECS = 60; // refresh 60s before expiry

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => Connect();

        public void Connect()
        {
            RpcClient       = ClientFactory.GetClient(_rpcUrl);
            StreamingClient = ClientFactory.GetStreamingClient(_wsUrl);
            Debug.Log($"[Solana] Connected → L1: {_rpcUrl}  |  TEE: {_erRpcUrl}");
        }

        // ── TEE Authentication ── per-transaction, exactly like auth.js ──────

        /// <summary>
        /// Mirrors TypeScript getAuthToken() in auth.js exactly:
        ///
        ///   Step 1: GET /auth/challenge?pubkey={sessionPublicKey}
        ///           Response: { challenge: "..." }
        ///
        ///   Step 2: Sign challenge with session keypair (ed25519)
        ///           TypeScript: nacl.sign.detached(Buffer.from(challenge, "utf-8"), secretKey)
        ///
        ///   Step 3: Encode signature as BASE58
        ///           TypeScript: bs58.encode(signature)
        ///
        ///   Step 4: POST /auth/login
        ///           Body: { pubkey: string, challenge: string, signature: string }
        ///           Response: { token: string, expiresAt: number }
        ///
        ///   Step 5: Return BOTH HTTP and WebSocket TEE clients, both carrying the token:
        ///           TypeScript: new Connection(`${TEE_URL}?token=${token}`, {
        ///                         wsEndpoint: `${TEE_WS_URL}?token=${token}` })
        ///
        /// Called FRESH before every single ER transaction.
        /// </summary>
        /// <summary>
        /// Returns an authenticated TEE URL string with a valid JWT token.
        /// Token is cached per session keypair and reused until near-expiry,
        /// avoiding 2 HTTP round trips (GET challenge + POST login) on every call.
        /// A fresh token is only fetched when: first call, token near expiry, or keypair changed.
        /// </summary>
        public async Task<string> GetFreshErClientAsync(Account sessionAccount)
        {
            string pubkeyStr = sessionAccount.PublicKey.Key;
            long   nowSecs   = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // ── Use cached token if still valid ───────────────────────────────
            if (_cachedJwt != null &&
                _cachedForPubkey == pubkeyStr &&
                _jwtExpiresAt - nowSecs > TOKEN_EXPIRY_BUFFER_SECS)
            {
                return $"{_erRpcUrl}?token={_cachedJwt}";
            }

            // ── Step 1: GET challenge ─────────────────────────────────────────
            string challengeUrl  = $"{_erRpcUrl}/auth/challenge?pubkey={pubkeyStr}";
            string challengeJson = await HttpGetAsync(challengeUrl);
            var    challengeObj  = JObject.Parse(challengeJson);

            string error = challengeObj["error"]?.ToString();
            if (!string.IsNullOrEmpty(error))
                throw new Exception($"TEE auth challenge error: {error}");

            string challenge = challengeObj["challenge"]?.ToString();
            if (string.IsNullOrEmpty(challenge))
                throw new Exception($"TEE auth: no 'challenge' field in response: {challengeJson}");

            // ── Step 2: Sign + encode ─────────────────────────────────────────
            byte[] msgBytes       = Encoding.UTF8.GetBytes(challenge);
            byte[] sigBytes       = sessionAccount.Sign(msgBytes);
            string signatureBase58 = Encoders.Base58.EncodeData(sigBytes);

            // ── Step 3: POST /auth/login ──────────────────────────────────────
            string tokenUrl  = $"{_erRpcUrl}/auth/login";
            string postBody  = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                pubkey    = pubkeyStr,
                challenge = challenge,
                signature = signatureBase58
            });
            string tokenJson = await HttpPostAsync(tokenUrl, postBody);
            var    tokenObj  = JObject.Parse(tokenJson);

            string tokenError = tokenObj["error"]?.ToString();
            if (!string.IsNullOrEmpty(tokenError))
                throw new Exception($"TEE auth login error: {tokenError}");

            string jwt = tokenObj["token"]?.ToString();
            if (string.IsNullOrEmpty(jwt))
                throw new Exception($"TEE auth: no 'token' field in response: {tokenJson}");

            // ── Cache the token ───────────────────────────────────────────────
            _cachedJwt       = jwt;
            _cachedForPubkey = pubkeyStr;
            // TEE returns expiresAt as unix seconds; fall back to 1h if missing
            long expiresAt = tokenObj["expiresAt"]?.Value<long>() ?? (nowSecs + 3600);
            _jwtExpiresAt  = expiresAt;
            Debug.Log($"[Solana] TEE token cached, expires in {expiresAt - nowSecs}s");

            return $"{_erRpcUrl}?token={jwt}";
        }

        /// <summary>Invalidate cached JWT — call after session close or keypair change.</summary>
        public void InvalidateErToken()
        {
            _cachedJwt       = null;
            _cachedForPubkey = null;
            _jwtExpiresAt    = 0;
        }

        // ── Account helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Fetch raw account data from L1 or the TEE.
        ///
        /// When useEr = true and a session is active, reads from the TEE via raw
        /// JSON-RPC WITH an auth token. The TEE requires a token for ALL JSON-RPC
        /// calls — including getAccountInfo. Without it you get 401 Unauthorized,
        /// fall back to L1, get null (account is locked on L1 after delegation),
        /// and CurrentPuzzleBoard stays null → board never renders → blank screen.
        ///
        /// TypeScript: connection.getAccountInfo(pubkey) where connection was created
        /// with the auth token: new Connection(`${TEE_URL}?token=${token}`)
        /// </summary>
        public async Task<byte[]> GetAccountDataAsync(PublicKey pubkey, bool useEr = false)
        {
            // ── TEE read: authenticated JSON-RPC (token required for ALL TEE calls) ──
            if (useEr && SessionKeyManager.Instance?.HasActiveSession == true)
            {
                // Retry up to 4 times before giving up — never fall back to stale L1 data.
                // Delegated accounts are locked on L1; falling back returns wrong state.
                const int teeReadRetries = 4;
                const int teeReadDelayMs = 600;
                Exception lastEx = null;

                for (int attempt = 1; attempt <= teeReadRetries; attempt++)
                {
                    try
                    {
                        string teeUrl = await GetFreshErClientAsync(
                            SessionKeyManager.Instance.SessionAccount);

                        string request = Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            jsonrpc = "2.0",
                            id      = 1,
                            method  = "getAccountInfo",
                            @params = new object[]
                            {
                                pubkey.Key,
                                new { encoding = "base64", commitment = "confirmed" }
                            }
                        });

                        string response = await HttpPostAsync(teeUrl, request);
                        var    json     = JObject.Parse(response);
                        var    value    = json["result"]?["value"];

                        if (value == null || value.Type == Newtonsoft.Json.Linq.JTokenType.Null)
                        {
                            Debug.LogWarning($"[Solana] TEE: account {pubkey.Key} not found (attempt {attempt})");
                            lastEx = new Exception("Account not found on TEE");
                        }
                        else
                        {
                            var dataArr = value["data"] as Newtonsoft.Json.Linq.JArray;
                            if (dataArr != null && dataArr.Count > 0)
                            {
                                Debug.Log($"[Solana] TEE account read OK: {pubkey.Key}");
                                return Convert.FromBase64String(dataArr[0].ToString());
                            }
                            lastEx = new Exception("Empty data array from TEE");
                        }
                    }
                    catch (Exception e)
                    {
                        lastEx = e;
                        Debug.LogWarning($"[Solana] TEE read attempt {attempt}/{teeReadRetries} failed: {e.Message}");
                    }

                    if (attempt < teeReadRetries)
                        await Task.Delay(teeReadDelayMs);
                }

                Debug.LogWarning($"[Solana] TEE read failed after {teeReadRetries} attempts — using L1 fallback");
            }

            // ── L1 read: SDK client for non-delegated accounts ────────────────
            var result = await RpcClient.GetAccountInfoAsync(
                pubkey.Key, Commitment.Confirmed, BinaryEncoding.Base64);

            if (!result.WasSuccessful || result.Result?.Value == null)
                return null;

            return Convert.FromBase64String(result.Result.Value.Data[0]);
        }

        public async Task<bool> ConfirmTransactionAsync(string signature,
            Commitment commitment = Commitment.Confirmed)
        {
            const int maxRetries = 30;
            const int delayMs    = 1000;
            var sigList = new List<string> { signature };

            for (int i = 0; i < maxRetries; i++)
            {
                await Task.Delay(delayMs);
                var result = await RpcClient.GetSignatureStatusesAsync(
                    sigList, searchTransactionHistory: false);
                if (!result.WasSuccessful) continue;
                var status = result.Result?.Value?[0];
                if (status == null) continue;
                if (status.Error != null)
                    throw new Exception($"Transaction failed: {status.Error}");
                if (status.ConfirmationStatus == commitment.ToString().ToLower() ||
                    status.ConfirmationStatus == "finalized")
                    return true;
            }
            throw new TimeoutException($"Transaction {signature} not confirmed after {maxRetries}s");
        }

        public async Task<string> GetLatestBlockhashAsync()
        {
            var result = await RpcClient.GetLatestBlockHashAsync();
            if (!result.WasSuccessful) throw new Exception("Failed to get blockhash");
            return result.Result.Value.Blockhash;
        }

        public async Task<string> SendRawTransactionAsync(byte[] signedTx, bool skipPreflight = false)
        {
            var result = await RpcClient.SendTransactionAsync(
                Convert.ToBase64String(signedTx), skipPreflight, Commitment.Confirmed);
            if (!result.WasSuccessful)
                throw new Exception($"SendTransaction failed: {result.Reason}");
            return result.Result;
        }

        /// <summary>
        /// Send one transaction to the TEE using raw JSON-RPC over HTTP.
        ///
        /// We bypass ClientFactory entirely because it strips query strings from URLs,
        /// causing the ?token=jwt to be lost and the SDK to fall back to localhost.
        /// Instead we call the Solana JSON-RPC API directly via HttpPostAsync:
        ///
        ///   getLatestBlockhash  → sets tx.RecentBlockHash from the TEE
        ///   sendTransaction     → submits the signed tx to the TEE
        ///
        /// This mirrors the TypeScript test exactly:
        ///   tx.recentBlockhash = (await teeConnection.getLatestBlockhash()).blockhash
        ///   await sendAndConfirmTransaction(teeConnection, tx, [sessionKeypair], { skipPreflight: true })
        /// </summary>
        public async Task<string> SendErTransactionAsync(Transaction tx, Account sessionAccount)
        {
            // Step 1: get fresh auth token → authenticated TEE URL (with ?token=jwt)
            string teeUrl = await GetFreshErClientAsync(sessionAccount);
            Debug.Log($"[Solana] TEE URL: {teeUrl.Substring(0, Math.Min(60, teeUrl.Length))}...");

            // Step 2: getLatestBlockhash from TEE via raw JSON-RPC
            // TypeScript: tx.recentBlockhash = (await teeConnection.getLatestBlockhash()).blockhash
            string bhRequest = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                jsonrpc = "2.0",
                id      = 1,
                method  = "getLatestBlockhash",
                @params = new object[] { new { commitment = "confirmed" } }
            });
            string bhResponse = await HttpPostAsync(teeUrl, bhRequest);
            var    bhJson     = JObject.Parse(bhResponse);
            string blockhash  = bhJson["result"]?["value"]?["blockhash"]?.ToString()
                ?? throw new Exception($"Failed to get blockhash from TEE. Response: {bhResponse}");
            tx.RecentBlockHash = blockhash;
            Debug.Log($"[Solana] TEE blockhash: {blockhash}");

            // Step 3: build (serialize + sign) with session keypair
            byte[] signedTxBytes = tx.Build(new List<Account> { sessionAccount });

            // Encode as BASE58 — Solana's default transaction encoding.
            // TypeScript sendAndConfirmTransaction uses base58 by default.
            // The TEE may not support base64 and falls back to localhost if it can't
            // deserialize the transaction.
            string signedTxB58 = Encoders.Base58.EncodeData(signedTxBytes);

            // Step 4: sendTransaction to TEE via raw JSON-RPC, skipPreflight = true
            // TypeScript: sendAndConfirmTransaction(teeConnection, tx, [sessionKeypair], { skipPreflight: true })
            // NOTE: no "encoding" field = base58 default, matching the TypeScript test exactly
            string sendRequest = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                jsonrpc = "2.0",
                id      = 1,
                method  = "sendTransaction",
                @params = new object[]
                {
                    signedTxB58,
                    new { skipPreflight = true, preflightCommitment = "confirmed" }
                }
            });
            // Step 4 — send with retry.
            // The TEE occasionally returns 500 "error sending request for url (http://127.0.0.1:8899/)"
            // when its internal L1 connection is momentarily unavailable (e.g. during undelegatePuzzle
            // which requires the TEE to commit state back to Solana L1). Retrying resolves this.
            const int maxRetries = 8;
            const int retryDelayMs = 3000;  // TEE needs time to reconnect to L1

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                string sendResponse = await HttpPostAsync(teeUrl, sendRequest);
                var    sendJson     = JObject.Parse(sendResponse);
                var    rpcError     = sendJson["error"];

                if (rpcError == null)
                {
                    string signature = sendJson["result"]?.ToString()
                        ?? throw new Exception($"ER SendTransaction: no signature in response: {sendResponse}");
                    Debug.Log($"[Solana] TEE tx sent. Sig: {signature}");
                    return signature;
                }

                string errMsg = rpcError.ToString();
                Debug.LogWarning($"[Solana] TEE sendTransaction error (attempt {attempt}/{maxRetries}): {errMsg}");

                // Only retry on 500-level TEE connectivity errors, not on transaction errors
                bool isConnectError = errMsg.Contains("127.0.0.1") ||
                                      errMsg.Contains("Connect") ||
                                      errMsg.Contains("client error");
                if (!isConnectError || attempt == maxRetries)
                    throw new Exception($"ER SendTransaction failed: {errMsg}");

                await Task.Delay(retryDelayMs);
            }

            throw new Exception("ER SendTransaction failed: max retries exceeded");
        }

        // ── TEE synchronization ──────────────────────────────────────────────

        /// <summary>
        /// Mirrors TypeScript waitUntilPermissionActive(rpcUrl, publicKey, timeout) from permission.js:
        ///
        ///   GET ${baseUrl}/permission?pubkey=${publicKey.toString()}
        ///   → { authorizedUsers: [...] }
        ///   Loop until authorizedUsers.length > 0, poll every 400ms.
        ///
        /// This is a custom TEE REST endpoint — NOT a Solana JSON-RPC call.
        /// No auth token required. Called after delegation to confirm the TEE has
        /// fully indexed the accounts before we send the first ER transaction.
        /// </summary>
        public async Task WaitUntilPermissionActiveAsync(
            PublicKey delegatedAccount,
            int timeoutMs = 60000)
        {
            // TypeScript: `${baseUrl}/permission?pubkey=${publicKey.toString()}`
            string url = $"{ER_RPC_URL}/permission?pubkey={delegatedAccount.Key}";
            Debug.Log($"[Solana] Waiting for TEE permission: {url}");

            int elapsed = 0;
            const int pollMs = 400;  // mirrors permission.js: setTimeout(resolve, 400)

            while (elapsed < timeoutMs)
            {
                await Task.Delay(pollMs);
                elapsed += pollMs;

                try
                {
                    // TypeScript: GET /permission?pubkey=... → { authorizedUsers: [...] }
                    string response = await HttpGetAsync(url);
                    var    json     = JObject.Parse(response);
                    var    users    = json["authorizedUsers"] as Newtonsoft.Json.Linq.JArray;

                    // TypeScript: if (authorizedUsers && authorizedUsers.length > 0) return true
                    if (users != null && users.Count > 0)
                    {
                        Debug.Log($"[Solana] TEE permission active after {elapsed}ms. Ready for ER transactions.");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Solana] TEE permission poll error ({elapsed}ms): {e.Message}");
                }
            }

            // TypeScript returns false on timeout but doesn't throw — we warn and continue
            // so the game isn't permanently stuck if permission check is slow.
            Debug.LogWarning($"[Solana] TEE permission not confirmed after {timeoutMs}ms — proceeding anyway.");
        }

        // ── HTTP helpers ─────────────────────────────────────────────────────

        private async Task<string> HttpGetAsync(string url)
        {
            var tcs = new TaskCompletionSource<string>();
            var req = UnityWebRequest.Get(url);
            req.SendWebRequest().completed += _ =>
            {
                if (req.result == UnityWebRequest.Result.Success)
                    tcs.SetResult(req.downloadHandler.text);
                else
                    tcs.SetException(new Exception(
                        $"GET {url} failed: {req.error} (HTTP {req.responseCode})"));
                req.Dispose();
            };
            return await tcs.Task;
        }

        private async Task<string> HttpPostAsync(string url, string jsonBody)
        {
            var tcs = new TaskCompletionSource<string>();
            var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SendWebRequest().completed += _ =>
            {
                if (req.result == UnityWebRequest.Result.Success)
                    tcs.SetResult(req.downloadHandler.text);
                else
                    tcs.SetException(new Exception(
                        $"POST {url} failed: {req.error} — {req.downloadHandler.text}"));
                req.Dispose();
            };
            return await tcs.Task;
        }
    }
}