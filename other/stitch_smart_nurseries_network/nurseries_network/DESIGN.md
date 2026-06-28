---
name: Nurseries Network
colors:
  surface: '#f8f9ff'
  surface-dim: '#d8dae1'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f1f3fa'
  surface-container: '#eceef4'
  surface-container-high: '#e6e8ef'
  surface-container-highest: '#e0e2e9'
  on-surface: '#181c21'
  on-surface-variant: '#404750'
  inverse-surface: '#2d3136'
  inverse-on-surface: '#eff0f7'
  outline: '#707882'
  outline-variant: '#c0c7d2'
  surface-tint: '#00639c'
  primary: '#00639c'
  on-primary: '#ffffff'
  primary-container: '#479fe6'
  on-primary-container: '#003455'
  inverse-primary: '#98cbff'
  secondary: '#45617c'
  on-secondary: '#ffffff'
  secondary-container: '#c3e0ff'
  on-secondary-container: '#47637e'
  tertiary: '#7f5600'
  on-tertiary: '#ffffff'
  tertiary-container: '#cb8d0b'
  on-tertiary-container: '#442c00'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#cee5ff'
  primary-fixed-dim: '#98cbff'
  on-primary-fixed: '#001d33'
  on-primary-fixed-variant: '#004a77'
  secondary-fixed: '#cee5ff'
  secondary-fixed-dim: '#adc9e8'
  on-secondary-fixed: '#001d33'
  on-secondary-fixed-variant: '#2d4963'
  tertiary-fixed: '#ffddae'
  tertiary-fixed-dim: '#ffba40'
  on-tertiary-fixed: '#281800'
  on-tertiary-fixed-variant: '#604100'
  background: '#f8f9ff'
  on-background: '#181c21'
  surface-variant: '#e0e2e9'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  display-lg-mobile:
    fontFamily: Inter
    fontSize: 36px
    fontWeight: '700'
    lineHeight: 44px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Inter
    fontSize: 30px
    fontWeight: '600'
    lineHeight: 38px
    letterSpacing: -0.01em
  headline-sm:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 8px
  xs: 4px
  sm: 12px
  md: 24px
  lg: 48px
  xl: 80px
  gutter: 24px
  margin-mobile: 16px
  container-max: 1280px
---

## Brand & Style
The design system for this platform is built on a foundation of **Modern Minimalism** infused with warmth and accessibility, now optimized for a **Light Mode** environment. The brand personality is that of a "Trusted Guide"—highly intelligent and professional, yet deeply empathetic to the needs of parents. 

The visual direction prioritizes clarity and calm. By utilizing generous negative space and a restricted, high-quality palette, the UI reduces cognitive load for busy parents. The style leverages soft tonal layering for depth and high-clarity "Corporate Modern" structures for primary data, creating an interface that feels both technologically advanced (AI-powered) and human-centric.

The emotional response should be one of **reassurance and clarity**. Every interaction should feel smooth, intentional, and high-end, reflecting the premium quality of childcare services curated by the platform in a bright and inviting digital space.

## Colors
This design system uses a primary palette of Sky Blue and Slate to balance authority with calm, now set against a clean, professional light theme.

- **Primary (Sky Blue):** Used for primary actions, branding, and navigation. It represents the AI-driven intelligence and reliability of the platform. In light mode, this color provides a strong, trustworthy focal point.
- **Secondary (Slate):** Used for secondary actions and structural elements. It provides a sophisticated and professional neutral contrast to the primary blue.
- **Tertiary (Ochre):** Reserved for highlights, featured nurseries, and "top-rated" badges to draw attention with a warm, premium feel.
- **Surface & Background:** The background uses a crisp, light palette. The primary background is white, while containers and surfaces use subtle grey tonal variations (`surface-container-low`) to create hierarchy and depth.

## Typography
The system exclusively uses **Inter** to maintain a clean, systematic, and utilitarian feel that remains highly readable across all densities.

- **Headlines:** Use tighter letter spacing and heavier weights (600-700) to create a strong visual anchor.
- **Body Text:** Uses standard weights (400) with a slightly increased line-height (1.5x) to ensure long-form descriptions of nurseries are easy to digest.
- **Labels:** Used for metadata (e.g., distance, pricing, age range). These should use Medium (500) or Semi-bold (600) weights to stand out at smaller sizes.
- **Scaling:** On mobile devices, `display-lg` should downscale to 36px to prevent awkward text wrapping, while body sizes remain constant for accessibility.

## Layout & Spacing
The layout follows a **12-column fluid grid** for desktop and a **4-column grid** for mobile.

- **Rhythm:** All spacing is based on an 8px baseline grid to ensure mathematical harmony.
- **Desktop:** The content is housed in a fixed-width container (1280px) centered on the page. Use 48px or 80px vertical spacing between major sections to emphasize the "clean and minimal" SaaS aesthetic.
- **Mobile:** Margins are reduced to 16px. Cards and interactive elements should span the full width of the 4-column grid minus margins.
- **Padding:** Use "generous whitespace" (md: 24px) inside cards and containers to prevent the UI from feeling cramped or overwhelming.

## Elevation & Depth
In this light-mode orientation, depth is achieved through **Tonal Layering** and **Subtle Shadows** to create a clean, stacked hierarchy.

1.  **Level 0 (Base):** The primary white background.
2.  **Level 1 (Cards):** Use a slightly darker surface-container color or a white surface with a very soft, low-opacity shadow to define boundaries against the base background.
3.  **Level 2 (Hover/Active):** Lift the element by slightly deepening the shadow and adding a 1px border in the primary color at a very low opacity (10%).
4.  **Overlays:** For floating navigation bars or filter overlays, use a backdrop blur (8px) with a semi-transparent white fill (80% opacity) and a 1px light border to create separation.

## Shapes
The shape language is **Rounded and Friendly**. 

- **Primary Elements:** Cards, input fields, and large containers use a consistent **16px (rounded-lg)** corner radius. This softens the professional tone, making it more approachable for parents.
- **Small Elements:** Buttons and tags use an **8px (rounded-md)** radius to maintain a crisp look at smaller scales.
- **Icons:** Use icons with rounded terminals (e.g., Lucide or Feather Icons) to match the UI's geometry. Avoid sharp-angled or "brutalist" iconography.

## Components

- **Buttons:** 
  - *Primary:* Blue background, white text, 8px radius. On hover, darken slightly to indicate interactivity.
  - *Secondary:* Transparent background, Slate text, 1px Slate border.
- **Cards:** Surface-container background or white with soft shadow, 16px radius. Use for nursery listings. Imagery within cards should also have a 12px internal radius.
- **Input Fields:** Light border with low contrast, 16px radius, 16px horizontal padding. On focus, transition border to Primary Blue and add a soft blue outer glow.
- **Chips/Badges:** Use light blue background tints with darker blue text for status indicators. Use Ochre tints for "Premium" or "Featured" badges.
- **AI Insight Component:** A specialized card variant featuring a subtle Primary-to-Tertiary gradient border (2px) and a "sparkle" icon to denote AI-generated content or nursery matching.
- **Micro-interactions:** Buttons should scale down slightly (98%) on click. Transitions between views should use a "fade and slide" motion (200ms ease-out).