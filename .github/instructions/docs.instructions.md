---
applyTo: "**/*.md,src/Snippets/**"
---

# Documentation

- User-facing behavior or API changes should update the relevant page under `docs/` (for example `docs/strategies/retry.md` for changes to the retry strategy).
- C# samples in the documentation are **not** written inline. They live in `src/Snippets/Docs` inside `#region` blocks and are injected into the Markdown with `dotnet mdsnippets` (see `src/Snippets/README.md`). Flag inline C# samples added to docs, and snippet regions that were changed without the Markdown being regenerated.
- Markdown is linted with markdownlint (`.markdownlint.json`) and spell-checked in CI. New technical terms that are not in backticks may need adding to `.github/wordlist.txt`.
- Do not edit `CHANGELOG.md` in feature PRs. It is updated automatically as part of the release process.
- Do not edit the benchmark reports under `bench/BenchmarkDotNet.Artifacts/results` by hand.
