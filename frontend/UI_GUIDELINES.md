# Frontend UI Guidelines

Use these standards for every screen and shared component in MariaDigital. Keep the interface calm, clear, and efficient for parish administration and family registration.

## Theme and Color

Use the semantic CSS variables in `src/index.css`; do not introduce one-off hex colors in components. The semantic tokens are mapped to Tailwind's built-in palette so shades stay consistent across screens.

- `background` and `foreground`: `stone-50` page surface and `stone-900` text.
- `card` and `popover`: `white` surfaces with `stone-900` foregrounds.
- `primary`: `emerald-800` for primary actions, selected navigation, and key links. Pair with white `primary-foreground`.
- `secondary`: `amber-100` surface with `amber-950` text for supporting emphasis.
- `highlight`: `amber-800` for sparse status emphasis and small decorative details. It is not a default button color.
- `muted` and `muted-foreground`: `stone-100` supporting surfaces and `stone-600` descriptions/metadata.
- `accent`: `emerald-50` interactive hover/selection surface, paired with `emerald-900` text.
- `destructive`: `red-700`, reserved for destructive actions and error states.
- `border` and `input`: `stone-200` dividers and `stone-300` form boundaries. `ring` uses `emerald-700` for visible keyboard focus.

Use `bg-background`, `text-foreground`, `bg-card`, `text-muted-foreground`, `bg-primary`, and the corresponding foreground utilities in components. Keep text readable against its surface; do not lower contrast to make secondary content appear subtle.

## Typography

- Use Playfair Display (`font-heading`) for page titles and a small number of section headings.
- Use Geist (`font-sans`) for body copy, navigation, forms, data tables, buttons, and dense operational content.
- Keep hierarchy compact and scannable. Do not use display-sized type in admin panels, cards, or tables.
- Use the mono token only for code, identifiers, and technical values that benefit from fixed-width alignment.

## Components and Layout

- Use the shared components in `src/components/ui` as the baseline. They are shadcn/ui-generated Base UI components, themed through the semantic tokens in `src/index.css`.
- The starter catalog includes buttons, badges, cards, labels, inputs, textareas, checkboxes, switches, selects, dialogs, destructive confirmation dialogs, dropdown menus, popovers, tooltips, tabs, tables, separators, skeletons, inline alerts, avatars, and Sonner notifications.
- Add more components from the frontend directory with `npx shadcn@latest add <component>`. Compose shared primitives locally; avoid one-off replacements for a component that already exists.
- Use `Button` variants consistently: default for the main action, `secondary` or `outline` for supporting actions, `ghost` for low-emphasis controls, and `destructive` only for destructive actions.
- Use `Dialog` for focused forms or details, `AlertDialog` when a user must confirm a consequential action, and `DropdownMenu` for compact related actions.
- Use Sonner for transient success, progress, and error feedback. Use `Alert` for persistent inline context or validation that should remain visible.
- Use `Skeleton` for loading placeholders that preserve the final content shape. Use semantic `Badge` variants for compact statuses, not as the only way to communicate important state.
- Wrap application content in `TooltipProvider`; use tooltips for icon-only controls that are not self-explanatory. Give those controls an accessible name as well.
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