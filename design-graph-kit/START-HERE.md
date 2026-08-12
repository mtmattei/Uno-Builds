# Start Here

If you want to test the idea today:

1. Read `docs/first-test.md`.
2. Run `python scripts/run_all.py`.
3. Pick one simple real UI design.
4. Hand-author its gold graph before involving the model.
5. Run `prompts/design-understanding.md` against the same design.
6. Validate and score the generated graph.
7. Repeat 5 times to measure stability.
8. Only after manual generation is reliable, test `SKILL.md`.
9. Then run the actual value test:
   - direct Design -> Uno;
   - Design -> Design Graph -> Uno.
10. Productize only if the graph improves downstream results.

The current package is intentionally v0.1. Avoid expanding the ontology until real eval failures justify it.
