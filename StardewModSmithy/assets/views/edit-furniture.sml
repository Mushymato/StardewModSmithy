<panel layout="100% 100%" horizontal-content-alignment="end">
  <include name="mushymato.StardewModSmithy/views/includes/draggable-texture-sheet" *context={:TextureContext}/>
  <panel layout="500px content" *context={:EditableContext}>
    <frame layout="stretch 50px" padding="30,20" border={@Mods/StardewUI/Sprites/ControlBorder}>
      <lane orientation="vertical">
        <form-row title={#gui.label.id}>
          <dropdown options={FurnitureDataList}
            option-format={:FurnitureDataName}
            option-min-width="250"
            selected-option={<>BoundsProvider}/>
        </form-row>
      </lane>
    </frame>
    <frame layout="stretch stretch" margin="0,70,0,0" padding="32,8" border={@Mods/StardewUI/Sprites/ControlBorder} *if={HasBoundsProvider}>
      <lane orientation="vertical" *context={SelectedFurniture}>
        <form-row title={#gui.label.name}>
          <textinput layout="content 54px" margin="-8,12" text={<>DisplayName} />
        </form-row>
        <form-row title={#gui.label.tilesheet}>
          <spin-box *context={TilesheetSizeX} />
          <spin-box *context={TilesheetSizeY} />
        </form-row>
        <form-row title={#gui.label.bounding}>
          <spin-box *context={BoundingBoxSizeX} />
          <spin-box *context={BoundingBoxSizeY} />
        </form-row>
        <form-row title={#gui.label.type}>
          <dropdown layout="stretch content"
            options={Type_Options}
            option-min-width="240"
            selected-option={<>Type} />
        </form-row>
        <form-row title={#gui.label.rotation}>
          <dropdown options={Rotation_Options}
            option-format={:RotationName}
            option-min-width="240"
            selected-option={<>Rotation}/>
        </form-row>
        <form-row title={#gui.label.placement}>
          <dropdown options={Placement_Options}
            option-format={:PlacementName}
            option-min-width="240"
            selected-option={<>Placement}/>
        </form-row>
      </lane>
    </frame>
  </panel>
</panel>

<template name="form-row">
  <lane layout="content content"
        vertical-content-alignment="middle">
    <label layout="140px content"
            margin="0,8"
            font="small"
            text={&title}
            shadow-alpha="0.8"
            shadow-color="#4448"
            shadow-offset="-2, 2" />
    <outlet />
  </lane>
</template>

<template name="spin-box">
  <lane orientation="horizontal" vertical-content-alignment="middle" margin="4,0">
    <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
      left-click=|Decrease()|/>
    <label wheel=|Wheel($Direction)| text={Value}
      font="dialogue"
      layout="content[64..] content"
      padding="4,0,2,0"
      focusable="true"
      horizontal-alignment="middle"
      shadow-alpha="0.8"
      shadow-color="#4448"
      shadow-offset="-2, 2"/>
    <image sprite={@Mods/StardewUI/Sprites/CaretRight} focusable="true"
      left-click=|Increase()|/>
  </lane>
</template>
