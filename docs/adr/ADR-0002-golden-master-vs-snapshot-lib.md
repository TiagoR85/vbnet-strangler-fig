# ADR-0002: Golden master versionado vs snapshot lib / tabelas inline

**Data:** 2026-10-16 · **Estado:** Aceite

## Contexto

Congelar o comportamento do legado podia usar uma biblioteca de snapshots
(ApprovalTests/Shapproval), tabelas `Theory/InlineData` inline, ou ficheiros
golden JSON versionados.

## Decisão

Ficheiros `*.golden.json` no repo (pares `request` → `esperado.status+body`),
comparados por **valor decimal** (`JsonCanonico.Igual`), com regeneração
env-gated (`CAPTURE_GOLDENS=1`).

## Consequências

- **+** Golden master clássico (Fowler): reviável em code review, diffável,
  legível por quem não escreveu o teste.
- **+** A MESMA coleção serve a characterization (legado vivo) e a paridade
  (C#) — prové dupla a partir de um único artefacto.
- **+** Comparação por valor torna irrelevante o formato do serializador
  (VB/Newtonsoft `4.90` vs C#/STJ `4.9`).
- **−** Sem workflow automático de aprovação (é manual e intencional).
- **−** Ficheiros podem crescer; mantidos a ~21 cenários focados em regras.
