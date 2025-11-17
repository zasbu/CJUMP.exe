# CJUMP

CJUMP.exe is a small utility that automates crouch-jumping in Counter-Strike: Source.  
You hold your jump key, and the program handles pressing and releasing duck for you with a consistent delay. It only activates when CS:S is the focused window and it backs off while you're typing or tabbed out.

This is for players who want to crouch-jump while still holding space (or any custom jump key) without having to use a differnt key to crouch-jump.

---

## Features

- Hold jump to perform a crouch-jump
- Automatic duck release with a configurable delay
- Custom key binds for jump and duck
- Typing suppression so it never messes with chat or console
- Auto-pauses when CS:S isn't the active window
- Config file stored next to the executable (`cjump.cfg`)

---

## VAC Safety

CJUMP.exe never injects into the game, never reads game memory, and never writes to it.  
It listens for global keys and sends normal OS-level key events back through Windows.

There is no DLL injection, no signature, no hooking into the CS:S process, and no direct interaction with the engine. Because it behaves like a regular keyboard device at the OS layer, it is **VAC safe**.

---

## Requirements

- [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

---

## Usage

1. Launch CJUMP.exe  
2. Set your jump key, duck key, and preferred delay  
3. Press **Update** to save the config  
4. Open CS:S — the tool enables itself automatically once the game is focused  
5. Hold your jump key and it will handle the crouch timing

**Important:** In CS:S, make sure your jump key (usually SPACE) is actually bound to `+jump`.  
You can confirm by typing this in console:  
bind space +jump

---

## Preview


`![CJUMP.exe](assets/screenshot.png)`
