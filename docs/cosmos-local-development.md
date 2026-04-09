# Cosmos DB: local development changes and Azure CLI role assignment

This note records **code and configuration** used for local API development against Azure Cosmos DB for NoSQL, and the **`az` command** to grant **data-plane RBAC** to your user when **local (key) auth is disabled** on the account.

---

## 1. Code change (API)

**File:** `src/api/Infrastructure/Configuration/CosmosServiceCollectionExtensions.cs`

**Behavior:**

- **`AZURE_COSMOS_ENDPOINT`** must be set or the Cosmos client is not registered (unchanged).
- **`AZURE_COSMOS_KEY`** (optional):
  - If **set** (non-whitespace): the app builds `CosmosClient` with **endpoint + account key** (classic key-based auth).
  - If **not set**: the app uses **`DefaultAzureCredential`** (Azure-hosted: managed identity; **local dev:** typically **Azure CLI** after `az login`).

**Why both paths exist:**

- Many Cosmos accounts have **local authorization disabled** (keys disallowed). Then only **Microsoft Entra ID + Cosmos data-plane RBAC** works; you **omit** `AZURE_COSMOS_KEY` and sign in with `az login`.
- If your account **allows** keys, you can set `AZURE_COSMOS_KEY` in user-secrets for local runs without CLI identity.

**No changes** were made to repository registration, health checks, or container names in `Program.cs` for this work item.

---

## 2. Configuration keys

| Key | Required | Purpose |
|-----|----------|---------|
| `AZURE_COSMOS_ENDPOINT` | Yes (for Cosmos) | Account URI, e.g. `https://<account>.documents.azure.com:443/` |
| `AZURE_COSMOS_KEY` | No | Primary/secondary key; **only** if local auth is enabled on the account |
| `AZURE_COSMOS_DATABASE_NAME` | No | Default: `App` |
| `AZURE_COSMOS_CONTAINER_NAME` | No | Default: `Items` |

**Local secrets:** use **dotnet user-secrets** (see [secrets-management.md](./secrets-management.md)); do not commit keys or endpoints with secrets to git.

Example (Development):

```bash
cd src/api
dotnet user-secrets set "AZURE_COSMOS_ENDPOINT" "https://YOUR_ACCOUNT.documents.azure.com:443/"
# Optional, only if account allows keys:
# dotnet user-secrets set "AZURE_COSMOS_KEY" "<key>"
```

---

## 3. Cosmos DB Built-in Data Contributor (data-plane RBAC)

Subscription **Owner/Contributor** on the resource group is **not** enough. The signed-in principal needs a **Cosmos DB SQL API role assignment** on the account (e.g. **Cosmos DB Built-in Data Contributor**) so the SDK can call data-plane operations used by the client and readiness check.

Built-in role definition GUID for **Data Contributor** (SQL API) is:

`00000000-0000-0000-0000-000000000002`

### 3.1 Resolve your principal ID (the user or service principal that runs the API locally)

```bash
az ad signed-in-user show --query id -o tsv
```

Use that value as `PRINCIPAL_ID` below. For an app registration / service principal, use its **object (principal) ID**, not the application ID.

### 3.2 (Optional) List role definitions on the account

```bash
az cosmosdb sql role definition list \
  --account-name "<COSMOS_ACCOUNT_NAME>" \
  --resource-group "<RESOURCE_GROUP>" \
  --query "[].{roleName:roleName, id:id}" \
  -o table
```

### 3.3 Create the role assignment (account scope)

Replace placeholders with your Cosmos account name, resource group, and principal ID.

```bash
PRINCIPAL_ID="$(az ad signed-in-user show --query id -o tsv)"

az cosmosdb sql role assignment create \
  --resource-group "<RESOURCE_GROUP>" \
  --account-name "<COSMOS_ACCOUNT_NAME>" \
  --role-definition-id 00000000-0000-0000-0000-000000000002 \
  --principal-id "$PRINCIPAL_ID" \
  --scope "/"
```

**`--scope "/"`** means the assignment applies at the **database account** scope (sufficient for typical dev). Narrower scopes (database/container) are possible if your organization requires least privilege.

**Reader-only local access:** use role definition **`00000000-0000-0000-0000-000000000001`** (Cosmos DB Built-in Data Reader) instead of `...0002`.

### 3.4 Example values used in one sandbox cohort (illustrative)

These are **environment-specific**; use your own names from the portal or `az cosmosdb list`.

| Placeholder | Example |
|---------------|---------|
| `<RESOURCE_GROUP>` | `rg-CsharpCosmosDev-Cohort1` |
| `<COSMOS_ACCOUNT_NAME>` | `cosmos-5t5z5fylmznra` |

---

## 4. Readiness check and cold start

`/health/ready` uses a **short timeout** when reading the database. The **first** request after startup or RBAC changes can exceed that window; a **second** request often succeeds once the client is warm.

---

## 5. Related docs

- [secrets-management.md](./secrets-management.md) — user-secrets and Key Vault
- [health-checks.md](./health-checks.md) — liveness vs readiness
