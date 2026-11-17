# Moscow script, English pass: "progress built on a lie"

*Date: 2026-11-16 · Status: English pass of `14_Script_Moscou.md`, **to be reviewed**. The French script is canon: any change goes there first, then here. Same keys, same speakers (see the speaker table in the French file). Fragment and QG texts are included.*

**Glossary used throughout:** L'Aube → The Dawn · le Passeur → the Smuggler · le Contremaître → the Foreman · le Conseiller → the Councillor · le Jardin / Sadovoye Koltso → the Garden Ring (Sadovoye Koltso) · la Place Rouge → Red Square · AMIF, Apex, Supernova unchanged · "chef de quai" → dock foreman.

## Speaker display names (EN)

| Id | EN name |
|---|---|
| `narrateur` | Narrator |
| `agent_m` / `agent_d` | Agent M / Agent D |
| `ilona_varga` / `giulia_rossi` / `lucas_mendes` | Ilona Varga / Giulia Rossi / Lucas Mendes |
| `passeur` / `enforcer` | The Smuggler / Enforcer |
| `contremaitre` | Fiodor Gromov, the Foreman |
| `orlan` | Dmitri Orlan |
| `conseiller` | Arkadi Zorin, the Councillor |
| `ilia` | Ilia |
| `ouvrier` / `technicien` / `apex_libere` | A Worker / A Technician / A Freed Apex |
| `commissaire` / `technicien_holo` / `civil_moscou` | The Auctioneer / Hologram Technician / A Civilian |
| `agent_amif` | An AMIF Agent |

**Route variants:** a replacement variant carries its own key suffixed `.d` (Agent D) or `.m` (Agent M) and replaces the base line for that route only; route-only additions have their own key.

---

## Chapter 1: Sergueiv

**Opening text:** *Sergueiv, its monasteries and its futuristic rail line: Moscow has armed itself against a changing world, yet it welcomes The Dawn. Behind the façade, someone is already pulling the strings.*

| Route | Added opening sentence |
|---|---|
| Agent M | *She knows these streets, these codes, these smiles. She never looked at what they covered.* |
| Agent D | *He has already seen a gang and an agency work hand in hand. Here, no one hides it: everything is polite.* |

#### zone_mos_1_1: Maglev station

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_1_1.intro.0` | narrateur | Holograms greet The Dawn by name. The platforms are swept, the smiles flawless. |
| `narration.mos_1_1.intro.1` | giulia_rossi | You'd think they were expecting a head of state. |
| `narration.mos_1_1.intro.2` | ilona_varga | They were expecting someone. I don't know yet who. |
| `narration.mos_1_1.intro.3` | agent_m | These platforms were washed tonight. I can see the brooms, the buckets... and nobody slept. |
| `narration.mos_1_1.intro.4` | lucas_mendes | A welcome of this quality takes days to prepare. |
| `narration.mos_1_1.intro.5` | ilona_varga | Or it's there to keep our eyes on the façade. |
| `narration.mos_1_1.elite.before.0` | enforcer | Welcome to Moscow. The welcoming committee kindly asks you to turn back. |
| `narration.mos_1_1.elite.before.1` | ilona_varga | A welcome that perfect is always hiding something. |
| `narration.mos_1_1.elite.after.0` | agent_m | This sealed cargo is on no manifest. I can see the trucks that left it here... three days before we arrived. |
| `narration.mos_1_1.elite.after.1` | lucas_mendes | Three days? Before we even decided to come? |
| `narration.mos_1_1.elite.after.2` | agent_m | Before we even decided to come. |
| `narration.mos_1_1.exit.0` | ilona_varga | We leave through the monastery archives. If someone's waiting there, it isn't to welcome us. |

**Variants.** Agent M: `intro.3` is spoken by her (she recognizes the staff). Agent D: `narration.mos_1_1.intro.3.d` — `agent_d`: "They washed these platforms for us. In New York, they hide the cameras, not the brooms." · Agent M, cathedral hint (single, ambiguous): `narration.mos_1_1.intro.6`, agent_m: "That smell of wax and incense on a train platform... I know it." *(played for Agent M only; meaning revealed in chapter 4)*

**Fragment.** `mos_maglev_welcome`: **The Perfect Welcome**: *Holograms greeted The Dawn by name, the platforms were swept, the smiles flawless. Yet a sealed cargo sat on track 9, with no name and no manifest. Moscow opened its arms wide, and looked the other way.*

**QG.** Archive: *At the maglev station, The Dawn is welcomed like a state delegation. A sealed cargo, absent from every manifest, was left there three days earlier.* Thread: *Who left the cargo on track 9?*

#### zone_mos_1_2: Monastery archives

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_1_2.intro.0` | narrateur | The monastery is silent. The Sergueiv archives sleep under the vaults, at the end of a corridor of candles. |
| `narration.mos_1_2.intro.1` | lucas_mendes | The candles are electric. Even the saints have an energy bill. |
| `narration.mos_1_2.intro.2` | agent_m | This fresco was repainted. Less than ten days ago. The pigment doesn't lie. |
| `narration.mos_1_2.intro.3` | giulia_rossi | Someone went to the trouble of painting over a wall. To hide what? |
| `narration.mos_1_2.intro.4` | agent_m | I see it... and it isn't the color I see, it's the door. |
| `narration.mos_1_2.elite.before.0` | enforcer | The archives are closed to the public. |
| `narration.mos_1_2.elite.before.1` | agent_m | Not for the woman restoring the fresco. |
| `narration.mos_1_2.elite.after.0` | agent_m | Behind the fresco, a ledger. I saw it before I touched it. |
| `narration.mos_1_2.elite.after.1` | lucas_mendes | Names... dates... deliveries. These are sales. |
| `narration.mos_1_2.exit.0` | ilona_varga | A ledger: half a proof. The other half is out by the refineries. |
| `narration.mos_1_2.exit.1` | agent_m | Then we head for Kapotnya. |

