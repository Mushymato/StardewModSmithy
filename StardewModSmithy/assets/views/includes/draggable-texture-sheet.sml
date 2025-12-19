<panel layout="stretch stretch" margin="0,64,0,0"
  draggable="true"
  drag-start=|SheetDragStart($Position)|
  drag=|SheetDrag($Position)|
  drag-end=|SheetDragEnd($Position)|>
  <image sprite={Sheet} margin={SheetMargin} opacity={SheetOpacity}/>
  <panel layout="content content" padding={BoundsPadding} *context={BoundsProvider}>
    <image *repeat={GUI_BoundingSquares} margin={:this} sprite={@mushymato.StardewModSmithy/sprites/cursors:tileGreen} />
    <image sprite={@mushymato.StardewModSmithy/sprites/cursors:borderWhite}
      layout={GUI_TilesheetArea}
      fit="Stretch"
      opacity="0.5"/>
    <label text={^SpriteIndex} font="dialogue" color="White" padding="8,4" scale="1.5" shadow-alpha="1" shadow-color="#4448" shadow-offset="-4, 4"/>
  </panel>
  <frame *context={:MovementMode}
      background={@Mods/StardewUI/Sprites/MenuSlotOutset}
      margin="0,-64,0,0"
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
        margin="12, 8"
        text={:Name}
        tooltip={:Description} />
    </segments>
  </frame>
</panel>
