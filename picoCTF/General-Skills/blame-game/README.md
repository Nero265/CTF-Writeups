# Blame Game

**Category:** General Skills  
**Platform:** picoCTF  
**Tools:** Git, `git log`, `git blame`  

## Recon

The challenge provided a `drop-in/` folder containing a single file, `message.py`:

```python
print("Hello, World!"
```

Running the script failed with a `SyntaxError` due to a missing closing parenthesis:

```
python3 message.py
  File ".../message.py", line 1
    print("Hello, World!"
         ^
SyntaxError:
```

The challenge prompt hinted that a commit was responsible for breaking the file, so the next step was to inspect the file's Git history.

## Investigation

`git show` (no arguments) only displayed the current `HEAD` commit:

```
commit 83afd3ebd7899251a19d290df92fd1bfc9998adb (HEAD -> master)
Author: picoCTF <ops@picoctf.com>
    important business work
```

Since this only shows one commit, `git log --oneline --all` was used to check the full history across all branches:

```
git log --oneline --all
```

This revealed **hundreds of commits**, all on a single branch (`master`), and all sharing the identical, uninformative commit message `"important business work"`. This ruled out reading commit messages/diffs one by one as a practical strategy — the history was deliberately padded to obscure the real change.

## Exploit/Extraction

Since the noise was in the commit *messages*, not the file content, `git blame` was used to directly attribute the current line in `message.py` to whichever commit last touched it:

```bash
git blame message.py
```

Output:

```
0351e047 (picoCTF{@sk_th3_1nt3rn_d2d29f22} 2024-03-12 00:07:01 +0000 1) print("Hello, World!"
```

The **author name** of the responsible commit was not a real name — it was the flag itself, stored via `git commit --author="<flag> <email>"`.

**Flag:** `picoCTF{@sk_th3_1nt3rn_d2d29f22}`

## Lessons Learned

- `git log`/`git show` without arguments only reflect `HEAD` — they don't reveal a padded or obscured history by themselves. `--all` and `--oneline` are essential first checks when a challenge history looks suspiciously short or repetitive.
- When commit *messages* are deliberately uninformative (identical spam messages across hundreds of commits), don't try to read them individually — pivot to `git blame`, which attributes each line of a file to its origin commit regardless of how much history sits on top of it.
- Git's `--author` field accepts an arbitrary string, not just real names — a reminder that metadata fields (author, committer, email) are worth checking directly with `git log --format="%an <%ae>"` when something feels hidden, not just commit content.
