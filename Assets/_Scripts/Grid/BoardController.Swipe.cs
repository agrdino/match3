using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Controller;
using _Scripts.Tile;
using _Scripts.Tile.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Grid
{
    public partial class BoardController
    {
        private NormalTilePosition _currentPosition;
        private void _OnBeginSwipe(Coordinates coordinates)
        {
            if (_boardState is EBoardState.Shuffling)
            {
                return;
            }
            _currentPosition = _tilePositions[coordinates.x, coordinates.y];
            if (_currentPosition.IsAvailable() || _currentPosition.PositionState() is not EPositionState.Free)
            {
                _currentPosition = null;
            }
        }

        private void _OnEndSwipe(ESwipeDirection direction)
        {
            if (_currentPosition == null)
            {
                return;
            }
            
            if (_boardState == EBoardState.Shuffling)
            {
                _currentPosition = null;
                return;
            }
            
            NormalTilePosition position2 = _GetSwipePosition(_currentPosition.Coordinates(), direction);

            if (position2 == null ||  position2.IsAvailable() || position2.PositionState() is not EPositionState.Free)
            {
                _currentPosition = null;
                return;
            }

            if (_currentPosition.CurrentTile is SpecialTile || position2.CurrentTile is SpecialTile)
            {
                _SpecialSwap(_currentPosition, position2);
            }
            else
            {
                _NormalSwap(_currentPosition, position2);
            }
            _currentPosition = null;
        }

        private void _SpecialSwap(NormalTilePosition position1, NormalTilePosition position2)
        {
            if (position1 == position2)
            {
                _TriggerSpecialTile(position1);
                return;
            }
            
            if (position1.CurrentTile is SpecialTile special1 && position2.CurrentTile is SpecialTile special2)
            {
                _MergeSpecialTile(position1, position2);
                return;
            }
            
            _HalfSpecialSwap(position1, position2);
        }

        private void _NormalSwap(NormalTilePosition position1, NormalTilePosition position2)
        {
            ITile gem1 = position1.CurrentTile;
            ITile gem2 = position2.CurrentTile;
            
            //Check
            position2.SetFutureGem(gem1, true);
            position1.SetFutureGem(gem2, true);

            //gem1 at p2 || gem2 at p1
            bool isMatchAtPosition1 = _IsMatchAt(position1.Coordinates(), out (NormalTilePosition origin, List<NormalTilePosition> matchedTile) match1);
            bool isMatchAtPosition2 = _IsMatchAt(position2.Coordinates(), out (NormalTilePosition origin, List<NormalTilePosition> matchedTile) match2);
            
            if (isMatchAtPosition1 || isMatchAtPosition2)
            {
                position1.ChangePositionState(EPositionState.Busy);
                position2.ChangePositionState(EPositionState.Busy);
                
                _ = gem1.Swap(position2.Transform().position, async () =>
                {
                    UniTask t1 = new UniTask();
                    if (isMatchAtPosition1)
                    {
                        t1 = _MatchHandler(match1, bySwipe: true);
                    }

                    if (isMatchAtPosition2)
                    {
                        t1 = _MatchHandler(match2, bySwipe: true);
                    }

                    position2.CompleteReceivedTile();
                });
            
                _ = gem2.Swap(position1.Transform().position, () =>
                {
                    position1.CompleteReceivedTile();
                });
            }
            else
            {
                //return
                position2.SetFutureGem(gem2);
                position1.SetFutureGem(gem1);
                
                _ = gem1.SwapAndSwapBack(position2.Transform().position, () =>
                {
                    position2.ChangePositionState(EPositionState.Free);
                });
                
                _ = gem2.SwapAndSwapBack(position1.Transform().position, () =>
                {
                    position1.ChangePositionState(EPositionState.Free);
                });
            }
        }

        private void _HalfSpecialSwap(NormalTilePosition position1, NormalTilePosition position2)
        {
            NormalTilePosition special, normal;
            if (position1.CurrentTile is SpecialTile)
            {
                special = position1;
                normal = position2;
            }
            else
            {
                special = position2;
                normal = position1;
            }

            if (special.CurrentTile.TileType() == ETileType.LightBall)
            {
                _MergeSpecialTile(special, normal);
                return;
            }
            
            _NormalSwap(position1, position2);
        }

        private NormalTilePosition _GetSwipePosition(Coordinates coordinates, ESwipeDirection direction)
        {
            switch (direction)
            {
                case ESwipeDirection.Cancel:
                {
                    break;
                }
                case ESwipeDirection.Left:
                {
                    coordinates.x -= 1;
                    break;
                }
                case ESwipeDirection.Right:
                {
                    coordinates.x += 1;
                    break;
                }
                case ESwipeDirection.Up:
                {
                    coordinates.y += 1;
                    break;
                }
                case ESwipeDirection.Down:
                {
                    coordinates.y -= 1;
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }

            if (!GridController.IsInBounds(coordinates.x, coordinates.y))
            {
                return null;
            }
            
            return _tilePositions[coordinates.x, coordinates.y];
        }
        
        private bool _IsMatchAt(Coordinates coordinates, out (NormalTilePosition origin, List<NormalTilePosition> matchedTile) match,
            bool predict = false)
        {
            int x = coordinates.x;
            int y = coordinates.y;
            
            match = new ()
            {
                matchedTile = new()
            };
            
            if (!GridController.IsInBounds(x, y))
            {
                return false;
            }

            NormalTilePosition center = _tilePositions[x, y];
            
            match.origin = center;
            if (center.IsAvailable())
            {
                return false;
            }

            if (predict && center.CurrentTile is SpecialTile)
            {
                match.matchedTile.Add(center);
                return true;
            }
            
            ETileType target = center.CurrentTile.TileType();

            List<NormalTilePosition> horizontal = new (){ center };

            int check = 0;
            
            // left
            check = x - 1;
            while (GridController.IsInBounds(check, y) && _IsTheSame(check, y, target, predict))
            {
                horizontal.Add(_tilePositions[check, y]);
                check--;
            }

            // right
            check = x + 1;
            while (GridController.IsInBounds(check, y) && _IsTheSame(check, y, target, predict))
            {
                horizontal.Add(_tilePositions[check, y]);
                check++;
            }

            // Nếu ngang >= 3 -> thêm vào result
            if (horizontal.Count >= 3)
            {
                Debug.Log("-----------------------");
                horizontal.ForEach(z =>
                {
                    Debug.Log(z.Coordinates() + " " + z.CurrentTile.TileType());
                });
                match.matchedTile.AddRange(horizontal);
            }
            
            List<NormalTilePosition> vertical = new() { center };
            //down
            check = y - 1;
            while (GridController.IsInBounds(x, check) && _IsTheSame(x, check, target, predict))
            {
                vertical.Add(_tilePositions[x, check]);
                check--;
            }

            //up
            check = y + 1;
            while (GridController.IsInBounds(x, check) && _IsTheSame(x, check, target, predict))
            {
                vertical.Add(_tilePositions[x, check]);
                check++;
            }

            if (vertical.Count >= 3)
            {
                Debug.Log("-----------------------");
                vertical.ForEach(z =>
                {
                    Debug.Log(z.Coordinates() + " " + z.CurrentTile.TileType());
                });
                match.matchedTile.AddRange(vertical);
            }

            match.matchedTile = match.matchedTile.Distinct().ToList();

            //square
            if (match.matchedTile.Count <= 3)
            {
                //offset
                (int x, int y)[] botLeftSquare = new []
                {
                    (x, y),
                    (x - 1, y),
                    (x, y - 1),
                    (x - 1, y - 1)
                };

                foreach (var origin in botLeftSquare)
                {
                    if (!GridController.IsInBounds(origin.x, origin.y))
                    {
                        continue;
                    }

                    if (!GridController.IsInBounds(origin.x + 1, origin.y + 1))
                    {
                        continue;
                    }
                    
                    NormalTilePosition bl = _tilePositions[origin.x, origin.y];
                    NormalTilePosition br = _tilePositions[origin.x + 1, origin.y];
                    NormalTilePosition tl = _tilePositions[origin.x, origin.y + 1];
                    NormalTilePosition tr = _tilePositions[origin.x + 1, origin.y + 1];

                    if (bl == null || br == null || tl == null || tr == null)
                    {
                        continue;
                    }

                    if (bl.IsAvailable() || br.IsAvailable() || tl.IsAvailable() || tr.IsAvailable())
                    {
                        continue;
                    }
                    
                    if (!_IsTheSame(br.Coordinates(), bl.CurrentTile.TileType(), predict)
                        || !_IsTheSame(tl.Coordinates(), bl.CurrentTile.TileType(), predict)
                        || !_IsTheSame(tr.Coordinates(), bl.CurrentTile.TileType(), predict))
                    {
                        continue;
                    }
                    
                    match.matchedTile.AddRange(new List<NormalTilePosition>()
                    {
                        bl, br, tl, tr
                    });
                    
                    match.matchedTile = match.matchedTile.Distinct().ToList();
                    break;
                }
            }

            return match.matchedTile.Count >= 3;
        }

        private bool _IsMatchAt(Coordinates coordinates, bool predict = false)
        {
            return _IsMatchAt(coordinates, out (NormalTilePosition origin, List<NormalTilePosition> matchedGem) _, predict);
        }

        private async UniTask _MatchHandler((NormalTilePosition origin, List<NormalTilePosition> matchedTile) match,
            int delayTime = 150,
            bool bySwipe = false,
            bool bySpecial = false,
            bool autoFill = true)
        {
            foreach (var gemPosition in match.matchedTile)
            {
                gemPosition.ChangePositionState(EPositionState.Busy);
            }
            
            ETileType specialTileType = ETileType.None;
            
            //check special gem
            if (!bySpecial)
            {
                int differentX = match.matchedTile
                    .Where(x => x.Coordinates().x != match.origin.Coordinates().x)
                    .GroupBy(x => x.Coordinates().y)
                    .Select(x => x.Count())
                    .DefaultIfEmpty(0)
                    .Max() + 1;
                int differentY = match.matchedTile
                    .Where(x => x.Coordinates().y != match.origin.Coordinates().y)
                    .GroupBy(x => x.Coordinates().x)
                    .Select(x => x.Count())
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                switch (match.matchedTile.Count)
                {
                    case 4:
                    {
                        if (differentX == differentY)
                        {
                            specialTileType = ETileType.PinWheel;
                        }
                        else
                        {
                            specialTileType = ETileType.Rocket;
                        }
                        break;
                    }
                    case 5:
                    {
                        if (differentX == 2 || differentY == 2)
                        {
                            specialTileType = ETileType.PinWheel;
                            break;
                        }

                        if (differentX == 1 || differentY == 1)
                        {
                            specialTileType = ETileType.LightBall;
                            break;
                        }

                        specialTileType = ETileType.Boom;
                        break;
                    }
                    case > 5:
                    {
                        if (differentX > 5 || differentY > 5)
                        {
                            specialTileType = ETileType.LightBall;
                            break;
                        }

                        specialTileType = ETileType.Boom;
                        break;
                    }
                }
                
                if (!bySwipe && specialTileType is not ETileType.None)
                {
                    match.origin = match.matchedTile.OrderByDescending(x => x.Coordinates().y).ThenBy(x => x.Coordinates().x).ToList()[0];
                }
            }
            
            await UniTask.Delay(delayTime);

            List<UniTask> allTileDestroyTask = new();
            foreach (var tilePosition in match.matchedTile)
            {
                if (tilePosition.CurrentTile is SpecialTile)
                {
                    _TriggerSpecialTile(tilePosition);
                    continue;
                }

                allTileDestroyTask.Add(CrushTile(tilePosition));
            }

            await UniTask.WhenAll(allTileDestroyTask);
            //create special tile
            if (specialTileType is not ETileType.None)
            {
                ITile specialTile = TileFactory(specialTileType, match.origin.Transform().position, 0);
                specialTile.GameObject().SetActive(true);
                match.origin.SetFutureGem(specialTile, true);
            }

            if (autoFill)
            {
                _FillBoard();
            }

            async UniTask CrushTile(NormalTilePosition tilePosition)
            {
                // if(bySwipe || (!bySwipe && !bySpecial))
                if (tilePosition.CurrentTile == null)
                {
                    tilePosition.ChangePositionState(EPositionState.Free);
                    return;
                }
                if (bySwipe || !bySpecial)
                {
                    await TileAnimationController.CrushAnimation.Play(tilePosition.CurrentTile.GameObject());
                }
                tilePosition.CrushTile();
                tilePosition.ChangePositionState(EPositionState.Free);
            }
        }

        private bool _IsTheSame(int x, int y, ETileType tileType, bool predict = false)
        {
            NormalTilePosition target = _tilePositions[x, y];
            
            if (target == null)
            {
                return false;
            }

            if (target.IsAvailable())
            {
                return false;
            }

            if (!predict)
            {
                if (target.PositionState() is EPositionState.Busy)
                {
                    return false;
                }
            }
            // if (!predict && (target.PositionState() is EPositionState.Busy || target.IsAvailable()))
            // {
            //     return false;
            // }

            if (target.CurrentTile is SpecialTile)
            {
                return false;
            }
            
            return target.CurrentTile.TileType() == tileType;
        }
        
        private bool _IsTheSame(Coordinates origin, ETileType tileType, bool predict = false)
        {
            return _IsTheSame(origin.x, origin.y, tileType, predict);
        }
    }
}