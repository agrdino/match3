using System;
using System.Collections.Generic;
using _Scripts.Grid;
using _Scripts.Tile;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace _Scripts.Controller
{
    public class PinWheelController : ISpecialTileController
    {
        private ITile _pinWheel;
        
        public async void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback)
        {
            NormalTilePosition target;
            _pinWheel = origin.CurrentTile;
            origin.ReleaseTile();
            completedActionCallback?.Invoke();

            _pinWheel.onCrushed += ChangeTarget;
            ChangeTarget();
            
            void ChangeTarget()
            {
                do
                {
                    target = grid[UnityEngine.Random.Range(0, Definition.BOARD_WIDTH),
                        UnityEngine.Random.Range(0, Definition.BOARD_HEIGHT)];
                } while (target == null || target.IsAvailable() || target.PositionState() == EPositionState.Busy);

                _pinWheel.Transform().DOKill();
                _pinWheel.Transform().DOMove(target.Transform().position, 0.75f).SetEase(Ease.InCubic).OnComplete(async () =>
                {
                    _pinWheel.onCrushed -= ChangeTarget;
                    _pinWheel.Crush();
                    crushTileAction?.Invoke(new List<NormalTilePosition>(){target});
                    await UniTask.Delay(150);
                    completedActionCallback?.Invoke();
                });
            }
        }
    }
}