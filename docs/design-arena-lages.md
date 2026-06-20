<!-- Gerado a partir do design Figma (figma/) por analise multi-agente. Fonte da verdade do app Arena Lages. -->

# Arena Lages — Especificação Técnica do App Mobile (.NET MAUI + MVVM)

> **Fonte da verdade** para construir o app mobile **Arena Lages** em .NET MAUI, consumindo a API REST existente do **Hub Esportes Lages** (JSON camelCase). Consolida cinco análises: telas/navegação, componentes/dados, design tokens, diretrizes estéticas e referências visuais.
>
> Origem do design: protótipo React/Tailwind em `C:/Users/elson.lopes/source/repos/hubesporteslages/figma/src/app/App.tsx` (1380 linhas, mock sem back-end). Este documento traduz aquele protótipo para arquitetura MAUI nativa, plugando-o na API real.

---

## 1. Visão geral do app e identidade visual

### 1.1 Propósito
**Arena Lages** é a plataforma esportiva mobile de Lages/SC. Reúne, num só lugar: descoberta de eventos esportivos (futsal, vôlei, corrida, basquete, futebol), página de cada evento com placar ao vivo, check-in do torcedor via QR, engajamento de torcida (votação, enquetes, mural), perfil/gamificação e um painel administrativo.

### 1.2 Público e plataformas
- **Torcedor** (uso principal): consome eventos, faz check-in, interage.
- **Administrador**: cadastra eventos e acompanha KPIs (tela em canvas claro, fora do frame mobile).
- **Alvo MAUI**: Android e iOS (phone-first, largura de referência ~390 px). Tablet/desktop não são alvo prioritário.

### 1.3 Identidade visual (destilada do tema ativo + referências Nike/Sentry)
- **Tema dark-only**: o `theme.css` tem `:root ≡ .dark` — não existe variante clara real para o app do torcedor. Adotar **um único tema escuro**.
- **Canvas violeta-meia-noite** (`#1F1633`) + cartões quase-pretos (`#150F23`).
- **Acento único elétrico**: verde-lima `#C2EF4E`, usado como "marca-texto" — raro, semântico, **um acento por viewport**. Nunca decoração de chrome.
- **Pontuação secundária**: rosa `#FA7FAA` (ações destrutivas/favoritar).
- **Tipografia de contraste extremo**: display gigante (Space Grotesk) reservado a heróis/campanha; UI calma em 16 px (Rubik). Quase sem meio-termo.
- **Caps com tracking leve (~0.2 px)** em botões e eyebrows ("cadência de console").
- **Cards planos, sem drop-shadow** decorativo. Profundidade vem de fotografia do esporte, textura (`Starfield`) e mascotes emoji flutuantes (⚡ ️).
- **CTA single-primary** que inverte polaridade conforme o fundo; sempre lê como a ação mais forte.
- **Feedback de toque marcante** (scale/opacity no press) e touch targets ≥ 44×44 px.
- **Grade base 8 px**; ritmo de seção generoso em heróis, denso em superfícies transacionais.

### 1.4 Diretrizes FAÇA / NÃO FAÇA (resumo acionável)
**FAÇA:** um tier de display gigante só no herói; paleta estreita (canvas + tinta + um acento raro); cards planos; cor semântica só para sinal (status, preço); um único vocabulário de forma de botão (escolher **pílula** OU raio moderado e repetir); layouts com `Grid`/`StackLayout` responsivos.
**NÃO FAÇA:** display gigante em títulos de seção/corpo; ampliar a paleta "para dar tom"; amolecer o quase-preto/violeta; drop-shadows de elevação; misturar geometrias de botão; cor semântica como fundo de chrome; animação estilo landing SaaS.

---

## 2. Design tokens → MAUI

Blocos prontos para colar em `Resources/Styles/`. Baseados no **tema ativo** (escuro roxo/lima). MAUI não suporta `oklch` nem `rgba()` em `<Color>`: alfa vai em hex de 8 dígitos `#AARRGGBB`. Conversão de alfa: `rgba(157,193,245,0.5)` → `0.5×255 ≈ 0x80` → `#809DC1F5`.

### 2.1 `Resources/Styles/Colors.xaml`

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- ===========================================================
         ARENA LAGES — Paleta (tema ativo: escuro roxo/lima)
         Origem: src/styles/theme.css
         =========================================================== -->

    <!-- Superfícies -->
    <Color x:Key="Background">#1F1633</Color>          <!-- fundo da página -->
    <Color x:Key="Foreground">#FFFFFF</Color>          <!-- texto principal -->
    <Color x:Key="Card">#150F23</Color>
    <Color x:Key="CardForeground">#FFFFFF</Color>
    <Color x:Key="Popover">#150F23</Color>
    <Color x:Key="PopoverForeground">#FFFFFF</Color>

    <!-- Marca / ações -->
    <Color x:Key="Primary">#150F23</Color>
    <Color x:Key="PrimaryForeground">#FFFFFF</Color>
    <Color x:Key="Secondary">#3F3849</Color>
    <Color x:Key="SecondaryForeground">#FFFFFF</Color>
    <Color x:Key="Accent">#C2EF4E</Color>              <!-- verde-lima de marca -->
    <Color x:Key="AccentForeground">#1F1633</Color>
    <Color x:Key="Destructive">#FA7FAA</Color>          <!-- rosa -->
    <Color x:Key="DestructiveForeground">#1F1633</Color>

    <!-- Atenuados / apoio -->
    <Color x:Key="Muted">#362D59</Color>
    <Color x:Key="MutedForeground">#BDB8C0</Color>

    <!-- Bordas, inputs, foco -->
    <Color x:Key="Border">#362D59</Color>
    <Color x:Key="Input">#3F3849</Color>               <!-- valor de .dark -->
    <Color x:Key="InputBackground">#FFFFFF</Color>
    <Color x:Key="SwitchBackground">#79628C</Color>
    <Color x:Key="Ring">#809DC1F5</Color>              <!-- rgba(157,193,245,0.5) -->

    <!-- Status de evento (mapeados do enum da API) -->
    <Color x:Key="StatusAgendado">#79628C</Color>      <!-- 0 Agendado / "Próximo" -->
    <Color x:Key="StatusAoVivo">#C2EF4E</Color>        <!-- 1 AoVivo -->
    <Color x:Key="StatusEncerrado">#3F3849</Color>     <!-- 2 Encerrado -->
    <Color x:Key="StatusAdiado">#FA7FAA</Color>        <!-- 3 Adiado -->
    <Color x:Key="StatusCancelado">#FA7FAA</Color>     <!-- 4 Cancelado -->

    <!-- Sidebar / Admin -->
    <Color x:Key="Sidebar">#150F23</Color>
    <Color x:Key="SidebarForeground">#FFFFFF</Color>
    <Color x:Key="SidebarPrimary">#C2EF4E</Color>
    <Color x:Key="SidebarPrimaryForeground">#1F1633</Color>
    <Color x:Key="SidebarAccent">#3F3849</Color>
    <Color x:Key="SidebarAccentForeground">#FFFFFF</Color>
    <Color x:Key="SidebarBorder">#362D59</Color>
    <Color x:Key="SidebarRing">#809DC1F5</Color>

    <!-- Charts (admin) -->
    <Color x:Key="Chart1">#C2EF4E</Color>
    <Color x:Key="Chart2">#FA7FAA</Color>
    <Color x:Key="Chart3">#79628C</Color>
    <Color x:Key="Chart4">#6A5FC1</Color>
    <Color x:Key="Chart5">#422082</Color>

    <!-- Brushes -->
    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource Background}" />
    <SolidColorBrush x:Key="CardBrush" Color="{StaticResource Card}" />
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource Accent}" />
    <SolidColorBrush x:Key="BorderBrush" Color="{StaticResource Border}" />