**Variants.** Agent M: `intro.2` is spoken by her (restorer). Agent D: `narration.mos_1_2.intro.2.d` — `agent_d`: "I draw walls, not frescoes. But this one's been touched up, I'm sure of it." · `narration.mos_1_2.intro.4.d` — agent_d: "I see the line that's off. I don't see why." · Agent M, cathedral hint: `narration.mos_1_2.intro.5`, agent_m: "The same corridor of candles as back home... before everything fell apart." *(meaning revealed in chapter 4)*

**Fragment.** `mos_monastery_register`: **The Ledger Under the Fresco**: *You have restored a thousand frescoes without ever looking for what they hid. This one covered a ledger: names, dates, deliveries. Someone had painted over the truth and entrusted it to the saints.*

**QG.** Archive: *In the monastery archives, a recently repainted fresco hid a ledger of deliveries. It is the first half of the proof of the black market.* Thread: *The ledger mentions Kapotnya.*

#### zone_mos_1_3: Maglev line

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_1_3.intro.0` | narrateur | The maglev races beneath the city, lit like a festival. A convoy runs alongside it, without lights. |
| `narration.mos_1_3.intro.1` | giulia_rossi | A convoy that's on no timetable, on a line that displays everything. |
| `narration.mos_1_3.intro.2` | agent_m | I see the crates... before they're loaded. They're the same as on track 9. |
| `narration.mos_1_3.intro.3` | lucas_mendes | Then it's the same pipeline. |
| `narration.mos_1_3.elite.before.0` | enforcer | This convoy doesn't exist. You saw nothing. |
| `narration.mos_1_3.elite.before.1` | ilona_varga | He means it. That's what bothers me: he's afraid for himself, not for the cargo. |
| `narration.mos_1_3.elite.after.0` | agent_m | These crates carry a Soviet code. Someone is digging up old research. |
| `narration.mos_1_3.elite.after.1` | lucas_mendes | Soviet? Five years after the Supernova, who's exhuming that? |
| `narration.mos_1_3.exit.0` | ilona_varga | A pre-Union code on crates from this year. Remember it. |
| `narration.mos_1_3.exit.1` | agent_m | I'll remember it. It doesn't fade. |

**Variants.** Agent M: `intro.2` by her (vision of the crates' past). Agent D: `narration.mos_1_3.intro.2.d` — `agent_d`: "I can't see their past, but I can see these crates were packed by hand. Someone cares about them."

**Fragment.** `mos_convoy_crates`: **Marked Crates**: *The convoy raced beneath the city, lit like a festival. The crates bore a code nobody uses anymore. New hands on very old research.*

**QG.** Archive: *The maglev convoy carries crates marked with a Soviet code. Someone is unearthing old research.* Thread: *What do the crates with the Soviet code contain?*

#### zone_mos_1_4: Maglev depot

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_1_4.intro.0` | narrateur | The depot is arranged like a shop. Every crate carries a label, every label a price. |
| `narration.mos_1_4.intro.1` | giulia_rossi | A smuggling warehouse with a tidier filing system than mine. |
| `narration.mos_1_4.intro.2` | agent_m | These crates leave tonight. I see the truck, the route, the address: a refinery. |
| `narration.mos_1_4.intro.3` | lucas_mendes | So everything converges on Kapotnya. |
| `narration.mos_1_4.elite.before.0` | enforcer | The depot isn't open. You must have the wrong counter. |
| `narration.mos_1_4.elite.after.0` | ilona_varga | He wasn't afraid of us. He was afraid of whoever's behind him. |
| `narration.mos_1_4.boss.before.0` | passeur | You're early. My clients usually make an appointment. |
| `narration.mos_1_4.boss.before.1` | passeur | I don't say victims. I don't say adversaries. I say: customers. |
| `narration.mos_1_4.boss.before.2` | ilona_varga | Don't listen. He's already counting what he can sell of us. |
| `narration.mos_1_4.boss.before.3` | passeur | Powers have a price, Miss Varga. Yours too. |
| `narration.mos_1_4.boss.phase1.0` | passeur | The customers are arriving. My apologies: the shop closes early tonight. |
| `narration.mos_1_4.boss.phase2.0` | passeur | Everything must go. You included. |
| `narration.mos_1_4.boss.after.0` | passeur | You think you found something? We let you find it. I arranged the welcome. |
| `narration.mos_1_4.boss.after.1` | agent_m | So that was to keep us away from Kapotnya. |
| `narration.mos_1_4.boss.after.2` | passeur | I never said "away." I said "welcomed." |
| `narration.mos_1_4.boss.after.3` | ilona_varga | Then that is where we go. |
| `narration.mos_1_4.chapter_end.0` | narrateur | The Dawn understands that the welcome was meant to keep it away from Kapotnya. The next convoy leaves at dawn. |

