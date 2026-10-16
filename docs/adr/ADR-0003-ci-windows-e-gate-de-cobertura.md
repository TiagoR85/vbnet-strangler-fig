# ADR-0003: CI em windows-latest + gate de cobertura só em Modern+Facade

**Data:** 2026-10-16 · **Estado:** Aceite

## Contexto

Os testes de caracterização fazem spawn do exe VB net48 — o runtime .NET
Framework só existe nos runners Windows. Ao mesmo tempo, o gate de cobertura
de 80% do portfólio não pode medir o legado (o processo VB corre fora do
host de testes e o Coverlet instrumenta apenas os assemblies do projeto de
teste: `Orcamentos.Modern` + `Orcamentos.Facade`).

## Decisão

Estender a reusable `ci-pipeline-templates` com inputs `runs-on`
(default `ubuntu-latest`) e `build-command` (default vazio) — consumidores
existentes continuam idênticos. Este repo usa `runs-on: windows-latest` e
`build-command: dotnet build -c Release`. Gate 80% aplicado apenas a
Modern+Facade; o VB legado fica fora, compensado pela caracterização.

## Consequências

- **+** Runner correto para net48 sem Mono/branch hacks.
- **+** Template fica preparado para futuros projetos Windows do portfólio.
- **+** Nunca baixar o threshold; o legado tem rede de segurança própria.
- **−** Runner Windows mais lento/caro que ubuntu.
- **−** Sem % publicável para o legado — aceite: ele é substituído, não
  refactorado.
