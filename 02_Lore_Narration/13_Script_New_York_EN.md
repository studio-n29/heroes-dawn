# New York script, English pass: "the showcase city"

*Date: 2026-11-16 · Status: English pass of `13_Script_New_York.md`, **to be reviewed**. The French script is canon: any change goes there first, then here. Same keys, same speakers (see the speaker table in the French file). Fragment and QG texts are included.*

**Glossary used throughout:** L'Aube → The Dawn · le Syndicat → the Syndicate · le Régisseur → the Steward · AMIF stays AMIF · Apex stays Apex · les Accords de Salem → the Salem Accords · l'Aube (team) → The Dawn / the squad.

## Speaker display names (EN)

| Id | EN name |
|---|---|
| `narrateur` | Narrator |
| `agent_d` / `agent_m` | Agent D / Agent M |
| `ilona_varga` / `giulia_rossi` / `lucas_mendes` | Ilona Varga / Giulia Rossi / Lucas Mendes |
| `rayner` / `stroke` / `general` | Brie Rayner / General Stroke / The General |
| `darius_brooks` / `brick` / `lieutenant` / `crocodile` | Darius Brooks ("Anchor") / Brick / Crocodile's Lieutenant / Crocodile |
| `regisseur` | Elias Voss, the Steward |
| `indic` / `captif` / `commercant` / `habitant` | An Informant / A Captive / A Vendor / A Resident |
| `braise` / `dr_colby` / `theo` | Dante "Braise" Reyes / Dr. Amina Colby / Th\xE9o |
| `enforcer` / `agent_amif` / `voix` | Enforcer / An AMIF Agent / Propaganda Voice |

**Route variants:** a replacement variant carries its own key suffixed `.d` (Agent D) or `.m` (Agent M) and replaces the base line for that route only; route-only additions have their own key.

---

## Chapter 1: Brooklyn

**Opening text:** *Red Hook, Gowanus, Dumbo: Brooklyn is the first front. The AMIF and the gangs, which pretend to fight each other, are quietly splitting the city between them. The Dawn proves itself here, but every success draws a little more of the AMIF's attention.*

| Route | Added opening sentence |
|---|---|
| Agent D | *He returns to the docks where the unseen is sold. Brooklyn is the first place where he shows himself.* |
| Agent M | *She saw in Moscow how an agency can buy a gang. Brooklyn shows her how one gets rid of it.* |

#### zone_ny_1_1: Red Hook, the docks

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_1_1.n23.before.0` | brick | "You crossed my kingdom of rust and salt to die on this dock. Admirable. Pointless." |
| `narration.ny_1_1.n23.before.1` | darius_brooks | Anchor, you're the wall. Frost, Echo: cover him. This night is ours, or no one will see it end. |
| `narration.ny_1_1.n23.after.0` | darius_brooks | Brick collapses among the containers, and the roar of the port falls silent. The cages are open, the ledgers speak: the truth is in your hands. |
| `narration.ny_1_1.n23.after.1` | darius_brooks | Leave before dawn. The AMIF will be back, and when it returns, it will find heroes. |

**Fragment (proposed).** `ny_redhook_docks`: **Cages Under the Cranes**: *There were cages on the dock, with numbers painted on them. You understood that the people being chained had no name to the ones selling them. That night, The Dawn became a name.*

**QG.** Existing in the code.

#### zone_ny_1_2: Brownsville, the hostages

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_1_2.n14.before.0` | lieutenant | "Crocodile gave me the keys to hell. Come in: he's kept a seat for you." |
| `narration.ny_1_2.n14.before.1` | darius_brooks | Innocents behind him. A door in front of us. We don't stop, not tonight. |
| `narration.ny_1_2.n14.after.0` | lieutenant | "You think you've won? You've just lit the fuse. Crocodile will rain fire down on your heads." |
| `narration.ny_1_2.n19.after.0` | darius_brooks | The chains fall. The hostages breathe free air and, for the first time in a long while, look up at a sky that doesn't lie to them. |
| `narration.ny_1_2.n19.after.1` | darius_brooks | But the shadows are lengthening. Crocodile never forgets, and what he swears, he writes in blood. |

**Fragment (proposed).** `ny_brownsville_cells`: **Crocodile's Cells**: *Behind every door, a name no one dared say. You opened the cells one by one, and each time someone looked up as though the light were a promise.*

