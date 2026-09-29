# PR Assessment

Review-only criteria: these apply when assessing a pull request, not when authoring code. The coding rules themselves live in [`AGENTS.md`](../../../AGENTS.md) and the path-specific instruction files under [`.github/instructions`](../../instructions) (see `## Where the Review Rules Live` in `SKILL.md`).

Use the criteria below to write the Motivation, Approach, and Summary fields of your review output. Before reviewing individual lines of code, evaluate the PR as a whole: is the change justified, does it take the right approach, and will it be a net positive for the codebase?

## Motivation & Justification

- **Every PR must articulate what problem it solves and why.** Don't accept vague or absent motivation. However, when the PR links to an accepted issue or prior discussion that already establishes the motivation, referencing that is sufficient.
- **Challenge every addition with "Do we need this?"** New code, APIs, options, and abstractions must justify their existence. Polly is used by a very large number of applications, so every addition to its API is effectively permanent.
- **Demand real-world use cases.** Hypothetical benefits are insufficient motivation for expanding the public API. Prefer solutions that can be achieved with existing extensibility points (custom strategies, `ResilienceContext` properties, predicates and delegates on existing options) over new built-in API.
- **Public API changes should start with an issue.** New or changed public API should be discussed and agreed with the maintainers in a GitHub issue before a PR is opened.

## Evidence & Data

- **Require measurable performance data before accepting optimization PRs.** Require BenchmarkDotNet results (see the [`performance-benchmark`](../performance-benchmark/SKILL.md) skill) and never accept performance claims at face value. Focus on the `Ratio` and `Alloc Ratio` columns compared to a baseline run on the same machine.
- **Distinguish real wins from micro-benchmark noise.** Small differences in mean execution time between runs on non-dedicated hardware are often noise. Allocation changes are deterministic and more trustworthy.
- **Investigate and explain regressions before merging.** Even if a PR shows a net improvement, regressions in specific scenarios (e.g. the generic pipeline, sync execution, or pipelines with multiple strategies) must be understood and explicitly addressed.
- **Bug fixes need a reproduction.** A bug fix should come with a test that fails without the fix, as required by `AGENTS.md`. Check that the test actually exercises the reported scenario.

## Approach & Alternatives

- **Check whether the PR solves the right problem at the right layer.** Look for whether it addresses the root cause or applies a workaround. For example, a fix in one strategy may belong in shared pipeline infrastructure, or vice versa.
- **When a PR takes a fundamentally wrong approach, redirect early.** Don't iterate on implementation details of a flawed design.
- **Ask "Why not just X?" and prefer the simplest solution.** The burden of proof is on the complex solution.
- **New features belong in the v8 API.** New functionality should target `Polly.Core` and the other v8 packages rather than the legacy `Polly` v7 API, unless the change is specifically about the legacy API or the interoperability between the two.

## Cost-Benefit & Complexity

- **Explicitly weigh whether the change is a net positive.** A trade-off that shifts costs around is not automatically beneficial.
- **Reject over-engineering - complexity is a first-class cost.** Unnecessary abstraction, extra indirection, and elaborate solutions for marginal gains should be pushed back on.
- **Every addition creates a maintenance obligation.** Code that is hard to maintain, increases API surface area, adds dependencies, or creates technical debt needs stronger justification.

## Scope & Focus

- **Require large or mixed PRs to be split into focused changes.** Each PR should address one concern. Mixed concerns make review harder and increase regression risk. Maintainers may waive this requirement.
- **Defer tangential improvements to follow-up PRs.** Even good ideas should wait if they're not part of the PR's core purpose.
- **Unrequested dependency changes are out of scope.** Adding or updating dependencies is not allowed unless specifically requested (see `AGENTS.md`). Dependency updates are otherwise handled by Dependabot.

## Risk & Compatibility

- **Flag breaking changes.** Polly follows semantic versioning. Binary or source breaking changes to public API, and behavioral changes that could affect existing consumers (e.g. changing default option values, the order or number of telemetry events, which outcomes are handled by default, or exception types thrown), need explicit maintainer approval.
- **Assess regression risk proportional to the change's blast radius.** Changes to shared infrastructure such as `ResiliencePipeline`, `ResilienceContext`, `ResilienceContextPool`, the pipeline components, or telemetry affect every strategy and need proportionally higher value and more thorough validation.
- **Consider all supported target frameworks.** A change that is correct on .NET 10 may behave differently, or not compile, on .NET Framework or .NET Standard 2.0.

## Codebase Fit & History

- **Ensure new code matches existing patterns and conventions.** Deviations from established patterns create confusion and inconsistency. If a rename or restructuring is warranted, do it uniformly in a dedicated PR, not piecemeal.
- **Check whether a similar approach has been tried and rejected before.** Search closed PRs and issues. If a prior attempt didn't work, require a clear explanation of what's different this time.
