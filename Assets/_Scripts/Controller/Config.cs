using System.Collections.Generic;
using _Data.LevelConfig;
using _Scripts.Helper;
using UnityEngine;

namespace _Scripts.Controller
{
    public class Config : Singleton<Config>
    {
        #region ----- Component Config -----

        [SerializeField] private List<LevelConfigModel> _levelConfigs;
        [SerializeField] private List<GemAvatarModel> _gemAvatars;

        #endregion

        #region ----- Properties -----

        public List<LevelConfigModel> levelConfigs => _levelConfigs;

        #endregion

        public Sprite this[EGemType gemType] => _gemAvatars.Find(x => x.GemType == gemType).Avatar;
    }
}