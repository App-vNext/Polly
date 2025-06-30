# Polly Incident Response Checklist

<!-- markdownlint-disable MD024 -->

This document is a checklist to help you through the _Incident Response Process_ (IRP) for a security
vulnerability reported in Polly. It is designed to help you triage, mitigate, scope, disclose, and
learn from the report, as appropriate.

This template should be copied into a privately created [security advisory][advisories] and used to
track the response.

---

## 🏷️ Phase 1️⃣ Triage

> [!TIP]
> Before starting the IRP, take a moment to reflect on the report and the vulnerability.
> Maybe grab a cup of coffee or tea.
>
> It is better to consider the report rather than start a knee-jerk reaction in panic.

### 📢 Updates

> [!TIP]
> Use this section to detail the report received, and include any reproduction results, mitigating
> factors and the impact of the issue.

For example:

> I've read the report and validated this vulnerability does exist.
>
> The exploit is difficult to achieve due to the following:
>
> - It require precise timing and multiple steps to achieve.
> - It requires a specific set of configurations and advanced knowledge.
>
> The impact of the exploit:
>
> - Causes an infinite loop in a resilience strategy.
> - Exhausts the CPU of the application.

### 🧑‍🏫 Guidance

> [!TIP]
> Use this section to link to any reference documentation from triaging the vulnerability, if any.

### ✅ Tasks

> [!TIP]
> Tasks that should be completed before moving on to mitigation if any vulnerabilities are confirmed.

- [ ] Review the report and reproduce the vulnerability
- [ ] Request additional information from the reporter if needed
- [ ] If not provided, request a minimal proof of concept (PoC) and/or exploit, if needed
- [ ] Understand the vulnerability/situation
- [ ] Update the vulnerability with the current understanding
- [ ] Determine the severity
- [ ] Determine which Polly packages are affected (Polly, Polly.Core, etc.)
- [ ] Determine which Polly versions are affected (e.g. 8.6.1)
- [ ] Determine whether any prominent dependent projects are affected (e.g. [ASP.NET Core][aspnetcore], etc.)
- [ ] Determine which platforms and/or operating systems are affected, if relevant
- [ ] Determine the pull request/commit where the vulnerability was first introduced
- [ ] Has the vulnerability been caused by, or caused, any of our secrets to be leaked?
- [ ] Coordinate with other maintainers if needed ([Slack][slack] can be used for real-time discussion if needed)
- [ ] Decide if the reported vulnerability is valid
  - [ ] Close if non-actionable or not within scope
  - [ ] Proceed to mitigation if actionable/confirmed

### 🗒️ For the record

> [!TIP]
> What did you determine during this phase?

- Is there a direct risk of [CIA][cia] being broken? `Yes|No`
- Which part of [CIA][cia] could be broken? `Confidentiality|Integrity|Availability`
- What user data is at risk, if any?
- What is required to exploit this vulnerability?
- Are there any mitigating factors and/or workarounds that reduce the risk of exploitation?
- The vulnerability was introduced on: `YYYY-MM-DD`
- Is there a pull request where the vulnerability was introduced? `<URL>`
- Provide deep links(s) to the relevant lines of code, if applicable: `<URL>`
- Do we need to notify any dependent projects? `Yes|No`
  - If yes, which projects? `<URL>`

---

## 🏷️ Phase 2️⃣ Mitigation

### 📢 Updates

> [!TIP]
> Use this section to call out any blockers, challenges, or successes. This section may contain a mitigation plan or strategy to execute.

For example:

> To prevent potential exploitation, we've disabled the feature where this vulnerability exists.
>
> OR
>
> We identified the root cause, and are now working on a pull request to fully mitigate it.

Once a mitigation plan is in place, work will start to prepare any fixes.