**Variants.** Agent M: `narration.mos_1_4.boss.before.3.m`, passeur (addresses her) instead of Ilona: "Your sight into the past is worth a great deal, miss. The bidding is open." · Agent D: `narration.mos_1_4.boss.before.3.d`, passeur (addressed to D): "An imagination that becomes matter. Now there's a product I've never had on my shelves." · Agent M, cathedral hint: `narration.mos_1_4.boss.after.4`, passeur: "Wax, incense, candles... that's a scent that comes up often among my clients." *(played for Agent M only; meaning revealed in chapter 4)*

**Fragment.** `mos_depot_passeur`: **The Smuggler**: *He never said victim, never enemy: only customer. In his depot everything had a price, even the welcome prepared for you. Tonight you understood the whole city had been invited to look elsewhere.*

**QG.** Archive: *The Smuggler ran the maglev depot like a shop. The welcome given to The Dawn was his staging, to keep it away from Kapotnya.* Thread: *On to Kapotnya, its refineries and its convoy.*

---

## Chapter 2: Kapotnya

**Opening text:** *Kapotnya, its refineries and its industrial fog: the oligarchs hide here what they don't want seen. The Dawn follows convoys, leaks, and a mob quietly allied with the AMIF.*

| Route | Added opening sentence |
|---|---|
| Agent M | *She knows this fog: her city has breathed it forever. She never knew what it covered.* |
| Agent D | *He has already seen a gang and an agency work hand in hand. Here, it is a state looking the other way.* |

#### zone_mos_2_1: Refinery

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_2_1.intro.0` | narrateur | Kapotnya breathes a yellow fog. The flare stacks light the pipes like candles. |
| `narration.mos_2_1.intro.1` | lucas_mendes | Gas alarm on block C. There are still people inside. |
| `narration.mos_2_1.intro.2` | agent_m | The valve was open before the alarm. Someone turned it by hand. |
| `narration.mos_2_1.intro.3` | giulia_rossi | Why cause a leak at your own refinery? |
| `narration.mos_2_1.elite.before.0` | enforcer | The evacuation is suspended. The transfer doesn't stop for an accident. |
| `narration.mos_2_1.elite.before.1` | ilona_varga | He mentions the transfer before the workers. He just gave himself away. |
| `narration.mos_2_1.elite.after.0` | ouvrier | We were told to leave the site for the night. A few of us refused. |
| `narration.mos_2_1.elite.after.1` | agent_m | The leak is a screen: while we watch the flames, they load something else. |
| `narration.mos_2_1.exit.0` | ilona_varga | A deliberate leak to cover a transfer. The trucks are heading for the convoy road. |
| `narration.mos_2_1.exit.1` | agent_m | Then we follow them. |

**Variants.** Agent M: `intro.2` by her (vision of the valve). Agent D: `narration.mos_2_1.intro.2.d` — `agent_d`: "I see the trace of a hand on this valve. Someone opened it on purpose." · Agent M, cathedral hint: `narration.mos_2_1.intro.4`, agent_m: "The refinery flares... from afar, they look like candles. Back home, everything burned like that."

**Fragment.** `mos_refinery_leak`: **The Leak That Was a Screen**: *The flares burned like candles on an altar of pipes. The leak was no accident: the valve was opened so everyone would watch the flames while something else was loaded in the dark.*

**QG.** Archive: *At the Kapotnya refinery, The Dawn plugs a deliberate leak that was covering a transfer of goods.* Thread: *The transfer trucks take the convoy road.*

#### zone_mos_2_2: Convoy road

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_2_2.intro.0` | narrateur | The convoy road crosses the fog. The trucks drive without headlights, as if they knew every bend. |
| `narration.mos_2_2.intro.1` | giulia_rossi | Four trucks, an escort, and not a single light on. |
| `narration.mos_2_2.intro.2` | agent_m | The trucks are loaded with what we saw on track 9. But heavier, more dangerous. |
| `narration.mos_2_2.elite.before.0` | enforcer | This convoy is authorized. You have permission to back off. |
| `narration.mos_2_2.elite.before.1` | lucas_mendes | Authorized by whom? He doesn't answer. |
| `narration.mos_2_2.elite.after.0` | giulia_rossi | The crates are glowing. These are Supernova residues. |
| `narration.mos_2_2.elite.after.1` | ilona_varga | Fragments of the breach, shipped like coal. |
| `narration.mos_2_2.exit.0` | agent_m | The same Soviet code as on the Sergueiv crates. Someone is linking the two. |
| `narration.mos_2_2.exit.1` | ilona_varga | Then the laboratory is at the end of this road. |

