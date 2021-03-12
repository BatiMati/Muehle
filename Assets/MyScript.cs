using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class MyScript : ImmediateModeShapeDrawer
{
    List<PolylinePath> pPath = new List<PolylinePath>();
    PolylinePath _p1;
    PolylinePath _p2;
    PolylinePath _p3;
    PolylinePath _p4;
    PolylinePath _p5;
    PolylinePath _p6;
    PolylinePath _p7;

    private List<Vector3> playPositions = new List<Vector3>();

    private List<Vector3> _whitePositions = new List<Vector3>();
    private List<Vector3> _blackPositions = new List<Vector3>();

    
    public List<Vector3> lockedWhitePos = new List<Vector3>();
    public List<Vector3> lockedBlackPos = new List<Vector3>();
    

    public int blackStones=9;
    public int whiteStones=9;
    
    

    private Vector3 _currentMousePos;
    private bool _currentPosSet=false;

    private Vector3 _currentSelectedPos;
    private bool _samePosition=false;

    private bool _currentPlayerBlack = false;

    private bool _pawnSetupReady = false;
    private bool _pawnToRemove = false;

    private int[] _line0 = new int[]{-3,0,3};
    private int[] _line1 = new int[]{-2,0,2};
    private int[] _line2 = new int[]{-1,0,1};
    private int[] _line3 = new int[]{-3,-2,-1,1,2,3};
    
    private Color _circleColor = Color.red;

    private Vector3 _currentSelectedPawn;
    private bool _firstPosSelected=false;
    private bool _secondPosSelected=false;

    public int whiteInPlay = 0;
    public int blackInPlay = 0;

    public float epsilon = 0.5f;

    public TextMeshPro phaseText;
    public TextMeshPro infoText;

    public int noLock = 0;
    private const int LockingNumber = 30;

    public bool gameOver = false;
    
    
    private void Awake()
    {
        CreatePlayfield();
        CreatePoints();
        
        phaseText.text = "Setup-Phase";
        
        
    }

    private Vector3 GetMousePosition()
    {
        //Gets the mouseclick on the display converts to only int values and clickable positions
      var mousePos = Input.mousePosition;
      mousePos.z = 10.3f;
        _currentMousePos=  Camera.main.ScreenToWorldPoint(mousePos);
        _currentMousePos.x = (int) _currentMousePos.x;
        _currentMousePos.y = (int) _currentMousePos.y;

        if (( Math.Abs(_currentMousePos.y)==3 & _line0.Contains((int)_currentMousePos.x))^
            (Math.Abs(_currentMousePos.y)==2 & _line1.Contains((int)_currentMousePos.x))^
            (Math.Abs(_currentMousePos.y)==1 & _line2.Contains((int)_currentMousePos.x))^
            (_currentMousePos.y ==0 &_line3.Contains((int)_currentMousePos.x)))
        {
            _currentPosSet = true;
            
        }
        else
        {
            _currentPosSet = false;
            
        }
        return new Vector3((int) _currentMousePos.x, (int) _currentMousePos.y, 0);
    }

    private void InPlay()
    {
        //Checks the Number of Stones in Play for each player to recognise, when game is over
        whiteInPlay = _whitePositions.Count;
        blackInPlay = _blackPositions.Count;

        if (_pawnSetupReady & whiteInPlay <3)
        {
            Debug.Log("BlackWon");
            infoText.text = "Game Over. Black Won";
        }

        if (_pawnSetupReady & blackInPlay <3)
        {
            Debug.Log("WhiteWon");
            infoText.text = "Game Over. White Won";
        }
    }

    private void ChangeCurrentPlayer()
    {
        //Changes the player that has to make an action 
        if (!_currentPlayerBlack)
        {
            _currentPlayerBlack = true;
        }
        else
        {
            _currentPlayerBlack = false;
        }

        
        if (_pawnSetupReady)
        {
            //Count to recognise tie when LockingNumber moves without 3 in a row
            noLock += 1;
            
            //Game Over, when a player has no possible moves left

            if (NoMovesPossible() & !_currentPlayerBlack)
            {
                infoText.text = "White Has no possible Moves. Black Won";
                gameOver = true;
            }
            else if (NoMovesPossible() & _currentPlayerBlack)
            {
                infoText.text = "Black has no possible moves. White Won";
                gameOver = true;
            }
        }
    }

    //Setup Phase placement of the stones
    private void PlacePawn()
    {
        
        var tempPos = GetMousePosition();
        //z set to zero, that all points are on one plane because the shapes need vector 3
        tempPos.z = 0; 
        if (_currentPosSet & !CheckForPawn(tempPos) & !_pawnSetupReady & !_pawnToRemove)
        {
            if (!_currentPlayerBlack)
            {
                //saving position in List for whites stones & reducing number of stones left to place
                _whitePositions.Add(tempPos);
                whiteStones -= 1;
                
                //Checks for 3 stones in a row before changing the current player
                if (!CheckLockPos(tempPos))
                {
                    ChangeCurrentPlayer();
                }
                else
                {
                    Debug.Log("White can Remove one black stone");
                    infoText.text = "Three in a row. Choose Opponent Stone to remove";
                    _pawnToRemove = true;
                    
                    _samePosition = false;
                }
                
            }
            else
            {
                //saving position in List for black stones & reducing number of stones left to place
                _blackPositions.Add((tempPos));
                blackStones -= 1;
                
                //Checks for 3 stones in a row before changing the current player
                if (!CheckLockPos(tempPos))
                {
                    ChangeCurrentPlayer();
                }
                else
                {
                    Debug.Log("Black can remove one white stone");
                    infoText.text = "Three in a row. Choose Opponent Stone to remove";
                    _pawnToRemove = true;
                    _samePosition = false;

                }

                
            }
            //Checks if all stones have been placed
            CheckGameSetupDone();
            
        }
        

    }

    private void RemovePawn()
    {
        //resets the number for moves without 3 in a row
        noLock = 0;
        var tempPos = GetMousePosition();
        tempPos.z = 0;
        if (_currentPosSet & _pawnToRemove )
        {
            if (!_currentPlayerBlack)
            {
                //Checks if stone that is selected to be removed is not Locked because of 3 in a row
                if (!lockedBlackPos.Contains(tempPos))
                {
                    
                    _blackPositions.RemoveAt(_blackPositions.IndexOf(tempPos));
                    _pawnToRemove = false;
                    _circleColor =Color.red;
                    ChangeCurrentPlayer();
                }

                //Allows removing of stones in Locked position, when there is no other choice
                else if (blackInPlay == lockedBlackPos.Count)
                {
                    _blackPositions.RemoveAt(_blackPositions.IndexOf(tempPos));
                    
                    if (lockedBlackPos.IndexOf(tempPos)<3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            lockedBlackPos.RemoveAt(0);
                        }
                    }
                    if (lockedBlackPos.IndexOf(tempPos)>=3 & lockedBlackPos.IndexOf(tempPos)<6)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            lockedBlackPos.RemoveAt(3);
                        }
                    }
            
                    if (lockedBlackPos.IndexOf(tempPos)>=6 & lockedBlackPos.IndexOf(tempPos)<9)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            lockedBlackPos.RemoveAt(6);
                        }
                    }
                    _pawnToRemove = false;
                    _circleColor =Color.red;
                    ChangeCurrentPlayer();
                }
            }
            else
            {
                if (!lockedWhitePos.Contains(tempPos))
                {
                    _whitePositions.RemoveAt(_whitePositions.IndexOf(tempPos));
                    _pawnToRemove = false;
                    _circleColor = Color.red;
                    ChangeCurrentPlayer();
                }

                else if (whiteInPlay == lockedWhitePos.Count)
                {
                    _whitePositions.RemoveAt(_whitePositions.IndexOf(tempPos));
                    
                    if (lockedWhitePos.IndexOf(tempPos)<3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            lockedWhitePos.RemoveAt(0);
                        }
                    }
                    if (lockedWhitePos.IndexOf(tempPos)>=3 & lockedWhitePos.IndexOf(tempPos)<6)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            lockedWhitePos.RemoveAt(3);
                        }
                    }
            
                    if (lockedWhitePos.IndexOf(tempPos)>=6 & lockedWhitePos.IndexOf(tempPos)<9)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            lockedWhitePos.RemoveAt(6);
                        }
                    }
                    _pawnToRemove = false;
                    _circleColor =Color.red;
                    ChangeCurrentPlayer();
                }
            }
        }

        infoText.text = "";
        if (_pawnSetupReady)
        {
            infoText.text = "Select Stone to Move";
        }

    }

    private void MovePawn()
    {
        var posToCheckAgainst =_whitePositions;
        if (!_currentPlayerBlack)
        {
             posToCheckAgainst = _whitePositions;
        }
        else
        {
             posToCheckAgainst = _blackPositions;
        }
        if (!_samePosition)
        {
            _currentSelectedPos = GetMousePosition();
            _samePosition = true;
        }
        else
        {
            var tempPos = GetMousePosition();
            tempPos.z = 0;
            if (_currentSelectedPos == tempPos)
            {
                if (_firstPosSelected)
                {
                    if (CanMoveTo(tempPos,_currentSelectedPawn) )
                    {
                        Debug.Log("ja");
                        
                        RepositionPawn(tempPos);
                        _secondPosSelected = true;
                    }

                    if (!CanMoveTo(tempPos,_currentSelectedPawn))
                    {
                        Debug.Log("nein");
                        
                    }

                }
                if (posToCheckAgainst.Contains(tempPos) & !_firstPosSelected & !_secondPosSelected )
                {
                    _currentSelectedPawn = tempPos;
                    _firstPosSelected = true;
                    Debug.Log("Please select position to move to");
                }
                
                if (_pawnToRemove)
                {
                    infoText.text = "Three in a row. Choose Opponent Stone to remove";
                    _circleColor = Color.blue;
                    RemovePawn();
                    
                }

                
                
            }
            
            _samePosition = false;
            _secondPosSelected = false;
        }


    }

    private void RepositionPawn(Vector3 position)
    {
        if (!_currentPlayerBlack)
        {
            //Removes moved Pawn and the other 2 corresponding stones from the Locked List
            if (lockedWhitePos.Contains(_currentSelectedPawn))
            {
                if (lockedWhitePos.IndexOf(_currentSelectedPawn)<3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        lockedWhitePos.RemoveAt(0);
                    }
                }

                if (lockedWhitePos.IndexOf(_currentSelectedPawn)>=3 & lockedWhitePos.IndexOf(_currentSelectedPawn)<6)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        lockedWhitePos.RemoveAt(3);
                    }
                }
            
                if (lockedWhitePos.IndexOf(_currentSelectedPawn)>=6 & lockedWhitePos.IndexOf(_currentSelectedPawn)<9)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        lockedWhitePos.RemoveAt(6);
                    }
                }
            }
            

            
            _whitePositions.RemoveAt(_whitePositions.IndexOf(_currentSelectedPawn));
            _whitePositions.Add(position);
            if (!CheckLockPos(position))
            {
                ChangeCurrentPlayer();
            }
            else
            {
                _pawnToRemove = true;
            }
        }
        else
        {
            //Removes moved Pawn and the other 2 corresponding stones from the Locked List
            if (lockedBlackPos.Contains(_currentSelectedPawn))
            {
                if (lockedBlackPos.IndexOf(_currentSelectedPawn)<3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        lockedBlackPos.RemoveAt(0);
                    }
                    
                    
                }

                if (lockedBlackPos.IndexOf(_currentSelectedPawn)>=3 & lockedBlackPos.IndexOf(_currentSelectedPawn)<6)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        lockedBlackPos.RemoveAt(3);
                    }

                }
            
                if (lockedBlackPos.IndexOf(_currentSelectedPawn)>=6 & lockedBlackPos.IndexOf(_currentSelectedPawn)<9)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        lockedBlackPos.RemoveAt(6);
                    }

                }
            }
            _blackPositions.RemoveAt(_blackPositions.IndexOf(_currentSelectedPawn));
            _blackPositions.Add(position);
            if (!CheckLockPos(position))
            {
                ChangeCurrentPlayer();
            }
            else
            {
                _pawnToRemove = true;
            }
        }

        _firstPosSelected = false;
        
        
    }

    private bool CanMoveTo(Vector3 position, Vector3 currentPawn)
    {
        //Checks if a position is blocked with another Stone
        if (_whitePositions.Contains(position)^ _blackPositions.Contains(position) )
        {
            return false;
        }

        //Checks the currentPawn position with the position the player wants the pawn to move to, if it is allowed
        // when player has only 3 stones it returns true, because jumping is now allowed
        if ((!_currentPlayerBlack & whiteInPlay>3)^(_currentPlayerBlack & blackInPlay >3))
        {
            if ((currentPawn - playPositions[0]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[1]).sqrMagnitude < epsilon ^ (position - playPositions[9]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[1]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[0]).sqrMagnitude < epsilon ^ (position - playPositions[2]).sqrMagnitude < epsilon ^ (position - playPositions[4]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[2]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[1]).sqrMagnitude < epsilon ^ (position - playPositions[14]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[3]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[4]).sqrMagnitude < epsilon ^ (position - playPositions[10]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[4]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[1]).sqrMagnitude < epsilon ^ (position - playPositions[3]).sqrMagnitude < epsilon ^ (position - playPositions[5]).sqrMagnitude < epsilon ^ (position - playPositions[7]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[5]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[4]).sqrMagnitude < epsilon ^ (position - playPositions[13]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[6]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[7]).sqrMagnitude < epsilon ^ (position - playPositions[11]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[7]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[4]).sqrMagnitude < epsilon ^ (position - playPositions[6]).sqrMagnitude < epsilon ^ (position - playPositions[8]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[8]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[7]).sqrMagnitude < epsilon ^ (position - playPositions[12]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[9]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[0]).sqrMagnitude < epsilon ^ (position - playPositions[10]).sqrMagnitude < epsilon ^ (position - playPositions[21]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[10]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[3]).sqrMagnitude < epsilon ^ (position - playPositions[9]).sqrMagnitude < epsilon ^ (position - playPositions[11]).sqrMagnitude < epsilon ^ (position - playPositions[18]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[11]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[6]).sqrMagnitude < epsilon ^ (position - playPositions[10]).sqrMagnitude < epsilon ^ (position - playPositions[15]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[12]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[8]).sqrMagnitude < epsilon ^ (position - playPositions[13]).sqrMagnitude < epsilon ^ (position - playPositions[17]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[13]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[5]).sqrMagnitude < epsilon ^ (position - playPositions[12]).sqrMagnitude < epsilon ^ (position - playPositions[14]).sqrMagnitude < epsilon ^ (position - playPositions[20]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[14]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[2]).sqrMagnitude < epsilon ^ (position - playPositions[13]).sqrMagnitude < epsilon ^ (position - playPositions[23]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[15]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[11]).sqrMagnitude < epsilon ^ (position - playPositions[16]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[16]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[15]).sqrMagnitude < epsilon ^ (position - playPositions[17]).sqrMagnitude < epsilon ^ (position - playPositions[19]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[17]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[12]).sqrMagnitude < epsilon ^ (position - playPositions[16]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[18]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[10]).sqrMagnitude < epsilon ^ (position - playPositions[19]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[19]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[16]).sqrMagnitude < epsilon ^ (position - playPositions[18]).sqrMagnitude < epsilon ^ (position - playPositions[20]).sqrMagnitude < epsilon ^ (position - playPositions[22]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[20]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[13]).sqrMagnitude < epsilon ^ (position - playPositions[19]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[21]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[9]).sqrMagnitude < epsilon ^ (position - playPositions[22]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[22]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[19]).sqrMagnitude < epsilon ^ (position - playPositions[21]).sqrMagnitude < epsilon ^ (position - playPositions[23]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
            if ((currentPawn - playPositions[23]).sqrMagnitude < epsilon )
            {
                if ((position - playPositions[14]).sqrMagnitude < epsilon ^ (position - playPositions[22]).sqrMagnitude < epsilon)
                {
                    return true;
                }
            }
        }
        else
        {
            return true;
        }
        return false;
    }

    //Checks the neighbours of a position in all directions if a player sets 3 stones in a row
    private bool CheckLockPos(Vector3 position)
    { 
        var tempPos = position;
       var currentPlayerPos = new List<Vector3>();
       var currentLockedPos = new List<Vector3>();
       if (!_currentPlayerBlack)
       {
           currentPlayerPos = _whitePositions;
           currentLockedPos = lockedWhitePos;
           
       }
       else
       {
           currentPlayerPos = _blackPositions;
           currentLockedPos = lockedBlackPos;
           
           
       }

       switch (tempPos.x)
       {
           //left half
           case -3:
           {
               if (tempPos.y ==3)
               {
                   if (currentPlayerPos.Contains(playPositions[1]) && currentPlayerPos.Contains(playPositions[2]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[1]);
                       currentLockedPos.Add(playPositions[2]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }


                   if (currentPlayerPos.Contains(playPositions[9]) && currentPlayerPos.Contains(playPositions[21]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[9]);
                       currentLockedPos.Add(playPositions[21]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
                   
               
               }
               if (tempPos.y ==0)
               {
               
                   if (currentPlayerPos.Contains(playPositions[0]) && currentPlayerPos.Contains(playPositions[21]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[0]);
                       currentLockedPos.Add(playPositions[21]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[10]) && currentPlayerPos.Contains(playPositions[11]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[10]);
                       currentLockedPos.Add(playPositions[11]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }

               }
               if (tempPos.y ==-3)
               {
                   if (currentPlayerPos.Contains(playPositions[0]) && currentPlayerPos.Contains(playPositions[9]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[0]);
                       currentLockedPos.Add(playPositions[9]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[22]) && currentPlayerPos.Contains(playPositions[23]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[22]);
                       currentLockedPos.Add(playPositions[23]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }

               }

               break;
           }
           case -2:
           {
               if (tempPos.y == 2)
               {
                   if (currentPlayerPos.Contains(playPositions[4]) && currentPlayerPos.Contains(playPositions[5]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[4]);
                       currentLockedPos.Add(playPositions[5]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[10]) && currentPlayerPos.Contains(playPositions[18]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[10]);
                       currentLockedPos.Add(playPositions[18]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
               }
               if (tempPos.y == 0)
               {
               
                   if (currentPlayerPos.Contains(playPositions[3]) && currentPlayerPos.Contains(playPositions[18]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[3]);
                       currentLockedPos.Add(playPositions[18]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[9]) && currentPlayerPos.Contains(playPositions[11]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[9]);
                       currentLockedPos.Add(playPositions[11]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
               }
               if (tempPos.y == -2)
               {
                   if (currentPlayerPos.Contains(playPositions[3]) && currentPlayerPos.Contains(playPositions[10]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[3]);
                       currentLockedPos.Add(playPositions[10]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[19]) && currentPlayerPos.Contains(playPositions[20]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[19]);
                       currentLockedPos.Add(playPositions[20]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
               }

               break;
           }
           case -1:
           {
               if (tempPos.y == 1)
               {
                   if (currentPlayerPos.Contains(playPositions[7]) && currentPlayerPos.Contains(playPositions[8]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[7]);
                       currentLockedPos.Add(playPositions[8]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[11]) && currentPlayerPos.Contains(playPositions[15]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[11]);
                       currentLockedPos.Add(playPositions[15]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
                
               }
               if (tempPos.y == 0)
               {
                   if (currentPlayerPos.Contains(playPositions[6]) && currentPlayerPos.Contains(playPositions[15]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[6]);
                       currentLockedPos.Add(playPositions[15]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[9]) && currentPlayerPos.Contains(playPositions[10]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[9]);
                       currentLockedPos.Add(playPositions[10]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
               }
               if (tempPos.y == -1)
               {
                   if (currentPlayerPos.Contains(playPositions[6]) && currentPlayerPos.Contains(playPositions[11]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[6]);
                       currentLockedPos.Add(playPositions[11]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[16]) && currentPlayerPos.Contains(playPositions[17]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[16]);
                       currentLockedPos.Add(playPositions[17]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
               }

               break;
           }
           //middle
           case 0:
           {
               //top
               if (tempPos.y == 3)
               {
                   if (currentPlayerPos.Contains(playPositions[0]) && currentPlayerPos.Contains(playPositions[2]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[0]);
                       currentLockedPos.Add(playPositions[2]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[4]) && currentPlayerPos.Contains(playPositions[7]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[4]);
                       currentLockedPos.Add(playPositions[7]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
              
               }
               if (tempPos.y == 2)
               {
                   if (currentPlayerPos.Contains(playPositions[1]) && currentPlayerPos.Contains(playPositions[7]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[1]);
                       currentLockedPos.Add(playPositions[7]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[3]) && currentPlayerPos.Contains(playPositions[5]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[3]);
                       currentLockedPos.Add(playPositions[5]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
               }
               if (tempPos.y == 1)
               {
                   if (currentPlayerPos.Contains(playPositions[1]) && currentPlayerPos.Contains(playPositions[4]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[1]);
                       currentLockedPos.Add(playPositions[4]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[6]) && currentPlayerPos.Contains(playPositions[8]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[6]);
                       currentLockedPos.Add(playPositions[8]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }
               //bottom
               if (tempPos.y == -1)
               {
                   if (currentPlayerPos.Contains(playPositions[15]) && currentPlayerPos.Contains(playPositions[17]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[15]);
                       currentLockedPos.Add(playPositions[17]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[19]) && currentPlayerPos.Contains(playPositions[22]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[19]);
                       currentLockedPos.Add(playPositions[22]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
               }

               if (tempPos.y == -2)
               {
                   if (currentPlayerPos.Contains(playPositions[16]) && currentPlayerPos.Contains(playPositions[22]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[16]);
                       currentLockedPos.Add(playPositions[22]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[18]) && currentPlayerPos.Contains(playPositions[20]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[18]);
                       currentLockedPos.Add(playPositions[20]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }

               if (tempPos.y == -3)
               {
                   if (currentPlayerPos.Contains(playPositions[16]) && currentPlayerPos.Contains(playPositions[19]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[16]);
                       currentLockedPos.Add(playPositions[19]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[21]) && currentPlayerPos.Contains(playPositions[23]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[21]);
                       currentLockedPos.Add(playPositions[23]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }

               break;
           }
           //right half
           case 1:
           {
               if (tempPos.y == 1)
               {
                   if (currentPlayerPos.Contains(playPositions[6]) && currentPlayerPos.Contains(playPositions[7]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[6]);
                       currentLockedPos.Add(playPositions[7]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[12]) && currentPlayerPos.Contains(playPositions[17]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[12]);
                       currentLockedPos.Add(playPositions[17]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }
               if (tempPos.y == 0)
               {
                   if (currentPlayerPos.Contains(playPositions[8]) && currentPlayerPos.Contains(playPositions[17]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[8]);
                       currentLockedPos.Add(playPositions[17]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[13]) && currentPlayerPos.Contains(playPositions[14]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[13]);
                       currentLockedPos.Add(playPositions[14]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }
               if (tempPos.y == -1)
               {
                   if (currentPlayerPos.Contains(playPositions[8]) && currentPlayerPos.Contains(playPositions[12]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[8]);
                       currentLockedPos.Add(playPositions[12]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[15]) && currentPlayerPos.Contains(playPositions[16]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[15]);
                       currentLockedPos.Add(playPositions[16]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }

               break;
           }
           case 2:
           {
               if (tempPos.y == 2)
               {
                   if (currentPlayerPos.Contains(playPositions[3]) && currentPlayerPos.Contains(playPositions[4]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[3]);
                       currentLockedPos.Add(playPositions[4]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[13]) && currentPlayerPos.Contains(playPositions[20]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[13]);
                       currentLockedPos.Add(playPositions[20]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }
               if (tempPos.y == 0)
               {
                   if (currentPlayerPos.Contains(playPositions[5]) && currentPlayerPos.Contains(playPositions[20]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[5]);
                       currentLockedPos.Add(playPositions[20]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[12]) && currentPlayerPos.Contains(playPositions[14]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[12]);
                       currentLockedPos.Add(playPositions[14]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }
               if (tempPos.y == -2)
               {
                   if (currentPlayerPos.Contains(playPositions[5]) && currentPlayerPos.Contains(playPositions[13]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[5]);
                       currentLockedPos.Add(playPositions[13]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[18]) && currentPlayerPos.Contains(playPositions[19]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[18]);
                       currentLockedPos.Add(playPositions[19]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }

               break;
           }
           case 3:
           {
               if (tempPos.y ==3)
               {
                   if (currentPlayerPos.Contains(playPositions[0]) && currentPlayerPos.Contains(playPositions[1]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[0]);
                       currentLockedPos.Add(playPositions[1]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[14]) && currentPlayerPos.Contains(playPositions[23]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[14]);
                       currentLockedPos.Add(playPositions[23]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
               }
               if (tempPos.y ==0)
               {
                   if (currentPlayerPos.Contains(playPositions[2]) && currentPlayerPos.Contains(playPositions[23]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[2]);
                       currentLockedPos.Add(playPositions[23]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[12]) && currentPlayerPos.Contains(playPositions[13]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[12]);
                       currentLockedPos.Add(playPositions[13]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }
               if (tempPos.y ==-3)
               {
                   if (currentPlayerPos.Contains(playPositions[2]) && currentPlayerPos.Contains(playPositions[14]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[2]);
                       currentLockedPos.Add(playPositions[14]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               
                   if (currentPlayerPos.Contains(playPositions[21]) && currentPlayerPos.Contains(playPositions[22]))
                   {
                       currentLockedPos.Add(tempPos);
                       currentLockedPos.Add(playPositions[21]);
                       currentLockedPos.Add(playPositions[22]);
                       //Lock all three stones for current player to list and call function that let's player remove stone from  opponent
                       return true;
                   }
               }

               break;
           }
       }

       return false;
    }

    private void SelectPosition()
    {
        if (!_samePosition)
        {
            _currentSelectedPos = GetMousePosition();
            _samePosition = true;
        }
        else
        {
            var tempPos = GetMousePosition();
            
            if (_currentSelectedPos == tempPos)
            {
                
                if (!_pawnToRemove & !_pawnSetupReady)
                {
                    PlacePawn();
                    InPlay();
                }
                
                if (_pawnToRemove)
                {
                    _circleColor = Color.blue;
                    RemovePawn();
                    InPlay();
                }

                CheckGameSetupDone();

                
                
            }
            _samePosition = false;
            
        }

        
    }

    private void CheckGameSetupDone()
    {
        if (blackStones == 0 & whiteStones == 0 & !_pawnToRemove)
        {
            _pawnSetupReady = true;
            phaseText.text = "Moving-Phase";
            infoText.text = "Select Stone to Move";

        }
    }

    private bool CheckForPawn(Vector3 pos)
    {
        if (_whitePositions.Contains(pos)| _blackPositions.Contains(pos))
        {
            Debug.Log("Pos schon besetzt");
            return true;
        }
        else
        {
            return false; 
        }
              
    }

    //Checks if a player has no possible moves left
    private bool NoMovesPossible()
    {
        if (!_currentPlayerBlack)
        {
            var tempCount = 0;
            foreach (var pawnPosition in _whitePositions)
            {
                foreach (var fieldPosition in playPositions)
                {
                    if (!CanMoveTo(fieldPosition, pawnPosition))
                    {
                        tempCount++;
                        //Debug.Log("White: " + tempCount);
                    }

                    
                }
            }

            if (tempCount == 24 * _whitePositions.Count)
            {
                Debug.Log("White has no Possible Moves Left");
                return true;
            }
        }
        else
        {
            var tempCount = 0;
            foreach (var pawnPosition in _blackPositions)
            {
                foreach (var fieldPosition in playPositions)
                {
                    if (!CanMoveTo(fieldPosition, pawnPosition))
                    {
                        tempCount++;
                        //Debug.Log("Black: " +tempCount);
                    }
                    
                }
            }

            if (tempCount == 24 * _blackPositions.Count)
            {
                Debug.Log("Black has no Possible Moves Left");
                return true;
            }
        }

        return false;
    }

    #region PlayfieldSetup

    private void CreatePlayfield()
    {
        _p1 = new PolylinePath();
        _p1.AddPoint(-3,3);
        _p1.AddPoint(3,3);
        _p1.AddPoint(3,-3);
        _p1.AddPoint(-3,-3);
        _p1.AddPoint(-3,3);
        pPath.Add(_p1);

        _p2 = new PolylinePath();
        _p2.AddPoint(-2,2);
        _p2.AddPoint(2,2);
        _p2.AddPoint(2,-2);
        _p2.AddPoint(-2,-2);
        _p2.AddPoint(-2,2);
        pPath.Add(_p2);

        _p3 = new PolylinePath();
        _p3.AddPoint(-1,1);
        _p3.AddPoint(1,1);
        _p3.AddPoint(1,-1);
        _p3.AddPoint(-1,-1);
        _p3.AddPoint(-1,1);
        pPath.Add(_p3);

        _p4 = new PolylinePath();
        _p4.AddPoint(0,3);
        _p4.AddPoint(0,1);
        pPath.Add(_p4);

        _p5 = new PolylinePath();
        _p5.AddPoint(1,0);
        _p5.AddPoint(3,0);
        pPath.Add(_p5);

        _p6 = new PolylinePath();
        _p6.AddPoint(0,-1);
        _p6.AddPoint(0,-3);
        pPath.Add(_p6);

        _p7 = new PolylinePath();
        _p7.AddPoint(-1,0);
        _p7.AddPoint(-3,0);
        pPath.Add(_p7);
    }

    private void CreatePoints()
    {
        var temp = new Vector3();
       temp = new Vector3(-3, 3, 0);
       playPositions.Add(temp);
       
       temp = new Vector3(0, 3, 0);
       playPositions.Add(temp);

       temp = new Vector3(3, 3, 0);
       playPositions.Add(temp);

       temp = new Vector3(-2, 2, 0);
       playPositions.Add(temp);

       temp = new Vector3(0, 2, 0);
       playPositions.Add(temp);

       temp = new Vector3(2, 2, 0);
       playPositions.Add(temp);

       temp = new Vector3(-1, 1, 0);
       playPositions.Add(temp);

       temp = new Vector3(0, 1, 0);
       playPositions.Add(temp);

       temp = new Vector3(1, 1, 0);
       playPositions.Add(temp);

       temp = new Vector3(-3, 0, 0);
       playPositions.Add(temp);

       temp = new Vector3(-2, 0, 0);
       playPositions.Add(temp);

       temp = new Vector3(-1, 0, 0);
       playPositions.Add(temp);

       temp = new Vector3(1, 0, 0);
       playPositions.Add(temp);

       temp = new Vector3(2, 0, 0);
       playPositions.Add(temp);

       temp = new Vector3(3, 0, 0);
       playPositions.Add(temp);

       temp = new Vector3(-1, -1, 0);
       playPositions.Add(temp);

       temp = new Vector3(0, -1, 0);
       playPositions.Add(temp);

       temp = new Vector3(1, -1, 0);
       playPositions.Add(temp);

       temp = new Vector3(-2, -2, 0);
       playPositions.Add(temp);

       temp = new Vector3(0, -2, 0);
       playPositions.Add(temp);

       temp = new Vector3(2, -2, 0);
       playPositions.Add(temp);

       temp = new Vector3(-3, -3, 0);
       playPositions.Add(temp);

       temp = new Vector3(0, -3, 0);
       playPositions.Add(temp);

       temp = new Vector3(3, -3, 0);
       playPositions.Add(temp);

    }
    

    #endregion

    private void Update()
    {
        //Game Over when LockingNumber Moves without 3 in a row
        if (noLock == LockingNumber)
        {
            infoText.text = "Game Over. Tie";
            gameOver = true;
        }

        
        
        if (Input.GetButtonDown("Fire1") & !gameOver)
        {
            if (!_pawnSetupReady)
            {
                SelectPosition();
            }

            if (_pawnSetupReady)
            {
                MovePawn();
                InPlay();
            }
            
            
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }

    private void ResetGame()
    {
        blackStones = 9;
        whiteStones = 9;
        whiteInPlay = 0;
        blackInPlay = 0;

        _whitePositions = new List<Vector3>();
        _blackPositions = new List<Vector3>();
        lockedWhitePos = new List<Vector3>();
        lockedBlackPos = new List<Vector3>();

        _currentPlayerBlack = false;

        _currentMousePos = new Vector3();
        _currentSelectedPos = new Vector3();
        _currentSelectedPawn = new Vector3();

        _currentPosSet = false;
        _samePosition = false;

        _pawnSetupReady = false;

        _firstPosSelected = false;
        _secondPosSelected = false;

        noLock = 0;

        gameOver = false;

        phaseText.text = "Setup-Phase";
        infoText.text = "";
    }

    //Draw Commands for the Shapes
    public override void DrawShapes(Camera cam)
    {
        using (Draw.Command(cam))
        {
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.LineThicknessSpace = ThicknessSpace.Pixels;
            Draw.LineThickness = 4;

            #region PlayArea

            foreach (PolylinePath t in pPath)
            {
                Draw.Polyline(t, closed:false, thickness:2.5f, Color.grey);
            }

            foreach (var position in playPositions)
            {
                Draw.Disc(position,0.15f,Color.grey);
            }

            #endregion
            

            //Highlightcircle for last selected Position
            if (_currentPosSet)
            {
                var highlightCircle = _currentMousePos;
                highlightCircle.z = 0.0f;
                Draw.Ring(highlightCircle,0.2f,_circleColor);

            }
            
            //firstselectedpos Highlightcircle that indicates the selected stone that the player wants to move
            if (_firstPosSelected)
            {
                var highlightCircle = _currentSelectedPawn;
                Draw.Ring(highlightCircle,0.2f,Color.green);
            }
            //whiteStones
            foreach (var w in _whitePositions)
            {
                Draw.Disc(w,0.2f,Color.white);
            }
            
            //blackStones
            foreach (var b in _blackPositions)
            {
                Draw.Disc(b,0.2f,Color.black);
            }

            //visual representation of stones black can still place
            for (int i = 0; i < blackStones; i++)
            {
                var pos = new Vector3(-5, -3 + (0.5f * i),0);
                Draw.Disc(pos, 0.2f,Color.black );
            }
            
            //visual representation of stones white can still place
            for (int i = 0; i < whiteStones; i++)
            {
                var pos = new Vector3(5, -3 + (0.5f * i),0);
                Draw.Disc(pos, 0.2f,Color.white );
            }
            
            //Triangle Next to Current Player Text that indicates which players has to do something
            if (!_currentPlayerBlack)
            {
                Draw.Triangle(new Vector3(4,-4,0),new Vector3(3.5f,-4.3f,0),new Vector3(3.5f,-3.7f,0),Color.white);
            }
            else
            {
                Draw.Triangle(new Vector3(-4,-4,0),new Vector3(-3.5f,-4.3f,0),new Vector3(-3.5f,-3.7f,0),Color.black);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var p in pPath)
        {
            p.Dispose();
        }
    }
}
