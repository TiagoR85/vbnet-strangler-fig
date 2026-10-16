# ADR-0001: Façade HTTP estrangladora vs branch-by-abstraction

**Data:** 2026-10-16 · **Estado:** Aceite

## Contexto

A migração VB.NET(net48) → C#(.NET 10) podia ser feita dentro do mesmo
processo (branch-by-abstraction, interfaces internas) ou com o legado a
continuar como serviço HTTP próprio, isolado atrás de uma façade.

## Decisão

O legado mantém-se como serviço HTTP self-host (`HttpListener`, porta por
argumento) e a façade .NET 10 encaminha cada pedido por header/config:
`legacy` (forward verbatim), `modern` (in-proc) ou `parity` (shadow).

## Consequências

- **+** Estrangulamento real no wire: clientes falam sempre com a façade; o
  corte do legado é um flip de config, não um deploy coordenado.
- **+** `parity` devolve o legado e compara em background — confiança sem risco.
- **+** Testes wire-real (a caracterização executou o binário net48 de facto).
- **−** Overhead HTTP de loopback por pedido e dois processos para gerir.
- **−** O contrato HTTP (JSON camelCase, status) ficou congelado pelos goldens;
  mudanças de contrato exigem regenerar a caracterização.
