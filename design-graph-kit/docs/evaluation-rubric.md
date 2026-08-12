# Evaluation Rubric

Use both deterministic scoring and human review.

A structurally valid graph can still be semantically poor.

## Human scoring

Score each category 0–5.

### 1. Structure

5:
- important hierarchy is correct;
- meaningless drawing wrappers are omitted;
- meaningful regions/components are preserved.

0:
- hierarchy is unusable or mostly a raw layer tree.

### 2. Semantics

5:
- visible controls/content are correctly classified;
- semantic roles are useful and supported.

0:
- frequent misclassification or invented semantics.

### 3. Consolidation

5:
- repeated structures become reusable concepts;
- unrelated structures are not incorrectly merged.

0:
- no reuse recognition or destructive over-consolidation.

### 4. State modeling

5:
- loading/empty/error/etc. are states of the correct concept.

0:
- states are modeled as unrelated screens/components.

### 5. Token normalization

5:
- recurring values are normalized meaningfully;
- one-offs are not over-tokenized.

0:
- no normalization or token explosion.

### 6. Relationships

5:
- graph edges are accurate and useful.

0:
- edges are missing, contradictory, or invented.

### 7. Uncertainty discipline

5:
- unsupported behavior is left unresolved;
- inference confidence is calibrated.

0:
- the graph confidently invents hidden behavior.

### 8. Stability

5:
- repeated runs converge on comparable concepts and IDs.

0:
- identical input produces materially different semantic graphs.

## Hallucination penalty

Treat unsupported behavior as a major defect.

Examples:
- invented `SaveProfileCommand`;
- invented navigation target;
- invented binding path;
- invented data model.

Recommended policy:
- any severe unsupported behavior prevents a run from being considered passing regardless of aggregate score.

## Suggested passing threshold

Prototype guidance:

- average >= 4.0/5 across categories;
- no severe hallucinations;
- schema validation passes;
- graph-integrity validation passes.

Adjust after real-world eval data is collected.
