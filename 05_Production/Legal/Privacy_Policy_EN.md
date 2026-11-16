# Privacy Policy — Heroes Dawn (draft)

> **Draft, to be reviewed by a lawyer before publication.** Written on 2026-11-16 from what the 1.0.0 code actually does. Bracketed fields must be filled in by the studio. French version (reference): `Politique_Confidentialite.md`.

Last updated: [publication date]

## 1. Who we are

Heroes Dawn is published by **[Studio N29 legal name, legal form, address, registration number]** ("we"), the data controller.
Contact: **[contact e-mail address]**.

## 2. Data we process and why

| Data | When | Purpose | Legal basis (GDPR) |
|---|---|---|---|
| E-mail address and account identifier; the identifier returned by Google or Apple if you use those sign-in options | If you create an account (optional: the game can be played without one) | Sign you in and restore your progress on another device | Performance of a contract |
| Game save (progress, team, virtual currencies, settings, nickname and player tag) | While playing; sent online only if you have an account | Cloud save, sync across devices | Performance of a contract |
| Your tournament team, nickname, points and ranking | If you take part in tournaments with an account | Duels against other players, leaderboard visible to other signed-in players | Performance of a contract |
| Purchase and transaction identifiers | If you make an in-app purchase | Deliver the purchase, prevent double grants, resume an interrupted purchase | Performance of a contract, legal obligation (accounting) |
| Anonymous usage statistics: screens visited, battles, zones cleared, game version, platform, session events | **Only if you accept** the question asked at first sign-in | Understand how the game is played and improve it | Consent, which you can withdraw at any time |

We do not collect your name, location, contacts or photos. The game shows no ads and shares no data for advertising. We do not sell data.

Payment data (card details, etc.) is handled by Google Play or the App Store; we never have access to it.

## 3. Service providers (processors)

| Provider | Role | Location |
|---|---|---|
| Supabase | Accounts, cloud save, tournaments | European Union (eu-central-1, Frankfurt) |
| Unity Technologies (Unity Gaming Services Analytics) | Usage statistics, only with your consent | [to confirm: processing region chosen in the UGS dashboard; transfers outside the EU covered by Unity's standard contractual clauses] |
| Google Play / Apple App Store (Unity IAP) | In-app purchases | Depends on the store |
| Google, Apple | "Sign in with Google / Apple", if you choose it | Depends on the provider |

## 4. Retention

- Account and cloud save: as long as the account exists; deleted on request (section 6) or after **[inactivity period to set, e.g. 36 months]**.
- Tournament data: team snapshots are no longer used for duels after 60 days; scores are kept for edition history until the account is deleted.
- Usage statistics: **[retention set in UGS, to confirm]**.
- Purchase identifiers: as long as accounting law requires.

## 5. Children

Heroes Dawn is not directed at children under **[13 or 16, to decide with the age rating]**. We do not knowingly collect data from children below that age; if you believe a child has sent us data, contact us to have it deleted.

## 6. Your rights

You may request access to, correction or deletion of your data, restriction of processing, portability, and you may object to processing. You can withdraw your consent to statistics at any time in **Settings ▸ Account & privacy**.

- **Delete your progress**: Settings ▸ Account & privacy ▸ "Erase save" (also erases the account's cloud save).
- **Delete your account**: write to **[contact e-mail address]** from the account's address; deletion is completed within 30 days.

You may also lodge a complaint with your country's data protection authority.

## 7. Security

All traffic is encrypted (HTTPS, WSS). Each player can only access their own account data (server-side row-level security). Passwords are handled by Supabase Auth and are never readable by us.

## 8. Changes

We may update this policy; the date at the top applies, and any significant change is announced in the game.
