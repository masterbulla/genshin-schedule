import React, { memo, useMemo } from "react";
import { Character } from "../../db/characters";
import { TalentMaterial } from "../../db/talentMaterials";
import { useConfig } from "../../configs";
import DropLabel from "../../dropLabel";
import Checkbox from "../../checkbox";
import { arrayToggle } from "../../utils";
import GameImage from "../../gameImage";
import { trackEvent } from "../../track";

const MaterialDisplay = ({
  character,
  material,
}: {
  character: Character;
  material: TalentMaterial;
}) => {
  const [list, setList] = useConfig("characters");

  const exists = useMemo(() => list.includes(character.name), [
    list,
    character,
  ]);

  return (
    <div className="py-4 space-y-4 text-sm flex flex-col">
      <a href={material.wiki}>
        <div className="space-x-2 flex flex-row">
          <GameImage name={material.item} className="w-12 h-12" />

          <div className="flex flex-col justify-center">
            <div className="text-lg">{material.name}</div>
            <div className="text-xs text-gray-600">{material.type}</div>
          </div>
        </div>
      </a>

      <DropLabel item={material} />

      <Checkbox
        value={exists}
        setValue={(value) => {
          setList((list) => arrayToggle(list, character.name, value));
          trackEvent("character", "materialToggle");
        }}
      >
        <div>Show on schedule</div>

        <div className="text-xs text-gray-600">
          Scheduled domains will appear on the days they are available.
        </div>
      </Checkbox>
    </div>
  );
};

export default memo(MaterialDisplay);
