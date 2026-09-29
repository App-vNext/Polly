---
name: code-review
description: Review code changes in App-vNext/Polly for correctness, performance, API compatibility, and consistency with project conventions. Use when reviewing pull requests (including GitHub Copilot code review) or code changes.
---

# Polly Code Review

Review code changes against the conventions and patterns established by the Polly maintainers.

**Reviewer mindset:** Be polite but skeptical. Your job is to help speed up the review process for maintainers, which includes not only finding problems the PR author may have missed but also questioning the value of the PR in its entirety. Treat the PR description and linked issues as claims to verify, not facts to accept. Question the stated direction, probe edge cases, and don't hesitate to flag concerns even when unsure.

## When to Use This Skill

Use this skill when:

- Reviewing a PR or code change in App-vNext/Polly
- Checking code for correctness, performance, style, or consistency issues before submitting a PR
- Asked to review, critique, or provide feedback on code changes
- Validating that a change follows Polly's conventions

## Review Process

### Step 0: Load Relevant Instructions

Before analyzing anything, load the following files (relative to the repository root):

- [`AGENTS.md`](../../../AGENTS.md) - the architecture overview, build/test commands and general guidelines for the repository. The general guidelines are review rules: a change that violates them is a finding.
- [`pr-assessment.md`](pr-assessment.md) - the criteria you will assess the PR against.
- The instruction files under [`.github/instructions`](../../instructions) whose `applyTo` paths match the files in the diff, as described in _Where the Review Rules Live_.

If a rule in an instruction file conflicts with this skill, the instruction file wins. If an instruction file conflicts with `AGENTS.md`, `AGENTS.md` wins.

### Step 1: Gather Code Context (No PR Narrative Yet)

Before analyzing anything, collect as much relevant **code** context as you can. **Critically, do NOT read the PR description, linked issues, or existing review comments yet.** You must form your own independent assessment of what the code does, why it might be needed, what problems it has, and whether the approach is sound before being exposed to the author's framing. Reading the author's narrative first anchors your judgment and makes you less likely to find real problems.

1. **Diff and file list**: Fetch the full diff and the list of changed files.
2. **Full source files**: For every changed file, read well beyond the diff hunks. Diff-only review is the #1 cause of false positives and missed issues. You need the surrounding code to understand invariants, thread-safety, call patterns, and data flow.
3. **Consumers and callers**: If the change modifies a public or internal API, a type that others depend on, or a virtual/abstract member, search for how consumers use it. Grep for callers, usages, and test sites across `src/`, `test/`, `bench/` and `src/Snippets/`.
4. **Sibling types and related code**: If the change fixes a bug or adds a pattern in one strategy, check whether sibling strategies (retry, circuit breaker, fallback, hedging, timeout, rate limiter, and the chaos strategies under `src/Polly.Core/Simmy`), the generic and non-generic variants (e.g. `Foo` and `Foo<TResult>`), and the sync and async execution paths have the same issue or need the same fix.
5. **Key utility/helper files**: If the diff calls into shared utilities (e.g. `src/Polly.Core/Utils`, `ResilienceContextPool`, `ResilienceStrategyTelemetry`), read them to understand their contracts.
6. **Git history**: Check recent commits to the changed files (`git log --oneline -20 -- <file>`). Look for related recent changes, reverts, or prior attempts to fix the same problem.
7. **Detect public API surface changes**: Check whether the PR adds, removes or changes public API. The strongest signal is a change to any `.PublicAPI/PublicAPI.*.txt` file, or to a `CompatibilitySuppressions.xml` file. Also look for new or changed `public`/`protected` members in `src/` projects that do not have a matching `PublicAPI.Unshipped.txt` change. If public API changes are detected, you **MUST** apply every rule in [`public-api.instructions.md`](../../instructions/public-api.instructions.md) during Step 4. This is a blocking gate: if any of those checks fails, the verdict must be ⚠️ Needs Changes or ❌ Reject, regardless of other findings. Maintainers may waive this requirement if there is a compelling reason, for example when updating the .NET SDK to a new major version.

### Step 2: Form an Independent Assessment

Based **only** on the code context gathered above (without the PR description or issue), answer these questions:

