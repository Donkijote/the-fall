# Development Workflow

Status: Confirmed

## Source of work

- All planned work starts with a GitHub issue.
- Each issue is assigned exactly one primary `kind/*` label.
- Issues are added to the The Fall GitHub project.
- New scoped work enters `Ready`; active work moves to `In progress`.

## Base branch

`main` is the base branch for issue work. This repository does not use `develop`.

## Issue branches

Use:

```text
<category>/ghi#<issue-number>
```

Examples:

- `documentation/ghi#1`
- `feature/ghi#24`
- `bugfix/ghi#38`
- `refactoring/ghi#51`

Supported categories:

- `feature`
- `improvement`
- `bugfix`
- `fix`
- `hotfix`
- `refactoring`
- `internal`
- `documentation`

Do not shorten the branch to `<category>/<issue-number>` and do not remove the literal `ghi#` segment.

## Pull requests

- Create a draft PR when issue work begins.
- Target `main`.
- Prefix the title with `[GHI#<issue-number>]`.
- Include `Closes #<issue-number>`.
- Keep changes within the issue scope.
- Document validation and unresolved risks before marking ready.

## Commits

Use Conventional Commits:

```text
type(optional-scope): imperative description
```

Common types include `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `ci`, and `build`.

## Protecting work

- Never discard local work without explicit approval.
- Stage only issue-relevant files.
- Do not mix project cleanup, product features, and documentation unless the issue explicitly owns them.
- Use `--force-with-lease` only when a justified rebase changes an already-pushed personal branch; never use an unconditional force push.

## Documentation expectation

Update the relevant documentation in the same PR when implementation changes an established rule, decision, workflow, or architectural boundary.