> [!IMPORTANT]
> Any fix preparation should be performed in a private fork of the repository linked to the security advisory.
> Only this private fork should be used to prepare the fix to avoid premature disclosure. As private forks do
> not have the capability to run GitHub Actions workflows, we cannot build and release any fixes in advance of
> disclosure. Instead, the fix should be submitted, merged and released as part of the disclosure process.
>
> This creates an unavoidable delay between the fix being merged and it being available to users, so technically
> the vulnerability will be a zero-day for a short period between the pull request being opened and the fixed
> version of any NuGet packages being published and indexed by NuGet.org.
>
> See here for information about creating a private fork: [_Collaborating in a temporary private fork to resolve a repository security vulnerability_][private-fork]

### ✅ Tasks

> [!TIP]
> Tasks that should be completed before moving on to scoping once the mitigation has been prepared.

- [ ] Re-assess severity and update if necessary
- [ ] Confirm libraries and versions that are affected by the vulnerability
- [ ] Confirm mitigation preparation across any identified packages
- [ ] Have any tests been added to ensure the vulnerability is mitigated?
- [ ] Is the proposed fix tactical (_"stop the bleeding"_) or comprehensive (_"addresses the root cause"_)?
  - [ ] If tactical, is it sufficient to mitigate the vulnerability?
- [ ] If any secrets (GitHub or NuGet token, Azure federated credentials) were leaked, have they been rotated?

### 🗒️ For the record

> [!TIP]
> What did you learn during this phase?

- The vulnerability was first mitigated on: `YYYY-MM-DD`
- The vulnerability affected: `Polly|Polly.Core|etc`
- Is there a link to the mitigation work? `<URL>`

---

## 🏷️ Phase 3️⃣ Scoping

### 📢 Updates

> [!TIP]
> Use this section to analyse the scope and impact of the vulnerability.

For example:

> We have run an analysis and identified this vulnerability affects a version
> downloaded 17 million times and is a dependency of ASP.NET Core 8.0.0 and later.

### ✅ Tasks

> [!TIP]
> Tasks that should be completed before moving on to disclosure.

Scoping is the process of interrogating data to determine if a vulnerability was used and/or exploited, and what the impact was. Some useful prompts are:

> - What is the goal of the scoping work? Writing down specific goals can help reframe the work that needs done.
> - How can you identify expected vs exploitative use?
> - Are you able to find known use of the vulnerability in the data?

- [ ] Review available information sources
- [ ] Confirm who was affected or might have been affected
- [ ] Do we need to coordinate with any dependent projects?
  - [ ] If yes, which projects? `<URL>`
  - [ ] If ASP.NET Core, is affected, should we contact [MSRC][msrc]?
    - [ ] If yes, contact MSRC and provide details of the vulnerability and await their response
- [ ] Does the mitigation need to be backported to previous versions?
  - [ ] If yes, which versions? `<URL>`
    - [ ] If yes, prepare a branch for each version
- [ ] Is the fix fully prepared, tested and ready for release?

### 📓 Notes

> [!TIP]
> Add your scoping notes here

### 🗒️ For the record

> [!TIP]
> What did you learn during this phase?

- What is the link to your scoping notes? `<URL>`
- What is your confidence in the completeness of the scoping? `low|medium|high`
- Does the vulnerability cause any [CIA][cia] breach? `Yes|No`
- Were you able to find the data you needed? If not, how come?

---

## 🏷️ Phase 4️⃣ Disclosure

### 📢 Updates

> [!TIP]
> Use this section to note preparation work before the advisory is published.

### ✅ Tasks

> [!TIP]
> Things that should be completed before publishing the advisory.

- Is the CI/CD pipeline ready to build and release the fix?
  - [ ] Is the NuGet API token still valid?
  - [ ] Is the Authenticode certificate still valid?
  - [ ] Are there any [Azure][azure-status], [GitHub][github-status] or [NuGet][nuget-status] incidents in progress that would prevent the release?
- [ ] Are any dependent projects we have coordinated with ready to publish their own advisories?
- [ ] Is any blog post for users ready to be published?
- [ ] Do we have a root cause analysis ready to be published if needed?
- [ ] Does the reporter of the vulnerability wish to be acknowledged in the advisory?
- [ ] What version(s) of Polly will the fix be released in (e.g. is it a patch, minor, major)?
- [ ] Is the advisory ready to be published?

