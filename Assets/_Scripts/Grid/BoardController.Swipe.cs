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
            if (_boardState is EBoardState.Shuffling)
            {
                return;
            }
            _currentPosition = _gemPositions[coordinates.x, coordinates.y];
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
            
            if (_boardState == EBoardState.Shuffling || direction == ESwipeDirection.Cancel )
            {
                _currentPosition = null;
                return;
            }
            
            NormalGemPosition position2 = _GetSwipePosition(_currentPosition.Coordinates(), direction);

            if (position2.IsAvailable() || position2.PositionState() is not EPositionState.Free)
            {
                _currentPosition = null;
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
            bool isMatchAtPosition1 = _IsMatchAt(position1.Coordinates(), out (NormalGemPosition origin, List<NormalGemPosition> matchedGem) match1);
            bool isMatchAtPosition2 = _IsMatchAt(position2.Coordinates(), out (NormalGemPosition origin, List<NormalGemPosition> matchedGem) match2);
            
            if (isMatchAtPosition1 || isMatchAtPosition2)
            {
                _ = gem1.Swap(position2.Transform().position, () =>
                {
                    position2.CompleteReceivedGem();
                    if (isMatchAtPosition1)
                    {
                        _MatchHandler(match1);
                    }

                    if (isMatchAtPosition2)
                    {
                        _MatchHandler(match2);
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
                
                _ = gem1.SwapAndSwapBack(position2.Transform().position, () =>
                {
                    position2.ChangePositionState(EPositionState.Free);
                });
                
                _ = gem2.SwapAndSwapBack(position1.Transform().position, () =>
                {
                    position1.ChangePositionState(EPositionState.Free);
                });
            }
            
            _currentPosition = null;
        }

        private NormalGemPosition _GetSwipePosition(Coordinates coordinates, ESwipeDirection direction)
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
            
            return _gemPositions[coordinates.x, coordinates.y];
        }
        
        private bool _IsMatchAt(Coordinates coordinates, out (NormalGemPosition origin, List<NormalGemPosition> matchedGem) match,
            bool predict = false)
        {
            int x = coordinates.x;
            int y = coordinates.y;
            
            match = new ()
            {
                matchedGem = new()
            };
            
            if (!_IsInBounds(x, y))
            {
                return false;
            }

            NormalGemPosition center = _gemPositions[x, y];
            
            match.origin = center;
            if (center.IsAvailable())
            {
                return false;
            }

            EGemType target = center.CurrentGem.GemType();

            List<NormalGemPosition> horizontal = new (){ center };

            int check = 0;
            
            // left
            check = x - 1;
            while (_IsInBounds(check, y) && _IsTheSame(check, y, target, predict))
            {
                horizontal.Add(_gemPositions[check, y]);
                check--;
            }

            // right
            check = x + 1;
            while (_IsInBounds(check, y) && _IsTheSame(check, y, target, predict))
            {
                horizontal.Add(_gemPositions[check, y]);
                check++;
            }

            // Nếu ngang >= 3 -> thêm vào result
            if (horizontal.Count >= 3)
            {
                Debug.LogError("-----------------------");
                horizontal.ForEach(z =>
                {
                    Debug.Log(z.Coordinates() + " " + z.CurrentGem.GemType());
                });
                match.matchedGem.AddRange(horizontal);
            }
            
            List<NormalGemPosition> vertical = new() { center };
            //down
            check = y - 1;
            while (_IsInBounds(x, check) && _IsTheSame(x, check, target, predict))
            {
                vertical.Add(_gemPositions[x, check]);
                check--;
            }

            //up
            check = y + 1;
            while (_IsInBounds(x, check) && _IsTheSame(x, check, target, predict))
            {
                vertical.Add(_gemPositions[x, check]);
                check++;
            }

            if (vertical.Count >= 3)
            {
                Debug.LogError("-----------------------");
                vertical.ForEach(z =>
                {
                    Debug.Log(z.Coordinates() + " " + (z as NormalGemPosition).CurrentGem.GemType());
                });
                match.matchedGem.AddRange(vertical);
            }

            match.matchedGem = match.matchedGem.Distinct().ToList();

            return match.matchedGem.Count >= 3;
        }

        private bool _IsMatchAt(Coordinates coordinates, bool predict = false)
        {
            return _IsMatchAt(coordinates, out (NormalGemPosition origin, List<NormalGemPosition> matchedGem) _, predict);
        }

        private void _MatchHandler((NormalGemPosition origin, List<NormalGemPosition> matchedGem) match)
        {
            foreach (var gemPosition in match.matchedGem)
            {
                gemPosition.CrushGem();
            }
        }

        private bool _IsTheSame(int x, int y, EGemType gemType, bool predict = false)
        {
            NormalGemPosition target = _gemPositions[x, y];
            
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
            
            return target.CurrentGem.GemType() == gemType;
        }
    }
}