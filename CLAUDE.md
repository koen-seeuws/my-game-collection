# CLAUDE.md

## Git workflow

Every feature or change I ask for goes through its own branch and pull request — never commit directly to `main`.

- **New feature/change:** start from an up-to-date `main` (`git checkout main && git pull`), create a new branch, commit the work there, push it, and open a PR to `main` with `gh pr create`.
- **Branch naming:** use a prefix that matches the kind of change, followed by a short kebab-case description, e.g. `feature/postgres-persistence`, `fix/duplicate-game-entries`, `chore/flatten-solution-folders`.
- **Follow-up changes to the same feature:** if I ask for changes to a feature that already has a branch/PR (open, not yet merged), check out that existing branch, commit there and push, so the existing PR gets updated. Do not create a new branch or PR for it.
- **Unsure which branch?** If it's unclear whether a request belongs to an existing feature branch or is a new change, ask before committing.
- Once a feature's PR is merged, any further changes to it are a new change: new branch from `main`, new PR.