</ResourceDictionary>
```

### 2.2 `Resources/Styles/Styles.xaml`

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- ===========================================================
         TIPOGRAFIA
         Registrar fontes no MauiProgram.cs:
           fonts.AddFont("Rubik-Regular.ttf",       "Rubik");
           fonts.AddFont("Rubik-Medium.ttf",        "RubikMedium");
           fonts.AddFont("SpaceGrotesk-Medium.ttf", "SpaceGrotesk");
           fonts.AddFont("SpaceGrotesk-Bold.ttf",   "SpaceGroteskBold");
         Base 16. Escala: base=16, lg=18, xl=20, 2xl=24. Line-height 1.5.
         (MAUI não tem peso 500 nativo: usar a face "Medium" da fonte.)
         =========================================================== -->

    <x:Double x:Key="FontSizeBase">16</x:Double>
    <x:Double x:Key="FontSizeLg">18</x:Double>
    <x:Double x:Key="FontSizeXl">20</x:Double>
    <x:Double x:Key="FontSize2xl">24</x:Double>
    <x:Double x:Key="FontSizeDisplay">44</x:Double>   <!-- herói/campanha (uso raro) -->

    <Style x:Key="Display" TargetType="Label">
        <Setter Property="FontFamily" Value="SpaceGroteskBold" />
        <Setter Property="FontSize" Value="{StaticResource FontSizeDisplay}" />
        <Setter Property="TextColor" Value="{StaticResource Foreground}" />
        <Setter Property="CharacterSpacing" Value="0.5" />
    </Style>

    <Style x:Key="H1" TargetType="Label">
        <Setter Property="FontFamily" Value="SpaceGrotesk" />
        <Setter Property="FontSize" Value="{StaticResource FontSize2xl}" />
        <Setter Property="TextColor" Value="{StaticResource Foreground}" />
        <Setter Property="LineHeight" Value="1.5" />
    </Style>

    <Style x:Key="H2" TargetType="Label">
        <Setter Property="FontFamily" Value="SpaceGrotesk" />
        <Setter Property="FontSize" Value="{StaticResource FontSizeXl}" />
        <Setter Property="TextColor" Value="{StaticResource Foreground}" />
        <Setter Property="LineHeight" Value="1.5" />
    </Style>

    <Style x:Key="H3" TargetType="Label">
        <Setter Property="FontFamily" Value="SpaceGrotesk" />
        <Setter Property="FontSize" Value="{StaticResource FontSizeLg}" />
        <Setter Property="TextColor" Value="{StaticResource Foreground}" />
        <Setter Property="LineHeight" Value="1.5" />
    </Style>

    <Style x:Key="H4" TargetType="Label">
        <Setter Property="FontFamily" Value="RubikMedium" />
        <Setter Property="FontSize" Value="{StaticResource FontSizeBase}" />
        <Setter Property="TextColor" Value="{StaticResource Foreground}" />
        <Setter Property="LineHeight" Value="1.5" />
    </Style>

    <Style x:Key="Body" TargetType="Label">
        <Setter Property="FontFamily" Value="Rubik" />
        <Setter Property="FontSize" Value="{StaticResource FontSizeBase}" />
        <Setter Property="TextColor" Value="{StaticResource Foreground}" />
        <Setter Property="LineHeight" Value="1.5" />
    </Style>

    <!-- Eyebrow: caps + tracking (cadência de marca) -->
    <Style x:Key="Eyebrow" TargetType="Label">
        <Setter Property="FontFamily" Value="RubikMedium" />
        <Setter Property="FontSize" Value="12" />
        <Setter Property="TextColor" Value="{StaticResource MutedForeground}" />
        <Setter Property="CharacterSpacing" Value="2" />
    </Style>

    <!-- ===========================================================
         BORDER-RADIUS (base 8: sm=4, md=6, lg=8, xl=12, full=999)
         =========================================================== -->
    <x:Double x:Key="RadiusSm">4</x:Double>
    <x:Double x:Key="RadiusMd">6</x:Double>
    <x:Double x:Key="RadiusLg">8</x:Double>
    <x:Double x:Key="RadiusXl">12</x:Double>
    <x:Double x:Key="RadiusPill">999</x:Double>

    <!-- ===========================================================
         BOTÕES (vocabulário único: pílula). Press-feedback via VSM.
         =========================================================== -->
    <Style x:Key="ButtonAccent" TargetType="Button">     <!-- CTA primário (lima) -->
        <Setter Property="BackgroundColor" Value="{StaticResource Accent}" />
        <Setter Property="TextColor" Value="{StaticResource AccentForeground}" />
        <Setter Property="FontFamily" Value="RubikMedium" />
        <Setter Property="FontSize" Value="{StaticResource FontSizeBase}" />
        <Setter Property="CharacterSpacing" Value="0.2" />
        <Setter Property="CornerRadius" Value="24" />
        <Setter Property="Padding" Value="20,12" />
        <Setter Property="MinimumHeightRequest" Value="44" />
        <Setter Property="VisualStateManager.VisualStateGroups">
            <VisualStateGroupList>
                <VisualStateGroup x:Name="CommonStates">
                    <VisualState x:Name="Normal">
                        <VisualState.Setters><Setter Property="Scale" Value="1" /></VisualState.Setters>
                    </VisualState>
                    <VisualState x:Name="Pressed">
                        <VisualState.Setters>
                            <Setter Property="Scale" Value="0.97" />
                            <Setter Property="Opacity" Value="0.85" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateGroupList>
        </Setter>
    </Style>

    <!-- Inverso: claro sobre fundo escuro -->
    <Style x:Key="ButtonInverted" TargetType="Button">
        <Setter Property="BackgroundColor" Value="{StaticResource Foreground}" />
        <Setter Property="TextColor" Value="{StaticResource Background}" />
        <Setter Property="FontFamily" Value="RubikMedium" />
        <Setter Property="CharacterSpacing" Value="0.2" />
        <Setter Property="CornerRadius" Value="24" />
        <Setter Property="Padding" Value="20,12" />
        <Setter Property="MinimumHeightRequest" Value="44" />
    </Style>

    <!-- Ghost: contorno -->
    <Style x:Key="ButtonGhost" TargetType="Button">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{StaticResource Foreground}" />
        <Setter Property="BorderColor" Value="{StaticResource Border}" />
        <Setter Property="BorderWidth" Value="1" />
        <Setter Property="FontFamily" Value="RubikMedium" />
        <Setter Property="CharacterSpacing" Value="0.2" />
        <Setter Property="CornerRadius" Value="24" />
        <Setter Property="Padding" Value="20,12" />
        <Setter Property="MinimumHeightRequest" Value="44" />
    </Style>

    <!-- Primário admin (canvas claro) -->
    <Style x:Key="ButtonPrimary" TargetType="Button">
        <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
        <Setter Property="TextColor" Value="{StaticResource PrimaryForeground}" />
        <Setter Property="FontFamily" Value="RubikMedium" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="Padding" Value="16,10" />
    </Style>

    <!-- ===========================================================
         CARD / SUPERFÍCIE (plano, sem sombra)
         =========================================================== -->
    <Style x:Key="Card" TargetType="Border">
        <Setter Property="BackgroundColor" Value="{StaticResource Card}" />
        <Setter Property="Stroke" Value="{StaticResource Border}" />
        <Setter Property="StrokeThickness" Value="1" />
        <Setter Property="Padding" Value="16" />
        <Setter Property="StrokeShape">
            <RoundRectangle CornerRadius="12" />
        </Setter>
    </Style>

    <!-- ===========================================================
         CHIPS (filtro de modalidade — vocabulário fechado)
         =========================================================== -->
    <Style x:Key="ChipInactive" TargetType="Border">
        <Setter Property="BackgroundColor" Value="{StaticResource Muted}" />
        <Setter Property="Stroke" Value="{StaticResource Border}" />
        <Setter Property="StrokeThickness" Value="1" />
        <Setter Property="Padding" Value="14,8" />
        <Setter Property="StrokeShape"><RoundRectangle CornerRadius="999" /></Setter>
    </Style>
    <Style x:Key="ChipActive" TargetType="Border">
        <Setter Property="BackgroundColor" Value="{StaticResource Foreground}" />
        <Setter Property="StrokeThickness" Value="0" />
        <Setter Property="Padding" Value="14,8" />
        <Setter Property="StrokeShape"><RoundRectangle CornerRadius="999" /></Setter>
    </Style>

    <!-- ===========================================================
         ENTRY / INPUT (fundo branco, texto escuro)
         =========================================================== -->
    <Style x:Key="Input" TargetType="Entry">
        <Setter Property="BackgroundColor" Value="{StaticResource InputBackground}" />
        <Setter Property="TextColor" Value="#1F1633" />
        <Setter Property="PlaceholderColor" Value="{StaticResource MutedForeground}" />
        <Setter Property="FontFamily" Value="Rubik" />
        <Setter Property="FontSize" Value="{StaticResource FontSizeBase}" />
    </Style>

    <Style x:Key="Switch" TargetType="Switch">
        <Setter Property="OnColor" Value="{StaticResource Accent}" />
        <Setter Property="ThumbColor" Value="{StaticResource Foreground}" />
    </Style>

    <!-- ProgressBar (barra de presença) -->
    <Style x:Key="Progress" TargetType="ProgressBar">
        <Setter Property="ProgressColor" Value="{StaticResource Accent}" />
        <Setter Property="BackgroundColor" Value="{StaticResource Muted}" />
    </Style>

    <!-- ===========================================================
         PÁGINA (dark-only para telas do torcedor)
         =========================================================== -->
    <Style TargetType="ContentPage" ApplyToDerivedTypes="True">
        <Setter Property="BackgroundColor" Value="{StaticResource Background}" />
    </Style>

</ResourceDictionary>
```

