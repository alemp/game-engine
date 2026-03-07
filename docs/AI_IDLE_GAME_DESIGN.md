# AI Idle – Documentação de Design

Documento de design do jogo idle **AI Idle**, incluindo o sistema de progressão de carreira via prestige.

---

## 1. Visão Geral

**Título:** AI Idle  
**Gênero:** Idle/Incremental  
**Tema:** Programação, IA e produtividade  
**Público:** Programadores, entusiastas de IA e jogadores de idle games  

**Elevator pitch:** Um programador começa a usar agentes de IA para automatizar o trabalho. Quanto mais tokens consome, mais desbloqueia tópicos (contexto, skills, soul) que fazem a IA conceder experiência ao programador. O objetivo é escalar a produtividade com múltiplos agentes e skills até ter um sistema quase autônomo — e subir de carreira usando prestige.

---

## 2. Narrativa e Contexto

### 2.1 Premissa

O protagonista é um dev júnior que descobre agentes de IA. Ele usa tokens para conversar com a IA e automatizar tarefas. Com o tempo, percebe que a IA recompensa quem usa bem o sistema: contexto, skills e "soul" aumentam a experiência que ela concede ao programador.

### 2.2 Arco Narrativo

| Fase | Momento do jogo | Narrativa |
|------|-----------------|-----------|
| **Descoberta** | Início | Primeiro contato com a IA; tokens são escassos |
| **Confiança** | Early game | Desbloqueia tópicos e começa a ganhar experiência |
| **Escala** | Mid game | Compra agentes e automatiza tarefas |
| **Autonomia** | Late game | Agente que "clica sozinho"; o dev vira gestor |
| **Carreira** | Prestige | Promoção (Jr I → Jr II, Pleno I → Pleno II, etc.) |
| **Transcendência** | Endgame | Sistema autônomo; foco em estratégia e evolução |

---

## 3. Core Loop

### 3.1 Loop Principal

```
[Consumir Tokens] → [Conversar com IA] → [Desbloquear Tópicos]
       ↑                                            ↓
       └──── [Ganhar Experiência] ←──────────────────┘
```

### 3.2 Loop Secundário (Agentes)

```
[Comprar Agente] → [Atribuir Tarefas] → [Agente Gera Tokens]
       ↑                                        ↓
       └──── [Upgrade Agente] ←── [Acumular Tokens] ──┘
```

### 3.3 Loop de Prestige (Carreira)

```
[Acumular Tokens] → [Requisito de Carreira] → [Prestige / Subir de Carreira]
       ↑                                                      ↓
       └──── [Reset: Perde Tokens] ←── [Ganha Bônus Permanentes] ──┘
```

---

## 4. Sistema de Carreira (Prestige)

### 4.1 Conceito

Em certo ponto do jogo, o programador pode **subir de carreira** usando prestige. Ao subir:

- **Perde:** todos os tokens acumulados (reset)
- **Ganha:** bônus permanentes do prestige (multiplicadores, desbloqueios, etc.)

A progressão de carreira reflete a evolução profissional do personagem.

### 4.2 Níveis de Carreira

| Carreira | Nível | Requisito (Tokens) | Bônus de Prestige |
|----------|-------|--------------------|-------------------|
| **Júnior I** | 1 | — (início) | — |
| **Júnior II** | 2 | 10.000 tokens | +25% produção de tokens |
| **Júnior III** | 3 | 50.000 tokens | +50% produção |
| **Pleno I** | 4 | 200.000 tokens | +100% produção, +1 slot de agente |
| **Pleno II** | 5 | 1.000.000 tokens | +150% produção |
| **Pleno III** | 6 | 5.000.000 tokens | +200% produção, +1 slot de agente |
| **Sênior I** | 7 | 25.000.000 tokens | +300% produção |
| **Sênior II** | 8 | 100.000.000 tokens | +400% produção, +1 slot de agente |
| **Sênior III** | 9 | 500.000.000 tokens | +500% produção |
| **Arquiteto** | 10 | 2.000.000.000 tokens | +750% produção, +2 slots de agente |

### 4.3 Comportamento do Reset

Ao subir de carreira (prestige):

1. **Resetam:** tokens, agentes comprados, upgrades comuns, tópicos desbloqueados (ou conforme design).
2. **Persistem:** nível de carreira, multiplicadores de prestige, upgrades épicos (se houver), moeda premium (se houver).

### 4.4 Integração com o Engine

O sistema de carreira usa o **TierModule** genérico, configurado via `tiers.json`:

- **TierModule:** cada carreira = um tier; `productionMultiplier` = bônus; `TryAscend()` faz o reset.
- **Schema:** `tiers.json` com `additionalSlots` (opcional) para limitar agentes por tier. O jogo lê `GetCurrentTier().AdditionalSlots` e aplica a restrição na compra de agentes.

---

## 5. Sistemas de Jogo

### 5.1 Sistema de Tokens e Experiência

| Recurso | Função |
|---------|--------|
| **Tokens** | Usados para conversar com a IA; consumidos nas interações. Gerados por cliques e agentes (tokens/s) |
| **Experiência** | Concedida pela IA ao programador quando ele consome tokens. Tópicos desbloqueados (contexto, skills, soul) aumentam o XP ganho por token consumido |

