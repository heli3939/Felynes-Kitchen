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

### 1. Purpose

**Primary Goal**

This evaluation aims to systematically assess both _usability_ and _player_ experience through a mixed-methods approach, combining **observational** and **querying** techniques to triangulate data and ensure comprehensive coverage of potential issues.

**_Specific Objectives_**

#### 1. Identify Usability Barriers

#### 2. Measure Player Engagement and Flow

#### 3. Validate Design Decisions

#### 4. Collect Actionable Feedback for Refinement

\***\*Why This Matters\*\***:
Without systematic evaluation, we risk submitting a game that works for developers but confuses players. This evaluation directly demonstrates our ability to iterate based on real user feedback.

---

---

### 2. Evaluation Techniques

| Technique                      | Type          | Description                                                                                                                                                                                                                                                           | Why We Chose It                                                                                                                   | Example Tasks                                                                                    |
| ------------------------------ | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| **Cooperative Evaluation**     | Observational | Evaluator and participant work together as partners. Evaluator actively asks questions during gameplay, and participant can ask clarification. Creates a dialogue to uncover issues in real-time. We should also note hesitation, confusion, and unexpected behavior. | Reveals not just what goes wrong, but WHY—by asking "What are you trying to do?" we understand player intent vs. system feedback. | “Why do you choose to go this way on the ground instead of that way jumping onto the furniture?” |
| **Post-Session Questionnaire** | Querying      | Participants complete a short form immediately after gameplay to reflect on their experience.                                                                                                                                                                         | Allows quantifiable analysis of enjoyment, clarity, difficulty, and appeal factors across multiple users.                         | Example: “Rate your overall enjoyment of the game on a scale from 1–5.”                          |

| **Questionnaire - Detailed Questions** |                                                                                                                                                                                    |                                                                                     |                                       |
| -------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- | ------------------------------------- |
| **Category**                           | **Question**                                                                                                                                                                       | **Purpose**                                                                         | **Response Type**                     |
| Enjoyment                              | Rate your overall enjoyment of the game (1 – Not fun, 5 – Very fun).                                                                                                               | Measure overall satisfaction and engagement.                                        | Likert scale (1–5)                    |
| Usability & Clarity                    | How intuitive were the controls? (1 – Confusing, 5 – Very intuitive)                                                                                                               | Evaluate ease of use and control design.                                            | Likert scale (1–5)                    |
| Usability & Clarity                    | Did you always understand your current objective?                                                                                                                                  | Assess clarity of gameplay goals and instructions.                                  | Multiple choice (Yes / No / Somewhat) |
| Usability & Clarity                    | How responsive did the character feel when you pressed keys or interacted? (1–5)                                                                                                   | Identify issues with input responsiveness and feedback.                             | Likert scale (1–5)                    |
| Appeal Factors                         | Which aspects most attracted you to the game? (Select up to 2): <br>• Art style <br>• Strong interactivity and environment reactions <br>• “Run from danger” mechanics <br>• Story | Discover what draws players in and informs future design priorities.                | Multiple choice                       |
| Difficulty & Balance                   | How challenging did you find the game? (1 – Too easy, 5 – Too hard)                                                                                                                | Evaluate balance and pacing of difficulty.                                          | Likert scale (1–5)                    |
| Error & Bug Experience                 | Did you encounter any errors or unexpected behaviour during gameplay?                                                                                                              | Detect and categorise potential bugs or technical issues affecting user experience. | Multiple choice (Yes / No)            |
| Error & Bug Experience                 | If yes, please briefly describe what happened and when.                                                                                                                            | Gather detailed feedback for debugging and improvement.                             | Short answer                          |
| Open Feedback                          | What was your favorite part of the game?                                                                                                                                           | Capture qualitative feedback on highlights and strengths.                           | Short answer                          |
| Open Feedback                          | What part would you improve or change?                                                                                                                                             | Gather suggestions for future iteration.                                            | Short answer                          |
| Open Feedback                          | What was your favorite part of the game?                                                                                                                                           | Capture qualitative feedback on highlights and strengths.                           | Short answer                          |
| Open Feedback                          | What part would you improve or change?                                                                                                                                             | Gather suggestions for future iteration.                                            | Short answer                          |

---

### 3. Participants

| Aspect              | Details                                                                                                                                                                                                                                                                              |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Target audience** | Casual gamers (**young adults** and teenagers), familar and/or have interests in **cooking**, **adventure** and **platformer** games (e.g. Overcooked!, Ori and the Blind Forest, Untitled Goose Game) in .                                                                          |
| **Recruitment**     | COMP30019 classmates and friends                                                                                                                                                                                                                                                     |
| **Count**           | 5 (Observational) + 5 (Querying) = 10 (total)                                                                                                                                                                                                                                        |
| **Criteria**        | Participants must: <br>• Be comfortable using keyboard controls <br>• Not be involved in the development of this project <br>• Have played at least one 2D or 3D platformer before <br>• (Querying only) Have access to a device capable of running the game on WebGL (PC or laptop) |

---

### 4. Data Collection

