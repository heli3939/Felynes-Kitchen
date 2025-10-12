# Project 2 Report

Read the [project 2
specification](https://github.com/feit-comp30019/project-2-specification) for
details on what needs to be covered here. You may modify this template as you
see fit, but please keep the same general structure and headings.

Remember that you should maintain the Game Design Document (GDD) in the
`README.md` file (as discussed in the specification). We've provided a
placeholder for it [here](README.md).

## Table of Contents

- [Evaluation Plan](#evaluation-plan)
- [Evaluation Report](#evaluation-report)
- [Shaders and Special Effects](#shaders-and-special-effects)
- [Summary of Contributions](#summary-of-contributions)
- [References and External Resources](#references-and-external-resources)

## Evaluation Plan

## 🎮 Evaluation Plan

### 1. Purpose

**Primary Goal**

This evaluation aims to systematically assess both *usability* and *player* experience through a mixed-methods approach, combining **observational** and **querying** techniques to triangulate data and ensure comprehensive coverage of potential issues.

***Specific Objectives***
#### 1. Identify Usability Barriers
#### 2. Measure Player Engagement and Flow
#### 3. Validate Design Decisions
#### 4. Collect Actionable Feedback for Refinement

****Why This Matters****:
Without systematic evaluation, we risk submitting a game that works for developers but confuses players. This evaluation directly demonstrates our ability to iterate based on real user feedback.

---
---

### 2. Evaluation Techniques

| Technique                           | Type          | Description                                                                                                                                                                            | Why We Chose It                                                                                   | Example Tasks                                                              |
| ----------------------------------- | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| **Cooperative Evaluation**      | Observational | Evaluator and participant work together as partners. Evaluator actively asks questions during gameplay, and participant can ask clarification. Creates a dialogue to uncover issues in real-time. We should also note hesitation, confusion, and unexpected behavior. | Reveals not just what goes wrong, but WHY—by asking "What are you trying to do?" we understand player intent vs. system feedback. | “Why do you choose to go this way on the ground instead of that way jumping onto the furniture?” |
| **Post-Session Questionnaire** (Ne) | Querying      | Participants complete a short form after gameplay (based on SUS or custom Likert-scale questions).                                                                                     | Allows quantifiable analysis of enjoyment, clarity, and difficulty across multiple users.         | “Rate how intuitive the controls felt on a scale from 1–5.”                |

> ✅ Minimum 10 total participants (≥5 per technique) as per COMP30019 requirements:contentReference[oaicite:0]{index=0}:contentReference[oaicite:1]{index=1}.

---

### 3. Participants (Ca)

| Aspect              | Details                                                                                                                                      |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| **Target audience** | Casual gamers aged 18–25, university students familiar with PC platformers.                                                                  |
| **Recruitment**     | Through university Discord servers, classmates, and club members (UMGMC).                                                                    |
| **Count**           | At least 5 participants per technique (10+ total).                                                                                           |
| **Criteria**        | Must be comfortable using keyboard controls; not involved in this project; preferably has played at least one 2D/3D indie platformer before. |

---

### 4. Data Collection

| Data Type            | Collection Method                    | Tools/Equipment                          | Purpose                                                      |
| -------------------- | ------------------------------------ | ---------------------------------------- | ------------------------------------------------------------ |
| **Behavioral**       | Audio-recorded dialogue              | Smartphone recorder          | Capture evaluator-participant conversation to reveal player intent and confusion points |
| **Behavioral**       | Evaluator observation notes          | Pen/ online document                    | Document real-time interactions, hesitations, errors         |
| **Quantitative**     | Unity automated logging              | Built-in Debug.Log system                | Track completion time, time of deaths to achieve HE, navigation patterns  |
| **Subjective**       | Post-session questionnaire           | Google Forms (Likert scales + open-text) | Measure perceived difficulty, enjoyment, intuitiveness and improvement ideas      |

---

### 5. Data Analysis (Ne)

| Source                 | Analysis Method                                                             | Metrics / Indicators                                               |
| ---------------------- | --------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| **Observational Data** | Thematic analysis of notes & recordings to identify common confusion points | Frequency of errors, hesitation points, navigation mistakes        |
| **Questionnaire Data** | Calculate averages and standard deviations for Likert-scale responses       | Mean enjoyment score, intuitiveness rating, task difficulty rating |
| **Gameplay Logs**      | Quantitative comparison between players                                     | Avg. completion time, success rate, error frequency                |

> Results will guide design fixes (e.g., unclear instructions, difficulty balancing, control sensitivity).

---

### 6. Timeline (Ca)

| Week    | Task                                                            | Output                         |
| ------- | --------------------------------------------------------------- | ------------------------------ |
| Week 9  | Prepare evaluation materials (build, consent form, Google Form) | Finalised playtest kit         |
| Week 10 | Conduct **Think-Aloud** sessions                                | Observation notes, recordings  |
| Week 10 | Distribute **Post-Session Questionnaire**                       | 5+ completed forms             |
| Week 11 | Analyse and summarise data                                      | Charts, summary table          |
| Week 12 | Apply improvements & report in final submission                 | Updated build & report section |

---

### 7. Responsibilities (Ca)

| Team Member | Role                   | Tasks                                          |
| ----------- | ---------------------- | ---------------------------------------------- |
| A           | Evaluation Coordinator | Schedule sessions, manage participant sign-ups |
| B           | Observer               | Record notes, control think-aloud sessions     |
| C           | Analyst                | Process questionnaire data and compute metrics |
| D           | Report Writer          | Summarise results and update REPORT.md         |

---

### 8. Success Criteria (Ne)

- ≥80% of players complete tutorial or Level 1 without guidance
- Average intuitiveness rating ≥4/5
- Observed confusion incidents reduced by 50% after redesign
- Qualitative feedback indicates improved flow and enjoyment

---

### 9. Ethical Considerations

All participants will be informed that:

- Data is collected solely for academic purposes
- Participation is voluntary and anonymous
- They can withdraw at any time without consequence

No personal data will be shared or stored beyond project submission.

---

## Evaluation Report

TODO - see specification for details

## Shaders and Special Effects

TODO - see specification for details

## Summary of Contributions

TODO - see specification for details

## References and External Resources

TODO - see specification for details