### 5.2 Tópicos Desbloqueáveis

| Tópico | Efeito | Pré-requisito |
|--------|--------|---------------|
| **Contexto** | +X% experiência ganha | 1.000 tokens |
| **Skills** | Desbloqueia árvore de skills | 5.000 tokens |
| **Soul** | Bônus emocional; experiência extra | 10.000 tokens |
| **Memória** | IA "lembra" melhor; mais XP por interação | 25.000 tokens |
| **Autonomia** | Agentes tomam decisões melhores | 50.000 tokens |
| **Sintonia** | Sincronização entre agentes | 100.000 tokens |

### 5.3 Sistema de Agentes

| Agente | Custo | Função |
|--------|-------|--------|
| Assistente Básico | 500 | Gera tokens passivamente |
| Code Reviewer | 2.000 | Revisa código; bônus de qualidade |
| Documentador | 5.000 | Gera docs; multiplicador de contexto |
| Debugger | 12.000 | Encontra bugs; bônus de soul |
| Arquiteto | 30.000 | Planeja features; multiplicador geral |
| Agente Autônomo | 100.000 | Clica sozinho; simula jogador |

**Slots de agentes:** limitados por carreira (ex.: Jr = 2, Pleno = 4, Sênior = 6, Arquiteto = 8).

### 5.4 Árvore de Skills (Multiplicadores)

Skills desbloqueadas com experiência, que aumentam a geração de tokens (Clareza, Especificidade, Iteração, Contexto Rico, Delegação, Sintonia). Resetam no prestige, exceto se marcadas como épicas.

---

## 6. Curva de Progressão

### 6.1 Early Game (0–30 min)

- Tutorial: clicar para gerar tokens
- Primeira conversa com a IA
- Desbloqueio de Contexto
- Primeiro agente

### 6.2 Mid Game (30 min – 2 h)

- Desbloqueio de Skills e Soul
- 2–3 agentes ativos
- Primeiros multiplicadores
- **Primeira promoção:** Jr I → Jr II (prestige)

### 6.3 Late Game (2–8 h)

- Memória e Autonomia desbloqueados
- Agente Autônomo
- **Promoções:** Jr III → Pleno I → Pleno II
- Idle forte; jogador intervém pouco

### 6.4 Endgame (8+ h)

- Sintonia desbloqueada
- **Carreira final:** Sênior III → Arquiteto
- Todos os agentes maximizados
- Conquistas e desafios opcionais

---

## 7. Mapeamento para o Engine

| Conceito AI Idle | Módulo / Config |
|------------------|-----------------|
| Tokens | Resource em `resources.json` |
| Experiência | Resource em `resources.json` |
| Consumir tokens → ganhar XP | Production com `inputs: [{resourceId: "tokens"}]`, `outputId: "experience"`, `trigger: "manual"` |
| Clique para gerar tokens | Production com `trigger: "manual"` |
| Tópicos | Upgrades com `unlockCondition` (tokens) que multiplicam a production de conversa |
| Skills | Upgrades com `costResourceId: "experience"` |
| Agentes | Productions (tick) + Upgrades para comprar |
| Carreira (subir de nível) | TierModule, `tiers.json` |
| Reset ao subir de carreira | `TierModule.TryAscend()` |
| Bônus por tier | `productionMultiplier` em TierEntry |
| Slots de agentes | `additionalSlots` em TierEntry; jogo aplica `agentesComprados < GetCurrentTier().AdditionalSlots` |

---

## 8. Configuração: `tiers.json`

O AI Idle usa o schema genérico `tiers.json` (TierModule). Cada carreira é um tier. O campo `additionalSlots` está disponível no schema do engine para jogos que precisem limitar slots por tier.

**Exemplo `tiers.json` para AI Idle:**

```json
{
  "tiers": [
    {
      "id": "junior_1",
      "displayKey": "career.junior_1",
      "productionMultiplier": 1.0,
      "unlockResourceId": null,
      "unlockMinAmount": 0,
      "additionalSlots": 2
    },
    {
      "id": "junior_2",
      "displayKey": "career.junior_2",
      "productionMultiplier": 1.25,
      "unlockResourceId": "tokens",
      "unlockMinAmount": 10000,
      "additionalSlots": 2
    },
    {
      "id": "mid_1",
      "displayKey": "career.mid_1",
      "productionMultiplier": 2.0,
      "unlockResourceId": "tokens",
      "unlockMinAmount": 200000,
      "additionalSlots": 4
    }
  ]
}
```

- Campos: ver [IDLE_V2_FEATURES.md](IDLE_V2_FEATURES.md) §5. `additionalSlots` é opcional; o jogo usa para limitar compra de agentes.

---

## 9. Resumo do Sistema de Carreira

| Aspecto | Comportamento |
|---------|---------------|
| **Trigger** | Atingir requisito de tokens da próxima carreira |
| **Custo** | Todos os tokens (reset) |
| **Benefício** | Multiplicador de produção + slots de agentes |
| **Persistência** | Nível de carreira e bônus permanecem |
| **Narrativa** | Promoção (Jr I → Jr II, Pleno I → Pleno II, etc.) |

---

*Documento de design do AI Idle. Última atualização: Março 2025.*
