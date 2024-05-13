Ingredient-Chef Action Aiming Flowchart
=======================================

```mermaid
flowchart TD
start([Focus on Ingredient]) --> pressSend
pressSend[/Press Send Up/] --> anyValid
anyValid{Any valid Chef Actions?} -->|No| cancelAim
anyValid -->|Yes| startAim
cancelAim([Cancel aiming])
startAim[Start aiming mode] --> aimInput
aimInput[/Left and Right to select, Accept to accept, Cancel to cancel/] --> actionSelected
actionSelected{Accepted or cancelled?} -->|Cancelled| cancelAim
actionSelected -->|Accepted| moveToSlot
moveToSlot[Move Ingredient to Chef Action, Start Action, Disable Ingredient while Action runs] --> stopAiming
stopAiming[Stop aiming mode] --> finish
finish([Done aiming])
```

After sending, focus should be moved to the first ingredient in the area that was left, or if not possible, the
left-most ingredient in the pantry, prep area, or plating area in that order.