### 2.3 Registro em `App.xaml`

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
            <ResourceDictionary Source="Resources/Styles/Styles.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 2.4 Notas de conversão
- **Dark-only**: `:root ≡ .dark` no `theme.css`. Não usar `AppThemeBinding` no app do torcedor. A tela **Admin** usa canvas claro próprio (definir cores claras locais se necessário).
- **`--input`**: difere `transparent` (`:root`) vs `#3F3849` (`.dark`). Adotado `#3F3849`. Fundo interno do campo é branco (`#FFFFFF`), por isso `Entry` tem `TextColor` escuro.
- **Pesos 500/600/700**: registrar a face específica da fonte (ex.: `Rubik-Medium.ttf` como família `RubikMedium`) em vez de `FontAttributes="Bold"`.
- **`LineHeight="1.5"`** funciona em `Label`; em `Button` não há equivalente (omitido).
- **Sem tokens de espaçamento/sombra** no design: herdar escala 8 px do Tailwind. `Padding` 16/12 por convenção.

---

## 3. Navegação global (AppShell)

No protótipo a navegação é um `useState<Screen>` (state-machine, sem router). Em MAUI use **Shell** com TabBar + rotas registradas.

### 3.1 Estrutura
- **Splash**: tela de abertura nativa (MAUI Splash) + uma `SplashPage` opcional para a animação de branding. Após ~2,8 s (ou toque em "Entrar na Arena"), navega para a `TabBar`.
- **TabBar** (5 abas, visível só nas telas principais): Início, Eventos, **Check-in (FAB central elevado)**, Times, Perfil.
- **Rotas fora da TabBar** (push, escondem a tab bar): `EventDetailPage`, `CheckInPage`, `InteractionPage`, `AdminPage` (+ modal `NewEventPopup`).

