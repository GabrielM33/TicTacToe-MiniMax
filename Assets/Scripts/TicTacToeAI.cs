using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState{none, cross, circle, tie}

[System.Serializable] public class WinnerEvent : UnityEvent<int>
{

}
	
public class TicTacToeAI : MonoBehaviour
{
	int _aiLevel;
	TicTacToeState[,] boardState;
	[SerializeField] private bool _isPlayerTurn;
	[SerializeField] private int _gridSize = 3;	
	[SerializeField] private TicTacToeState playerState = TicTacToeState.cross;
	TicTacToeState aiState = TicTacToeState.circle;
	[SerializeField] private GameObject _xPrefab;
	[SerializeField] private GameObject _oPrefab;
	public UnityEvent onGameStarted;
	public WinnerEvent onPlayerWin;
	private bool _isAITurn = false;
	private bool _isPlayerStartsNext;
	public UnityEvent onGameTied;

	ClickTrigger[,] _triggers;
	
	private void Awake()
	{
		if(onPlayerWin == null)
		{
			onPlayerWin = new WinnerEvent();
		}

		if(onGameTied == null)
		{
			onGameTied = new UnityEvent();
		}

		_isPlayerStartsNext = UnityEngine.Random.value > 0.5f;
    	StartGame();
	}

	public void StartAI(int AILevel)
	{
		_aiLevel = AILevel;
		aiState = playerState == TicTacToeState.cross ? TicTacToeState.circle : TicTacToeState.cross;
		_isPlayerStartsNext = _aiLevel % 2 == 0;
		StartGame();
	}


	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	private void StartGame()
	{
		_triggers = new ClickTrigger[_gridSize,_gridSize];
		boardState = new TicTacToeState[_gridSize,_gridSize];
		for (int i = 0; i < _gridSize; i++) 
		{
			for (int j = 0; j < _gridSize; j++) 
			{
				boardState[i, j] = TicTacToeState.none;
			}
		}
    	onGameStarted.Invoke();

		if (_isPlayerStartsNext)
		{
			// Player's turn
			_isAITurn = false;
		}
		else
		{
			// AI's turn
			_isAITurn = true;
			Invoke("AiTurn", 1.5f);

			// Switch the starting player for the next game
			_isPlayerStartsNext = !_isPlayerStartsNext;
		}
	}

	public void PlayerSelects(int coordX, int coordY)
	{
		// If it's the AI's turn, don't allow the player to make a move
		if (_isAITurn) return;

		boardState[coordX, coordY] = playerState;
		SetVisual(coordX, coordY, playerState);
		TicTacToeState result = CheckWinner();
		if (result != TicTacToeState.none)
		{
			// Get a Win or tie condition
			EndGame(result);
		}
		else
		{
			// It's now the AI's turn
			_isAITurn = true;
			// Ai waits 1.5 seconds before making a move
			Invoke("AiTurn", 1.5f);
		}
	}

	public void AiSelects(int coordX, int coordY)
	{
		boardState[coordX, coordY] = aiState;
		SetVisual(coordX, coordY, aiState);
		TicTacToeState winner = CheckWinner();
		if (winner != TicTacToeState.none)
		{
			//Win or tie condition
			EndGame(winner);
		}
		else
		{
			// Player's turn
			_isAITurn = false;
		}
	}

	void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
	}

	int Minimax(TicTacToeState[,] boardState, int depth, bool isMaximizingPlayer)
		{
			TicTacToeState winner = CheckWinner();
			if (winner != TicTacToeState.none)
			{
				return winner == aiState ? 1 : winner == playerState ? -1 : 0;
			}

			if (isMaximizingPlayer)
			{
				int maxEval = int.MinValue;
				for (int i = 0; i < _gridSize; i++)
				{
					for (int j = 0; j < _gridSize; j++)
					{
						if (boardState[i, j] == TicTacToeState.none)
						{
							boardState[i, j] = aiState;
							int eval = Minimax(boardState, depth + 1, false);
							boardState[i, j] = TicTacToeState.none;
							maxEval = Math.Max(maxEval, eval);
						}
					}
				}
				return maxEval;
			}
			else
			{
				int minEval = int.MaxValue;
				for (int i = 0; i < _gridSize; i++)
				{
					for (int j = 0; j < _gridSize; j++)
					{
						if (boardState[i, j] == TicTacToeState.none)
						{
							boardState[i, j] = playerState;
							int eval = Minimax(boardState, depth + 1, true);
							boardState[i, j] = TicTacToeState.none;
							minEval = Math.Min(minEval, eval);
						}
					}
				}
				return minEval;
			}
		}

	void AiTurn() 
	{
		int bestScore = int.MinValue;
		int moveX = -1;
		int moveY = -1;
		for (int i = 0; i < _gridSize; i++) 
		{
			for (int j = 0; j < _gridSize; j++) 
			{
				// is the spot available?
				if (boardState[i, j] == TicTacToeState.none) 
				{
					boardState[i, j] = aiState;
					int score = Minimax(boardState, 0, false);
					boardState[i, j] = TicTacToeState.none;
					if (score > bestScore) 
					{
						bestScore = score;
						moveX = i;
						moveY = j;
					}
				}
			}
		}
		// AI makes the move
		if(moveX != -1 && moveY != -1) 
		{
			AiSelects(moveX, moveY);
		}
	}

	TicTacToeState CheckWinner()
	{
		for (int i = 0; i < 3; i++)
		{
			// Rows
			if (boardState[i, 0] != TicTacToeState.none &&
				boardState[i, 0] == boardState[i, 1] && boardState[i, 0] == boardState[i, 2])
			{
				return boardState[i, 0];
			}

			// Columns
			if (boardState[0, i] != TicTacToeState.none &&
				boardState[0, i] == boardState[1, i] && boardState[0, i] == boardState[2, i])
			{
				return boardState[0, i];
			}
		}

		// Diagonals
		if (boardState[0, 0] != TicTacToeState.none &&
			boardState[0, 0] == boardState[1, 1] && boardState[0, 0] == boardState[2, 2])
		{
			return boardState[0, 0];
		}

		if (boardState[0, 2] != TicTacToeState.none &&
			boardState[0, 2] == boardState[1, 1] && boardState[0, 2] == boardState[2, 0])
		{
			return boardState[0, 2];
		}

		// Check for a tie
		for (int i = 0; i < _gridSize; i++)
		{
			for (int j = 0; j < _gridSize; j++)
			{
				if (boardState[i, j] == TicTacToeState.none)
				{
					// If any cell is still in the 'none' state, the game is not a tie
					return TicTacToeState.none;
				}
			}
		}
    	return TicTacToeState.tie;
	}

	 void EndGame(TicTacToeState result)
	{
		 if (result == TicTacToeState.tie)
		{
			// Tie condition
			onPlayerWin.Invoke(-1);
		}
		else if (result == playerState)
		{
			// Player win condition
			onPlayerWin.Invoke(2);
		}
		else if (result == aiState)
		{
			// AI win condition
			onPlayerWin.Invoke(1);
		}
	}

	bool GameOver(TicTacToeState[,] boardState)
	{
		// Check for a winner
		TicTacToeState winner = CheckWinner();
		if (winner != TicTacToeState.none)
		{
			return true;
		}

		// Check if there are any empty spaces left
		for (int i = 0; i < _gridSize; i++)
		{
			for (int j = 0; j < _gridSize; j++)
			{
				if (boardState[i, j] == TicTacToeState.none)
				{
					return false;
				}
			}
		}
		// If there's no winner and no empty spaces, it's a tie
		return true;
	}
}