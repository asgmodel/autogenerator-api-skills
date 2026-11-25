# AutoGenerator API Skills

[![skills.sh](https://skills.sh/b/asgmodel/autogenerator-api-skills)](https://skills.sh/asgmodel/autogenerator-api-skills)

Agent skills for ASP.NET Core backend generation with [WasmAI.AutoGenerator.API](https://github.com/asgmodel/WasmAI.AutoGenerator.API). The initial release contains one core skill, with focused references for each generation mode. It guides an AI coding agent in using the upstream generator; installing the skill does not install the .NET package or generate code on its own.

## Install

Run inside the consuming project with a supported Node.js version and the [Skills CLI](https://www.skills.sh/docs/cli):

```bash
npx skills add asgmodel/autogenerator-api-skills --list
npx skills add asgmodel/autogenerator-api-skills --skill autogenerator-api-skills
```

For Codex specifically:

```bash
npx skills add asgmodel/autogenerator-api-skills --skill autogenerator-api-skills -a codex
```

To inspect the unpacked local project from its root instead, use `npx skills add . --list`.

## Use

Open the target ASP.NET Core project in your coding agent and request, for example:

> Use autogenerator-api-skills to inspect my Product entity, generate Builder CRUD in an isolated checkout, preserve custom code, and report the build and test results.

The agent follows this workflow:

```mermaid
flowchart TD
    A[Inspect host and resolved package] --> B[Check ITModel and DbContext]
    B --> C[Select generation mode]
    C --> D[Generate in isolated workspace]
    D --> E[Inspect diff and contracts]
    E --> F[Verify DI, mapping and routes]
    F --> G[Build and run available tests]
    G --> H[Report evidence and limitations]
```

## Sections

| Section | Coverage |
| --- | --- |
| Host and model | .NET 8 snapshot, EF Core, ITModel, DbContext and assembly discovery |
| Generation | Standard CRUD, BPR Builder DTO, Share DTO and DSO |
| Integration | Controllers, repositories, validators, DI and AutoMapper |
| Infrastructure | SQL Server, Swagger and authentication configuration |
| Quality | Safe regeneration, build, tests and troubleshooting |

Read the [core skill](skills/autogenerator-api-skills/SKILL.md), [generation guide](skills/autogenerator-api-skills/references/generation.md), [source compatibility notes](skills/autogenerator-api-skills/references/repository.md) and [usage scenarios](skills/autogenerator-api-skills/references/examples.md).

## Generator commands

The original reference was reviewed against a specific upstream commit. Verify the installed version and run from the consuming startup project, ideally in an isolated checkout:

```bash
dotnet run -- generate
dotnet run -- generate /bpr c:build
dotnet run -- generate /bpr c:share
dotnet run -- generate /bpr c:dso
```

Select one mode for the requested task; these are alternatives, not a mandatory sequence. Check the generated files, preserve manual logic, build and run available tests. See the generation guide for merge and auth flags and their caveats.

## Repository layout

```text
skills/
  autogenerator-api-skills/
    SKILL.md
    agents/openai.yaml
    assets/icon.svg
    references/
      generation.md
      repository.md
      examples.md
README.md
```

## Publishing and maintenance

Verify discovery with the `--list` command and install the skill from the public repository. The Skills.sh leaderboard is based on installation telemetry; publication alone does not guarantee immediate listing. See the [Skills.sh FAQ](https://www.skills.sh/docs/faq).

Keep detailed generator and procedure examples in the references directory as they become available. Do not claim runtime compatibility without checking the resolved package and running the relevant tests. No upstream source code is bundled in this repository; the provided skill references an upstream source snapshot.