### 3.2 `AppShell.xaml` (esqueleto)

```xml
<Shell ...>
    <!-- Splash como rota inicial isolada -->
    <ShellContent Route="splash" ContentTemplate="{DataTemplate pages:SplashPage}"
                  Shell.FlyoutBehavior="Disabled" Shell.NavBarIsVisible="False" />

    <TabBar Route="main">
        <Tab Title="Início" Icon="tab_home.png">
            <ShellContent Route="home" ContentTemplate="{DataTemplate pages:HomePage}" />
        </Tab>
        <Tab Title="Eventos" Icon="tab_calendar.png">
            <ShellContent Route="events" ContentTemplate="{DataTemplate pages:EventsPage}" />
        </Tab>
        <Tab Title="Check-in" Icon="tab_qr.png">   <!-- central, estilizada como FAB -->
            <ShellContent Route="checkin" ContentTemplate="{DataTemplate pages:CheckInPage}" />
        </Tab>
        <Tab Title="Times" Icon="tab_users.png">    <!-- label "Times"; tela intitulada "TORCIDA" -->
            <ShellContent Route="interaction" ContentTemplate="{DataTemplate pages:InteractionPage}" />
        </Tab>
        <Tab Title="Perfil" Icon="tab_user.png">
            <ShellContent Route="profile" ContentTemplate="{DataTemplate pages:ProfilePage}" />
        </Tab>
    </TabBar>
</Shell>
```

Rotas push (registrar em `AppShell` code-behind via `Routing.RegisterRoute`):

```csharp
Routing.RegisterRoute("detail", typeof(EventDetailPage));
Routing.RegisterRoute("admin",  typeof(AdminPage));
// checkin e interaction também são alcançáveis por push a partir de detail,
// passando o slug do evento como parâmetro de query.
```

### 3.3 Mapa de transições

| De | Para | Gatilho |
|---|---|---|
| splash | home (main) | timeout 2,8 s ou botão "Entrar na Arena" |
| home | detail | toque no card ao vivo ou em card de "Próximos" |
| home | events | "Ver eventos" / "Ver todos" |
| home | checkin | botão "Check-in" do herói |
| home | admin | ícone gráfico (`BarChart2`) no topo |
| events | detail | toque em qualquer card |
| detail | checkin | "Fazer Check-in" (oculto se status=2 Encerrado) |
| detail | interaction | "Interagir" |
| detail | (voltar) | `GoToAsync("..")` |
| checkin | interaction | "Interagir com a Torcida" (após confirmar) |
| checkin | (voltar) | volta para detail |
| interaction | (voltar) | volta para detail |
| profile | admin | "Painel Administrativo" |
| admin | home | seta voltar; abre/fecha `NewEventPopup` |

### 3.4 Regras de navegação
- **Tab bar visível** apenas em home, events, checkin, interaction, profile. **Escondida** em splash, detail, admin (`Shell.TabBarIsVisible="False"` nas páginas push).
- **Aba central (Check-in)** estilizada como FAB elevado/circular; ativo pinta label de lima.
- **Parâmetro de evento**: detail/checkin/interaction recebem o `slug` (ou `id`) via `[QueryProperty]`. Diferente do protótipo (que usava índice fixo `EVENTS[1]` como default), em MAUI **sempre passe o slug** — não há "evento default".
- **Admin** é full-screen em **canvas claro** (sobrescrever `BackgroundColor` da página), sem tab bar.

---

## 4. Inventário de telas

Convenções de dados aplicadas a todas as telas:
- `status` (enum int da API) → `StatusBadge`: `0 Agendado`→"Próximo", `1 AoVivo`→"Ao Vivo", `2 Encerrado`→"Encerrado", `3 Adiado`→"Adiado", `4 Cancelado`→"Cancelado". **O protótipo só cobre 3 estados (upcoming/live/ended); 3 e 4 devem ser adicionados no badge.**
- `inicio` (ISO 8601) → formatar `dd MMM` + `HH'h'mm` (ex.: "22 Jun · 19h30") com cultura `pt-BR`.
- `imagemUrl` (URL pronta da API) substitui a função `imgUrl()` que montava URL Unsplash.
- `placar` (string da API) → no detalhe usar `placarCasa`/`placarVisitante` (ints).

---

### 4.1 SplashPage (`splash`)
- **Propósito**: abertura/branding; entra automaticamente.
- **Seções (cima→baixo)**: fundo `Starfield` escuro com mascotes flutuantes (topo-dir, ⚡ inf-esq); eyebrow "Plataforma Esportiva · Lages/SC"; display "ARENA **LAGES**" (LAGES em chip lima); subtítulo; botão inverso "Entrar na Arena →"; barra de loading animada (2,7 s).
- **Componentes**: `Grid` + `GraphicsView`/imagem de textura; `Label` Display; `Button` ButtonInverted; `ProgressBar`/animação.
- **Dados/endpoint**: nenhum (100% estático/marketing).
- **Entrada/saída**: entrada = launch do app; saída = `home` (timer ou botão).

