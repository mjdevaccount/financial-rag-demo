# Financial RAG Demo

A production-grade **Retrieval-Augmented Generation (RAG)** system built on the Microsoft Azure stack, designed to answer questions grounded in financial documents such as Federal Reserve meeting minutes and SEC filings.

Built with **C# / .NET 8**, **Azure OpenAI**, and **Azure AI Search**.

---

## Architecture

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────────┐
│   PDF Documents │────▶│  Ingestion Pipeline   │────▶│  Azure AI Search    │
│  (Fed Minutes,  │     │  - PdfPig extraction  │     │  (Vector Index)     │
│   SEC Filings)  │     │  - Text chunking      │     │                     │
└─────────────────┘     │  - Embedding via      │     └────────┬────────────┘
                        │    Azure OpenAI        │              │
                        └──────────────────────┘              │
                                                               │ Vector Search
┌─────────────────┐     ┌──────────────────────┐              │
│   REST Client   │────▶│   RagDemo.Api         │◀────────────┘
│  (Swagger UI)   │     │  - /api/rag/ask       │
└─────────────────┘     │  - Source citations   │     ┌─────────────────────┐
                        │  - Grounded answers   │────▶│   Azure OpenAI      │
                        └──────────────────────┘     │   GPT-4o-mini       │
                                                      └─────────────────────┘
```

---

## Features

- **PDF Ingestion Pipeline** — Bulk ingest financial PDFs with automatic text extraction, chunking (512 tokens, 50 token overlap), and vector embedding
- **Hybrid Vector Search** — Azure AI Search HNSW vector index for semantic retrieval
- **Grounded Generation** — GPT-4o-mini answers are constrained to retrieved context, minimizing hallucination
- **Source Citations** — Every response includes the source documents used to generate the answer
- **REST API** — Clean ASP.NET Core Web API with Swagger UI for interactive exploration

---

## Tech Stack

| Component | Technology |
|---|---|
| Language | C# / .NET 8 |
| LLM | Azure OpenAI (GPT-4o-mini) |
| Embeddings | Azure OpenAI (text-embedding-3-small) |
| Vector Store | Azure AI Search (HNSW index) |
| PDF Extraction | PdfPig |
| API Framework | ASP.NET Core Web API |
| Configuration | .NET User Secrets |

---

## Project Structure

```
RagDemo.sln
├── RagDemo.Core          # Shared services and models
│   ├── DocumentChunk.cs       # Search index model
│   ├── EmbeddingService.cs    # Azure OpenAI embedding client
│   ├── SearchIndexService.cs  # Index creation and management
│   ├── DocumentIngester.cs    # Chunking and upsert pipeline
│   ├── PdfIngester.cs         # PDF extraction and ingestion
│   ├── RetrievalService.cs    # Vector search retrieval
│   └── ChatService.cs         # Grounded generation
├── RagDemo.Ingestion     # Console app for ingesting documents
└── RagDemo.Api           # ASP.NET Core REST API
```

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Azure subscription
- Azure OpenAI resource with deployments:
  - `text-embedding-3-small`
  - `gpt-4o-mini`
- Azure AI Search resource (Free tier sufficient)

### Configuration

This project uses .NET User Secrets to keep credentials out of source control.

In both `RagDemo.Ingestion` and `RagDemo.Api`, run:

```bash
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR_RESOURCE.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:Key" "YOUR_KEY"
dotnet user-secrets set "AzureOpenAI:EmbeddingDeployment" "text-embedding-3-small"
dotnet user-secrets set "AzureOpenAI:ChatDeployment" "gpt-4o-mini"
dotnet user-secrets set "AzureSearch:Endpoint" "https://YOUR_SEARCH.search.windows.net"
dotnet user-secrets set "AzureSearch:Key" "YOUR_KEY"
dotnet user-secrets set "AzureSearch:IndexName" "financial-docs"
```

### Ingest Documents

1. Create a folder for your PDFs (e.g. `C:\RagDocs`)
2. Drop in financial PDFs — Federal Reserve meeting minutes work great (available free at [federalreserve.gov](https://www.federalreserve.gov/monetarypolicy/fomccalendars.htm))
3. Update the folder path in `RagDemo.Ingestion/Program.cs`
4. Run `RagDemo.Ingestion` — it will create the index and ingest all PDFs automatically

### Run the API

Set `RagDemo.Api` as the startup project and run. Navigate to:

```
https://localhost:{port}/swagger
```

### Example Request

```http
POST /api/rag/ask
Content-Type: application/json

{
  "question": "What did the Fed decide about interest rates and how did markets react?"
}
```

### Example Response

```json
{
  "answer": "The Federal Reserve maintained the target range for the federal funds rate at 3½ to 3¾ percent, citing solid economic expansion while acknowledging elevated inflation. Two members dissented, advocating for a lower target range. Markets responded with volatility as investors repriced rate expectations.",
  "sources": [
    "fomcminutes20260128"
  ]
}
```

---

## How It Works

**Ingestion:**
1. PDFs are extracted to plain text using PdfPig
2. Text is split into overlapping chunks (512 words, 50 word overlap)
3. Each chunk is embedded using `text-embedding-3-small` (1536 dimensions)
4. Chunks and embeddings are upserted into Azure AI Search

**Retrieval + Generation:**
1. The user's question is embedded using the same model
2. Azure AI Search performs a k-nearest-neighbor vector search (k=3)
3. The top matching chunks are injected into the system prompt
4. GPT-4o-mini generates an answer grounded strictly in the retrieved context
5. Source document names are returned alongside the answer

---

## License

MIT