#### zone_ny_1_3: Coney Island, the trap

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_1_3.n18.before.0` | crocodile | "All of The Dawn on my island... I wouldn't have dared dream of such a gift. Welcome to your own funeral." |
| `narration.ny_1_3.n18.before.1` | darius_brooks | We knew it was a trap. No matter: when the world calls, we answer. Civilians first, then him. |
| `narration.ny_1_3.n18.after.0` | crocodile | "Impossible... I am... a king..." |
| `narration.ny_1_3.n18.after.1` | darius_brooks | Crocodile collapses, and the dark rides of Coney Island fall silent. Victory tastes of ash: some wounds only heal as legend. |

**Fragment (proposed).** `ny_coney_trap`: **A King on an Island**: *He waited for you at the end of his trap, and you walked in anyway. The civilians got out before the last burst of laughter. Some victories smell of ash.*

#### zone_ny_1_4: Dumbo and the Brooklyn Bridge

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_1_4.n18.before.0` | stroke | "You're surrounded. Lay down your powers at the feet of History, and perhaps History will spare you." |
| `narration.ny_1_4.n18.before.1` | darius_brooks | Hold the line. A bridge is only an oath in stone: today, we are the ones holding it. |
| `narration.ny_1_4.n18.after.0` | rayner | "Enough! Stroke, I have enough to put the whole city face to face with your little secrets. Shall I tell them here, in front of your men?" |
| `narration.ny_1_4.n18.after.1` | stroke | "... Fall back. Everyone." |
| `narration.ny_1_4.n18.after.2` | general | "Crocodile will carry the crime. One more shadow in the picture: he will make an admirable culprit." |
| `narration.ny_1_4.n18.after.3` | general | "Remember this day, Rayner. When the Salem Accords are signed, The Dawn will answer for everything." |

**Fragment (proposed).** `ny_dumbo_bridge`: **The Stone Bridge**: *A bridge is only an oath in stone. You held it until someone else spoke the name you didn't dare say. Stroke backed down, and you understood the war was only beginning.*

---

## Chapter 2: Queens

**Opening text:** *Flushing, Astoria, Jamaica: in Queens, the Syndicate rules by fear over the markets and rail yards. Captives, informants, cameras: The Dawn discovers the Syndicate is not acting alone.*

| Route | Added opening sentence |
|---|---|
| Agent D | *He knows the sound of these streets. He is about to learn who listens to them.* |
| Agent M | *She has seen this pattern before in Moscow: a gang, an agency, a contract.* |

#### zone_ny_2_1: Flushing, the market under cameras

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_2_1.intro.0` | narrateur | Flushing teems. Every stall has a camera above the register, and no one looks up. |
| `narration.ny_2_1.intro.1` | giulia_rossi | Ten cameras over a market of onions. The Syndicate doesn't miss a transaction. |
| `narration.ny_2_1.intro.2` | lucas_mendes | Or someone lends it the gear. |
| `narration.ny_2_1.intro.3` | commercant | They've been running since summer. They see us. They never help us. |
| `narration.ny_2_1.intro.4` | agent_d | If we cut them all at once, the whole market will know. We go stall by stall. |
| `narration.ny_2_1.intro.5` | commercant | You're The Dawn. Be quick, before someone talks. |
| `narration.ny_2_1.elite.before.0` | enforcer | You don't cut what protects the neighborhood. Least of all for visitors. |
| `narration.ny_2_1.elite.before.1` | ilona_varga | "Protect": are you sure that's the word they gave you? |
| `narration.ny_2_1.elite.after.0` | lucas_mendes | Look at the housing. It's not a street model. There's a logo engraved underneath. |
| `narration.ny_2_1.elite.after.1` | giulia_rossi | An AMIF contractor. The Syndicate's cameras are leased from the agency. |
| `narration.ny_2_1.exit.0` | ilona_varga | A gang leasing its eyes from a state agency. We follow the trail to Astoria. |
| `narration.ny_2_1.exit.1` | agent_d | Or we follow the cameras. They all point the same way. |

**Variants.** Agent D: `intro.4` as written. Agent M: `narration.ny_2_1.intro.4.m` — `agent_m`: "I see what each camera filmed yesterday. We cut them in order: the ones watching the register last." · Agent M, recognition (act III): `narration.ny_2_1.exit.2`, agent_m: "In Moscow it was holograms. Here, cameras. The same polite smile behind them."

**Fragment.** `ny_flushing_cameras`: **Eyes for Rent**: *Ten cameras over a market of onions, and no one looking up. They saw every coin, every face, every Apex who hesitated at a stall. Under one of them, an agency logo: it isn't the Syndicate watching you, it's whoever rents it the view.*

**QG.** Archive: *In Flushing, the market's cameras belong to the Syndicate, but their housings carry the logo of an AMIF contractor.* Thread: *The Syndicate rents its eyes to the AMIF. On to Astoria.*

#### zone_ny_2_2: Astoria, the informants

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_2_2.intro.0` | narrateur | Astoria, one in the morning. The café is half closed, the room half full. Everyone speaks in low voices. |
| `narration.ny_2_2.intro.1` | lucas_mendes | The man in the back hasn't touched his coffee. He checks the door every thirty seconds. |
| `narration.ny_2_2.intro.2` | indic | I'm not a snitch. I'm a guy with a daughter. |
| `narration.ny_2_2.intro.3` | agent_d | We're not asking you to betray anyone. We're asking you to say what we can no longer ignore. |
| `narration.ny_2_2.intro.4` | ilona_varga | He isn't lying. He's afraid for someone, not for himself. |
| `narration.ny_2_2.elite.before.0` | enforcer | Everybody stay seated. We have visitors who weren't invited. |
| `narration.ny_2_2.elite.after.0` | giulia_rossi | Out, quick. Before the others figure it out. |
| `narration.ny_2_2.exit.0` | indic | Every Thursday, the Syndicate brings a list. Names, neighborhoods, powers. A guy in a suit takes it, and never gives his name. |
| `narration.ny_2_2.exit.1` | giulia_rossi | A suit from the AMIF? |
| `narration.ny_2_2.exit.2` | indic | A suit that never gets dirty. New Apex show up on the list before they've even been afraid. |
| `narration.ny_2_2.exit.3` | ilona_varga | We're taking you to a safe house. Your daughter too. |

