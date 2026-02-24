# C# HTTP/1.1 Server

This repository contains my solution to the
["Build Your Own HTTP server" challenge](https://app.codecrafters.io/courses/http-server/overview).

Built a concurrent HTTP/1.1 server with incremental request parsing, routing,
and persistent connections.

Designed a modular request-response pipeline with dependency injection and
negotiated compression.

## What I have done

- Implemented a TCP server in [`src/HttpServer.cs`](src/HttpServer.cs) that:
  - listens on port `4221`,
  - accepts clients concurrently (`Task.Run` per connection),
  - resolves a scoped request pipeline per client.
- Added connection handling in
  [`src/Services/ClientConnectionHandler.cs`](src/Services/ClientConnectionHandler.cs)
  to:
  - keep HTTP/1.1 connections open for multiple requests,
  - process complete requests as soon as they are available in the buffer,
  - close on `Connection: close`.
- Built incremental request parsing in
  [`src/Services/RequestReader.cs`](src/Services/RequestReader.cs) with:
  - stream reads into a rolling `StringBuilder` buffer,
  - request boundary detection using `\r\n\r\n`,
  - `Content-Length`-aware body framing.
- Implemented HTTP models in
  [`src/Models/HttpRequest.cs`](src/Models/HttpRequest.cs) and
  [`src/Models/HttpResponse.cs`](src/Models/HttpResponse.cs) for:
  - request line + header parsing,
  - response line/header/body serialization to bytes.
- Added routing and handler dispatch with
  [`src/Routing/RouteMatcher.cs`](src/Routing/RouteMatcher.cs) and
  [`src/Services/ResponseParser.cs`](src/Services/ResponseParser.cs):
  - `/` -> root handler,
  - `/echo/{message}` -> text echo,
  - `/user-agent` -> reflected `User-Agent`,
  - `/files/{filename}` -> file read/write.
- Added compression negotiation in
  [`src/Services/CompressionService.cs`](src/Services/CompressionService.cs)
  for `br`, `gzip`, and `deflate` based on `Accept-Encoding`.
- Centralized response header construction in
  [`src/Services/ResponseHeaderBuilder.cs`](src/Services/ResponseHeaderBuilder.cs)
  for consistent `Content-Type`, `Content-Length`, and `Content-Encoding`.
- Implemented file endpoint safety in
  [`src/Handlers/FileHandler.cs`](src/Handlers/FileHandler.cs):
  - configurable base directory via `--directory`,
  - normalized path checks to block directory traversal.

## Architecture choices

- Used `ServerBuilder` + `Microsoft.Extensions.DependencyInjection` for a
  modular pipeline:
  - singleton routing/compression/header services,
  - keyed scoped handlers for route-specific behavior,
  - scoped parser/reader/connection services per client.
- Separated concerns across layers:
  - socket accept loop (`HttpServer`),
  - connection and stream lifecycle (`ClientConnectionHandler`),
  - request framing (`RequestReader`),
  - route resolution + handler dispatch (`ResponseParser` + handlers),
  - wire-format response serialization (`HttpResponse`).
- Reused a shared header/compression flow so text and file endpoints can apply
  consistent response semantics.

## What I have learnt

- HTTP request framing is a separate problem from request parsing; handling
  `Content-Length` correctly is essential for persistent connections.
- Keeping handlers focused and moving shared behavior into services makes the
  server easier to extend without duplicating logic.
- Compression support is mostly a negotiation problem: once
  `Accept-Encoding` parsing is centralized, endpoint integration is simple.
- File endpoints need strict path normalization checks to avoid traversal
  vulnerabilities.

## Run locally

1. Ensure `.NET 9` is installed.
1. Run `./your_program.sh`.
1. Optionally set a file base directory:
   `./your_program.sh --directory /tmp/http-files`.