**Variants.** Agent M: `intro.2` by her. Agent D: `narration.mos_2_2.intro.2.d` — `agent_d`: "These crates were packed by hands that know what they're carrying."

**Fragment.** `mos_convoy_residues`: **Coal That Glows**: *The residues of the breach traveled in coal crates, under a tarp. They glowed with a light that belonged to no lamp. You thought of Paris, and of those who had wondered why everyone wanted that glow.*

**QG.** Archive: *On the Kapotnya road, The Dawn intercepts trucks loaded with residues from the breach. The crates carry a Soviet code.* Thread: *The road leads to a clandestine laboratory.*

#### zone_mos_2_3: Clandestine laboratory

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_2_3.intro.0` | narrateur | Beneath the cold-storage warehouse, a laboratory. The corridors are white, the tubes full of a light that isn't electricity. |
| `narration.mos_2_3.intro.1` | lucas_mendes | I don't like that light. It's the color of Paris. |
| `narration.mos_2_3.intro.2` | apex_libere | Don't... don't touch me. They already took what shone inside me. |
| `narration.mos_2_3.intro.3` | ilona_varga | You're safe. Breathe. We're getting you out. |
| `narration.mos_2_3.elite.before.0` | technicien | The files don't leave. I'll destroy them before I let you have them. |
| `narration.mos_2_3.elite.before.1` | agent_m | I see his hands before he moves. He's going to throw the files into the incinerator. |
| `narration.mos_2_3.elite.after.0` | giulia_rossi | The files are intact. Every delivery, every client's name. |
| `narration.mos_2_3.elite.after.1` | lucas_mendes | One client comes up on every page: the AMIF. |
| `narration.mos_2_3.elite.after.2` | ilona_varga | And in the margins, annotations signed "Volkov." Figures from before the Union. |
| `narration.mos_2_3.exit.0` | apex_libere | Thank you. I thought no one would come. |
| `narration.mos_2_3.exit.1` | agent_m | The laboratory files, together with the monastery ledger: the black market is proven. |
| `narration.mos_2_3.exit.2` | ilona_varga | We still need the one who pays them. |

**Variants.** Agent M: `elite.before.1` by her (vision). Agent D: `narration.mos_2_3.elite.before.1.d` — `agent_d`: "He doesn't look like a chemist. He looks like a man afraid of whoever reads these papers." · Agent M, cathedral hint: `narration.mos_2_3.intro.4`, agent_m: "That smell... burnt wax, here? Under a laboratory?" *(meaning revealed in chapter 4)*

**Fragment.** `mos_lab_files`: **The Laboratory Files**: *Everything was filed, dated, signed: Apex, extractions, clients. One client came up on every page. In the margins, an old handwriting annotated every line, as if the dead were still keeping the books.*

**QG.** Archive: *In Kapotnya's clandestine laboratory, The Dawn frees extracted Apex and seizes the files showing the AMIF as a client: the second part of the proof of the black market.* Thread: *The files point to the loading dock.*

#### zone_mos_2_4: Loading dock

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_2_4.intro.0` | narrateur | The loading dock stretches beneath the cranes. One last truck waits, headlights off. |
| `narration.mos_2_4.intro.1` | giulia_rossi | Every waybill carries the same signature, line after line. |
| `narration.mos_2_4.elite.before.0` | enforcer | This dock is closed. The dock foreman will see you, but not tonight. |
| `narration.mos_2_4.elite.after.0` | ilona_varga | The guard has fallen. He's waiting on the gangway. |
| `narration.mos_2_4.boss.before.0` | contremaitre | Gromov, dock foreman. You're late: the last truck has already left. |
| `narration.mos_2_4.boss.before.1` | contremaitre | I don't ask questions. I check the waybills and I sign. That's what I'm paid for. |
| `narration.mos_2_4.boss.before.2` | agent_m | By whom? |
| `narration.mos_2_4.boss.before.3` | contremaitre | A dock foreman doesn't answer that question. A dock foreman weighs, counts, ships. |
| `narration.mos_2_4.boss.phase1.0` | contremaitre | Last load. I'm sorry to keep you waiting. |
| `narration.mos_2_4.boss.phase2.0` | contremaitre | Everything is signed. You have nothing left to contest. |
| `narration.mos_2_4.boss.after.0` | contremaitre | Enough. I'm only a dock foreman: I count, I load, I sign. |
| `narration.mos_2_4.boss.after.1` | contremaitre | Orlan pays. |
| `narration.mos_2_4.boss.after.2` | agent_m | Orlan. |
| `narration.mos_2_4.chapter_end.0` | narrateur | The Dawn now knows who the trucks work for. Orlan's name leads to the towers of Sadovoye Koltso. |

