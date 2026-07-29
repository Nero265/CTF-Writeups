# Git History Forensics — Force Push

**Category:** Forensics

**Difficulty:** Easy

## Scenario
A leaked backup of the `crownspire-deploy` repository allegedly had production
credentials committed by mistake, then "cleaned up" from history.

## Methodology

1. Checked visible history — looked clean:

   git log --oneline --all

2. Rebase/reset removes commits from branch history but doesn't delete the
   underlying objects until `git gc --prune=now` runs. Checked for orphaned
   objects:

   git fsck --full --no-reflog

   → found a dangling commit: `3c8803d7146cd07c75325d6b555116200f2569e`

3. Inspected the orphaned commit directly:

   git show 3c8803d7146cd07c75325d6b555116200f2569e

   Revealed a `reliquary.creds` file added in a commit titled
   `temp: add reliquary.creds to debug 403 on manifest push (REVERT ME)`.

## Flag
`HTB{th3_r3l1qu4ry_n3v3r_f0rg3ts}`

## Takeaway
Commits removed from branch history remain recoverable as dangling objects
until garbage collection runs. `git fsck --full --no-reflog` surfaces them.
