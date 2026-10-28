# Frontend UI Guidelines

Use these standards for every screen and shared component in Guadalupe Digital. Keep the interface calm, clear, and efficient for parish administration and family registration.

## Theme and Color

Use the semantic CSS variables in `src/index.css`; do not introduce one-off hex colors in components.

- `background` and `foreground`: warm linen page background and charcoal text.
- `card` and `popover`: clean white elevated surfaces with their paired foreground tokens.
- `primary`: Marian pine green for primary actions, selected navigation, and key links. Pair with `primary-foreground`.
- `secondary`: quiet warm-gold surface for secondary actions and supporting emphasis. Pair with `secondary-foreground`.
- `highlight`: antique gold for sparse status emphasis, seasonal notices, and small decorative details. It is not a default button color.
- `muted`: supporting surfaces; `muted-foreground`: descriptions, metadata, and helper text.
- `accent`: pale green interactive hover/selection surface. Pair with `accent-foreground`.
- `destructive`: reserved for destructive actions and error states.
- `border`, `input`, and `ring`: consistent dividers, form boundaries, and visible keyboard focus.

Use `bg-background`, `text-foreground`, `bg-card`, `text-muted-foreground`, `bg-primary`, and the corresponding foreground utilities in components. Keep text readable against its surface; do not lower contrast to make secondary content appear subtle.

## Typography

- Use Playfair Display (`font-heading`) for page titles and a small number of section headings.
- Use Geist (`font-sans`) for body copy, navigation, forms, data tables, buttons, and dense operational content.
- Keep hierarchy compact and scannable. Do not use display-sized type in admin panels, cards, or tables.
- Use the mono token only for code, identifiers, and technical values that benefit from fixed-width alignment.

## Components and Layout

- Use shadcn/ui components as the baseline for common controls. Add components with `npx shadcn@latest add <component>` and compose them locally; do not duplicate a shared primitive for a single screen.
- Use `--radius: 0.5rem` as the base. Keep controls and surfaces moderately rounded; reserve pill shapes for compact status badges when useful.
- Prefer clear page structure, restrained separators, and compact repeated records over nested cards or decorative containers.
- Make frequent workflows keyboard-friendly. Preserve a visible `:focus-visible` ring and use semantic buttons, links, labels, and form controls.
- Ensure layouts remain usable on narrow screens; tables and dense records should have an intentional responsive treatment rather than overflow by default.

## Iconography

- Use Lucide React (`lucide-react`) for interface icons; do not mix icon libraries or draw substitute SVG icons.
- Keep stroke width consistent, normally `1.75` or `2`, and use icons at a stable size appropriate to their control.
- Give icon-only buttons an accessible name and a tooltip when the action is not universally recognizable.

## Adding Tokens

Add new semantic tokens in `:root` and map them in the `@theme inline` block in `src/index.css`. Prefer a role-based token over a named color; keep the light theme as the source of truth until a dark theme is deliberately designed.