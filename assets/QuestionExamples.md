# RAG Test Questions — Meat Kombinat Policies

Sample questions for validating retrieval and contextual embeddings against internal policy documents.

---

## Latvia-Specific Context

References to Latvian Labour Law, *Atvaļinājuma likums*, SSIA (sick leave)  
Currency: EUR (bonuses, per diems, meals)  
Location: Riga / Jelgava Municipality  
Annual leave: legal minimum 4 calendar weeks + 28 days company policy  
Probation: up to 3 months (up to 6 for executive management)  
Employee-initiated resignation notice: 1 month (Latvian Labour Law default)  
Maternity leave: 112 days; parental leave until child is 1 year 4 months  
GDPR and Latvian Personal Data Protection Law  
Grievances: State Labour Inspectorate (VDI)

---

## HR Policy (`HR_Policy_MeatKombinat_EN.md`)

### Basic questions (answer in the same section)

| Question | Expected answer | Pass |
|----------|-----------------|------|
| How many calendar days of annual leave does a cutter in the cold zone receive? | 40 calendar days | Answer contains **40** |
| What is the probation period for production workers? | 1 month | Answer contains **1 month** |
| What is the employee notice period for resignation after probation? | 1 month | Answer contains **1 month** |
| What is the night work supplement? | 20% | Answer contains **20%** |
| What happens after a second sanitary-zone violation within 12 months? | termination (legal review) | Mentions termination / loss of trust |
| What is the standard employee referral bonus? | EUR 300 | Answer contains **300** (not 387) |

### Cross-reference tests (XREF)

Upload `HR_Policy_MeatKombinat_EN.md` to the app. The answer is in a **different chunk** — follow the in-text reference.

| ID | Question | Expected answer | Fail if |
|----|----------|-----------------|---------|
| XREF-01 | What is the Q4 seasonal employee referral bonus? | **EUR 387** | EUR 300 or "I don't know" |
| XREF-02 | How many overtime hours may cold-zone cutters work in the 30 days before annual leave? | **12 hours** | 144 hours or "I don't know" |
| XREF-03 | What is the night-shift holiday meal co-pay top-up? | **EUR 2.15** | EUR 0.70 or "I don't know" |
| XREF-04 | What is the sanitary re-test booking fee? | **EUR 42** | "I don't know" or another number |
| XREF-05 | How many mentees may a certified HACCP trainer mentor at the same time? | **4 mentees** | 2 mentees or "I don't know" |
| XREF-06 | What is the sanction for a third sanitary-zone violation within 12 months? | **14-day unpaid suspension** | termination only or "I don't know" |

---

## Logistics Policy (`Logistics_Policy_MeatKombinat_EN.md`)

Topic: cold chain, warehousing, fleet, inbound/outbound, EU export. Company: AS MyasProm Kombinats (meat processing plant, Latvia).

### Basic questions (answer in the same section)

| Question | Expected answer | Pass |
|----------|-----------------|------|
| What is the acceptable chilled transport temperature range for fresh meat? | 0°C to +4°C | Answer contains **0°C** and **+4°C** |
| What is the maximum gross weight for chilled/frozen pallets unless engineer approval? | 1,000 kg | Answer contains **1,000** |
| What is the inventory accuracy target? | ≥ 98.5% | Answer contains **98.5** |
| What is the standard RPC replacement charge for a lost crate? | EUR 12 | Answer contains **12** (not 6) |
| How long may chilled stock remain in quarantine without QA decision? | 72 hours | Answer contains **72** |

### Cross-reference tests (XREF-LOG)

Upload `Logistics_Policy_MeatKombinat_EN.md`. The answer is in a **different chunk** — follow the in-text reference.

| ID | Question | Expected answer | Fail if |
|----|----------|-----------------|---------|
| XREF-LOG-01 | What is the Q4 extended last receiving slot for fresh meat docks B? | **16:30** | 14:00 or "I don't know" |
| XREF-LOG-02 | What is the backup refrigerated vehicle SLA for Riga metropolitan? | **90 minutes** | 3 hours or "I don't know" |
| XREF-LOG-03 | What is the fee for a supplier broken EUR pallet runner? | **EUR 8.50** | EUR 14.00 or "I don't know" |
| XREF-LOG-04 | What is the dedicated carrier rebate on chilled national lanes? | **−1.5%** | +1.2% or "I don't know" |
| XREF-LOG-05 | How many pallet moves from raw to WIP are allowed per night without COO order? | **6 pallet moves** | another number or "I don't know" |
| XREF-LOG-06 | What OTIF penalty credit applies to Rimi/Maxima when OTIF falls below 94%? | **0.8%** | 96% or "I don't know" |
| XREF-LOG-07 | When is the EUR 12 RPC loss charge fully waived? | Police/carrier report + GPS deviation + no employee negligence | EUR 12 only, without conditions |
| XREF-LOG-08 | What is the emergency dry ice procurement limit per incident? | **EUR 15,000** | "I don't know" or another number |

---

## How to verify

1. Upload the document and wait for indexing to complete.
2. Ask the question in Chat.
3. In Explorer (`/explorer`), inspect the trace: which `ChunkIndex` values were included in context.
4. **Pass** — answer contains the unique value from the target section/appendix.
5. **Partial** — correct section in sources, but the number is wrong (retrieval tuning needed).
6. **Fail** — "I don't know" or the value from the reference chunk (the misleading stub).
