---
name: track-chat
description: Create a GitHub issue/branch/PR for the current chat session's work, then implement the work itself
---
Create a GitHub issue in this repository (`syntaxonline-rewhitley/Service-Tracker-POC`) to track the
current chat session, using the `gh` CLI, then actually do the work needed to resolve it on a
dedicated branch. Extra instructions from the user, if any: $ARGUMENTS.

This skill assumes tracking has already been judged worthwhile — at session start that judgment
call belongs to the `intern` agent (`.claude/agents/intern.md`), not this
skill. If you're invoking this directly without going through that agent, make sure a tracked
issue doesn't already exist for this work before proceeding.

1. Review the conversation so far and summarize:
   - What the user asked for / the goal of this chat.
   - What has been done so far (files touched, decisions made) — check `git status` / `git diff`
     for concrete evidence rather than relying only on memory of the conversation.
   - What remains outstanding, if anything.
2. Draft an issue:
   - **Title**: short, specific, under ~70 characters, describing the task (not "Chat session on <date>").
   - **Body**: use these sections, omitting any that are empty:
     - `## Summary` — 1-3 sentences on the goal.
     - `## Work done` — bullet list of concrete changes/findings.
     - `## Remaining` — bullet list of open items, or omit if nothing remains.
     - A trailing note identifying it as chat-generated, e.g.
       `_Filed from a Claude Code chat on <YYYY-MM-DD>._`
   - Do not include secrets, credentials, tokens, or full stack traces from logs.
3. Show the drafted title and body to the user and ask for confirmation (or edits) before creating
   anything — creating a GitHub issue is a visible action on a shared system.
4. Once confirmed, create the issue:
   ```bash
   gh issue create --repo syntaxonline-rewhitley/Service-Tracker-POC --title "<title>" --body "<body>"
   ```
   Add `--label` flags only for labels that already exist in the repo (check with
   `gh label list --repo syntaxonline-rewhitley/Service-Tracker-POC` first) — don't guess at labels.
   Note the issue number from the URL `gh issue create` returns.
5. Create a feature branch to track work against the issue:
   - Check `git status` first; if there are uncommitted changes, stop and ask the user how to
     proceed rather than switching branches out from under them.
   - Note the current branch name before switching — this is the **source branch** the work will
     eventually merge back into.
   - Slugify the issue title (lowercase, spaces/punctuation → `-`, trimmed) and branch from the
     current branch, following the `feature/`-prefixed naming convention in
     `.claude/rules/git.md`:
     ```bash
     git checkout -b feature/issue-<number>-<slug>
     ```
   - Push the new branch to remote and set upstream:
     `git push -u origin feature/issue-<number>-<slug>`.
6. Immediately open a **draft** pull request from this branch back into the source branch noted
   in step 5 — do not wait for the user to say the work is complete:
   - Create the draft PR, referencing the tracked issue so it auto-closes on merge:
     ```bash
     gh pr create --repo syntaxonline-rewhitley/Service-Tracker-POC --draft \
       --base <source-branch> --head feature/issue-<number>-<slug> \
       --title "<title>" --body "Closes #<number>

     <summary of the work done so far, or a note that work is just starting>"
     ```
   - Show the drafted PR title/body to the user for confirmation before running `gh pr create`,
     same as with the issue — this is also a visible action on a shared system.
   - Report back the PR URL returned by `gh pr create`.
7. Do the work to resolve the issue, on this branch, before finishing:
   - Treat the issue body as the task spec. Read whatever existing code/config is relevant before
     changing it, and implement the change(s) needed to address the user's original request —
     don't stop at just filing the issue and opening an empty PR.
   - Follow this repo's normal engineering conventions while doing so: `CLAUDE.md` for
     build/test commands (`dotnet build`/`dotnet test` for backend changes, `npm run build` for
     frontend changes) and architecture notes, plus `.claude/rules/code-style.md` and
     `.claude/rules/security.md`. Run the relevant build/test command before considering the work
     done.
   - Commit as you would for any other task (see the main commit guidelines — only commit when
     it's the natural conclusion of a unit of work, not one commit per file) and push to
     `feature/issue-<number>-<slug>` so the draft PR reflects real progress.
   - If the request turns out to be ambiguous, larger than expected, or you hit a decision only
     the user can make, stop and ask — the same judgment calls that apply to any other
     implementation task apply here too. Filing the issue/PR doesn't authorize guessing on scope.
   - If the conversation that triggered this skill was purely a question or exploration with no
     concrete change to make (this shouldn't normally reach this skill — see the note about the
     `intern` agent above — but can happen on a direct invocation), say so plainly instead of
     inventing busywork, and leave the PR as an empty draft for the user to close or fill in.
8. Once the change is in place (or you've paused for input per step 7), update the draft PR
   description via `gh pr edit <number> --body "<updated body>"` so its `## Work done` /
   `## Remaining` sections reflect what actually happened, then report back the issue URL, branch
   name, PR URL, and a short summary of what was implemented.
9. Do not mark the PR ready for review yourself — that stays the user's call. If asked to keep
   working on this same tracked issue later in the session, keep committing to the same branch
   and keep the PR description in sync; there is no separate "PR creation" step to repeat, since
   it already exists from step 6.
