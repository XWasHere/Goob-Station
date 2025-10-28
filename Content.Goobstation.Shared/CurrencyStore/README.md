# Goobstation Currency Store System
This folder contains most of the logic for the Goobcoin shop, the system is quite
complex and has a lot of moving parts, so I have opted to document it here so that
it is easier for people to use or add onto it in the future. If you make changes to
the Goobshop's core code, please edit this document accordingly.

## Overview
In order to make it easier to add new behaviors to the store and it's items, the
effects that tokens have are kept entirely separate from the store code. This allows
less experienced contributors to add functionality to the store without having to have
an in-depth understanding of how it works internally. Unless you are changing the
store itself, the only folders that you will likely need to touch are the Conditions
and Effects folders. Predicted conditions are kept in Content.Goobstation.Shared, while
effects and conditions that cannot be predicted are kept in Content.Goobstation.Server.

## Todo before merging
- [ ] Merge new database migrations into a single really big migration.
- [ ] Get item list from admins
- [ ] Make ServerCurrencyStoreManager block the main thread less
- [ ] Why the fuck is ServerCurrencyStoreSystem also using NetMessages???
    - [ ] Use a RequestActivationMessage and a RequestActivationResponseMessage event
- [x] Replace hardcoded strings with localized strings
- [x] Move all item condition and activation code to ServerCurrencyStoreSystem
- [x] Change ServerCurrencyStoreManager to output error strings to a output parameter instead of passing in a channel so that ServerCurrencyStoreSystem can use the messages in item activation requests/responses when they're made into events.
- [ ] The voucher/item DB code is almost exactly the same. Find ways to reuse/simplify it
- [ ] Ask admins whether or not immediate items redeemed from vouchers should be immediate or not.
- [x] Remove the ability to depend on SharedCurrencyStoreManager. It shouldn't be used for any reason.
  - [x] Remove SharedCurrencyStoreManager entirely. The server and client have completely different needs.
- [ ] Autocomplete for the token and permanentitem commands that filter out permanent items and tokens respectively.
- [ ] Try to split up items and permanent items (DO: Holy Shit)
- [ ] ServerCurrencyStoreSystem thread safety larp.
- [ ] Split up CurrencyStoreSystem and CurrencyStoreManager as partial classes.
- [ ] Explain in this document how to add conditions and effects
- [ ] Explain in this document how to utilize permanent items in other systems
- [ ] Explain in this document how to make new categories in YAML
- [ ] Explain in this document how to make new vouchers in YAML
- [ ] Explain in this document how to make new items in YAML

## NOTES

CURRENCYSTORESYSTEM:
- item activation
- item conditions
- items in general
- generally items simulation side

CURRENCYSTOREMANAGER:
- item ownership
- generally items database side
- vouchers
- permanent items

STICK TO THE PLAN\
STICK TO THE PLAN\
STICK TO THE PLAN

## See Also
- [The Goobshop design document](https://discord.com/channels/1202734573247795300/1413434936077189150/1420829711059255327)