**Variants.** Agent M: `boss.before.2` by her. Agent D: `narration.mos_2_4.boss.before.2.d` — `agent_d`: "Who signs your paychecks?" · Agent M, cathedral hint: `narration.mos_2_4.boss.after.3`, contremaitre: "The wax on the crates? A habit of the men who entrust them to us. A church habit." *(meaning revealed in chapter 4)*

**Fragment.** `mos_quay_gromov`: **A Dock Foreman**: *He asked no questions. He weighed, counted, signed. Every waybill bore the same name, and you understood that the politest evil is the kind that merely signs.*

**QG.** Archive: *At Kapotnya's loading dock, The Dawn defeats the Foreman. Before falling he lets a name slip: "Orlan pays."* Thread: *On to Sadovoye Koltso and Orlan's towers.*

---

## Chapter 3: Sadovoye Koltso

**Opening text:** *Along the Garden Ring, the city's circle, the oligarchs' towers glitter with a thousand holograms. The Dawn realizes that power no longer hides: it stages itself.*

| Route | Added opening sentence |
|---|---|
| Agent M | *She grew up in the shadow of these towers. She had never seen the inside.* |
| Agent D | *He has seen gangs hide their power. Here, it is displayed like a shop window.* |

#### zone_mos_3_1: Glass towers

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_3_1.intro.0` | narrateur | The glass towers are reflected in the Garden Ring. Inside, silent corridors, noiseless elevators. |
| `narration.mos_3_1.intro.1` | giulia_rossi | Everything's too clean. Even the silence has been scrubbed. |
| `narration.mos_3_1.intro.2` | lucas_mendes | This lounge... there are jars. With light inside. |
| `narration.mos_3_1.intro.3` | agent_m | They're not jars. They're trophies. |
| `narration.mos_3_1.elite.before.0` | enforcer | Guests are kindly asked to remain in the reception lounge. |
| `narration.mos_3_1.elite.before.1` | ilona_varga | He smiles while he blocks the door. The smile is part of the service. |
| `narration.mos_3_1.elite.after.0` | giulia_rossi | Every jar has a label, a name, a price. |
| `narration.mos_3_1.exit.0` | agent_m | That's the light they took from the Apex in the laboratory. I recognize it. |
| `narration.mos_3_1.exit.1` | ilona_varga | They display it like a painting. Bidding starts soon. |

**Variants.** Agent M: `intro.3` by her. Agent D: `narration.mos_3_1.intro.3.d` — `agent_d`: "They exhibit people the way they exhibit paintings. Someone takes pride in it." · Agent M, cathedral hint: `narration.mos_3_1.exit.2`, agent_m: "The same scent of wax in this elevator. Someone leaves their signature everywhere."

**Fragment.** `mos_towers_trophies`: **Trophies in Jars**: *Under glass bells, lights awaited buyers. Every jar had a label, a name, a price. You understood that Moscow's power wasn't that of the Apex, but of those who collect them.*

**QG.** Archive: *In the glass towers of Sadovoye Koltso, The Dawn finds a trophy lounge: Apex powers preserved in jars and displayed for sale.* Thread: *The auctions are held on the floor below.*

#### zone_mos_3_2: Private auctions

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_3_2.intro.0` | narrateur | The room is dimmed. Masked bidders raise a finger, never their voice. |
| `narration.mos_3_2.intro.1` | commissaire | Ladies and gentlemen, the next lot: an awakened Apex, rank five, in excellent condition. |
| `narration.mos_3_2.intro.2` | giulia_rossi | A lot. They say "lot." |
| `narration.mos_3_2.intro.3` | agent_m | I see the man they're presenting, his life before the sale. They're selling him with his past. |
| `narration.mos_3_2.elite.before.0` | enforcer | The sale is private. Please return to your seats. |
| `narration.mos_3_2.elite.before.1` | ilona_varga | He doesn't threaten. He asks politely. It's worse. |
| `narration.mos_3_2.elite.after.0` | lucas_mendes | The exits are ours. The lot is free. |
| `narration.mos_3_2.exit.0` | commissaire | The sale resumes, ladies and gentlemen. An incident of no importance. |
| `narration.mos_3_2.exit.1` | ilona_varga | An incident of no importance. To them, we're already forgotten. |

**Variants.** Agent M: `intro.3` by her (vision). Agent D: `narration.mos_3_2.intro.3.d` — `agent_d`: "I'd gladly draw a bar in front of every lot. But it isn't time yet."

**Fragment.** `mos_auction`: **A Lot**: *They said "lot" the way one says "painting." No one in the room raised their voice. You understood that the greatest cruelty never shouts, it bids.*

**QG.** Archive: *At Sadovoye's private auctions, The Dawn disrupts the sale of an Apex and frees the lot before the bidders' eyes.* Thread: *A hologram studio is preparing a response.*

