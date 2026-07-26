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
5. Report back the issue URL returned by `gh issue create`.