**Variants.** Agent D: `intro.3` as written. Agent M: `narration.ny_2_2.intro.3.m` — `agent_m`: "I see what happened to you this morning. We're not asking you to talk: we're asking you to let us listen." *(her vision replaces a dialogue)* · Agent M, recognition: `narration.ny_2_2.exit.4`, agent_m: "In Moscow they say clients. Here, lists. It's the same word."

**Fragment.** `ny_astoria_informant`: **Thursday's List**: *He wasn't a snitch, he was a father. Every Thursday a list changed hands: names, neighborhoods, powers still asleep. Someone in an immaculate suit filed it away without looking up.*

**QG.** Archive: *In Astoria, a Syndicate informant admits that lists of Apex are handed every week to an AMIF contact.* Thread: *Thursday's lists lead to Jamaica.*

#### zone_ny_2_3: Jamaica rail yard, the convoy

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_2_3.intro.0` | narrateur | The Jamaica rail yard sleeps under orange lamps. An unmarked convoy waits on track 14. |
| `narration.ny_2_3.intro.1` | lucas_mendes | No label, no number, and an armed escort. That's not freight. |
| `narration.ny_2_3.intro.2` | giulia_rossi | There are people in those cars. I can hear them breathing. |
| `narration.ny_2_3.intro.3` | agent_d | Then we don't slow down. We open the last car. |
| `narration.ny_2_3.elite.before.0` | enforcer | This convoy is under contract. Back off, or the contract will include you. |
| `narration.ny_2_3.elite.before.1` | ilona_varga | He's reciting. He doesn't even know who he works for. |
| `narration.ny_2_3.elite.after.0` | captif | You're... from The Dawn? They told us we were going somewhere safe. |
| `narration.ny_2_3.elite.after.1` | giulia_rossi | Who told you that? |
| `narration.ny_2_3.elite.after.2` | captif | A man with a badge. He said the agency would take care of us. |
| `narration.ny_2_3.exit.0` | ilona_varga | Awakened Apex, delivered like parcels. To the agency. |
| `narration.ny_2_3.exit.1` | agent_d | This convoy's ledger will be at the freight terminal. That's where we'll find it. |

**Variants.** Agent D: `intro.3` as written. Agent M: `narration.ny_2_3.intro.3.m` — `agent_m`: "I see who climbed into those cars, and what they were promised. We don't slow down." · Agent M, recognition: `narration.ny_2_3.exit.2`, agent_m: "In Moscow they call them trophies. Here, captives."

**Fragment.** `ny_jamaica_convoy`: **Track 14**: *The cars had no name or number, and yet everything about them was expected. You heard breathing behind the steel, and a voice repeating that they were going somewhere safe. It had never had a reason to distrust a badge.*

**QG.** Archive: *The Jamaica convoy carries awakened Apex, drugged and handcuffed, bound for the AMIF. The Dawn frees them.* Thread: *This convoy's ledger is at the freight terminal.*

#### zone_ny_2_4: Freight terminal

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_2_4.intro.0` | narrateur | The freight terminal overlooks the bay. At the top of a crane, a glass room watches every container. |
| `narration.ny_2_4.intro.1` | lucas_mendes | From up there, you see everything. Exactly what a manager wants. |
| `narration.ny_2_4.intro.2` | giulia_rossi | Deliveries, schedules, numbers... and beside them, the same logo as on the cameras. |
| `narration.ny_2_4.intro.3` | agent_d | We don't take excerpts. We take the whole ledger. |
| `narration.ny_2_4.elite.before.0` | enforcer | Management doesn't receive without an appointment. |
| `narration.ny_2_4.elite.after.0` | ilona_varga | There's only him up there now. And he's expecting us. |
| `narration.ny_2_4.boss.before.0` | regisseur | You're late. The last convoy left without you, and yet here you are. What a way to deliver. |
| `narration.ny_2_4.boss.before.1` | regisseur | I don't run a gang. I manage a contract. Contracts don't stop because someone regrets them. |
| `narration.ny_2_4.boss.before.2` | agent_d | People were locked in your cars. |
| `narration.ny_2_4.boss.before.3` | regisseur | People were transported. There's a difference: one pays, and the other complains. |
| `narration.ny_2_4.boss.phase1.0` | regisseur | The contract is signed. All that's left is for you to carry it out. |
| `narration.ny_2_4.boss.phase2.0` | regisseur | Termination. I don't like losing a client. |
| `narration.ny_2_4.boss.after.0` | giulia_rossi | The ledger! Every delivery carries the same signatory: the agency. |
| `narration.ny_2_4.boss.after.1` | lucas_mendes | The Syndicate hands over the captives. The AMIF provides the surveillance, and resells what it doesn't keep. Everything is recorded. |
| `narration.ny_2_4.boss.after.2` | ilona_varga | It's the proof Brooklyn only let us guess at: they don't fight each other, they partner. |
| `narration.ny_2_4.boss.after.3` | regisseur | You think you're taking down a gang. You've just disrupted a contract. |
| `narration.ny_2_4.chapter_end.0` | narrateur | The captives are freed. The transport ledger is safe: the first proof of collusion between the Syndicate and the AMIF. |
| `narration.ny_2_4.chapter_end.1` | ilona_varga | Keep it. We'll need it in Salem. |

