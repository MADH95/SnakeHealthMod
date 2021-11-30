using System.Collections.Generic;

using UnityEngine;

namespace SnakeHealthMod.Game
{
	using Position = Vector2Int;

	internal class Snake
	{
		private const int START_LENGTH = 3;

		public List<History> body = new( START_LENGTH );

		public int Length => body.Count;

		private bool toExtend = false;
		private History tail;


		public Snake( Position headPos, Direction facingDir )
		{
			var faceVec = VectorFromDirection( facingDir );

			body.AddRange( new History[ START_LENGTH ]
			{
					new()
					{
						position = headPos,
						direction = facingDir,
					},

					new()
					{
						position = headPos - faceVec,
						direction = facingDir,
					},

					new()
					{
						position = headPos - ( faceVec * 2 ),
						direction = facingDir,
					}
			} );
		}

		public bool InvalidMove( Direction dir )
		{
			Position head = body[ 0 ].position + VectorFromDirection( dir );

			if ( OutOfBounds( head ) )
				return true;

			if ( body.Exists( elem => elem.position == head ) )
				return true;

			return false;
		}

		public void Move( Direction dir )
		{
			History head = new()
			{
				position = body[ 0 ].position + VectorFromDirection( dir ),
				direction = dir
			};

			for ( int i = body.Count - 1; i > 0; i-- )
			{
				body[ i ] = body[ i - 1 ];
			}

			body[ 0 ] = head;

			if ( toExtend )
			{
				toExtend = false;

				body.Add( tail );
			}
		}

		public void Extend()
		{
			toExtend = true;

			tail = body[ body.Count - 1 ];
		}

		public static bool OutOfBounds( Position pos )
		{
			return pos.x >= SnakeGame.GridSize.x || pos.x < 0 || pos.y >= SnakeGame.GridSize.y || pos.y < 0;
		}

		public static Position VectorFromDirection( Direction dir )
			=> dir switch
		{
			Direction.Up	=> Position.up,
			Direction.Down	=> Position.down,
			Direction.Left	=> Position.left,
			Direction.Right	=> Position.right,
						_	=> Position.zero
		};
	}
}
