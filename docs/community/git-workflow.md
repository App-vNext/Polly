# Git Workflow

Our recommended process for working with Polly is:

1. [Fork](https://docs.github.com/get-started/quickstart/fork-a-repo) our repository on GitHub
2. Clone your fork locally
3. Configure the upstream (`git remote add upstream https://github.com/App-vNext/Polly.git`)
4. Switch to the default branch (i.e. `main`) using `git checkout main`
5. Create a new local branch for your changes (`git checkout -b my-branch`).
6. Work on your changes
7. Rebase if required (see below)
8. Check that the solution builds successfully by running `dotnet test` from the root of the repository
    - There should be no errors or warnings
    - All tests should pass
    - The code coverage level is maintained
    - Bug fixes should include at least one test where practical
9. If adding new functionality, or checking existing behaviour, check whether there is any documentation that should be added or updated
10. Push the branch up to GitHub (`git push origin my-branch`)
11. [Create a Pull Request][create-a-pr] on GitHub - the PR should target (have as base branch) the default branch (i.e. `main`).

You should not work on a clone of the default branch, and you should not send a pull request from it - please always work from a branch. The reasons for this are detailed below.

## Learning Git Workflow

For an introduction to Git, check out [GitHub's _Git Guide_](https://github.com/git-guides). For more information about GitHub Flow, please head over to the [GitHub Flow](https://docs.github.com/get-started/quickstart/github-flow) documentation.

## Handling Updates from the default branch

While you're working away in your branch, it's possible that one or more new commits have been added to the repository's default branch. If this happens you should:

1. [Stash](https://git-scm.com/book/en/v2/Git-Tools-Stashing-and-Cleaning) any uncommitted changes you need to
2. `git checkout main`
3. `git pull upstream main`
4. `git checkout my-branch`
5. `git rebase main`
6. [Sync your fork](https://docs.github.com/pull-requests/collaborating-with-pull-requests/working-with-forks/syncing-a-fork) (optional) - this makes sure your remote main branch is up to date

This ensures that your history is "clean" i.e. you have one branch off from `main` followed by your changes in a straight line. Failing to do this ends up with several "messy" merges in your history, which we don't want. This is the reason why you should always work in a branch and you should never be working in, or sending pull requests from, `main`.

Rebasing public commits is [considered to be bad practice](https://git-scm.com/book/en/v2/Git-Branching-Rebasing#The-Perils-of-Rebasing), which is why we ask you to rebase any updates from `upstream/main`.

If you're working on a long running feature then you may want to do this quite often, rather than run the risk of potential merge issues further down the line.

## Sending a Pull Request

While working on your feature you may well create several branches, which is fine, but before you send a pull request you should ensure that you have rebased back to a single "feature branch" - we care about your commits, and we care about your feature branch; but we don't care about how many or which branches you created while you were working on it.

When you're ready to go you should confirm that you are up-to-date and rebased with upstream (see _"Handling Updates from the default branch"_ above), and then:

1. `git push origin my-branch`
1. Send a descriptive [Pull Request][create-a-pr] on GitHub - making sure you have selected the correct branch in the GitHub UI.

It is not the end of the world if the commit history in your pull request ends up being messy - we can always squash it down to a single commit before merging. However, if you follow the steps above you should end up with a neater history.

[create-a-pr]: https://docs.github.com/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request
