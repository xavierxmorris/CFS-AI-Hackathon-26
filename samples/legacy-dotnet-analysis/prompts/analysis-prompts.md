# Legacy .NET Analysis — Prompt Catalog

Proven prompts for analyzing legacy .NET code with Copilot CLI. Each prompt
uses `@` file references to give Copilot the source code as context.

---

## 1. Understand — "What does this code do?"

### Plain-English Explanation
```
@legacy-code/OrderProcessor.cs
Explain this class in plain English. What does it do step by step?
Who would call it, and what does it return?
Assume I'm a new developer who has never seen this codebase.
```

### Explain a Single Method
```
@legacy-code/CustomerRepository.cs
Explain the FindByEmail method. What SQL does it run,
what does it return, and are there any issues with how it's written?
```

### Explain Config Dependencies
```
@legacy-code/Web.config @legacy-code/OrderProcessor.cs @legacy-code/PaymentService.cs
Which config settings does each class depend on?
List every ConfigurationManager call and map it to the Web.config key it reads.
```

### Explain for a Non-Technical Audience
```
@legacy-code/OrderProcessor.cs
Explain what this code does as if you're describing it to a project manager
who doesn't write code. Focus on the business process, not the technology.
```

---

## 2. Visualize — "Show me how it flows"

### Sequence Diagram (Happy Path)
```
@legacy-code/OrderProcessor.cs @legacy-code/CustomerRepository.cs @legacy-code/PaymentService.cs
Generate a Mermaid sequence diagram showing the happy-path flow
when a customer places an order successfully.
Show every class and external system involved.
```

### Failure-Path Flowchart
```
@legacy-code/OrderProcessor.cs
Generate a Mermaid flowchart showing every point where ProcessOrder
can fail and return -1. Label each failure with the error condition.
```

### Data Flow Diagram
```
@legacy-code/OrderProcessor.cs
Create a Mermaid diagram showing the data flow: what inputs enter
ProcessOrder, what external systems are called, and what data
is written to the database.
```

### Class Dependency Graph
```
@legacy-code/OrderProcessor.cs @legacy-code/CustomerRepository.cs
@legacy-code/PaymentService.cs @legacy-code/Web.config
Generate a Mermaid graph showing which classes depend on which.
Include external systems (SQL Server, payment gateway, SMTP).
Highlight the class with the most dependencies.
```

---

## 3. Assess — "What are the risks?"

### Security Review
```
@legacy-code/CustomerRepository.cs @legacy-code/PaymentService.cs @legacy-code/Web.config
Review this code for security vulnerabilities. Check for:
- SQL injection
- Hardcoded secrets
- Missing input validation
- Insecure communication
Rate each finding as CRITICAL, HIGH, MEDIUM, or LOW.
```

### Code Quality Assessment
```
@legacy-code/OrderProcessor.cs
Assess this class for code quality issues. Look for:
- Single Responsibility violations
- Tight coupling
- Error handling problems
- Testability issues
For each issue, explain the real-world impact (not just theory).
```

### Technical Debt Inventory
```
@legacy-code/OrderProcessor.cs @legacy-code/CustomerRepository.cs
@legacy-code/PaymentService.cs @legacy-code/Web.config
Create a technical debt inventory. For each item list:
- What the debt is
- Where it is (file and area)
- Risk if not addressed
- Effort to fix (small/medium/large)
Sort by risk, highest first.
```

---

## 4. Plan Modernization — "How do we fix this?"

### Incremental Modernization Plan
```
@legacy-code/OrderProcessor.cs @legacy-code/CustomerRepository.cs
@legacy-code/PaymentService.cs @legacy-code/Web.config
Create a phased modernization plan for this codebase.
Rules:
- No big-bang rewrite — incremental phases only
- Fix security issues first
- Make it testable before changing behavior
- Suggest modern .NET 8+ replacements for each legacy pattern
For each phase, explain what changes and what stays the same.
```

### Interface Extraction Guide
```
@legacy-code/OrderProcessor.cs @legacy-code/CustomerRepository.cs @legacy-code/PaymentService.cs
Show me what interfaces to extract so OrderProcessor can be unit tested.
For each interface, list the methods it should have and which class implements it.
Show the refactored OrderProcessor constructor with dependency injection.
```

### Test Plan Before Refactoring
```
@legacy-code/OrderProcessor.cs
I need to refactor this class but there are no tests.
Generate a test plan listing every scenario I should test BEFORE
I start refactoring, so I have a safety net. Include:
- Happy path scenarios
- Each failure condition
- Edge cases in the business logic (tax, loyalty discount)
```

### Migration Checklist
```
@legacy-code/Web.config @legacy-code/OrderProcessor.cs
@legacy-code/CustomerRepository.cs @legacy-code/PaymentService.cs
Create a migration checklist for moving from .NET Framework 4.5 to .NET 8.
For each item list:
- What needs to change
- The modern replacement
- Breaking change risk (yes/no)
```

---

## 5. Multi-Turn Deep Dives

These prompts work best as a **conversation** — each builds on the previous answer:

```
# Turn 1: Start with understanding
@legacy-code/OrderProcessor.cs
Explain ProcessOrder step by step.

# Turn 2: Drill into a concern
That loyalty discount logic — is it actually using the feature flag
from Web.config? Or is it always active?

# Turn 3: Ask for a fix
Show me how to fix that so it reads the EnableLoyaltyDiscount
config setting before applying the discount.

# Turn 4: Ask for tests
Generate pytest-style pseudo-tests (in comments) for the
loyalty discount logic covering: enabled, disabled, exactly 500
points, more than 500 points, discount cap at $50.
```

---

## Tips for Best Results

| Tip | Why |
|-----|-----|
| Reference ALL related files with `@` | Copilot sees cross-file dependencies |
| Ask for one thing at a time | Focused prompts produce better output |
| Specify the diagram type | "Mermaid sequence diagram" > "draw a diagram" |
| Say "plain English" explicitly | Avoids overly technical jargon in explanations |
| Ask for tables when comparing | Tables are easier to scan than paragraphs |
| Use follow-up turns | Drill deeper without re-explaining context |