| Data Type        | Collection Method           | Tools/Equipment                          | Purpose                                                                                 |
| ---------------- | --------------------------- | ---------------------------------------- | --------------------------------------------------------------------------------------- |
| **Behavioral**   | Audio-recorded dialogue     | Smartphone recorder                      | Capture evaluator-participant conversation to reveal player intent and confusion points |
| **Behavioral**   | Evaluator observation notes | Pen/ online document                     | Document real-time interactions, hesitations, errors                                    |
| **Quantitative** | Unity automated logging     | Built-in Debug.Log system                | Track completion time, time of deaths to achieve HE, navigation patterns                |
| **Subjective**   | Post-session questionnaire  | Google Forms (Likert scales + open-text) | Measure perceived difficulty, enjoyment, intuitiveness and improvement ideas            |

---

### 5. Data Analysis

| Source                 | Analysis Method                                                                                                               | Metrics / Indicators                                                                                  |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| **Observational Data** | Thematic analysis of notes and screen recordings to identify confusion points, hesitation patterns, and behavioural trends.   | Counts of errors, hesitation points, navigation mistakes                                              |
| **Questionnaire Data** | Statistical analysis of post-session responses (e.g., calculating means, standard deviations, and distribution patterns).     | Mean enjoyment score, intuitiveness rating, perceived difficulty, reported bug frequency              |
| **Gameplay Logs**      | Quantitative comparison across participants and correlation analysis between performance metrics and questionnaire responses. | Total time to complete the game, number of deaths, QTE success rate, error frequency, completion rate |

> Results from these analyses will guide targeted design improvements — for example, refining unclear tutorials, rebalancing difficulty, or improving QTE loops.

---

### 6. Timeline

| Due Date     | Task                                                                                                            |
| ------------ | --------------------------------------------------------------------------------------------------------------- |
| Oct 16 (Thu) | Prepare evaluation materials (session design for Observational and Questionnaire for Querying)                  |
| Oct 17 (Fri) | Finish scheduling for observational game-test sessions; send game link and questionnaire for querying game-test |
| Oct 23 (Thu) | **Data Collection** Finish conducting all observational sessions and get back feedback from all questionnaries  |
| Oct 28 (Tue) | **Data Analysis** Finish data analyse and summarise collected data (observations, logs, and responses)          |
| Oct 31 (Fri) | Finish report writing                                                                                           |

---

### 7. Responsibilities

| Team Member             | Responsibilities (Aligned with Timeline)                                                                                                                                                                                                                                                                                                                       |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Kexin Liang**         | **Preparation (Oct 16–17):** Lead design of evaluation materials (Think-Aloud protocol and questionnaire). <br> **Data Collection (Oct 17–23):** Host and observe live game-testing sessions, take observation notes and manage screen recordings. <br> **Reporting (Oct 28–31):** Summarise observational findings and provide insights for the final report. |
| **Difei Li**            | **Preparation (Oct 16–17):** Handle participant recruitment and scheduling for both evaluation methods. <br> **Data Collection (Oct 17–23):** Distribute and track questionnaire responses; ensure all forms are returned. <br> **Reporting (Oct 28–31):** Clean and compile all raw data for analysis.                                                        |
| **Xinyue (Cassie) Luo** | **Data Analysis (Oct 23–28):** Lead analysis of both qualitative (Think-Aloud) and quantitative (questionnaire) data; create charts and tables. <br> **Reporting (Oct 28–31):** Draft the Evaluation section in REPORT.md, integrate findings into the final submission, and ensure consistency across all results.                                            |

---

### 8. Success Criteria

| **Criterion**              | **Target**                                                               |
| -------------------------- | ------------------------------------------------------------------------ |
| **Completion Rate**        | ≥ 80% of players complete the game with the help of the tutorial.        |
| **Confusion Reduction**    | Observed confusion incidents reduced by ≥ 80% after redesign iterations. |
| **Performance Indicators** | ≥ 70% average QTE success (perfect/good) rate among players.             |
| **Engagement**             | ≥ 80% of participants report positive enjoyment (rating ≥ 4 / 5).        |

---

### 9. Ethical Considerations

All participants will be informed that:

- Data is collected solely for academic purposes
- Participation is voluntary and anonymous
- They can withdraw at any time without consequence

No personal data will be shared or stored beyond project submission.

---

## Evaluation Report

### summarise your evaluation:

() people tested by () method
both method at the same time, iteratively changing
changes you made (briefly mention feedback and effect):
first round of changes:

### changes we made:

First Round:
(most important)

- long press high jump -> allow double jump (smooth control)
- finish cake at any time (early quit)
- 3 Hp + lower fire chance (difficulty balance)
  (less importnat)
- adjust model for smoothier ...
- cat starting point
- cat won't -hp when performing oven QTE
- force first-time player to read tutorial
- if pot is not at original position, can't perform QTE
- add a sign for pot direction
- adjust main camera angle and height

Second round of changes: (more coming...)

- don't get stuck in oven
- fridge only milk can pick up
- cardboard fix
- clearer instruction for using oven
- ban other button when one is using

### Findings:

Both:
Player demographic: (most player are ..., link back to target audience in evaluation plan / GDD)

- play platformer
- game development
  Playing time (pie chart for all testers): explain result

Observational:

- first ingredient they found
- before we change controls and model position, hard to control and got stuck ...

Query:

- talk about some result of "Evaluate our game for following criteria"
- top 3 (?) LIKE and IMPROVED in query

## Shaders and Special Effects

- descriptions: what? what does the shader do (effect)?
- rationales: why we use the shader in the game (enhance/ improve...)
- exact path (with link)
- images/gifs
- showwing how the shader effects fit into the rendering pipeline/Unity engine (link to theory)

## Summary of Contributions

code contribution on each .cs/.shader

## References and External Resources

TODO - see specification for details
