# FoundationaLLM DevOps Rules and Guidelines

FoundationaLLM adheres to the trunk-based development philosophy. If you are a member of the FoundationaLLM core development team or you want to contribute to the code, we strongly encourage you to go through the following materials:

- [Microsoft's Azure DevOps team branching strategy](https://devblogs.microsoft.com/devops/release-flow-how-we-do-branching-on-the-vsts-team/)
- [How Microsoft develops modern software with DevOps](https://docs.microsoft.com/en-us/devops/develop/how-microsoft-develops-devops)
- [DORA's research program (Google)](https://www.devops-research.com/research.html)
- [The state of DevOps - 2021 edition (DORA, Google)](./assets/state-of-devops-2021.pdf)
- [Trunk-based development (Google Cloud Architecture Center)](https://cloud.google.com/architecture/devops/devops-tech-trunk-based-development)
- [Trunk-based development e-book by Paul Hammant](https://trunkbaseddevelopment.com/)

Our move to trunk-based development is driven by the vision to build and deliver the FoundationaLLM solution using a solid DevOps process centered on a Git-centric release flow.

## How?

The core rules of development:

1. When a developer starts working on a feature or a bug fix, a new branch gets created from `main`. When the work ends, a PR is created to bring the work back into `main`. We encourage a granular approach, where each individual feature or bug fix has it's own branch. Work should be moved back into the trunk via a PR as soon as possible.

    >NOTE:
    >
    >Committing directly into `main` is not allowed. All changes are merged via PRs which allows for consistent review, validation, and testing processes.

2. When a sprint ends, a release branch is created. The release branch will be used to deploy the changes to production. Work on `main` ca resume right after the release branch was created.

3. The release branch will live until the next release branch is created.

4. In the case of critical bug fixes, the process will follow the same approach as described above at step 1. Once the PR is approved and the code merged to `main`, it will also be cherry-picked into the current release branch. From there, it will be deployed into production.

    >NOTE:
    >
    >This approach ensures that no matter how critical a bug fix is and no matter how much pressure there is to fix an issue in production, the code for the fix always ends up in `main`.

## Why?

Analysis of DevOps Research and Assessment (DORA) data from [2016](./assets/state-of-devops-2016.pdf), [2017](assets/state-of-devops-2017.pdf) shows that teams achieve higher levels of software delivery and operational performance (delivery speed, stability, and availability) if they follow these practices:

- Have three or fewer active branches in the application's code repository.
- Merge branches to trunk at least once a day.
- Don't have code freezes and don't have integration phases.

### Common pitfalls

Some common obstacles to full adoption of trunk-based development include the following:

- **An overly heavy code-review process**. Many organizations have a heavyweight code review process that requires multiple approvals before changes can be merged into trunk. When code review is laborious and takes hours or days, developers avoid working in small batches and instead batch up many changes. This in turn leads to a downward spiral where reviewers procrastinate with large code reviews due to their complexity.

    Consequently, merge requests often languish because developers avoid them. Because it is hard to reason about the impact of large changes on a system through inspection, defects are likely to escape the attention of reviewers, and the benefits of trunk-based development are diminished.

- **Performing code reviews asynchronously**. If your team practices pair programming, then the code has already been reviewed by a second person. If further reviews are required, they should be performed synchronously: when the developer is ready to commit the code, they should ask somebody else on the team to review the code right then. They should not ask for asynchronous review—for example, by submitting a request into a tool and then starting on a new task while waiting for the review. The longer a merge is delayed, the more likely it is to create merge conflicts and associated issues. Implementing synchronous reviews requires the agreement of the team to prioritize reviewing each others' code over other work.

- **Not running automated tests before committing code**. In order to ensure trunk is kept in a working state, it's essential that tests are run against code changes before commit. This can be done on developer workstations, and many tools also provide a facility to run tests remotely against local changes and then commit automatically when they pass. When developers know that they can get their code into trunk without a great deal of ceremony, the result is small code changes that are easy to understand, review, test, and which can be moved into production faster.


## Actions to take

- Develop in small batches (even smaller than we are used to)
- Speed up code review so that commits do not need to wait long times to get into `main`.
- Have comprehensive, automated testing
- Have a fast build

## Metrics

Metric | Goal
--- | ---
Number of active branches (not including the ones developers create for their tasks) | Three or less
Code freeze periods (merge, stabilization, etc.) | No code freeze
Frequency of merging branches/forks to trunk | At least once per day
Time spent in code review (includes time waiting for code review) | Average code review time per PR less than 30 min

We strongly encourage you to go through the following:

- [A synthetic description of the journey of our goals](assets/Trunk_Correlated_Practices_v2.8.pdf) - sourced from `https://devops.paulhammant.com/trunk-correlated-practices-chart/`
- [A tool to calculate the DevOps performance our our team](assets/Sofware_dev_and_delivery_project_guidelines.xlsx) - sourced from `https://paulhammant.com/2021/10/08/software-project-guidelines/`

## Feature flags

To be used extensively to control the stability of the releases in production.