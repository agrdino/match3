using System;
using System.Collections.Generic;
using _Scripts.Grid;
using _Scripts.Tile;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Redcode.Extensions;
using UnityEngine;

namespace _Scripts.Controller
{
    public class RocketHandler : ISpecialTileHandler
    {
        public async void Active(NormalTilePosition origin, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction, Action completedActionCallback)
        {
            //fake anim
            await UniTask.Delay(100);
            
            ITile animUp = BoardController.TileFactory(ETileType.Rocket, origin.Transform().position, 0);
            ITile animDown = BoardController.TileFactory(ETileType.Rocket, origin.Transform().position, 0);
            animUp.GameObject().SetActive(true);
            animDown.GameObject().SetActive(true);

            animUp.Transform().DOMove(
                origin.Transform().position + Vector3.up * Definition.BOARD_HEIGHT, 
                Definition.BOARD_HEIGHT * 0.1f)
                .SetEase(Ease.Linear);
            animDown.Transform().DOMove(
                origin.Transform().position + Vector3.down * Definition.BOARD_HEIGHT,
                Definition.BOARD_HEIGHT * 0.1f)
                .SetEase(Ease.Linear);
            
            //force vertical
            int x = origin.Coordinates().x;
            int y = origin.Coordinates().y;
            
            origin.CrushTile();

            for (int i = 0; i < Definition.BOARD_HEIGHT; i++)
            {
                int check = y - i;
                List<NormalTilePosition> target = new();
                if (GridController.IsInBounds(x, check))
                {
                    if (grid[x, check] != null && !grid[x, check].IsAvailable()) 
                    {
                        grid[x, check].ChangePositionState(ETileState.Matching);
                        target.Add(grid[x, check]);
                    }
                }

                check = y + i;
                if (GridController.IsInBounds(x, check))
                {
                    if (grid[x, check] != null && !grid[x, check].IsAvailable())
                    {
                        grid[x, check].ChangePositionState(ETileState.Matching);
                        target.Add(grid[x, check]);
                    }
                }
                crushTileAction?.Invoke(target);
                
                await UniTask.Delay(100);
            }
            
            animDown.Crush();
            animUp.Crush();
            completedActionCallback?.Invoke();
        }

        public async void Merge(NormalTilePosition origin, NormalTilePosition target, NormalTilePosition[,] grid,
            Action<List<NormalTilePosition>> crushTileAction,
            Action completedActionCallback, bool isSwapped = false)
        {
            NormalTilePosition center = isSwapped ? origin : target;
            switch (target.CurrentTile.TileType())
            {
                case ETileType.PinWheel:
                case ETileType.LightBall:
                {
                    SpecialTileHandler.Merge(target, origin, grid, crushTileAction, completedActionCallback, isSwapped);
                    return;
                }
                case ETileType.Boom:
                {
                    List<NormalTilePosition> targets = new ();
                    for (int i = -1; i <= 1; i++)
                    {
                        if (BoardController.IsNormalTilePosition(center.Coordinates().x - i, center.Coordinates().y, out NormalTilePosition affectedPosition))
                        {
                            affectedPosition.CrushTile();
                            ITile newRocket = BoardController.TileFactory(ETileType.Rocket,
                                affectedPosition.GameObject().transform.position, 0);
                            newRocket.GameObject().SetActive(true);
                            affectedPosition.SetFutureTile(newRocket);
                            targets.Add(affectedPosition);
                        }
                    }

                    await UniTask.Delay(200);
                    crushTileAction?.Invoke(targets);
                    break;
                }
                case ETileType.Rocket:
                {
                    origin.CrushTile();
                    await UniTask.Delay(100);
                    
                    ITile animUp = BoardController.TileFactory(ETileType.Rocket, center.Transform().position, 0);
                    ITile animDown = BoardController.TileFactory(ETileType.Rocket, center.Transform().position, 0);
                    ITile animLeft = BoardController.TileFactory(ETileType.Rocket, center.Transform().position, 0);
                    ITile animRight = BoardController.TileFactory(ETileType.Rocket, center.Transform().position, 0);
                    
                    animUp.GameObject().SetActive(true);
                    animDown.GameObject().SetActive(true);
                    animLeft.GameObject().SetActive(true);
                    animRight.GameObject().SetActive(true);

                    animUp.Transform().DOMove(
                            center.Transform().position + Vector3.up * Definition.BOARD_HEIGHT, 
                            Definition.BOARD_HEIGHT * 0.1f)
                        .SetEase(Ease.Linear);
                    animDown.Transform().DOMove(
                            center.Transform().position + Vector3.down *  Definition.BOARD_HEIGHT,
                            Definition.BOARD_HEIGHT * 0.1f)
                        .SetEase(Ease.Linear);
                    
                    animLeft.Transform().DOMove(
                            center.Transform().position + Vector3.left * Definition.BOARD_WIDTH, 
                            Definition.BOARD_WIDTH * 0.1f)
                        .SetEase(Ease.Linear);
                    animRight.Transform().DOMove(
                            center.Transform().position + Vector3.right * Definition.BOARD_WIDTH,
                            Definition.BOARD_WIDTH * 0.1f)
                        .SetEase(Ease.Linear);

                    target.CrushTile();
                    int x = center.Coordinates().x;
                    int y = center.Coordinates().y;

                    int i = 1, j = 1;
                    while (i < Definition.BOARD_HEIGHT && j < Definition.BOARD_WIDTH)
                    {
                        int check = y - i;
                        List<NormalTilePosition> targets = new();
                        if (BoardController.IsNormalTilePosition(x, check, out NormalTilePosition down))
                        {
                            if (!down.IsAvailable())
                            {
                                down.ChangePositionState(ETileState.Matching);
                                targets.Add(down);
                            }
                        }

                        check = y + i;
                        if (BoardController.IsNormalTilePosition(x, check, out NormalTilePosition up))
                        {
                            if (!up.IsAvailable())
                            {
                                up.ChangePositionState(ETileState.Matching);
                                targets.Add(up);
                            }
                        }

                        check = x + i;
                        if (BoardController.IsNormalTilePosition(check, y, out NormalTilePosition right))
                        {
                            if (!right.IsAvailable())
                            {
                                right.ChangePositionState(ETileState.Matching);
                                targets.Add(right);
                            }
                        }

                        check = x - i;
                        if (BoardController.IsNormalTilePosition(check, y, out NormalTilePosition left))
                        {
                            if (!left.IsAvailable())
                            {
                                left.ChangePositionState(ETileState.Matching);
                                targets.Add(left);
                            }
                        }

                        crushTileAction?.Invoke(targets);
                        i++;
                        j++;
                        await UniTask.Delay(100);
                    }

                    animDown.Crush();
                    animUp.Crush();
                    animLeft.Crush();
                    animRight.Crush();
                    
                    completedActionCallback?.Invoke();
                    break;
                }
                default:
                {
                    throw new ArgumentException();
                }
            }
        }
    }
}