### 4.2 HomePage (`home`)
- **Propósito**: hub inicial; vitrine do evento ao vivo + próximos + atalhos.
- **Seções**:
  1. Top bar: título "ARENA LAGES"; botões redondos lupa (busca) e gráfico (→ admin).
  2. Herói de campanha (`Starfield`, mascote ️): eyebrow "Temporada 2025", display "VIVA O **ESPORTE** DE LAGES."; CTAs "Ver eventos" (→ events) e "Check-in" (→ checkin).
  3. "Acontecendo agora" (ponto lima pulsante): card grande do evento ao vivo — imagem, badge "Ao Vivo", placar grande, nomes dos times, local. Toque → detail.
  4. "Próximos Eventos" (link "Ver todos" → events): até 3 cards compactos. Toque → detail.
  5. Faixa de estatísticas (3 colunas): Eventos / Check-ins / Torcedores. **Lacuna (ver §6).**
- **Componentes**: `ScrollView` raiz; `EventCardLive`; `CollectionView` (próximos) com `EventCardCompact`; `StatsBand`.
- **Dados/endpoint**:
  - Evento ao vivo: `GET /api/eventos/destaques` ou `GET /api/eventos?Periodo=Hoje` filtrando `status==1` → `EventoResumoDto` (`titulo`, `placar`, `equipeCasa`/`equipeVisitante`, `localNome`, `imagemUrl`).
  - Próximos: `GET /api/eventos?Periodo=Proximos&TamanhoPagina=3`.
  - Stats band: **sem fonte** → propor `GET /api/estatisticas`.
- **Entrada/saída**: entrada = tab/splash; saída = detail, events, checkin, admin.

### 4.3 EventsPage (`events`)
- **Propósito**: catálogo completo com filtro por modalidade.
- **Seções**: header (eyebrow + display "EVENTOS"); barra de filtros horizontal rolável (chips de modalidade); mascote contextual que muda com o filtro; lista de cards completos (imagem+gradiente, `StatusBadge`, placar se ao vivo, título, modalidade, data·hora, local, confronto, rodapé "X/Y confirmados" + "Ver detalhes →").
- **Componentes**: `CollectionView` horizontal (chips `VioletChip`); `CollectionView` vertical (`EventCardFull`).
- **Dados/endpoint**:
  - Lista: `GET /api/eventos` (paginado; `Modalidade=slug`, `Busca`, `Periodo`, `ApenasGratuitos`, `Pagina`, `TamanhoPagina`).
  - Filtros: `GET /api/catalogo/modalidades` (`nome`, `slug`, `icone`, `corHex`) — substituir array hardcoded `SPORTS`.
  - "X/Y confirmados": **sem fonte no resumo** (`EventoResumoDto` não traz check-ins; `capacidade` só no detalhe). **Lacuna crítica (§6).**
- **Entrada/saída**: entrada = tab; saída = detail.

### 4.4 EventDetailPage (`detail`) — sem tab bar
- **Propósito**: página do evento selecionado.
- **Seções**: herói (imagem ~280 px + gradiente, botão voltar, `StatusBadge`, título display); grid 2 col (Modalidade / Data e Hora); card Local; card Equipes (escudos + placar grande); card "Presença confirmada" (`checkins/capacidade` + `ProgressBar`); card Patrocinadores; ações no rodapé (CTA "Fazer Check-in" oculto se Encerrado; "Ver Rota" + "Interagir").
- **Componentes**: `ScrollView`; `Grid`; `Card` (Border); `ProgressBar`; botões.
- **Dados/endpoint**: `GET /api/eventos/{slug}` → `EventoDetalheDto` (`titulo`, `descricao`, `modalidadeNome`, `inicio`/`fim`, `localNome`/`localEndereco`/`capacidade`/`mapaUrl`, `equipeCasa`/`equipeVisitante` + `equipeCasaEscudo`/`equipeVisitanteEscudo`, `placarCasa`/`placarVisitante`, `imagemUrl`, `status`).
  - **"Ver Rota"** → abrir `mapaUrl` (`Launcher.OpenAsync`). No protótipo era inerte.
  - **Presença confirmada (`checkins`)**: `capacidade` existe; **contagem de check-ins não existe** → §6.
  - **Patrocinadores**: **sem fonte** → §6.
  - `descricao` existe no DTO mas não era exibida — **exibir** num card "Sobre".
- **Entrada/saída**: entrada = home/events; saída = checkin, interaction, voltar; "Ver Rota" abre mapa externo.

### 4.5 CheckInPage (`checkin`)
- **Propósito**: confirmar presença via QR/manual; dois estados.
- **Seções — Estado A (pendente)**: pill do evento (emoji, título, data·hora·local); bloco QR ("Apresente na entrada"), `QRCodeView`, código monospace "TOR-2025-{id}-A4B7"; divisor "ou confirme manualmente"; botão "Confirmar Presença".
- **Seções — Estado B (sucesso)**: mascote , "CHECK-IN REALIZADO!"; dois cards de pontos ("+50 PTS" / "Total 400 PTS"); CTA "Interagir com a Torcida"; link "Voltar ao evento".
- **Componentes**: `Border` pill; `QRCodeView` (gerar QR real a partir de token da API); botões; alternância de estado via propriedade `IsDone` no ViewModel.
- **Dados/endpoint**:
  - Dados do evento: do `slug` recebido.
  - QR + código + confirmação: **sem endpoint no contrato** (`POST /api/inscricoes` é inscrição de atleta/equipe, não check-in de torcedor). **Lacuna crítica (§6)** — propor `POST /api/eventos/{slug}/checkin` retornando token/QR.
  - Pontos (+50 / total): **sem fonte** (gamificação) → §6.
- **Entrada/saída**: entrada = detail (push com slug); saída = interaction, voltar para detail.

### 4.6 InteractionPage (`interaction`) — título de UI "TORCIDA"
- **Propósito**: engajamento da torcida.
- **Seções**: header (voltar + "TORCIDA" + título do evento); card Favoritar Equipe (toggle coração, rosa quando ativo); card Jogador da Partida (lista selecionável → "Votar em {nome}" → "✓ Voto registrado"); card Enquete Rápida (3 opções → barras de %); card Mensagem de Torcida (input + enviar; lista rolável com avatares).
- **Componentes**: `CollectionView` (jogadores, mensagens); `Switch`/ícone toggle; barras de progresso; `Entry` + `Button` send.
- **Dados/endpoint**: **quase tudo sem fonte no contrato** (§6):
  - Favoritar equipe: `POST/DELETE /api/favoritos/equipes/{id}` (catálogo base: `GET /api/catalogo/equipes`).
  - Jogador da Partida: escalação (`GET /api/eventos/{slug}/jogadores`) + voto (`POST /api/eventos/{slug}/mvp`).
  - Enquete: `GET /api/eventos/{slug}/enquetes` + `POST .../voto`.
  - Mural: `GET`/`POST /api/eventos/{slug}/mensagens` (idealmente realtime).
