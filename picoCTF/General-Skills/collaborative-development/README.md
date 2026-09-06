# Collaborative Development

**Category:** General Skills  
**Difficulty:** Easy  
**Platform:** picoCTF 2024  
**Author:** Jeffery John  

## Challenge Description

> My team has been working very hard on new features for our flag printing program! I wonder how they'll work together?

Provided file: [`challenge.zip`](https://artifacts.picoctf.net/c_titan/70/challenge.zip)

## Tools

- `git` — branch inspection and switching
- `python3` — running the flag-printing script

## Recon

Unzipping `challenge.zip` revealed a project folder (`drop-in`) containing a single visible file, `flag.py`, plus a full `.git` directory — meaning this wasn't just a script drop, but an entire repository with history.

```bash
cd drop-in
cat flag.py
```

```python
print("Printing the flag...")
```

Running it produced only `Printing the flag...` — no actual flag. `git status` on `main` showed a clean working tree with nothing hidden in uncommitted changes, so the real content had to be elsewhere in the repo's history or branches rather than in the working directory.

## Investigation

Checking all branches (not just `main`) was the key step:

```bash
git branch -a
```

```
  feature/part-1
  feature/part-2
  feature/part-3
* main
```

Three unmerged feature branches existed alongside `main` — exactly the "team working on features" the prompt hinted at. Each branch's `main` never received these commits, so `flag.py` on `main` stayed stuck at its placeholder print statement while the real logic sat isolated on each feature branch.

## Exploit

Switched to each feature branch in turn and inspected `flag.py`:

```bash
git switch feature/part-1
cat flag.py
```
```python
print("Printing the flag...")
print("picoCTF{t3@mw0rk_", end='')
```

```bash
git switch feature/part-2
cat flag.py
```
```python
print("Printing the flag...")

print("m@k3s_th3_dr3@m_", end='')
```

```bash
git switch feature/part-3
cat flag.py
```
```python
print("Printing the flag...")

print("w0rk_7ffa0077}")
```

Each branch prints one fragment of the flag string (using `end=''` on the first two to avoid inserting newlines mid-flag). Concatenating the three fragments in branch order reconstructs the full flag:

```
t3@mw0rk_  +  m@k3s_th3_dr3@m_  +  w0rk_7ffa0077}
```

```
picoCTF{t3@mw0rk_m@k3s_th3_dr3@m_w0rk_7ffa0077}
```

(Read aloud: "teamwork makes the dream work" — a fitting pun for a challenge about unmerged feature branches.)

## Lessons Learned

- **Unmerged branches can hold the entire point of a challenge.** `git branch -a` should be one of the very first commands run whenever a `.git` directory is present — `git status`/`git log` on the current branch alone only shows one slice of the repository's actual content.
- **Manually re-typing fragments across separate shell commands invites transcription errors.** While reconstructing the flag by hand with `echo ... >> flag.txt`, an unescaped `}` triggered a zsh brace-expansion parse error on the first attempt; retyping the command without the brace also accidentally dropped the leading `w`, silently producing a broken flag (`..._dr3@m_0rk_...` instead of `..._dr3@m_w0rk_...`). Piecing together multi-part strings is safer done in a text editor (or a single Python one-liner reading directly from each branch) than via several separate shell commands, and any reconstructed value worth submitting is worth a quick character-by-character diff against the source before trusting it.
- **Quoting matters in interactive shells.** `{`/`}` are meaningful to zsh (brace expansion); wrapping the string in quotes (`echo 'w0rk_7ffa0077}' >> ../flag.txt`) would have avoided the parse error in the first place without needing to omit the character.
