# Wplay ⚡

A high-performance **Webhook & Inbound Email Inspection Platform** built with **ASP.NET Core MVC**, **HTMX**, and **UIkit CSS**. Inspired by tools like `Webhook.site` and `Svix Play`, Wplay provides real-time HTTP payload inspection, custom mock responses, forced status code testing, and dynamic failure rate simulations.

---

## 🌟 Key Features

* **Instant Webhook Ingress**: Capture webhooks with sub-millisecond execution times across all HTTP verbs (`GET`, `POST`, `PUT`, `DELETE`, `PATCH`, `OPTIONS`).
* **Dynamic URL Overrides (`/force-code`)**: Override response codes directly on the fly using route parameters (e.g., `/hp/{uuid}/force-500` or `/hp/{uuid}/404`).
* **Failure Lottery Simulator**: Simulate unreliable endpoints and network degradation by appending chance parameters (e.g., `/hp/{uuid}?lottery=30` for a 30% failure rate).
* **HTMX Live Stream**: Lightweight real-time feed updates powered by HTMX partial rendering and UIkit CSS—zero heavy JavaScript frameworks required.
* **Dual Ingress**: Capture both HTTP Webhooks and structured Inbound Emails (`TextBody`, `HtmlBody`, MIME raw headers, and attachments metadata).
* **Dual Database Support**: Zero-config file-based storage with **SQLite** for local development, with seamless scalability to **MariaDB / MySQL** via Pomelo EF Core.
* **Clean & Modular Entity Architecture**: Built using `IEntityTypeConfiguration<T>` auto-discovery, custom `JsonNode` properties (memory-safe alternative to `JsonDocument`), and automatic timestamp tracking.

---

## 🛠️ Tech Stack

| Layer | Technology |
| --- | --- |
| **Framework** | C# / ASP.NET Core MVC |
| **Data Layer** | Entity Framework Core (EF Core) |
| **Database** | SQLite (Default) / MariaDB (`Pomelo.EntityFrameworkCore.MySql`) |
| **Frontend** | Razor Views, HTMX, UIkit CSS |
| **JSON Handling** | `System.Text.Json` & `System.Text.Json.Nodes` (`JsonNode`) |