- **Entrada/saída**: entrada = detail/checkin/tab; saída = voltar para detail.

### 4.7 ProfilePage (`profile`)
- **Propósito**: perfil do torcedor, pontos, favoritos, histórico.
- **Seções**: título display "PERFIL"; card destaque violeta (mascote ⚡, avatar, nome "@handle", "400 PTS"; métricas Eventos/Check-ins/Votos); "Equipes Favoritas" (chips com coração); "Histórico de Check-ins" (cards com thumbnail, título, data·local, check ✓); link "Painel Administrativo" (→ admin).
- **Componentes**: `Card`; `CollectionView` (favoritos, histórico).
- **Dados/endpoint**: **sem fonte no contrato** (§6):
  - Perfil/pontos/stats: `GET /api/usuarios/me` (não há DTO de usuário no contrato).
  - Favoritos: `GET /api/favoritos/equipes`.
  - Histórico: `GET /api/usuarios/me/checkins` (mapearia para `titulo`/`inicio`/`localNome`/`imagemUrl`).
- **Entrada/saída**: entrada = tab; saída = admin.

### 4.8 AdminPage (`admin`) — canvas claro, full-screen, sem tab bar
- **Propósito**: painel administrativo (KPIs, gráficos, tabela, cadastro).
- **Seções**: nav-bar clara (voltar + "ARENA LAGES — ADMIN" + botão "Cadastrar Evento"); grid de 6 KPIs (Eventos, Check-ins, Torcedores, Taxa Interação, Engajamento, Evento Top); gráficos ("Check-ins por Modalidade" em barras; "Distribuição por modalidade" em %); "Tabela de Eventos" (Evento, Modalidade, Data·Hora, Local, Público, Status, Ações Ver/Editar).
- **Sub-overlay — `NewEventPopup`**: formulário (Nome, Local, Modalidade [select], Data, Horário, Capacidade, Equipe 1/2) + Cancelar/Cadastrar. Usar **`CommunityToolkit.Maui.Popup`**.
- **Componentes**: `Grid` de KPI cards; biblioteca de gráficos (**Microcharts** ou `Syncfusion`); `CollectionView` como tabela; `Popup` para o modal.
- **Dados/endpoint**:
  - Tabela de eventos: `GET /api/eventos` (admin, paginado).
  - Dropdowns do modal: `GET /api/catalogo/modalidades`, `/locais`, `/equipes`.
  - KPIs e série "check-ins por modalidade": **sem endpoints** → §6 (`ModalidadeDto.eventosFuturos` é contagem de eventos, **não** de check-ins).
  - Criar evento (`POST /api/eventos`) e editar (`PUT /api/eventos/{id}`): **não constam no contrato** → §6.
  - "Público {checkins}/{capacity}": **`checkins` sem fonte** → §6.
- **Entrada/saída**: entrada = home/profile; saída = home; abre/fecha popup.

---

## 5. Inventário de componentes reutilizáveis → MAUI

### 5.1 Primitivos de UI (sem dados de domínio)

| Componente (protótipo) | Papel | Controle MAUI equivalente |
|---|---|---|
| `Starfield` | textura de fundo | `GraphicsView` (desenho) ou `Image` PNG em `Grid` de fundo |
| `LimeChip` | realce inline lima | `Span` com `BackgroundColor` (em `FormattedString`) ou `Border` lima |
| `BtnInverted` / `BtnPrimary` / `BtnGhost` / `BtnAccent` | CTAs | `Button` + Styles `ButtonInverted`/`ButtonPrimary`/`ButtonGhost`/`ButtonAccent` |
| `VioletChip` | chip de filtro | `Border` (StrokeShape pill) + `TapGestureRecognizer`; estados `ChipActive`/`ChipInactive` |
| `StatusBadge` | badge de status | `Border` + `Label`; cor via `IValueConverter` (enum→cor) |
| `QRCodeVisual` | QR | **substituir por QR real**: `ZXing.Net.Maui` `BarcodeGeneratorView` a partir de token da API |
| `BottomNav` | navegação | Shell `TabBar` (não recriar manualmente) |
| `Heart`/`Award`/`Activity`/ícones | ícones | fonte de ícones (FontImageSource) ou SVG via `Image` |

### 5.2 Componentes de domínio (com dados)

| Componente | Conteúdo | Composição MAUI |
|---|---|---|
| **EventCardLive** (home, destaque) | imagem, badge "Ao Vivo", placar grande, times, local | `Border` (Card) + `Grid` (imagem de fundo + overlay), `TapGestureRecognizer` → detail |
| **EventCardCompact** (home, próximos) | thumb, título, modalidade, data·hora, chevron | `Border` + `Grid` 2 col |
| **EventCardFull** (events) | imagem+gradiente, status, placar, título, modalidade, data·hora, local, confronto, rodapé confirmados | `Border` + `Grid` multi-linha |
| **EventRow** (admin tabela) | thumb+nome, modalidade, data·hora, local, público, status, ações | `Grid` de colunas dentro de `CollectionView` |
| **StatsBand** | 3 métricas | `Grid` 3 col de `Label`s |
| **TeamCrest** | escudo + nome | `Image` (`equipeCasaEscudo`) + `Label`; fallback emoji da modalidade |
| **PresenceBar** | "X/Y" + barra | `Label` + `ProgressBar` (Style `Progress`) |
| **PollOption** | opção + % | `Border` + `Label` + barra |
| **MessageItem** | avatar + texto | `Grid` 2 col em `CollectionView` |
| **NotificationItem** (nova, ver §6) | ícone+cor da modalidade, título, mensagem, importante, lida | `Border` + `Grid`; consome `NotificacaoDto` |

