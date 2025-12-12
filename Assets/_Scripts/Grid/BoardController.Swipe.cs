using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Gem;
using _Scripts.Grid.Gem;
using UnityEngine;

namespace _Scripts.Grid
{
    public partial class BoardController
    {
        private NormalGemPosition _currentPosition;
        private void _OnBeginSwipe(Coordinates coordinates)
        {
            IGemPosition temp = _gem[coordinates.x, coordinates.y];
            if (temp.IsAvailable() || temp.PositionState() is not EPositionState.Free)
            {
                return;
            }

            if (temp is NormalGemPosition normalGemPosition)
            {
                _currentPosition = normalGemPosition;
            }
        }

        private void _OnEndSwipe(ESwipeDirection direction)
        {
            if (_currentPosition == null)
            {
                return;
            }
            
            if (direction is ESwipeDirection.Cancel)
            {
                return;
            }

            IGemPosition temp = _GetSwipePosition(_currentPosition.Coordinates(), direction);
            if (temp is not NormalGemPosition position2)
            {
                return;
            }

            if (position2.IsAvailable() || position2.PositionState() is not EPositionState.Free)
            {
                return;
            }
            
            //swap
            NormalGemPosition position1 = _currentPosition;
            IGem gem1 = position1.CurrentGem;
            IGem gem2 = position2.CurrentGem;
            
            //Check
            position2.SetFutureGem(gem1);
            position1.SetFutureGem(gem2);

            //gem1 at p2 || gem2 at p1
            bool isMatchAtPosition1 = _IsMatchAt(position1.Coordinates(), gem2.GemType(), out List<IGemPosition> gems1);
            bool isMatchAtPosition2 = _IsMatchAt(position2.Coordinates(), gem1.GemType(), out List<IGemPosition> gems2);
            
            if (isMatchAtPosition1 || isMatchAtPosition2)
            {
                _ = gem1.Swap(position2.Transform().position, () =>
                {
                    position2.CompleteReceivedGem();
                    if (isMatchAtPosition1)
                    {
                        gems1.ForEach(x => x.CrushGem());
                    }

                    if (isMatchAtPosition2)
                    {
                        gems2.ForEach(x => x.CrushGem());
                    }
                    _FillBoard();
                });
            
                _ = gem2.Swap(position1.Transform().position, () =>
                {
                    position1.CompleteReceivedGem();
                });
            }
            else
            {
                //return
                position2.SetFutureGem(gem2);
                position1.SetFutureGem(gem1);
                
                position1.ChangePositionState(EPositionState.Busy);
                position2.ChangePositionState(EPositionState.Busy);
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

        private IGemPosition _GetSwipePosition(Coordinates coordinates, ESwipeDirection direction)
        {
            switch (direction)
            {
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

            if (!_IsInBounds(coordinates.x, coordinates.y))
            {
                return null;
            }
            
            if (coordinates.x >= Definition.BOARD_HEIGHT || coordinates.y >= Definition.BOARD_WIDTH)
            {
                return null;
            }
            
            return _gem[coordinates.x, coordinates.y];
        }
        
        private bool _IsMatchAt(Coordinates coordinates, EGemType target, out List<IGemPosition> matchedGem, bool predict = false)
        {
            int x = coordinates.x;
            int y = coordinates.y;
            
            matchedGem = new List<IGemPosition>();
            if (!_IsInBounds(x, y))
            {
                return false;
            }

            IGemPosition center = _gem[x, y];
            if (!predict)
            {
                if (center.IsAvailable())
                {
                    return false;
                }
            
                if (center is not NormalGemPosition centerGem)
                {
                    return false;
                }
            }

            List<IGemPosition> horizontal = new (){ center };

            // left
            int check = x - 1;
            while (_IsInBounds(check, y) && _IsTheSame(check, y, target, predict))
            {
                horizontal.Add(_gem[check, y]);
                check--;
            }

            // right
            check = x + 1;
            while (_IsInBounds(check, y) && _IsTheSame(check, y, target, predict))
            {
                horizontal.Add(_gem[check, y]);
                check++;
            }

            // Nếu ngang >= 3 -> thêm vào result
            if (horizontal.Count >= 3)
            {
                Debug.LogError("-----------------------");
                horizontal.ForEach(z =>
                {
                    if (predict)
                    {
                        return;
                    }
                    Debug.Log(z.Coordinates() + " " + (z as NormalGemPosition).CurrentGem.GemType());
                });
                matchedGem.AddRange(horizontal);
            }
            
            List<IGemPosition> vertical = new() { center };
            //down
            check = y - 1;
            while (_IsInBounds(x, check) && _IsTheSame(x, check, target, predict))
            {
                vertical.Add(_gem[x, check]);
                check--;
            }

            //up
            check = y + 1;
            while (_IsInBounds(x, check) && _IsTheSame(x, check, target, predict))
            {
                vertical.Add(_gem[x, check]);
                check++;
            }

            if (vertical.Count >= 3)
            {
                Debug.LogError("-----------------------");
                vertical.ForEach(z =>
                {
                    if (predict)
                    {
                        return;
                    }
                    Debug.Log(z.Coordinates() + " " + (z as NormalGemPosition).CurrentGem.GemType());
                });
                matchedGem.AddRange(vertical);
            }

            matchedGem = matchedGem.Distinct().ToList();

            return matchedGem.Count >= 3;
        }

        private bool _IsTheSame(int x, int y, EGemType gemType, bool predict = false)
        {
            IGemPosition temp = _gem[x, y];
            
            if (temp is not NormalGemPosition target)
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
            
            if (target.IsAvailable())
            {
                return false;
            }

            return target.CurrentGem.GemType() == gemType;
        }
    }
}