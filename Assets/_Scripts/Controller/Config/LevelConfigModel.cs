using System;
using System.Collections.Generic;
using _Scripts;
using UnityEngine;

namespace _Data.LevelConfig
{
    public class LevelConfigModel : ScriptableObject
    {
        public int levelID;
        public List<GridConfigModel> gridConfigs;

        public void CreateDefaultData(int levelID)
        {
            int midX = Definition.BOARD_WIDTH / 2;
            int midY = Definition.BOARD_HEIGHT / 2;

            this.levelID = levelID;

            gridConfigs ??= new List<GridConfigModel>();
            GridConfigModel gridConfig;
            for (int x = -midX; x < midX; x++)
            {
                for (int y = -midY; y < midY; y++)
                {
                    gridConfig = new GridConfigModel()
                    {
                        coordinates = new Coordinates(x + midX, y + midY),
                        type = EGridPositionType.Tile
                    };
                    gridConfigs.Add(gridConfig);
                }
            }
        }
    }

    [Serializable]
    public class GridConfigModel
    {
        public Coordinates coordinates;
        public EGridPositionType type;
    }
}