### 5.3 Listas
- Toda lista vertical → **`CollectionView`** (`DataTemplate` + `ItemsSource` no ViewModel).
- Filtros horizontais → **`CollectionView`** com `ItemsLayout` horizontal.
- Tabela admin → `CollectionView` com `DataTemplate` em grid de colunas (não há `DataGrid` nativo; usar Syncfusion `SfDataGrid` se quiser ordenação/edição).

### 5.4 Conversores (em `Converters/`)
- `StatusToTextConverter`, `StatusToColorConverter` (enum→"Ao Vivo"/cor).
- `IsoDateToDisplayConverter` (`inicio`→"22 Jun · 19h30").
- `PlacarToTupleConverter` (string→casa/visitante) — ou usar `placarCasa`/`placarVisitante` do detalhe.
- `BoolToVisibilityConverter` (ocultar "Fazer Check-in" se Encerrado).
- `NotificationTypeToIconConverter`.

---

## 6. Mapeamento tela ↔ endpoint e LACUNAS da API

### 6.1 Matriz tela → endpoint (o que JÁ existe)

| Tela | Endpoint(s) que alimentam | DTO |
|---|---|---|
| Splash | — (estático) | — |
| Home (ao vivo) | `GET /api/eventos/destaques` ou `?Periodo=Hoje` | `EventoResumoDto` |
| Home (próximos) | `GET /api/eventos?Periodo=Proximos&TamanhoPagina=3` | `EventoResumoDto` |
| Eventos (lista) | `GET /api/eventos` (filtros) | `EventoResumoDto` paginado |
| Eventos (filtros) | `GET /api/catalogo/modalidades` | `ModalidadeDto` |
| Detalhe | `GET /api/eventos/{slug}` | `EventoDetalheDto` |
| Detalhe (rota) | `mapaUrl` (campo do DTO) | — |
| Perfil (histórico — base) | mapeável de `GET /api/eventos/resultados` | `EventoResumoDto` |
| Admin (tabela) | `GET /api/eventos` | `EventoResumoDto` |
| Admin (dropdowns modal) | `GET /api/catalogo/modalidades` / `/locais` / `/equipes` | `ModalidadeDto` / locais / equipes |
| (sem UI no design) | `GET /api/notificacoes?quantidade=N` | `NotificacaoDto` |
| (sem UI no design) | `POST /api/inscricoes` | `CriarInscricaoDto` |

### 6.2 LACUNAS — o design pede, a API NÃO entrega (ordenadas por impacto)

| # | Necessidade do design | Telas afetadas | Situação na API | Proposta de endpoint |
|---|---|---|---|---|
| 1 | **Contagem de check-ins / "presença confirmada"** | Eventos, Detalhe, Admin (tabela + KPI + gráfico) | `EventoResumoDto` não tem; `EventoDetalheDto` só tem `capacidade` | adicionar `checkinsCount` aos DTOs, ou `GET /api/eventos/{slug}/checkins/contagem` |
| 2 | **Check-in do torcedor (QR/token + confirmar)** | Check-in | só existe `POST /api/inscricoes` (inscrição de atleta) | `POST /api/eventos/{slug}/checkin` → `{ ticketCodigo, qrToken, pontosGanhos }` |
| 3 | **Usuário/Perfil autenticado** | Perfil | sem DTO de usuário | `GET /api/usuarios/me` → `{ nome, handle, avatarUrl, pontos, stats }` |
| 4 | **Gamificação (pontos)** | Check-in (+50), Perfil (400) | sem fonte | incluir em `/usuarios/me` + retorno do check-in |
| 5 | **Favoritos de equipe** | Interação (toggle), Perfil (lista) | só catálogo de equipes | `GET /api/favoritos/equipes`, `POST/DELETE /api/favoritos/equipes/{id}` |
| 6 | **Interação ao vivo**: MVP/jogadores, enquetes, mural de mensagens | Interação | inexistente | `GET /api/eventos/{slug}/jogadores`, `POST .../mvp`, `GET .../enquetes` + `POST .../voto`, `GET`/`POST .../mensagens` (realtime: SignalR) |
| 7 | **Patrocinadores do evento** | Detalhe | ausente no `EventoDetalheDto` | adicionar `patrocinadores[]` ao DTO ou `GET /api/eventos/{slug}/patrocinadores` |
| 8 | **Estatísticas agregadas públicas** | Home (band) | sem fonte | `GET /api/estatisticas` → `{ eventos, checkins, torcedores }` |
| 9 | **Dashboard admin (KPIs + check-ins por modalidade)** | Admin | sem endpoints (`eventosFuturos` ≠ check-ins) | `GET /api/admin/dashboard`, `GET /api/admin/checkins-por-modalidade` |
| 10 | **CRUD admin de eventos** | Admin (modal/editar) | `POST`/`PUT /api/eventos` não constam | `POST /api/eventos`, `PUT /api/eventos/{id}` |

### 6.3 Lacuna inversa — API entrega, design NÃO usa (oportunidades)
- **`GET /api/notificacoes`** (`NotificacaoDto` completo): **não há tela de notificações** no design, apesar do ícone de sino na referência `image-1.png`. **Recomendação**: criar uma `NotificationsPage` (push a partir de um sino na top bar da Home) consumindo este endpoint. Tipos: `0 NovoEvento, 1 Lembrete, 2 AlteracaoHorario, 3 Resultado, 4 Cancelamento`; `importante`/`lida` controlam destaque; `eventoSlug` permite deep-link para o detalhe.
- **`POST /api/inscricoes`** (`CriarInscricaoDto`): não há tela de inscrição de atleta/equipe. **Recomendação**: criar uma `InscricaoPage` (formulário: nome, email, telefone, modalidadeId, equipeId, receberEmail, receberPush). Não confundir com o modal admin "Cadastrar Evento".
- **Campos não usados** que a UI deveria aproveitar: `slug`, `campeonato`, `bairro`, `gratuito`, `precoIngresso`, `destaque`, `modalidadeIcone`, `modalidadeCor` (usar cor/ícone reais em vez de emoji hardcoded), `equipeCasaEscudo`/`equipeVisitanteEscudo` (escudos reais), e filtros `LocalId`, `EquipeId`, `ApenasGratuitos`, `Periodo` (Hoje/Semana/Mês).

