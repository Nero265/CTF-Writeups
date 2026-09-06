# Special

**Category:** General Skills  
**Difficulty:** Medium  
**Platform:** picoCTF 2023  
**Author:** LT 'syreal' Jones  

## Challenge Description

> Don't power users get tired of making spelling mistakes in the shell? Not anymore! Enter Special, the Spell Checked Interface for Affecting Linux. Now, every word is properly spelled and capitalized... automatically and behind-the-scenes!

Connection: `ssh -p <port> ctf-player@saturn.picoctf.net`

## Tools

- `ssh`
- Only the remote "Special" shell itself — no local tooling required, this is pure blind-box behavioral probing

## Recon

Logging in drops straight into a custom `Special$` prompt instead of a normal shell. Every command typed gets echoed back — but altered — before (or instead of) executing:

```
Special$ ls
Is
sh: 1: Is: not found
```

`ls` became `Is` and failed. Testing single characters and other common commands confirmed a pattern: input is being run through some kind of spell-checker/autocorrect before reaching the underlying shell.

## Investigation

Systematically probed how tokens get rewritten:

```
Special$ /
Absolutely not paths like that, please!      # explicit hard block, not spell-check behavior

Special$ t
I
sh: 1: I: not found

Special$ echo
Echo
sh: 1: Echo: not found

Special$ .
.                                              # single punctuation char: untouched

Special$ ..
I
sh: 1: I: not found

Special$ . .
. .
sh: 1: .: .: not found

Special$ .:
I
sh: 1: I: not found

Special$ :
:                                               # single punctuation char: untouched

Special$ :ls
Als
sh: 1: Als: not found
```

Pattern that emerged: the line is tokenized on whitespace, and each token is run through a spell-checker (autocorrect + capitalization, à la `aspell`/`ispell`). If a token's alphabetic content isn't a valid dictionary word, it gets replaced with the nearest suggestion and capitalized (`ls` → `Is`, `t` → `I`). If it's already a valid word, it passes through — capitalized, but otherwise intact (`echo` → `Echo`, still fails only because it's now capitalized). Isolated single-character punctuation (`.`, `:`) seemed to dodge correction entirely, while punctuation-only tokens with no letters left after stripping (`..`, `.:`) collapsed to an empty "word" and got the generic default suggestion `I`.

Testing a real dictionary word that also happens to be a Unix command confirmed the theory directly:

```
Special$ ((cat))
((cat))
                                                 # hangs waiting on stdin — cat ran, untouched
```

`cat` is a valid English word, so the whole token — parentheses included — passed through unmodified and `cat` actually executed. The same held for `ls`:

```
Special$ ((ls))
((ls))
blargh
```

This confirmed the core bypass idea: **wrap a command in a way that keeps the spell-checker from flagging it**, either by using a command that's already an English word, or by finding some other trick to make it skip correction.

## Exploit

`ls` (via `((ls))`) revealed a file: `blargh`. Reading it directly failed — as a bare token, `blargh` isn't a dictionary word, so it got "corrected":

```
Special$ ((cat)) < blargh
((cat)) < large
sh: 1: cannot open large: No such file
```

`blargh` → `large` (nearest dictionary suggestion). The fix: append a path-like suffix containing a `/`. A token containing a slash appears to fall outside whatever the spell-checker treats as a checkable "word" entirely, so it passes through completely unmodified — even though `blargh` itself still isn't a real word:

```
Special$ ((cat)) < blargh/flag.txt
((cat)) < blargh/flag.txt
picoCTF{5p311ch3ck_15_7h3_w0r57_3befb794}
```

Turns out `blargh` was actually a directory containing `flag.txt`, not a file — the earlier `cat blargh` failure was doubly doomed (wrong filename *and* would have been "is a directory" even if spelled right).

## Lessons Learned

- **Any input filter that operates per-token can potentially be defeated by making the token something the filter doesn't recognize as checkable in the first place**, rather than fighting to make the token pass the filter's actual validation logic. Here, slashes exempted a token from spell-checking entirely — a much more general bypass than hunting for commands that coincidentally happen to be real English words.
- **Systematic single-variable probing pays off against black-box input transforms.** Testing one character, one punctuation mark, and one known word at a time (rather than guessing full commands) isolated exactly which property (word-validity, single-char punctuation, presence of `/`) controlled the behavior.
- **A restrictive-looking custom shell's blocklist can be narrower than it looks.** The explicit `/` rejection only fired on a token that was *just* `/` — it didn't generalize to slashes appearing anywhere inside a longer token, which is exactly the gap the final exploit used.
