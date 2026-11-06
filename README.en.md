# Underground Gunsmith Permits & Custom Gun Orders

中文: [README.md](README.md)

Adds 3 underground gunsmith permits to the game. They work exactly like the vanilla Weapon Permits, except that Security never revokes them for low reputation, and they never convert the contraband tag on weapons into the restricted tag.

Note that this mod's permits do not change how contraband is sold — you will still be investigated by Security for smuggling.

What you get: the same Custom Gun Order customers as the vanilla Weapon Permits, and the advanced stock the retired gunsmith sells.

Requires game version **0.46D** (Harmony patches may stop working after major updates).

## Build from source

```bash
dotnet build -c Release
# output goes to <game folder>/Mods/UndergroundGunsmith/
```

Assumes the repo sits at `<game folder>/Modding/UndergroundGunsmith/`; otherwise:

```bash
dotnet build -c Release -p:GameDir="D:\Steam\steamapps\common\Probably Stolen\"
```

## Compatibility

- Customers, order pool, stock pool and settlement all reuse the vanilla implementation, so other mods' edits to those apply here too.
- If another mod also rewrites the display-case customer prediction or the network upgrade icon lookup, both run.
- Nothing is written to `ActivePermits`, so vanilla weapon permits and chemical-permit style mods are unaffected.

## License

[MIT](LICENSE)
