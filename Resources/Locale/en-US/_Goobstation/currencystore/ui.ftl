currencystore-unknown-user = an unknown user

currencystore-item-activated = Your {$item} was activated.

# inventory change notifications
currencystore-event-item-add-generic = You got {INDEFINITE($item)} {$item}.
currencystore-event-item-add-admin = You were given {INDEFINITE($item)} {$item} by {$actor}.
currencystore-event-item-add-transfer = You were given {INDEFINITE($item)} {$item} by {$actor}.
currencystore-event-item-add-purchase = You purchased {INDEFINITE($item)} {$item}.
currencystore-event-item-remove-generic = Your {$item} was removed.
currencystore-event-item-remove-admin = Your {$item} was removed by {$actor}.
currencystore-event-item-remove-transfer = Your {$item} was transferred to {$owner}
currencystore-event-item-remove-purchase = You purchased {INDEFINITE($item)} {$item}, but due to a catastrophic bug, lost it instead.
currencystore-event-voucher-add-generic = You got {INDEFINITE($voucher)} {$item}.
currencystore-event-voucher-add-admin = You were given {INDEFINITE($item)} {$item} by {$actor}.
currencystore-event-voucher-add-transfer = You were given {INDEFINITE($item)} {$item} by {$actor}.
currencystore-event-voucher-add-purchase = You purchased {INDEFINITE($item)} {$item}.
currencystore-event-voucher-remove-generic = Your {$item} was removed.
currencystore-event-voucher-remove-admin = Your {$item} was removed by {$actor}.
currencystore-event-voucher-remove-transfer = Your {$item} was transferred to {$owner}
currencystore-event-voucher-remove-purchase = You purchased {INDEFINITE($item)} {$item}, but due to a catastrophic bug
currencystore-event-permanent-add-generic = You got {INDEFINITE($item)} {$item}.
currencystore-event-permanent-add-admin = You were given {INDEFINITE($item)} {$item} by {$actor}.
currencystore-event-permanent-add-transfer = You were given {INDEFINITE($item)} {$item} by {$actor}.
currencystore-event-permanent-add-purchase = You purchased {INDEFINITE($item)} {$item}.
currencystore-event-permanent-remove-generic = Your {$item} was removed.
currencystore-event-permanent-remove-admin = Your {$item} was removed by {$actor}.
currencystore-event-permanent-remove-transfer = Your {$item} was transferred to {$owner}
currencystore-event-permanent-remove-purchase = You purchased {INDEFINITE($item)} {$item}, but due to a catastrophic bug, lost it instead., lost it instead.

# other errors
currencystore-error-prototype = Invalid prototype
currencystore-error-condition = You cannot activate this item right now: {$reason}
currencystore-error-immediatefailure = Failed to activate item, waiting until next round.
currencystore-error-roundstate = You cannot activate this item right now.
currencystore-error-offline = Player is currently offline.
currencystore-error-notowned = You do not own this item.
currencystore-error-noimmediate = This server does not allow transferring items that are pending activation.
currencystore-error-alreadyowned = You already own this item.
currencystore-error-hidden = This item is not available for purchase.
currencystore-error-broke = You cannot afford this item.
currencystore-error-voucherdisallowed = You cannot redeem this voucher for this item.

# it's the same wrap that popupsystem uses
currencystore-chat-notification-message-wrap = [font size=12][color=#AEABC4]{$message}[/color][/font]

# store eui
currencystore-ui-open-store = Server Store

# store item trait descriptions
currencystore-item-trait-uses = { $uses ->
    [1] Single use
    *[other] {$uses} uses
}
currencystore-item-trait-immediate = Activated on purchase
currencystore-item-trait-redeemable-inround = Redeemable in-round
currencystore-item-trait-redeemable-preround = Redeemable pre-round
currencystore-item-trait-permanent = One-time purchase
