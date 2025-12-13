<lane orientation="horizontal" layout="1280px 720px" horizontal-content-alignment="middle">
  <panel layout="896px 640px" margin="0,0" draggable="true"
    drag-start=|SheetDragStart($Position)|
    drag=|SheetDrag($Position)|
    drag-end=|SheetDragEnd($Position)|>
    <image sprite={FurnitureSheet}
      margin={FurnitureSheetMargin}
    />
    <panel padding="256,192">
      <image *repeat={FurnitureBoundingSquares} margin={this} sprite={@mushymato.StardewModSmithy/sprites/cursors:tileGreen} />
      <image sprite={@mushymato.StardewModSmithy/sprites/cursors:borderWhite}
        layout={FurnitureTilesheetArea}
        fit="Stretch"
        opacity="0.7"/>
      <!-- <image sprite={@mushymato.StardewModSmithy/sprites/cursors:borderRed}
        layout={FurnitureBoundingBoxArea}
        fit="Stretch"
        margin="4"/> -->
    </panel>
      <!-- <image *repeat={FurnitureTilesheetSquares} margin={this} sprite={@mushymato.StardewModSmithy/sprites/cursors:tileGreen} />
      <image *repeat={FurnitureBoundingSquares} margin={this} sprite={@mushymato.StardewModSmithy/sprites/cursors:tileRed} /> -->
  </panel>
  <panel layout="stretch 720px">
    <frame layout="stretch 64px" margin="0,0,0,0" padding="16,20,0,0" border={@Mods/StardewUI/Sprites/ControlBorder}>
      <dropdown options={FurnitureDataList}
        option-format={FurnitureDataName}
        option-min-width="300"
        selected-option={<>SelectedFurniture}/>
    </frame>
    <frame layout="stretch stretch" margin="0,64,0,0" padding="32,8" border={@Mods/StardewUI/Sprites/ControlBorder}>
      <lane orientation="vertical" *context={SelectedFurniture}>
        <textinput layout="stretch 54px" margin="0,12" text={<>DisplayName} />
        <button text={#gui.button.tilesheet.select} layout="stretch content" margin="0,8,0,0" hover-background={@Mods/StardewUI/Sprites/ButtonLight} />
        <button text={#gui.button.bounding.select} layout="stretch content" margin="0,0,0,8" hover-background={@Mods/StardewUI/Sprites/ButtonLight} />
        <dropdown layout="stretch content"
          options={Type_Options}
          option-min-width="200"
          selected-option={<>Type} />
        <dropdown layout="stretch content"
          options={Rotation_Options}
          option-min-width="200"
          selected-option={<>Rotation} />
        <dropdown layout="stretch content"
          options={Placement_Options}
          option-min-width="200"
          selected-option={<>Placement} />
        <button text={#gui.button.create} layout="stretch content" margin="0,8,0,0" hover-background={@Mods/StardewUI/Sprites/ButtonLight} />
        <button text={#gui.button.export} layout="stretch content" margin="0,0,0,8" hover-background={@Mods/StardewUI/Sprites/ButtonLight} />
      </lane>
    </frame>
  </panel>
</lane>