**Variants.** Agent D: `boss.before.2` as written. Agent M: `narration.ny_2_4.boss.before.2.m` — `agent_m`: "I see those cars full, and what you collect for each one." · Agent M, recognition: `narration.ny_2_4.boss.after.4`, agent_m: "Moscow had its oligarchs. New York has a steward. The logic is identical: people as merchandise."

**Fragment.** `ny_terminal_register`: **The Transport Ledger**: *Everything was recorded: dates, cars, the names of those who'd been taken. Beside every line, a single signatory, and it wasn't the Syndicate. The captives weren't being delivered to a gang, but to an agency.*

**QG.** Archive: *At the freight terminal, The Dawn seizes the transport ledger: proof that the Syndicate delivers captive Apex to the AMIF, which supplies it with surveillance.* Thread: *Proof no. 1 in hand. Heading to Harlem.*

---

## Chapter 3: Harlem

**Opening text:** *In Harlem, the blackout left scars the gangs exploit. Blocks burn, families flee. The Dawn must choose whom to protect first, and at what price.*

| Route | Added opening sentence |
|---|---|
| Agent D | *It's his neighborhood. He knows every sidewalk, every fire escape.* |
| Agent M | *It isn't her neighborhood. She is learning what a burning city costs.* |

#### zone_ny_3_1: The burning block

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_3_1.intro.0` | narrateur | The blackout left Harlem's buildings without power, without water pressure, without firefighters. This one has been burning for an hour. |
| `narration.ny_3_1.intro.1` | giulia_rossi | There are people on every floor. I can hear children. |
| `narration.ny_3_1.intro.2` | habitant | My neighbor says it was a short circuit. A short circuit that smells of gasoline? |
| `narration.ny_3_1.intro.3` | agent_d | We open the service stairs. I'll build you a walkway. Hold the rail and don't look down. |
| `narration.ny_3_1.intro.4` | lucas_mendes | The third-floor ceiling won't hold. We have four minutes. |
| `narration.ny_3_1.elite.before.0` | enforcer | Nobody leaves. The tenants got an offer, and they're going to take it. |
| `narration.ny_3_1.elite.before.1` | ilona_varga | He talks about an offer while the block burns. He isn't even ashamed. |
| `narration.ny_3_1.elite.after.0` | giulia_rossi | Gas cans, in the cellar. Someone set this up. |
| `narration.ny_3_1.exit.0` | habitant | Men came yesterday offering to buy the building. We said no. |
| `narration.ny_3_1.exit.1` | ilona_varga | They changed the answer themselves. |

**Variants.** Agent D: `intro.3` as written (his walkway is a drawing made real). Agent M: `narration.ny_3_1.intro.3.m` — `agent_m`: "I see which floor will give way first. We evacuate in that order." · Agent D, personal: `narration.ny_3_1.exit.2`, agent_d: "I was born three streets from here. I drew on that wall when I was fifteen. Nothing's left of it."

**Fragment.** `ny_harlem_fire`: **The Block That Burned**: *The residents had said no to the buyout offer. In the night, the fire gave another answer. You watched families come down through the smoke with photos under their arms, as if a neighborhood could be saved by carrying out its faces.*

**QG.** Archive: *In Harlem, a block set on fire by a gang is evacuated in time. The residents had refused a buyout offer.* Thread: *The gang is emptying Harlem's blocks. The clinic is next.*

#### zone_ny_3_2: The makeshift clinic

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_3_2.intro.0` | narrateur | The clinic occupies a former church. A generator coughs in the yard, and a line of wounded waits at the door. |
| `narration.ny_3_2.intro.1` | giulia_rossi | No badge, no forms. They walk in, that's all. |
| `narration.ny_3_2.intro.2` | dr_colby | Evacuate? The downtown hospitals admit nobody from this neighborhood. They only get treated here. If I leave, they have nowhere to go. |
| `narration.ny_3_2.intro.3` | ilona_varga | She isn't trying to be brave. She's done the math. |
| `narration.ny_3_2.intro.4` | agent_d | Then we hold the clinic. Just tell us which door must never fall. |
| `narration.ny_3_2.elite.before.0` | enforcer | We're emptying the building. It'll be simpler for everyone. |
| `narration.ny_3_2.elite.before.1` | dr_colby | Simpler for whom? Not for the people lying behind me. |
| `narration.ny_3_2.elite.after.0` | lucas_mendes | The entrance holds. The patients are safe. |
| `narration.ny_3_2.exit.0` | dr_colby | I won't thank you. I've thanked too many people who couldn't stay. |
| `narration.ny_3_2.exit.1` | dr_colby | Stay, if you like. The gang isn't done. |

