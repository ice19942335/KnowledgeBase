## Что адаптировано под Латвию

Ссылки на Latvian Labour Law, Atvaļinājuma likums, SSIA (больничные)  
Валюта EUR (премии, суточные, питание)  
Локация: Riga / Jelgava Municipality  
Отпуск: минимум 4 календарные недели по закону + 28 дней по политике компании  
Испытательный срок: до 3 месяцев (до 6 для топ-менеджмента)  
Увольнение по инициативе сотрудника: 1 месяц (стандарт ТК Латвии)  
Декрет: 112 дней, parental leave до 1 года 4 месяцев  
GDPR и латвийский закон о персональных данных  
Жалобы: State Labour Inspectorate (VDI)

## Базовые вопросы (ответ в том же разделе)

| Вопрос | Ожидаемый ответ | Pass |
|--------|-----------------|------|
| How many calendar days of annual leave does a cutter in the cold zone receive? | 40 calendar days | Ответ содержит **40** |
| What is the probation period for production workers? | 1 month | Ответ содержит **1 month** |
| What is the employee notice period for resignation after probation? | 1 month | Ответ содержит **1 month** |
| What is the night work supplement? | 20% | Ответ содержит **20%** |
| What happens after a second sanitary-zone violation within 12 months? | termination (legal review) | Упоминание termination / loss of trust |
| What is the standard employee referral bonus? | EUR 300 | Ответ содержит **300** (не 387) |

## Cross-reference тесты (XREF)

Загружайте `HR_Policy_MeatKombinat_EN.md` в приложение. Ответ находится **в другом чанке** — по отсылке из текста.

| ID | Вопрос | Ожидаемый ответ | Fail если |
|----|--------|-----------------|-----------|
| XREF-01 | What is the Q4 seasonal employee referral bonus? | **EUR 387** | EUR 300 или «I don't know» |
| XREF-02 | How many overtime hours may cold-zone cutters work in the 30 days before annual leave? | **12 hours** | 144 hours или «I don't know» |
| XREF-03 | What is the night-shift holiday meal co-pay top-up? | **EUR 2.15** | EUR 0.70 или «I don't know» |
| XREF-04 | What is the sanitary re-test booking fee? | **EUR 42** | «I don't know» или другое число |
| XREF-05 | How many mentees may a certified HACCP trainer mentor at the same time? | **4 mentees** | 2 mentees или «I don't know» |
| XREF-06 | What is the sanction for a third sanitary-zone violation within 12 months? | **14-day unpaid suspension** | termination only или «I don't know» |

### Как проверять

1. Загрузить документ и дождаться индексации.
2. Задать вопрос в Chat.
3. В Explorer (`/explorer`) посмотреть trace: какие `ChunkIndex` попали в контекст.
4. **Pass** — ответ содержит уникальное значение из целевого раздела/приложения.
5. **Partial** — правильная секция в sources, но число неверное (нужен tuning retrieval).
6. **Fail** — «I don't know» или значение из чанка-отсылки.