### 6.4 Notas de impedância (transformações no cliente)
- `status`: enum int → texto/cor; **adicionar Adiado/Cancelado** ao badge (protótipo só tem 3 estados).
- `inicio` ISO → `dd MMM · HH'h'mm` (pt-BR).
- `placar` string → casa/visitante; preferir `placarCasa`/`placarVisitante` no detalhe.
- `imagemUrl` pronta → substitui `imgUrl()`/Unsplash.

---

## 7. Plano de implementação incremental

### Fase 0 — Infra e fundação (sem UI visível)
1. **Projeto**: criar solução MAUI (não há `.csproj` ainda). Estrutura sugerida: `ArenaLages.sln` → `ArenaLages` (app) com pastas `Views/`, `ViewModels/`, `Models/` (DTOs), `Services/` (ApiClient), `Converters/`, `Resources/Styles/`, `Resources/Fonts/`.
2. **Pacotes**: `CommunityToolkit.Maui`, `CommunityToolkit.Mvvm` (source generators `[ObservableProperty]`/`[RelayCommand]`), `Microsoft.Extensions.Http` (Refit ou `HttpClientFactory`), `ZXing.Net.Maui` (QR), `Microcharts.Maui` ou `Syncfusion` (gráficos admin).
3. **Design tokens**: colar `Colors.xaml` e `Styles.xaml` (§2) em `Resources/Styles/`; registrar fontes Rubik/Space Grotesk no `MauiProgram.cs`; mergear dicionários em `App.xaml`.
4. **DTOs** (`Models/`): `EventoResumoDto`, `EventoDetalheDto`, `ModalidadeDto`, `LocalDto`, `EquipeDto`, `NotificacaoDto`, `CriarInscricaoDto`, e enum `EventoStatus`. JSON camelCase → `JsonSerializerOptions { PropertyNamingPolicy = CamelCase }`.
5. **ApiClient** (`Services/IArenaApiClient`): interface tipada (Refit recomendado) cobrindo os endpoints existentes do §6.1. Configurar `BaseAddress`, timeout, tratamento de erro/offline, paginação.
6. **DI**: registrar `HttpClient`/ApiClient, serviços, ViewModels e Pages no `MauiProgram.cs` (`builder.Services.AddSingleton/AddTransient`). Usar Shell + injeção de dependência em construtores de página.
7. **AppShell** (§3): TabBar + rotas; `MVVM` base (`ObservableObject`, `BaseViewModel` com `IsBusy`).
8. **Conversores** (§5.4).

### Fase 1 — Caminho feliz de leitura (telas que a API JÁ alimenta)
9. **SplashPage** (estático; valida fontes/tokens e navegação inicial).
10. **EventsPage** — `GET /api/eventos` + `GET /api/catalogo/modalidades`. Valida `CollectionView`, paginação, filtro por slug, conversores de data/status. (Maior retorno: exercita o núcleo de dados.)
11. **EventDetailPage** — `GET /api/eventos/{slug}`. Valida deep-link por query, escudos reais, `mapaUrl` (Launcher), card "Sobre" (`descricao`).
12. **HomePage** — `destaques` + `?Periodo=Proximos`. Reaproveita os cards da Fase 1. (Stats band com placeholder até a lacuna #8.)

### Fase 2 — Telas que dependem de NOVOS endpoints (coordenar com backend)
13. **NotificationsPage** (lacuna inversa — endpoint já existe, baixo custo, alto valor): `GET /api/notificacoes`. Sino na top bar da Home.
14. **CheckInPage** — bloqueado pela lacuna #2 (e #4). Implementar UI com mock do token; plugar quando `POST /api/eventos/{slug}/checkin` existir. QR real via ZXing.
15. **ProfilePage** — bloqueado por #3/#4/#5. Implementar com `GET /api/usuarios/me` + favoritos + histórico quando disponíveis.
16. **InteractionPage** — bloqueado por #5/#6. Implementar incrementalmente: favoritar → enquete → MVP → mural (mural idealmente com SignalR).

### Fase 3 — Admin
17. **AdminPage** (tabela) — `GET /api/eventos`; canvas claro; `CollectionView` como tabela.
18. **NewEventPopup** — dropdowns de catálogo; bloqueado por #10 (`POST /api/eventos`).
19. **Gráficos + KPIs** — bloqueados por #9 (`GET /api/admin/dashboard`, `/checkins-por-modalidade`); Microcharts/Syncfusion.

### Fase 4 — Extras de contrato e polimento
20. **InscricaoPage** — `POST /api/inscricoes` (feature de API sem UI no design).
21. **Polimento**: animações de press (VSM), estados vazios/erro/loading, pull-to-refresh, acessibilidade (touch ≥ 44 px, contraste), cache de imagens.

### Ordem de prioridade resumida
**Infra (Fase 0) → Eventos → Detalhe → Home** (entregam valor imediato só com a API atual) → **Notificações** (ganho rápido) → demais telas conforme o backend fecha as lacunas #1–#10, sendo a **#1 (contagem de check-ins)** e a **#2 (endpoint de check-in)** as mais bloqueantes por aparecerem em múltiplas telas.

---

**Arquivos de referência (caminhos absolutos):**
- `C:/Users/elson.lopes/source/repos/hubesporteslages/figma/src/app/App.tsx` — todas as telas, dados mock e navegação do protótipo
- `C:/Users/elson.lopes/source/repos/hubesporteslages/figma/src/main.tsx` — bootstrap React
- `C:/Users/elson.lopes/source/repos/hubesporteslages/figma/src/styles/theme.css` — tema ativo (hex), fonte dos design tokens
- `C:/Users/elson.lopes/source/repos/hubesporteslages/figma/src/styles/fonts.css` — Rubik, Space Grotesk
- `C:/Users/elson.lopes/source/repos/hubesporteslages/figma/default_shadcn_theme.css` — tema shadcn (oklch, não importado; referência para tema claro do Admin)
- `C:/Users/elson.lopes/source/repos/hubesporteslages/figma/guidelines/Guidelines.md` — template vazio (preencher com tokens reais antes de codar telas)
- `C:/Users/elson.lopes/source/repos/hubesporteslages/figma/src/imports/DESIGN-nike.md`, `DESIGN-sentry.md` — referências estéticas
