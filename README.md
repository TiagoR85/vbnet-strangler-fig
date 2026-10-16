# vbnet-strangler-fig

[![ci](https://github.com/TiagoR85/vbnet-strangler-fig/actions/workflows/ci.yml/badge.svg)](https://github.com/TiagoR85/vbnet-strangler-fig/actions/workflows/ci.yml)
[![release](https://img.shields.io/github/v/release/TiagoR85/vbnet-strangler-fig)](https://github.com/TiagoR85/vbnet-strangler-fig/releases)
[![license: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Demonstração do padrão **Strangler Fig**: migração de um serviço de orçamentos
VB.NET (net48) para C# (.NET 10), com façade de encaminhamento, *shadow traffic*
e testes de caracterização golden-master.

## About this repository

Monorepo com 4 projetos: `LegadoOrcamentos.Vb` (serviço HTTP legado),
`Orcamentos.Modern` (reimplementação C#), `Orcamentos.Facade` (roteador
legacy/modern/parity) e `Orcamentos.Tests` (xunit wire-real, zero Docker).
A CI corre em `windows-latest` porque os testes de caracterização executam o
runtime net48 ao vivo. É o único projeto do portfólio onde `net48` é permitido.

## Quickstart (Windows, PowerShell)

```powershell
dotnet build
# terminal 1 — legado (porta 5101 por omissão)
& .\LegadoOrcamentos.Vb\bin\Debug\net48\LegadoOrcamentos.Vb.exe 5101
# terminal 2 — façade
dotnet run --project Orcamentos.Facade
# pedidos com qualquer um dos três modos
$body = '{"linhas":[{"sku":"A-1","quantidade":3,"precoUnitario":19.99}],"pais":"PT"}'
Invoke-RestMethod -Uri http://localhost:5000/orcamentos/calcular -Method Post -Body $body -ContentType 'application/json'
Invoke-RestMethod -Uri http://localhost:5000/orcamentos/calcular -Method Post -Body $body -ContentType 'application/json' -Headers @{ 'X-Strangler-Target' = 'parity' }
Invoke-RestMethod -Uri http://localhost:5000/health
```

## Modos de encaminhamento

| Modo | Header | Comportamento |
|------|--------|---------------|
| `legacy` (default) | `X-Strangler-Target: legacy` | forward HTTP verbatim ao serviço VB; 502 se em baixo |
| `modern` | `X-Strangler-Target: modern` | cálculo in-proc em C# |
| `parity` | `X-Strangler-Target: parity` | chama os dois, devolve o **legado**, loga `ParityDiff` |

A config `Strangler:Target` (appsettings) dá o default; o header tem
precedência por-pedido. Header inválido → 400.

## Testes de caracterização

`Orcamentos.Tests/characterization/*.golden.json` congela o comportamento do
legado (21 cenários: fronteiras de escalão, midpoint de arredondamento,
portes grátis, erros de validação). A mesma coleção corre contra o legado vivo
(characterization) e contra a reimplementação C# (paridade).

```powershell
# regenerar goldens após mudança INTENCIONAL do legado
$env:CAPTURE_GOLDENS = '1'
dotnet test Orcamentos.Tests\Orcamentos.Tests.csproj --filter "FullyQualifiedName~CharacterizationTests"
$env:CAPTURE_GOLDENS = $null
```

## Decisões de design

- [ADR-0001](docs/adr/ADR-0001-strangler-facade-vs-branch-by-abstraction.md) — façade HTTP em vez de branch-by-abstraction
- [ADR-0002](docs/adr/ADR-0002-golden-master-vs-snapshot-lib.md) — golden master em vez de snapshot lib
- [ADR-0003](docs/adr/ADR-0003-ci-windows-e-gate-de-cobertura.md) — CI Windows + gate só em Modern+Facade