#### zone_mos_3_3: Hologram studio

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_3_3.intro.0` | narrateur | The studio is a black stage. Spotlights draw faces that don't exist. |
| `narration.mos_3_3.intro.1` | technicien_holo | In an hour, the city will see The Dawn strike civilians. It will have seen everything, and lived nothing. |
| `narration.mos_3_3.intro.2` | lucas_mendes | They copied our silhouettes, our insignia, our movements. |
| `narration.mos_3_3.intro.3` | agent_m | I see the video before it exists. Someone rehearsed it a hundred times. |
| `narration.mos_3_3.elite.before.0` | enforcer | The servers are under protection. You will only see what we broadcast. |
| `narration.mos_3_3.elite.after.0` | giulia_rossi | The original recordings are here. Every doctored shot is dated. |
| `narration.mos_3_3.exit.0` | ilona_varga | We take the video apart. With its dates, it proves by itself that it's fake. |
| `narration.mos_3_3.exit.1` | agent_m | But it's already circulated. Somewhere, someone believes it. |

**Variants.** Agent M: `intro.3` by her. Agent D: `narration.mos_3_3.intro.3.d` — `agent_d`: "They copied my lines, my colors, my habits. Even my mistakes. Someone's been studying."

**Fragment.** `mos_holo_studio`: **The Fake Video**: *They had filmed you doing what you had never done. Every gesture was perfect, every shadow exact. You understood that the most dangerous lie is the one that looks like you.*

**QG.** Archive: *At the hologram studio, The Dawn dismantles a fake video in which it attacks civilians. The denial is published, but the rumor spreads.* Thread: *Orlan's tower overlooks the ring.*

#### zone_mos_3_4: Orlan Tower

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_3_4.intro.0` | narrateur | Orlan's floor is a pane of glass over the whole city. Moscow's holograms spin at his feet like toys. |
| `narration.mos_3_4.intro.1` | giulia_rossi | He watches us arrive from a floor above. He saw everything before we did. |
| `narration.mos_3_4.elite.before.0` | enforcer | Mr. Orlan receives by appointment. My apologies. |
| `narration.mos_3_4.elite.after.0` | ilona_varga | The door is open. He's expecting us. |
| `narration.mos_3_4.boss.before.0` | orlan | Welcome. You've seen my trophies, my auctions, my cameras. I'm honored by your curiosity. |
| `narration.mos_3_4.boss.before.1` | orlan | Moscow welcomed you, miss. I merely offered you a show worthy of it. |
| `narration.mos_3_4.boss.before.2` | agent_m | People are not a show. |
| `narration.mos_3_4.boss.before.3` | orlan | Everything is a show, miss. The only question is who holds the room. |
| `narration.mos_3_4.boss.phase1.0` | orlan | Last bid. I leave the stakes to you. |
| `narration.mos_3_4.boss.phase2.0` | orlan | Liquidation. Everything must go, myself included. |
| `narration.mos_3_4.boss.after.0` | orlan | You think you've exposed me. I'm only an executor, like so many others. |
| `narration.mos_3_4.boss.after.1` | agent_m | For whom? |
| `narration.mos_3_4.boss.after.2` | orlan | For someone above. Someone who never needed to sign. |
| `narration.mos_3_4.chapter_end.0` | narrateur | The fake video is denied, but the rumor circulates. Orlan has confessed: someone is still pulling the strings, higher up. |

**Variants.** Agent M: `narration.mos_3_4.boss.before.1` (base, already addressed to her) for Agent M; `boss.before.2` is spoken by her. Agent D: `narration.mos_3_4.boss.before.1.d`, orlan: "Moscow welcomed you, sir." · `narration.mos_3_4.boss.before.2.d` — `agent_d`: "People are not a show." · Agent M, cathedral hint: `narration.mos_3_4.boss.after.3`, orlan: "Someone who loves incense and wax. A man of order, miss." *(meaning revealed in chapter 4)*

**Fragment.** `mos_orlan_tower`: **Someone Above**: *He received you like a perfect host, in a tower overlooking the city. When he admitted he was not the last link, you understood that Moscow's lie had more than one floor.*

**QG.** Archive: *At Orlan Tower, The Dawn defeats the oligarch. He admits to working for "someone above." The fake video is denied, but the rumor circulates.* Thread: *On to Red Square.*

---

## Chapter 4: Red Square

**Opening text:** *Red Square, the Kremlin, the curfew: Moscow's final chapter is a face-to-face. The Dawn must decide whether to trust those who welcome it, or those who manipulate it.*

| Route | Added opening sentence |
|---|---|
| Agent M | *She returns to the city she thought she knew, on the square where it all began for her.* |
| Agent D | *He arrives on the square where a state decides what you get to see. He has already seen what a shop window hides.* |