**Variants.** Agent D: `intro.4` as written. Agent M: `narration.ny_3_2.intro.4.m` — `agent_m`: "I see where they'll come in. Just tell me which door you want to keep."

**Fragment.** `ny_harlem_clinic`: **The Church Clinic**: *There were no forms, no badges, no questions. Only white sheets under cracked stained glass. When the doctor said she wouldn't leave, you understood that some places exist only because someone refuses to abandon them.*

**QG.** Archive: *In Harlem, a makeshift clinic in a former church treats the whole neighborhood. Dr. Colby refuses to evacuate despite the gang's attack.* Thread: *The clinic holds. The market is the next weak point.*

#### zone_ny_3_3: The street market

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_3_3.intro.0` | narrateur | The market vendors raise their tarps before daybreak. Two streets away, the clinic's lights come on. |
| `narration.ny_3_3.intro.1` | commercant | Everybody knows everybody here. If the market falls, people have nothing to eat. |
| `narration.ny_3_3.intro.2` | lucas_mendes | Two alerts at once. The gang is hitting the clinic and the market. |
| `narration.ny_3_3.intro.3` | giulia_rossi | We can't cover both. Not enough people. |
| `narration.ny_3_3.choice.0` | ilona_varga | It's your call. Whatever you decide, the other side will pay for it. |
| `narration.ny_3_3.choice.1` | narrateur | Choice: defend the clinic first, or the market first. |
| `narration.ny_3_3.branch_clinic.0` | agent_d | The clinic first. People are lying there who can't run. |
| `narration.ny_3_3.branch_clinic.1` | commercant | The market... You're leaving us? |
| `narration.ny_3_3.branch_clinic.2` | agent_d | I'll be back. Hide what you can. |
| `narration.ny_3_3.branch_market.0` | agent_d | The market first. Families there live on what they sell today. |
| `narration.ny_3_3.branch_market.1` | dr_colby | The clinic? You're leaving us? |
| `narration.ny_3_3.branch_market.2` | agent_d | Hold the door. I'll be back. |
| `narration.ny_3_3.exit.0` | giulia_rossi | We held one side. For the other, we'll arrive after. |
| `narration.ny_3_3.exit.1` | ilona_varga | You rarely win without losing something. Remember what it costs. |

**Variants.** Agent M: `narration.ny_3_3.branch_clinic.0.m`: "The clinic first. I see the beds, the drips: they couldn't flee." · `narration.ny_3_3.branch_market.0.m`: "The market first. I see the families counting their last dollars there."

**Fragment.** `ny_harlem_market`: **Two Alerts**: *Two alerts at the same moment, two places you already loved. You knew one hand can't hold two doors. What you chose first, the neighborhood will know. What you left behind, it will remember.*

**QG.** Archive: *In Harlem, The Dawn defends the clinic or the market first while the gang attacks both. Each choice leaves a visible loss.* Thread: *The gang's leader is waiting on Workshop Street.*

#### zone_ny_3_4: Workshop Street

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_3_4.intro.0` | narrateur | Workshop Street is narrow, the shutters closed. Tags cover the walls: the gang's signature. |
| `narration.ny_3_4.intro.1` | lucas_mendes | There's no one on this street, but the windows are watching us. |
| `narration.ny_3_4.intro.2` | giulia_rossi | They're expecting us. Every door is held. |
| `narration.ny_3_4.elite.before.0` | enforcer | Braise said: nobody gets through. He also said you'd try. |
| `narration.ny_3_4.elite.after.0` | ilona_varga | The street is open. He's at the end, on the sidewalk. |
| `narration.ny_3_4.boss.before.0` | braise | There you are. I expected you sooner, but fire always takes its time. |
| `narration.ny_3_4.boss.before.1` | braise | The blackout emptied this city of its rules. I only picked up what was lying around. |
| `narration.ny_3_4.boss.before.2` | braise | The Steward paid well, before you disturbed his contract. A free block is worth more than an occupied one. |
| `narration.ny_3_4.boss.before.3` | ilona_varga | He doesn't apologize. He explains, as if it were obvious. |
| `narration.ny_3_4.boss.phase1.0` | braise | It burns. That's the word I learned. |
| `narration.ny_3_4.boss.phase2.0` | braise | The whole block. If I fall, I don't fall alone. |
| `narration.ny_3_4.boss.after.0` | braise | You don't understand. If it isn't me, it'll be someone else. |
| `narration.ny_3_4.boss.after.1` | ilona_varga | Maybe. But tonight, you're the one who falls. |
| `narration.ny_3_4.chapter_end.0` | narrateur | The gang leader is down. The neighborhood breathes a little, but the price is visible in the street. |

