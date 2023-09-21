# Git workflow

The general process for working with FoundationaLLM is:

1. [Fork](http://help.github.com/forking/) on GitHub
1. Clone your fork locally
1. Configure the upstream repo (`git remote add upstream git://github.com/solliancenet/foundationallm`)
1. Switch to the default branch (e.g. main, using `git checkout main`)
1. Create a local branch from that (`git checkout -b myBranch`)
1. Work on your feature
1. Rebase if required (see below)
1. Push the branch up to GitHub (`git push origin myBranch`)
1. Send a Pull Request on GitHub - the PR should target (have as base branch) the default branch (e.g. `main`).

You should **never** work on a clone of the default branch, and you should **never** send a pull request from it - always from a branch. The reasons for this are detailed below.

## Learning Git Workflow

For an introduction to Git, check out [GitHub Guides](https://guides.github.com/). For more information about GitHub Flow, please head over to the [Understanding the GitHub Flow](https://guides.github.com/introduction/flow/index.html) illustrated guide, both by the awesome people at GitHub.

## Handling Updates from the default branch

While you're working away in your branch, it's quite possible that the upstream target branch may be updated. If this happens you should:

1. [Stash](http://git-scm.com/book/en/v2/Git-Tools-Stashing-and-Cleaning) any uncommitted changes you need to
1. `git checkout vX_Y_Z`
1. `git pull upstream main`
1. `git rebase main myBranch`
1. `git push origin main` - (optional) this this makes sure your remote main branch is up to date

This ensures that your history is "clean" i.e. you have one branch off from `main` followed by your changes in a straight line. Failing to do this ends up with several "messy" merges in your history, which we don't want. This is the reason why you should always work in a branch and you should never be working in, or sending pull requests from, `main`.

Rebasing public commits is [very problematic](http://git-scm.com/book/ch3-6.html#The-Perils-of-Rebasing), which is why we require you to rebase any updates from `upstream/main`.

If you're working on a long running feature then you may want to do this quite often, rather than run the risk of potential merge issues further down the line.

## Sending a Pull Request

While working on your feature you may well create several branches, which is fine, but before you send a pull request you should ensure that you have rebased back to a single "Feature branch" - we care about your commits, and we care about your feature branch; but we don't care about how many or which branches you created while you were working on it :)

When you're ready to go you should confirm that you are up to date and rebased with upstream (see "Handling Updates from the default branch" above), and then:

1. `git push origin myBranch`
1. Send a descriptive [Pull Request](http://help.github.com/pull-requests/) on GitHub - making sure you have selected the correct branch in the GitHub UI!