#### zone_mos_4_1: Streets under curfew

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_4_1.intro.0` | narrateur | The curfew falls without warning. Shutters close, streets empty, and civilians are left outside. |
| `narration.mos_4_1.intro.1` | giulia_rossi | No siren, no notice. Just patrols wearing no insignia. |
| `narration.mos_4_1.intro.2` | civil_moscou | They told us to go home. But our door is on the other side of their roadblocks. |
| `narration.mos_4_1.intro.3` | agent_m | We'll open a path for you. Follow us, don't run. |
| `narration.mos_4_1.elite.before.0` | enforcer | Closure order. No written order, but an order. |
| `narration.mos_4_1.elite.before.1` | ilona_varga | He doesn't know who gave it. He obeys anyway. |
| `narration.mos_4_1.elite.after.0` | lucas_mendes | The civilians are safe. The roadblocks are lifted. |
| `narration.mos_4_1.exit.0` | giulia_rossi | Someone's watching us, over there, under the archway. No suit, no insignia. |
| `narration.mos_4_1.exit.1` | ilona_varga | He has a badge hidden under his coat. I know that logo. |
| `narration.mos_4_1.exit.2` | agent_m | The AMIF. It's been in Moscow from the start: it only showed itself here. |

**Variants.** Agent M: `intro.3` as written. Agent D: `narration.mos_4_1.intro.3.d` — `agent_d`: "Follow me. I'll draw the way. Don't look at the roadblocks." · Agent D, recognition: `narration.mos_4_1.exit.3`, agent_d: "New York had Stroke. Here, the AMIF stays in the shadow of a state. It's more patient."

**Fragment.** `mos_curfew`: **No Official Order**: *The streets were closing without any order, and no one dared ask who was closing them. You saw a man under an archway, a badge hidden under his coat. Moscow had learned to obey orders that don't exist.*

**QG.** Archive: *Under the Red Square curfew, The Dawn protects stranded civilians and spots an AMIF envoy in plain clothes.* Thread: *The tunnels beneath the Kremlin lead to the council.*

#### zone_mos_4_2: Tunnels beneath the Kremlin

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_4_2.intro.0` | narrateur | Beneath the Kremlin, the tunnels smell of wet stone and wax. Old cables run along the walls. |
| `narration.mos_4_2.intro.1` | lucas_mendes | These tunnels date from before the Union. Someone maintains them. |
| `narration.mos_4_2.intro.2` | giulia_rossi | Research rooms. Cages. Anatomy charts... Apex, before the word even existed. |
| `narration.mos_4_2.intro.3` | agent_m | I see the hands that worked here. They write the way they do in the laboratory margins. |
| `narration.mos_4_2.elite.before.0` | enforcer | The council is expecting you. Nobody said it was expecting you in one piece. |
| `narration.mos_4_2.elite.after.0` | ilona_varga | The passages are clear. The council is at the end. |
| `narration.mos_4_2.exit.0` | giulia_rossi | One word keeps coming up in the files: "Khost." |
| `narration.mos_4_2.exit.1` | ilona_varga | I don't know what it is, but these files talk about a place far from here. Salem. |
| `narration.mos_4_2.exit.2` | agent_m | Then all of this is the beginning of something else. |

**Variants.** Agent M: `intro.3` by her. Agent D: `narration.mos_4_2.intro.3.d` — `agent_d`: "These cages are man-sized. I'd rather not draw what was inside." · Agent M, cathedral hint: `narration.mos_4_2.intro.4`, agent_m: "That smell... wax, incense, cold stone. It's the cathedral's, before the explosion. I don't understand."

**Fragment.** `mos_kremlin_tunnels`: **Beneath the Kremlin**: *The tunnels smelled of wax, like a buried church. In the research rooms, the margin notes were the same as in the laboratory. You understood that Moscow's story didn't end at its walls.*

**QG.** Archive: *Beneath the Kremlin, The Dawn passes through old Soviet research rooms. The files carry one name: "Khost." The trail leads to Salem.* Thread: *The council waits in the back chamber.*

