<lane orientation="horizontal"
  horizontal-content-alignment="middle"
  vertical-content-alignment="start">
  <lane layout="140px content"
        margin="0, 32, 0, 0"
        orientation="vertical"
        horizontal-content-alignment="end"
        z-index="2">
    <frame *repeat={:AllTabs}
        layout="120px 64px"
        margin={Margin}
        padding="16, 0"
        horizontal-content-alignment="middle"
        vertical-content-alignment="middle"
        background={@mushymato.StardewModSmithy/sprites/MenuTiles:TabButton}
        focusable="true"
        click=|^SelectTab(this)|>
      <label text={:Label} />
    </frame>
  </lane>
  <frame *switch={SelectedTab}
      background={@Mods/StardewUI/Sprites/MenuBackground}
      border={@Mods/StardewUI/Sprites/MenuBorder}
      border-thickness="36, 36, 40, 36"
      margin="-20,0,0,0"
      padding="8"
      layout="70%[1000..] 80%[650..]">
    <!-- Packs -->
    <lane *case="packs" *context={:PackListing} orientation="vertical">
      <banner margin="8" text={#gui.label.editable-content-packs} layout="content content"/>
      <scrollable peeking="128" scrollbar-margin="8,0,0,0">
        <lane orientation="vertical">
          <frame *repeat={PackDisplayList}
            background={@Mods/StardewUI/Sprites/MenuSlotInset}
            layout="content[500..] content"
            padding="16,6"
            margin="6">
            <expander layout="stretch content"
              margin="0,0,0,4"
              is-expanded={<>IsExpanded}>
              <lane *outlet="header" *switch={:IsLoaded} vertical-content-alignment="middle" orientation="horizontal">
                <image *case="true" sprite={@mushymato.StardewModSmithy/sprites/emojis:checkmark} tooltip={#gui.tooltip.is-loaded} layout="36px 36px" margin="0,12,12,12"/>
                <image *case="false" sprite={@mushymato.StardewModSmithy/sprites/emojis:cross} tooltip={#gui.tooltip.not-loaded} layout="36px 36px" margin="0,12,12,12"/>
                <lane orientation="vertical" layout="stretch content" >
                  <label font="dialogue" max-lines="1" margin="4" text={PackName} />
                  <label font="small" max-lines="1" text={PackUniqueID} />
                </lane>
                <lane layout="220px content" margin="8,0" orientation="vertical">
                  <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                    layout="stretch content"
                    font="small"
                    margin="0,8"
                    text={#gui.button.edit-furniture}
                    opacity="0.5"
                    +state:canshow={:CanShowEdit_Furniture}
                    +state:canshow:opacity="1.0"
                    left-click=|ShowEdit_Furniture()|
                  />
                  <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                    layout="stretch content"
                    font="small"
                    text={#gui.button.edit-wallfloor}
                    opacity="0.5"
                    tooltip={#gui.tooltip.no-wallfloor-tx}
                    +state:canshow={:CanShowEdit_WallFloor}
                    +state:canshow:opacity="1.0"
                    +state:canshow:tooltip=""
                    left-click=|ShowEdit_WallFloor()|
                  />
                </lane>
                <image sprite={@mushymato.StardewModSmithy/sprites/cursors:hammerButton}
                  layout="64px 64px"
                  margin="8"
                  focusable="true"
                  +hover:scale="1.1"
                  +transition:scale="100ms EaseOutCubic"
                  left-click=|BrowsePackFolder()|
                  tooltip={#gui.tooltip.open-pack-folder}
                />
              </lane>
              <lane orientation="vertical" *if={IsExpanded}>
                <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,0,0" fit="Stretch"/>
                <form-row-small title={#gui.label.author}>
                  <textinput font="small" layout="300px 54px" text={<>PackAuthor} />
                  <label text={#gui.label.mod-name} margin="32,8,8,8" font="small"/>
                  <textinput font="small" layout="stretch 54px" text={<>PackName} />
                </form-row-small>
                <form-row-small title={#gui.label.mod-desc}>
                  <textinput font="small" layout="stretch 54px" text={<>PackDescription} />
                </form-row-small>
                <form-row-small title={#gui.label.version}>
                  <textinput font="small" layout="200px 54px" text={<>PackVersion} />
                  <label text={#gui.label.nexus-id} margin="32,8,8,8" font="small"/>
                  <textinput font="small" layout="200px 54px" text={<>PackNexusID} />
                </form-row-small>
              </lane>
            </expander>
          </frame>
          <frame
            background={@Mods/StardewUI/Sprites/MenuSlotInset}
            layout="content[500..] 96px"
            padding="24,6"
            margin="6"
            vertical-content-alignment="middle">
            <lane orientation="horizontal" vertical-content-alignment="middle">
              <label font="dialogue" text={#gui.label.new-pack} />
              <textinput font="dialogue" layout="stretch 70px" margin="16,4,16,0" text={<>NewModName} />
              <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                  layout="content[150..] content"
                  font="dialogue"
                  text={#gui.button.create}
                  left-click=|CreateAndEdit()|
                  opacity={NewModErrorOpacity}
                  tooltip={NewModTooltip}
                />
            </lane>
          </frame>
        </lane>
      </scrollable>
    </lane>
    <!-- Textures -->
    <lane *case="textures" orientation="vertical" *context={:PackListing}>
      <lane>
        <banner margin="8" text={#gui.label.textures} layout="stretch content"/>
        <frame background={@mushymato.StardewModSmithy/sprites/cursors2:blankButton}
          layout="64px 64px"
          margin="8"
          transform-origin="0.5,0.5"
          focusable="true"
          tooltip={#gui.tooltip.reload-textures}
          +hover:transform="scale: 1.1"
          +transition:transform="100ms EaseOutCubic">
          <image sprite={@mushymato.StardewModSmithy/sprites/cursors2:resetIcon}
            left-click=|ReloadTextures()|/>
        </frame>
        <image sprite={@mushymato.StardewModSmithy/sprites/cursors2:paintButton}
          layout="64px 64px"
          margin="8"
          focusable="true"
          +hover:scale="1.1"
          +transition:scale="100ms EaseOutCubic"
          left-click=|BrowseTextureFolder()|
          tooltip={#gui.tooltip.open-texture-folder}
          />
      </lane>
      <scrollable *if={HasTextures} peeking="128" scrollbar-margin="8,0,0,0">
        <grid item-layout="length: 200" item-spacing="12,12">
          <frame *repeat={Textures}
            border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
            border-thickness="4"
            background={@Mods/StardewUI/Sprites/White}
            background-tint="Transparent"
            focusable="true"
            padding="4"
            tooltip={:UITooltip}
            >
            <image layout="192px 192px" sprite={:UISpriteSmall} fit="Cover" vertical-alignment="middle"/>
          </frame>
        </grid>
      </scrollable>
      <label *!if={HasTextures} font="dialogue" text={:PutTexturesMessage}/>
    </lane>
    <!-- Config -->
    <lane *case="config" *context={:ModConfig} orientation="vertical">
      <banner margin="8" text={#gui.label.mod-configuration} layout="content content"/>
      <scrollable peeking="128" scrollbar-margin="8,0,0,0">
        <lane orientation="vertical">
          <form-row title={#gui.label.show-workspace-key} tooltip={#gui.tooltip.show-workspace-key}>
            <keybind-editor button-height="64"
                sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:default-default-0.5}
                editable-type="MultipleKeybinds"
                add-button-text={#gui.label.show-workspace-key.add}
                focusable="true"
                keybind-list={<>ShowWorkspaceKey} />
          </form-row>
          <form-row title={#gui.label.toggle-drag-mode-key} tooltip={#gui.tooltip.toggle-drag-mode-key}>
            <keybind-editor button-height="64"
                sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:default-default-0.5}
                editable-type="MultipleKeybinds"
                add-button-text={#gui.label.toggle-drag-mode-key.add}
                focusable="true"
                keybind-list={<>ToggleDragModeKey} />
          </form-row>
          <form-row title={#gui.label.sync-drag-key} tooltip={#gui.tooltip.sync-drag-key}>
            <keybind-editor button-height="64"
                sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:default-default-0.5}
                editable-type="MultipleKeybinds"
                add-button-text={#gui.label.sync-drag-key.add}
                focusable="true"
                keybind-list={<>SyncDragKey} />
          </form-row>
          <form-row title={#gui.label.author} tooltip={#gui.tooltip.author-name}>
            <textinput layout="50% 64px" margin="-8,0" font="dialogue" placeholder={#gui.placeholder.author-name} text={<>AuthorName} />
          </form-row>
          <form-row title={#gui.label.autosave} tooltip={#gui.tooltip.autosave-frequency}>
            <frame *context={:AutosaveFrequency}
                background={@Mods/StardewUI/Sprites/MenuSlotOutset}
                padding="4">
              <segments balanced="true"
                  highlight={@Mods/StardewUI/Sprites/White}
                  highlight-tint="#39d"
                  highlight-transition="150ms EaseOutQuart"
                  separator={@Mods/StardewUI/Sprites/White}
                  separator-tint="#c99"
                  separator-width="2"
                  selected-index={<>SelectedIndex}>
                <label *repeat={Segments}
                  layout="content[160..] content"
                  padding="16"
                  text={:Name}
                  tooltip={:Description} />
              </segments>
            </frame>
          </form-row>
          <form-row title={#gui.label.auto-symlink-reload} tooltip={#gui.tooltip.auto-symlink-reload}>
            <checkbox is-checked={<>AutoSymlinkAndPatchReload}/>
          </form-row>
          <form-row title={#gui.label.app-icon-style} >
            <dropdown layout="stretch content"
              options={IconStyleValues}
              option-min-width="240"
              selected-option={<>IconStyle} />
          </form-row>
          <form-row title={#gui.label.quick-patch-reload-key} tooltip={#gui.tooltip.quick-patch-reload-key}>
            <keybind-editor button-height="64"
                sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:default-default-0.5}
                editable-type="MultipleKeybinds"
                add-button-text={#gui.label.quick-patch-reload-key.add}
                focusable="true"
                keybind-list={<>QuickPatchReloadKey} />
          </form-row>
          <form-row title={#gui.label.quick-patch-reload-modid} tooltip={#gui.tooltip.quick-patch-reload-modid}>
            <textinput layout="50% 64px" margin="-8,0" font="small" placeholder={#gui.label.quick-patch-reload-modid} text={<>QuickPatchReloadModIds} />
          </form-row>
        </lane>
      </scrollable>
    </lane>
    <!-- About -->
    <scrollable *case="about" peeking="128" scrollbar-margin="8,0,0,0">
      <lane orientation="vertical">
        <about-banner text={#gui.label.about} />
        <about-label text={#gui.paragraph.about.0} />
        <about-label text={#gui.paragraph.about.1} />
        <about-label text={#gui.paragraph.about.2} />
        <about-banner text={#gui.paragraph.faq.0.q} />
        <about-label text={#gui.paragraph.faq.0.a.0} />
        <about-label text={#gui.paragraph.faq.0.a.1} />
        <about-banner text={#gui.paragraph.faq.1.q} />
        <about-label text={#gui.paragraph.faq.1.a} />
        <about-banner text={#gui.paragraph.faq.2.q} />
        <about-label text={#gui.paragraph.faq.2.a.0} />
        <about-label text={#gui.paragraph.faq.2.a.1} />
        <about-label text={#gui.paragraph.faq.2.a.2} />
        <about-label text={#gui.paragraph.faq.2.a.3} />
        <about-label text={#gui.paragraph.faq.2.a.4} />
        <about-banner text={#gui.paragraph.faq.3.q} />
        <about-label text={#gui.paragraph.faq.3.a} />
        <about-banner text={#gui.paragraph.faq.4.q} />
        <about-label text={#gui.paragraph.faq.4.a.0} />
        <about-label text={#gui.paragraph.faq.4.a.1} />
        <about-banner text={#gui.paragraph.faq.5.q} />
        <about-label text={#gui.paragraph.faq.5.a} />
      </lane>
    </scrollable>
  </frame>
</lane>

<template name="form-row">
  <lane layout="content content"
        vertical-content-alignment="middle"
        margin="16">
    <label layout="300px content"
            font="dialogue"
            text={&title}
            tooltip={&tooltip}
            shadow-alpha="0.8"
            shadow-color="#4448"
            shadow-offset="-2, 2" />
    <outlet />
  </lane>
</template>

<template name="form-row-small">
  <lane layout="content content"
        vertical-content-alignment="middle"
        margin="8">
    <label layout="content[150..] content"
            font="small"
            text={&title}/>
    <outlet />
  </lane>
</template>

<template name="about-banner">
  <banner text={&text} margin="8" layout="content content"/>
</template>

<template name="about-label">
  <label text={&text}
    font="dialogue"
    margin="8"
    shadow-alpha="0.8"
    shadow-color="#4448"
    shadow-offset="-2, 2" />
</template>