**Variants.** Agent D: at the sidewalk, `narration.ny_3_4.boss.before.4.d`, agent_d: "It's here. This is where Théo fell. I drew the barrier in chalk, right there. It held for one second." · `narration.ny_3_4.boss.after.2.d`, agent_d: "I didn't save Théo. But this street, I took back."
Agent M: `narration.ny_3_4.boss.before.4.m`, habitant: "This is where my brother was shot. He sold bread. He'd never done anything." · `narration.ny_3_4.boss.after.2.m`, agent_m: "I see what this sidewalk saw. We won't forget his name." *(both lines are played in front of the same sidewalk, with a resident in Théo's place)*

**Fragment.** `ny_harlem_chalk`: **The Chalk Sidewalk**: *There was still a line of chalk on the edge of the sidewalk. You felt the barrier would hold forever, just one second longer. You understood that what happened here can't be repaired: you make sure it never happens again.*

**QG.** Archive: *In Harlem, The Dawn ends the reign of Braise's gang on Workshop Street. The neighborhood bears the marks of the price paid.* Thread: *On to Manhattan, where everything converges.*

#### Chapter epilogues (depending on the choice in 3.3)

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_3.epilogue.clinic.0` | narrateur | By morning, the clinic is intact. The market is nothing but a row of blackened tarps. |
| `narration.ny_3.epilogue.clinic.1` | commercant | We saved our lives. Not our stalls. I don't blame you, but I'll remember. |
| `narration.ny_3.epilogue.clinic.2` | dr_colby | The market people just got treated at my clinic. They've never had more reason to thank us, or less desire to. |
| `narration.ny_3.epilogue.market.0` | narrateur | By morning, the market stands. The clinic has shattered stained glass and torn-off doors. |
| `narration.ny_3.epilogue.market.1` | dr_colby | I'm hurt, not beaten. But my clinic will close for a few weeks. The sick will go where they can. |
| `narration.ny_3.epilogue.market.2` | commercant | We kept the market. We lost the clinic. I don't know if it was the right call. |
| `narration.ny_3.epilogue.common.0` | ilona_varga | We couldn't save everything. That doesn't make you a bad hero. It makes you a hero who looks at what he leaves behind. |
| `narration.ny_3.epilogue.common.1` | giulia_rossi | Manhattan, now. Where everything gets decided. |

---

## Chapter 4: Manhattan

**Opening text:** *Midtown, Times Square, the United Nations: everything converges on Manhattan. The circles of power, the AMIF and The Dawn clash in the open. This is where the city tips.*

| Route | Added opening sentence |
|---|---|
| Agent D | *He sees the AMIF with its face uncovered for the first time: until now, it was only a Titan and lists.* |
| Agent M | *She has seen the AMIF harvest in Paris and its clients smile in Moscow. Here, she sees them together.* |

#### zone_ny_4_1: Times Square

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_4_1.intro.0` | narrateur | Times Square never sleeps. Tonight, every screen repeats the same message: The Dawn is a threat. |
| `narration.ny_4_1.intro.1` | voix | "Unsupervised meta-humans endanger civilians. The AMIF protects you." |
| `narration.ny_4_1.intro.2` | giulia_rossi | They've reused the Brooklyn footage. Backwards. |
| `narration.ny_4_1.intro.3` | lucas_mendes | The control room is in the basement. If we reach it, we can replace the feed. |
| `narration.ny_4_1.intro.4` | agent_d | We don't answer them with words. We show them. |
| `narration.ny_4_1.elite.before.0` | agent_amif | This area is under control. Please step back. You are being filmed. |
| `narration.ny_4_1.elite.before.1` | ilona_varga | He doesn't say "forbidden." He says "filmed." He wants it seen. |
| `narration.ny_4_1.elite.after.0` | lucas_mendes | Control room reached. The feed is ours. |
| `narration.ny_4_1.exit.0` | narrateur | On every screen, the evacuation of the Harlem block: a child, arms, a door that holds. |
| `narration.ny_4_1.exit.1` | ilona_varga | Not the ledger, not yet. Proof released too early will be denied. Let's first show who we are. |
| `narration.ny_4_1.exit.2` | giulia_rossi | That's enough for tonight. People watched. |

**Variants.** Agent D: `intro.4` as written. Agent M: `narration.ny_4_1.intro.4.m` — `agent_m`: "I see what these screens will show in a minute, if we let them. Let's replace it before that." · Agent M, recognition: `narration.ny_4_1.exit.3`, agent_m: "In Moscow, it was holograms. Here, billboards. The staging is the same."

**Fragment.** `ny_times_square`: **Screens Played Backwards**: *Every screen told your story, but backwards. You saw the fire without the hands that carried the children. For one night, you took back the floor: you said nothing, you only showed.*

**QG.** Archive: *At Times Square, The Dawn takes the screen control room and broadcasts the Harlem evacuation in place of the AMIF's propaganda.* Thread: *The circles of power await in Midtown.*

#### zone_ny_4_2: Midtown towers

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_4_2.intro.0` | narrateur | The glass tower dominates Midtown. On the forty-second floor, a gala gathers foundations, boards, well-paid smiles. |
| `narration.ny_4_2.intro.1` | giulia_rossi | We're the only ones without a gold badge. |
| `narration.ny_4_2.intro.2` | lucas_mendes | A room of "partners." The names are displayed like trophies. |
| `narration.ny_4_2.intro.3` | agent_d | We're not stealing anything. We're comparing. |
| `narration.ny_4_2.elite.before.0` | enforcer | This room is reserved. Please rejoin the lounge. |
| `narration.ny_4_2.elite.before.1` | ilona_varga | He looks polite. He's very worried. |
| `narration.ny_4_2.elite.after.0` | giulia_rossi | I have the AMIF list. You have the one from the Queens ledger. |
| `narration.ny_4_2.elite.after.1` | lucas_mendes | Three names. Four. Five. They're on both. |
| `narration.ny_4_2.exit.0` | giulia_rossi | The people who pay the AMIF also pay the Syndicate. It isn't collusion, it's a shared budget. |
| `narration.ny_4_2.exit.1` | ilona_varga | And there: a foundation whose name rings a bell. Archives from the eighties. |
| `narration.ny_4_2.exit.2` | agent_d | Not tonight. But we keep the name. |

**Variants.** Agent D: `intro.3` as written. Agent M: `narration.ny_4_2.intro.3.m` — `agent_m`: "I see who signed these names, and in what order. We're not stealing: we're reading." · Agent M, recognition: `narration.ny_4_2.exit.3`, agent_m: "Orlan, the Smuggler, these financiers... They're the same people, only more polite."

**Fragment.** `ny_midtown_list`: **Names on Two Lists**: *You thought the Syndicate and the AMIF were two enemies who made deals. You understood they shared a ledger. Five names came up on both lists, like a signature nobody bothers to hide anymore.*

**QG.** Archive: *In a Midtown tower, The Dawn discovers that several funders of the AMIF and the Syndicate overlap. One name points to archives from the eighties.* Thread: *The announcement of the Accords is being prepared at the UN.*

#### zone_ny_4_3: United Nations headquarters

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_4_3.intro.0` | narrateur | The UN hall is full of flags, microphones and eyes. In an hour, the Salem Accords will be announced. |
| `narration.ny_4_3.intro.1` | giulia_rossi | Someone is already cutting the broadcast cables. |
| `narration.ny_4_3.intro.2` | lucas_mendes | Sabotage on the technical floor. If the announcement doesn't air, all of this was for nothing. |
| `narration.ny_4_3.intro.3` | agent_d | We hold the room. Nobody cuts this broadcast. |
| `narration.ny_4_3.elite.before.0` | agent_amif | For the delegates' safety, the room is being evacuated. Please cooperate. |
| `narration.ny_4_3.elite.before.1` | ilona_varga | Evacuating the room is smothering the announcement. He knows it. |
| `narration.ny_4_3.elite.after.0` | giulia_rossi | The room is secured. Rayner can speak. |
| `narration.ny_4_3.stage.0` | rayner | Apex don't ask permission to exist. They ask to be recognized, not controlled. |
| `narration.ny_4_3.stage.1` | stroke | Ms. Rayner speaks of recognition. I speak of responsibility. A power that isn't measured is a power that isn't protected. |
| `narration.ny_4_3.stage.2` | rayner | Measuring, harvesting, locking away: that isn't protecting. It's owning. |
| `narration.ny_4_3.stage.3` | stroke | The numbers from Paris, madam, don't lie. I don't own anyone. I count. |
| `narration.ny_4_3.stage.4` | rayner | Then count the ones you locked up in Queens too. There were many. |
| `narration.ny_4_3.exit.0` | narrateur | The room holds its breath. For the first time, the voice of the AMIF and the voice of The Dawn clash before everyone. |
| `narration.ny_4_3.exit.1` | ilona_varga | He kept his composure. He'll keep it a while longer. He's waiting outside. |

**Variants.** Agent D: `intro.3` as written. Agent M: `narration.ny_4_3.intro.3.m` — `agent_m`: "I see the hands that will cut the cables in ten seconds. I stop them first." · Agent D, first encounter: `narration.ny_4_3.stage.5.d`, agent_d: "That's him. He doesn't have a villain's face. That's the scariest part." · Agent M: `narration.ny_4_3.stage.5.m`, agent_m: "The numbers from Paris. I was there. They didn't say everything they counted."

**Fragment.** `ny_un_stage`: **Two Voices**: *One voice spoke of protection, the other of freedom. They used almost the same words, but not for the same people. The world was listening. For the first time, no one could pretend not to hear.*

**QG.** Archive: *At the UN, Rayner and Stroke clash publicly before the negotiators while The Dawn protects the broadcast of the Accords announcement.* Thread: *Stroke waits on the plaza.*

#### zone_ny_4_4: UN plaza

| Key | Speaker | EN |
|---|---|---|
| `narration.ny_4_4.intro.0` | narrateur | The UN plaza is flooded with light. Press cameras line the perimeter. At the center, a lone man in a gray uniform. |
| `narration.ny_4_4.intro.1` | lucas_mendes | He doesn't have the guards from earlier. He wanted to be seen alone. |
| `narration.ny_4_4.elite.before.0` | agent_amif | The General asked to speak with you. Alone. |
| `narration.ny_4_4.elite.before.1` | ilona_varga | "Alone," in front of a thousand cameras. |
| `narration.ny_4_4.elite.after.0` | giulia_rossi | The guard is down. Nothing left between him and us. |
| `narration.ny_4_4.boss.before.0` | stroke | I commend you. You held your room, took back your screens, turned a few informants. Only the logical next step is missing. |
| `narration.ny_4_4.boss.before.1` | stroke | Register. Voluntarily. I guarantee your rank, your immunity, your autonomy. The Dawn would become a section of the agency. |
| `narration.ny_4_4.boss.before.2` | agent_d | Your agency harvests what it calls protecting. We saw the cars. |
| `narration.ny_4_4.boss.before.3` | stroke | You saw cars. I saw thousands dead in Paris. We disagree about the accounting. |
| `narration.ny_4_4.boss.phase1.0` | stroke | Emergency procedure. I regret it has come to this. |
| `narration.ny_4_4.boss.phase2.0` | stroke | Retention protocol. Nothing personal, nothing human: only order. |
| `narration.ny_4_4.boss.after.0` | stroke | You think you've won. The plaza is watching you, but it's in Salem that everything gets settled. |
| `narration.ny_4_4.boss.after.1` | ilona_varga | He's withdrawing. He doesn't look beaten. |
| `narration.ny_4_4.boss.after.2` | stroke | Salem will decide. |
| `narration.ny_4_4.chapter_end.0` | narrateur | The Salem Accords come out into the open. Stroke has withdrawn, but his promise weighs on the city. |
| `narration.ny_4_4.chapter_end.1` | rayner | The negotiations begin in Salem. That's where everything will be played out. |

**Variants.** Agent D: `boss.before.2` as written. Agent M: `narration.ny_4_4.boss.before.2.m` — `agent_m`: "In Paris I saw what you harvest. Thousands dead, you say? You also kept the remains." · Agent D, first encounter: `narration.ny_4_4.boss.after.3.d`, agent_d: "I don't know what he's preparing in Salem. But he knows we'll be there." · Agent M: `narration.ny_4_4.boss.after.3.m`, agent_m: "Salem. Where Moscow and Paris meet. He knows I'll be there too."

**Fragment.** `ny_un_esplanade`: **Salem Will Decide**: *He didn't threaten you. He spoke of accounting, of numbers, of order. He stood alone on the plaza, calm, convinced. You understood you don't beat a man like him in one blow: you take away his stage.*

**QG.** Archive: *On the UN plaza, The Dawn faces General Stroke. He withdraws, promising that Salem will decide. The Accords are now public.* Thread: *On to Salem, where the Accords will be played out.*