#### zone_mos_4_3: Council chamber

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_4_3.intro.0` | narrateur | The council chamber is round, paneled, silent. Only one chair is occupied. |
| `narration.mos_4_3.intro.1` | conseiller | Welcome. I have long awaited a visit this precise. |
| `narration.mos_4_3.intro.2` | conseiller | The AMIF hunts you. It harvests and resells your kind. I can drive it out of this city. |
| `narration.mos_4_3.intro.3` | conseiller | In exchange for your neutrality. Let Moscow settle its own affairs, and The Dawn will be free to leave. |
| `narration.mos_4_3.intro.4` | ilona_varga | He talks of neutrality. He talks of freedom. He says nothing of his trophies or his laboratories. |
| `narration.mos_4_3.elite.before.0` | agent_amif | It's time you withdrew. This file is not yours. |
| `narration.mos_4_3.elite.before.1` | conseiller | You weren't invited, sir. My men will see you out. |
| `narration.mos_4_3.elite.after.0` | giulia_rossi | The envoy has fallen. The AMIF showed itself, and it lost. |
| `narration.mos_4_3.exit.0` | conseiller | You see: I keep my promises. Yours remains. |
| `narration.mos_4_3.exit.1` | agent_m | A promise to a man who stands behind all of this. |

**Variants.** Agent M: `narration.mos_4_3.intro.1.m`, conseiller: "Miss, it was mostly you I was waiting for." Agent D: `intro.1` as written. · Agent M, cathedral hint: `narration.mos_4_3.intro.5`, agent_m: "Wax on his hands. The same scent. It's him."

**Fragment.** `mos_council_offer`: **The Offer**: *He offered you the end of the AMIF in exchange for your silence. He didn't raise his voice. You understood that lies aren't always told: sometimes one merely proposes what is convenient.*

**QG.** Archive: *In the council chamber, the Councillor offers The Dawn an alliance against the AMIF in exchange for its neutrality. An AMIF envoy is unmasked.* Thread: *The Councillor awaits an answer.*

#### zone_mos_4_4: Council chamber, the Councillor

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_4_4.intro.0` | narrateur | The council chamber empties. The guards fall back. The Councillor remains alone at the table. |
| `narration.mos_4_4.elite.before.0` | enforcer | The Councillor wouldn't harm you. Not him. He delegates. |
| `narration.mos_4_4.elite.after.0` | ilona_varga | His last men have fallen. He has only his words left. |
| `narration.mos_4_4.boss.before.0` | conseiller | You refuse. That is a choice, and I respect it. I am not a man who insists. |
| `narration.mos_4_4.boss.before.1` | conseiller | Moscow doesn't need heroes. It needs order. You are an accident that doesn't know it is one. |
| `narration.mos_4_4.boss.before.2` | agent_m | An accident? The people you sold were not an accident. |
| `narration.mos_4_4.boss.before.3` | conseiller | They were a resource. The city was hungry. |
| `narration.mos_4_4.boss.phase1.0` | conseiller | Final offer. It is no longer negotiable. |
| `narration.mos_4_4.boss.phase2.0` | conseiller | Necessary reality. I regret it has come to this. |
| `narration.mos_4_4.boss.after.0` | conseiller | You have won. I ask only a few words before you decide about me. |
| `narration.mos_4_4.choice.0` | ilona_varga | You can arrest him, or hear him out. Whatever you choose, Moscow will remember it. |
| `narration.mos_4_4.choice.1` | narrateur | Choice: arrest the Councillor, or hear him out. |
| `narration.mos_4_4.branch_arrest.0` | agent_m | Enough. You will answer for your acts before those you betrayed. |
| `narration.mos_4_4.branch_arrest.1` | conseiller | I will answer. But they will never know everything. |
| `narration.mos_4_4.branch_listen.0` | agent_m | Speak. I'm listening. |
| `narration.mos_4_4.branch_listen.1` | conseiller | The AMIF has a project, miss. Something is being prepared in Salem. My research leads there. I am not its author. |
| `narration.mos_4_4.chapter_end.0` | narrateur | The Councillor is defeated. The Soviet chambers open onto an ancient passage: it leads to Salem. |

**Variants (the cathedral revelation).** Agent M: `narration.mos_4_4.boss.after.1.m`, conseiller: "You want to know why the cathedral? A shock was needed. A real one. I needed an Apex who would learn to see. Your brother was to be the price." · `after.2`, agent_m: "Ilia... You hurt him to awaken me." · `after.3`, agent_m: "So that night was no accident. You had planned it."
Agent M, flashback: `narration.mos_4_4.boss.after.4`, ilia: "Mara... I'm here. I'm still here." *(voice-over, brief; Ilia is alive but wounded)*
Agent D: `narration.mos_4_4.boss.after.1.d`, conseiller: "You came a long way to understand a system. I could have explained it, but nobody ever wants to hear." *(no personal revelation; the scene stays political)*
Wax, incense, candles (payoff for Agent M): `narration.mos_4_4.boss.after.5`, conseiller: "My men mark their crates with candle wax. An old church habit. It has betrayed me more than once."

**Fragment.** `mos_place_rouge_choice`: **The Man of Order**: *He never shouted. He spoke of order, of resources, of hunger. When you understood that your life had been a means, you were given a choice: hand him over, or hear him out. Both have a price.*

**QG.** Archive: *In the council chamber, The Dawn defeats the Councillor. The choice to arrest or hear him shapes the epilogue. The Soviet chambers lead to Salem.* Thread: *On to Salem.*

#### Chapter epilogues (depending on the choice in 4.4)

| Key | Speaker | EN |
|---|---|---|
| `narration.mos_4.epilogue.arrest.0` | narrateur | By morning, the Councillor is handed to the authorities. The circle of power freezes, then turns against The Dawn. |
| `narration.mos_4.epilogue.arrest.1` | ilona_varga | We did what had to be done. Moscow won't forgive us any time soon. |
| `narration.mos_4.epilogue.listen.0` | narrateur | By morning, the Councillor disappears, leaving behind names and a trail. The Dawn is free, but it owes him something. |
| `narration.mos_4.epilogue.listen.1` | ilona_varga | We finally know what's at stake in Salem. We just don't know what it will cost us. |
| `narration.mos_4.epilogue.common.0` | giulia_rossi | A passage beneath the Soviet chambers. It leads farther than Moscow. |
| `narration.mos_4.epilogue.common.1` | ilona_varga | Salem. That's where everything comes together. |
