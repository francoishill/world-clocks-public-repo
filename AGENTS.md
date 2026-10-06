# AGENTS.md

## Jira is the source of truth

Site: francois-hill.atlassian.net (cloudId `f4aafb30-18ae-4240-a02d-a99f7f772c9a`), project `FP`. Use the Atlassian MCP.

- Substantive work (anything that changes behavior) needs a Jira work item. Create one first if missing. Typos, formatting and doc wording are exempt.
- Keep the item's status and comments current: comment on the plan, implementation notes, verification and PR link.
- Branch `FP-123-short-name`; PR title starts with `FP-123`; PR body says `Relates to FP-123`, never a closing keyword.
- Merging a PR does not complete the item. Move it to Done only after the user confirms manual testing in the item.
