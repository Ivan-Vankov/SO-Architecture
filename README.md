# Scriptable Object (SO) Architecture
[![Releases](https://img.shields.io/github/release/Ivan-Vankov/SO-Architecture.svg)](https://github.com/Ivan-Vankov/SO-Architecture/releases)

SO Architecture provides scriptable object-based game events, game event listeners and constants.

The project is based on [Ryan Hipple's talk at Unite Austin 2017](https://www.youtube.com/watch?v=raQ3iHhE_Kk).

Game events named `<Game Event Name> Event` can be referenced in code as `GameEvents.<Game Event Name>`.

Constants named `<Constant Name> Constant` can be referenced in code as `Constants.<Constant Name>`.

## Requirements
Api Compatibility Level should be set to .NET 4.x. It can be set in:
```
Edit > Project Settings > Player > Other Settings > Api Compatibility Level
```
## Installation
Add `https://github.com/Ivan-Vankov/SO-Architecture.git` in:
```
Window > Package Manager > + > Add package from git URL...
```