> [!TIP]
> Things that should be completed to start the disclosure process.

- [ ] Create new commits in the public Polly repository to prepare the fix from the private fork
  - [ ] Rewrite commit messages to be minimally descriptive (i.e. what was changed, not why)
- [ ] Create a pull request to merge the fix into the public Polly repository for any relevant branches
  - [ ] The pull request should be minimally descriptive (i.e. what was changed, not why)
- [ ] Once all pull request(s) are merged, run the [release workflow][release-workflow] to prepare
      the draft GitHub release(s) for the fixed version(s).
  - [ ] The release information should be minimally descriptive (i.e. what was changed, not why)
- [ ] Once everything is ready, publish the GitHub releases, which will start the build/publish process to NuGet.org
- [ ] Wait for the [published][published] workflow(s) to complete, which signify that the NuGet packages are available for download
- [ ] Publish the advisory
- [ ] Create a [discussion][discussion] to magnify the advistory and for users to ask questions about the advisory
- [ ] Publish any blog post that was prepared
- [ ] Amplify the advisory on social media, such as on Bluesky, etc.

> [!TIP]
> Things that should be completed before moving on to the learn phase.

- [ ] Update the GitHub release(s) to specifically reference the advisory
- [ ] Update the GitHub pull request(s) to specifically reference the advisory

### 🗒️ For the record

> [!TIP]
> What happened during this phase?

- When was advisory published? `YYYY-MM-DD:HH-MM-SSZ`
- What is the link to the mitigation pull request(s)? `<URL>`
- When was the mitigation pull request(s) merged? `YYYY-MM-DD:HH-MM-SSZ`
- When were the NuGet packages available for download? `YYYY-MM-DD:HH-MM-SSZ`
- Is there a link to any blog post that was published? `<URL>`
- Is there a link to the advisory? `<URL>`
- Is there a link to the discussion? `<URL>`

---

## 🏷️ Phase 5️⃣ Learn

### 📢 Updates

> [!TIP]
> Use this section to discuss anything that occurred after the advisory was published.

### ✅ Tasks

> [!TIP]
> Things that should be completed before the incident reponse is completed.

- [ ] Do we need to update any documentation?
- [ ] Is there anything in the IRP that could be improved?
- [ ] Is there anything we can do to prevent similar vulnerabilities in the future?
- [ ] If the fix was tactical, is there a plan to address the root cause?
- [ ] Is there any negative community feedback we need to address?

### 🗒️ For the record

> [!TIP]
> What happened during this phase?

- What did we learn from this incident?
- Is there a link to any post-incident review? `<URL>`
- What changes have we made a result of this incident? `<URL>`

[advisories]: https://github.com/App-vNext/Polly/security/advisories "Polly Security Advisories"
[aspnetcore]: https://github.com/dotnet/aspnetcore "ASP.NET Core"
[azure-status]: https://azure.status.microsoft/status "Azure Status"
[cia]: https://www.energy.gov/femp/operational-technology-cybersecurity-energy-systems#cia "Confidentiality Integrity Availability Triad"
[discussion]: https://github.com/App-vNext/Polly/discussions "Polly Discussions"
[github-status]: https://www.githubstatus.com/ "GitHub Status"
[msrc]: https://msrc.microsoft.com/ "Microsoft Security Response Center"
[nuget-status]: https://status.nuget.org/ "NuGet Status"
[private-fork]: https://docs.github.com/code-security/security-advisories/working-with-repository-security-advisories/collaborating-in-a-temporary-private-fork-to-resolve-a-repository-security-vulnerability "Collaborating in a temporary private fork to resolve a repository security vulnerability"
[published]: https://github.com/App-vNext/Polly/actions/workflows/nuget-packages-published.yml "NuGet Packages Published Workflow"
[release-workflow]: https://github.com/App-vNext/Polly/actions/workflows/release.yml "Polly Release Workflow"
[slack]: https://pollytalk.slack.com "Polly Slack Community"
