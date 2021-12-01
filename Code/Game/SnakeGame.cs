
using System.Collections;

using UnityEngine;

namespace SnakeHealthMod.Game
{

	using Position = Vector2Int;

	internal class SnakeGame
	{
		public static Position GridSize = new( 10, 8 );

		private SnakeRenderer renderer;
		
		private Snake snake;

		private Fruit apple;

		public bool HasStarted { get; private set; }

		public bool HasEnded { get; private set; }

		public int Score { get; private set; }

		public Direction MoveDir { get; set; }

		public IEnumerator Setup( Transform owner )
		{
			yield return new WaitForSeconds( .2f );

			renderer = new( owner );

			yield break;
		}

		public void Start()
		{
			renderer.SetGrid();

			GenerateGamestate();

			renderer.AddFruit( apple.position );

			renderer.AddSnakeHead( snake.body[ 0 ] );
			
			renderer.AddSnakeSegment( snake.body[ 1 ] );

			renderer.AddSnakeTail( snake.body[ 2 ] );

			HasStarted = true;
		}

		public void Step()
		{
			if ( snake.InvalidMove( MoveDir ) )
			{
				HasEnded = true;
				return;
			}

			snake.Move( MoveDir );

			if ( snake.body[ 0 ].position == apple.position )
			{
				Score++;

				snake.Extend();

				while ( snake.body.Exists( elem => elem.position == apple.position ) )
					apple.position = RandomPosition();
			}

			renderer.Render( apple.position, snake.body );
		}

		public IEnumerator End()
		{
			if ( HasEnded )
				yield return renderer.DeathAnim();

			renderer.CleanUp();

			yield break;
		}

		private static Position RandomPosition()
		{
			return new(
				Random.Range( 0, GridSize.x ),
				Random.Range( 0, GridSize.y )
			);
		}

		private void GenerateGamestate()
		{
			Position pos = RandomPosition();

			MoveDir = ( Direction ) Random.Range( 0, 4 );

			Position faceVec = Snake.VectorFromDirection( MoveDir );


			int tries = 0;
			while ( Snake.OutOfBounds( pos ) ||
					Snake.OutOfBounds( pos + faceVec ) ||
					Snake.OutOfBounds( pos - faceVec ) ||
					Snake.OutOfBounds( pos - ( faceVec * 2 ) ) )
			{
				pos = RandomPosition();

				MoveDir = ( Direction ) Random.Range( 0, 4 );

				faceVec = Snake.VectorFromDirection( MoveDir );

				tries++;

				if( tries >= 100)
				{
					pos = new( 5, 4 );
					MoveDir = Direction.Down;
					break;
				}
			}

			snake = new( pos, MoveDir );

			apple = new( RandomPosition() );

			while ( snake.body.Exists( elem => elem.position == apple.position ) )
				apple.position = RandomPosition();

		}
	}

	internal class Fruit
	{
		public Position position;

		public Fruit( Position startPos )
		{
			position = startPos;
		}
	}

	internal class History
	{
		public Position position;
		public Direction direction;
	}

	internal enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}
}