1. **What does this change actually do?** Describe the behavioral change in your own words. What was the old behavior? What is the new behavior?
2. **Why might this change be needed?** Infer the motivation from the code itself. What bug, gap, or improvement does it appear to address?
3. **Is this the right approach?** Would a simpler alternative be more consistent with the codebase? Could the goal be achieved with existing functionality (e.g. an existing strategy option, a custom strategy, or `ResilienceContext` properties)? Are there correctness, performance, or safety concerns?
4. **What problems do you see?** Identify bugs, edge cases, missing validation, thread-safety issues, allocation or performance regressions, API design problems, test gaps, and anything else that concerns you.

Write down your independent assessment before proceeding, including an assessment using the criteria in [`pr-assessment.md`](pr-assessment.md).

### Step 3: Incorporate PR Narrative and Reconcile

Now read the PR description, labels, linked issues (in full), author information, existing review comments, and any related open issues in the same area. Treat all of this as **claims to verify**, not facts to accept.

1. **PR metadata**: Fetch the PR description, labels, linked issues, and author. Read linked issues in full as they often contain the repro, root cause analysis, and constraints the fix must satisfy.
2. **Related issues**: Search for other open issues in the same area. This can reveal known problems the PR should also address, or constraints the author may not be aware of.
3. **Existing review comments**: Check whether there are already review comments on the PR to avoid duplicating feedback.
4. **Reconcile your assessment with the author's claims.** Where your independent reading of the code disagrees with the PR description or issue, investigate further, but do not simply defer to the author's framing. If the PR claims a bug fix, a performance improvement, or a behavioral correction, verify those claims against the code and any provided evidence. If your independent assessment found problems the PR narrative doesn't acknowledge, those problems are more likely to be real, not less.
5. **Update your holistic assessment** if the additional context genuinely changes your evaluation (e.g. a linked issue proves the bug is real, or an existing review comment already identified the same concern). Do not soften findings just because the PR description sounds reasonable.
6. **AI disclosure**: If the PR appears to be authored by an AI agent, check that the description discloses which agent was used, as required by the [pull request guidelines in `AGENTS.md`](../../../AGENTS.md#pull-requests).

### Step 4: Detailed Analysis

1. **Focus on what matters.** Prioritize bugs, behavioral changes, performance and allocation regressions, thread-safety issues, resource management problems (e.g. `ResilienceContext` not returned to the pool, undisposed `CancellationTokenSource` instances), incorrect assumptions about data or state, and API design problems. Do not comment on trivial style issues unless they violate an explicit rule. Do not repeat fedback that a CI failure would identify anyway.
2. **Consider collateral damage.** For every changed code path, actively brainstorm: what other scenarios, callers, or inputs flow through this code? Consider generic vs. non-generic pipelines, sync vs. async execution, pipelines composed of multiple strategies, pipelines created through `Polly.Extensions` dependency injection and dynamic reloads, and the V7 interop bridge (`AsAsyncPolicy()`/`AsSyncPolicy()`). If you identify any plausible risk, even one you can't fully confirm, surface it so the author can evaluate it.
3. **Be specific and actionable.** Every comment should tell the author exactly what to change and why. Reference the relevant convention. Include evidence of how you verified the issue is real, e.g. "looked at all callers and none of them validate this parameter".
4. **Flag severity clearly:**
   - ❌ **error** - Must fix before merge. Bugs, security issues, unapproved public API changes, breaking changes, missing tests for behavior changes or bug fixes.
   - ⚠️ **warning** - Should fix. Performance or allocation issues, missing validation, inconsistency with established patterns, missing documentation.
   - 💡 **suggestion** - Consider changing. Minor readability wins, optional optimizations.
5. **Don't pile on.** If the same issue appears many times, flag it once on the primary file with a note listing all affected files.
6. **Respect existing style.** When modifying existing files, the file's current style takes precedence over general guidelines.
7. **Don't flag what CI catches.** The build treats warnings as errors and runs the .NET, StyleCop and Sonar analyzers, the public API analyzers and NuGet package validation. Linters also run for Markdown, spelling, GitHub Actions workflows and PowerShell scripts. Do not flag issues these would catch (e.g. formatting, missing `using` directives, missing XML documentation, unrecorded public API). Assume CI will run separately.
8. **Avoid false positives.** Before flagging any issue:
   - **Verify the concern actually applies** given the full context, not just the diff. Confirm the issue isn't already handled by a caller, callee, or wrapper layer before claiming something is missing.
   - **Skip theoretical concerns with negligible real-world probability.** "Could happen" is not the same as "will happen."
   - **If you're unsure, either investigate further until you're confident, or surface it explicitly as a low-confidence question rather than a firm claim.**
   - **Trust the author's context.** If a pattern seems odd but is consistent with the rest of the repository, assume it's intentional.
   - **Never assert that something "does not exist," "is deprecated," or "is unavailable" based on training data alone.** Your knowledge has a cutoff date. When uncertain, ask rather than assert.
9. **Ensure code suggestions are valid.** Any code you suggest must be syntactically correct, complete, and compile against every target framework of the project being changed.
10. **Format code suggestions correctly.** Any code you suggest must be indented to match the surrounding code and follow the same formatting and code style.
11. **Label in-scope vs. follow-up.** Distinguish between issues the PR should fix and out-of-scope improvements. Be explicit when a suggestion is a follow-up rather than a blocker.
12. **Performance claims need evidence.** If the PR claims a performance improvement, or you suspect a regression on a hot path, use the [`performance-benchmark`](../performance-benchmark/SKILL.md) skill to validate it, or ask the author for BenchmarkDotNet results.

## Multi-Model Review

When the environment supports launching sub-agents with different models, you may run the review in parallel across multiple model families to get diverse perspectives, as different models catch different classes of issues. Each sub-agent re-reads the diff and the surrounding files, so this multiplies the cost of the review by the number of models. This is worth it for a substantial or risky change, but not for a one-line fix. If the environment does not support this, proceed with a single-model review.

**How to execute (when supported):**

1. Select models from 2-3 distinct model families, up to 3 sub-agent models in total:
   - Pick only from models explicitly listed as available in the environment. Do not guess or assume model names.
   - From each selected family, pick the most capable model available. Never pick models labeled "mini", "fast", or "cheap" for code review.
   - Do not select the model that is already running the primary review.
2. Launch a sub-agent for each selected model in parallel, giving each the same review prompt: the PR diff, the review rules from this skill, and instructions to produce findings in the severity format defined above.
3. Wait for all agents to complete, then synthesize: deduplicate findings that appear across models, elevate issues flagged by multiple models (higher confidence), and include unique findings from individual models that meet the confidence bar. If a sub-agent has not completed after 10 minutes and you have results from the others, proceed with the results you have. Note in the output which models contributed.
4. Present a single unified review, noting when an issue was flagged by multiple models. Once the review has been posted successfully, do not wait for any remaining sub-agents or retry the post.

---

## Review Output Format

When presenting the final review (whether as a PR comment or as output to the user), use the following structure.

> 📝 **AI-generated content disclosure:** When posting review content to GitHub (PR reviews, PR comments) under a user's credentials - i.e. the account is **not** a dedicated bot account or app (e.g. `github-actions[bot]`, `copilot`) - you **MUST** include a concise, visible note (e.g. a `> [!NOTE]` alert) at the bottom of the content indicating the content was AI-generated and which agent generated it. Skip this only if the user explicitly asks you to omit it.

### Structure

```markdown
## PR Review

**Motivation**: <1-2 sentences on whether the PR is justified and the problem is real>

**Approach**: <1-2 sentences on whether the fix/change takes the right approach>

**Summary**: <✅ LGTM / ⚠️ Needs Human Review / ⚠️ Needs Changes / ❌ Reject>. <2-3 sentence summary of the overall verdict and key points. If "Needs Human Review", explicitly state which findings you are uncertain about and what a human reviewer should focus on.>

---

### Detailed Findings

#### ✅/⚠️/❌/💡 <Category Name> - <Brief description>

<Explanation with specifics. Reference code, line numbers, interleavings, etc.>

(Repeat for each finding category. Group related findings under a single heading.)

<!-- AI disclosure note: place any AI-generated content disclosure below this line. -->
<!-- Example: > [!NOTE] This review was generated by GitHub Copilot. -->
```

### Guidelines

- Begin the review body with `## PR Review`, immediately followed by the `**Motivation**:`, `**Approach**:`, and `**Summary**:` fields in that order. Do not rename or substitute those fields.
- **Detailed Findings** uses emoji-prefixed category headers:
  - ✅ for things that are correct/look good (use to confirm important aspects were verified)
  - ⚠️ for warnings or impactful suggestions (should fix, or follow-up)
  - ❌ for errors (must fix before merge)
  - 💡 for minor suggestions or observations (nice-to-have)
- **Cross-cutting analysis** should be included when relevant: check whether related code (sibling strategies, generic/non-generic variants, sync/async paths) is affected by the same issue or needs a similar fix.
- **Test quality** should be assessed as its own finding when tests are part of the PR.
- **Summary** gives a clear verdict: LGTM (no blocking issues - use only when confident), Needs Human Review (code may be correct but you have unresolved concerns or uncertainty that require human judgment), Needs Changes (with blocking issues listed), or Reject (explaining why this should be closed outright). **Never give a blanket LGTM when you are unsure.**
- Keep the review concise but thorough. Every claim should be backed by evidence from the code.

### Verdict Consistency Rules

The summary verdict **must** be consistent with the findings in the body:

1. **The verdict must reflect your most severe finding.** If you have any ⚠️ findings, the verdict cannot be "LGTM". Only use "LGTM" when all findings are ✅ or 💡 and you are confident the change is correct and complete.
2. **When uncertain, always escalate to human review.** If you are unsure whether a concern is valid, whether the approach is sufficient, or whether you have enough context to judge, the verdict must be "Needs Human Review". A false LGTM is far worse than an unnecessary escalation.
3. **Separate code correctness from approach completeness.** A change can be correct code that is an incomplete approach (e.g. it treats symptoms without addressing the root cause, or fixes one strategy but not its siblings). The verdict must reflect that gap.
4. **Classify each ⚠️ and ❌ finding as merge-blocking or advisory.** For each finding ask: "Would I be comfortable if this merged as-is?" If any answer is "no", the verdict must be "Needs Changes". If any answer is "I'm not sure", the verdict must be "Needs Human Review".
5. **Devil's advocate check before finalizing.** Re-read all your ⚠️ findings. For each one, ask whether it represents an unresolved concern about the approach, scope, or risk of masking deeper issues. If so, the verdict must reflect that tension. Do not default to optimism because the diff is small.

---

## Where the Review Rules Live

Review rules are split by activity:

- **[`AGENTS.md`](../../../AGENTS.md)** - the architecture overview and general guidelines for all changes to the repository. **Always load this.**
- **[`pr-assessment.md`](pr-assessment.md)** - the PR assessment criteria: Motivation, Evidence, Approach, Cost-Benefit, Scope, Risk, and Codebase Fit. **Always load this.** Use it to write the Motivation, Approach, and Summary fields of your output.
- **`.github/instructions/*.instructions.md`** - the code conventions themselves, shared with code authoring and with the built-in GitHub Copilot code review so that there is a single source of truth. **You MUST load the files whose `applyTo` paths match the diff and treat them as the rule set for this review.**

Load, based on the paths in the diff:

- **Library code changed** (`src/Polly`, `src/Polly.Core`, `src/Polly.Extensions`, `src/Polly.RateLimiting` or `src/Polly.Testing`):
  - [`public-api.instructions.md`](../../instructions/public-api.instructions.md) - public API tracking, breaking changes and API design.
  - [`library.instructions.md`](../../instructions/library.instructions.md) - strategy implementation, time, cancellation, telemetry, options validation, thread-safety and disposal.
  - [`performance.instructions.md`](../../instructions/performance.instructions.md) - allocations and performance on the execution path.
  - [`compatibility.instructions.md`](../../instructions/compatibility.instructions.md) - target frameworks, native AoT and the legacy `Polly` package.
- **Tests changed** (`test/**`): [`tests.instructions.md`](../../instructions/tests.instructions.md) - test libraries, regression tests and determinism.
- **Documentation or snippets changed** (`**/*.md`, `src/Snippets/**`): [`docs.instructions.md`](../../instructions/docs.instructions.md) - docs updates, code snippets and changelog rules.
- **Benchmarks changed** (`bench/**`): [`benchmarks.instructions.md`](../../instructions/benchmarks.instructions.md) - benchmark conventions.

A library change without matching test changes is itself a finding: also load `tests.instructions.md` to judge what tests are missing. If any required instruction file cannot be loaded, note it in the review and fall back to a careful first-principles review of that area.
