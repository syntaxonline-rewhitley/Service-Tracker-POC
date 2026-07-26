---
name: track-chat
description: Create a GitHub issue that tracks the current chat session's work
disable-model-invocation: true
---
Create a GitHub issue in this repository (`syntaxonline-rewhitley/Service-Tracker-POC`) to track the
current chat session, using the `gh` CLI. Extra instructions from the user, if any: $ARGUMENTS.

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
     current branch:
     ```bash
     git checkout -b issue-<number>-<slug>
     ```
   - Do not push the branch automatically — leave that to the user unless they ask for it.
6. Report back the issue URL and the branch name created.
7. When the user later indicates the work on this branch is complete, open a **draft** pull
   request back into the source branch noted in step 5:
   - Confirm there are no uncommitted changes (`git status`); if there are, ask the user whether
     to commit them first rather than opening the PR against a dirty tree.
   - Push the branch if it isn't already on the remote: `git push -u origin <branch>`.
   - Create the draft PR, referencing the tracked issue so it auto-closes on merge:
     ```bash
     gh pr create --repo syntaxonline-rewhitley/Service-Tracker-POC --draft \
       --base <source-branch> --head <branch> \
       --title "<title>" --body "Closes #<number>

     <summary of the work done>"
     ```
   - Show the drafted PR title/body to the user for confirmation before running `gh pr create`,
     same as with the issue — this is also a visible action on a shared system.
   - Report back the PR URL returned by `gh pr create`.
