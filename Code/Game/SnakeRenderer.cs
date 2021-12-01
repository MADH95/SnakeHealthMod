
using System.Collections.Generic;

using UnityEngine;

using APIPlugin;
using Pixelplacement.TweenSystem;
using System.Collections;

namespace SnakeHealthMod.Game
{
	using static Mono.Security.X509.X520;

	using Position = Vector2Int;

	internal class SnakeRenderer
	{
		private static readonly float cellOffest = 0.11f;
		private static readonly Vector3 gridPos = new( -.5f, -.38f, -.0005f );
		private static readonly Vector3 gamePos = new( 0f, .15f, -.0001f );

		private readonly GameObject minigame;
		private GameObject fruit;
		private GameObject snakeHead;
		private GameObject snakeTail;
		private readonly List< GameObject > snakeBody;

		public SnakeRenderer( Transform owner )
		{
			minigame = new( nameof( SnakeGame ), new[] { typeof( SpriteRenderer ) } );

			minigame.transform.parent = owner;

			minigame.transform.localPosition = gamePos;
			minigame.transform.rotation = owner.transform.rotation;

			var sr = minigame.GetComponent<SpriteRenderer>();

			sr.enabled = true;

			sr.sprite = GameSprites.Rules;

			snakeBody = new();
		}

		public void CleanUp()
		{
			foreach ( var obj in snakeBody )
			{
				GameObject.Destroy( obj );
			}

			snakeBody.Clear();

			GameObject.Destroy( minigame );
		}

		public IEnumerator DeathAnim()
		{
			for ( int i = 0; i < 3; i++ )
			{
				yield return FlashSnake();
			}
		}

		private IEnumerator FlashSnake()
		{
			ToggleEnabledSprites( false );

			yield return new WaitForSeconds( .3f );

			ToggleEnabledSprites( true );

			yield return new WaitForSeconds( .3f );
		}

		private void ToggleEnabledSprites( bool toggle )
		{
			snakeHead.SetActive( toggle );

			foreach ( var obj in snakeBody )
			{
				obj.SetActive( toggle );
			}

			snakeTail.SetActive( toggle );
		}

		public void SetGrid()
			=> minigame.GetComponent<SpriteRenderer>().sprite = GameSprites.Grid;

		public void AddSnakeHead( History head )
		{
			snakeHead = new( "SnakeHead", new[] { typeof( SpriteRenderer ) } );

			snakeHead.transform.parent = minigame.transform;

			snakeHead.transform.rotation = minigame.transform.rotation;

			snakeHead.transform.localRotation = RotationFromDirection( head.direction );

			snakeHead.transform.localPosition = ToRenderSpace( head.position );


			var sr = snakeHead.GetComponent<SpriteRenderer>();

			sr.enabled = true;

			sr.sprite = GameSprites.SnakeHead;
		}

		public void AddSnakeTail( History tail )
		{
			snakeTail = new( "SnakeTail", new[] { typeof( SpriteRenderer ) } );

			snakeTail.transform.parent = minigame.transform;

			snakeTail.transform.rotation = minigame.transform.rotation;

			snakeTail.transform.localRotation = RotationFromDirection( tail.direction );

			snakeTail.transform.localPosition = ToRenderSpace( tail.position );

			var sr = snakeTail.GetComponent<SpriteRenderer>();

			sr.enabled = true;

			sr.sprite = GameSprites.SnakeTail;
		}

		public void AddSnakeSegment( History segment )
		{
			GameObject obj = new( string.Concat( "SnakeSegment", snakeBody.Count ), new[] { typeof( SpriteRenderer ) } );

			obj.transform.parent = minigame.transform;

			obj.transform.rotation = minigame.transform.rotation;

			obj.transform.localRotation = RotationFromDirection( segment.direction );

			obj.transform.localPosition = ToRenderSpace( segment.position );

			var sr = obj.GetComponent<SpriteRenderer>();

			sr.enabled = true;

			sr.sprite = GameSprites.SnakeBody;

			snakeBody.Add( obj );
		}

		public void AddFruit( Position pos )
		{
			fruit = new( nameof( Fruit ), new[] { typeof( SpriteRenderer ) } );

			fruit.transform.parent = minigame.transform;

			fruit.transform.rotation = minigame.transform.rotation;

			fruit.transform.localRotation = Quaternion.identity;

			fruit.transform.localPosition = ToRenderSpace( pos );

			var sr = fruit.GetComponent<SpriteRenderer>();

			sr.enabled = true;

			sr.sprite = GameSprites.Apple;
		}

		public void Render( Position fruitPos, List<History> snake )
		{
			if ( ( snake.Count - 2 ) > snakeBody.Count )
			{
				AddSnakeSegment( snake[ snake.Count - 1 ] );
			}

			fruit.transform.localPosition = ToRenderSpace( fruitPos );

			snakeHead.transform.localPosition = ToRenderSpace( snake[ 0 ].position );
			snakeHead.transform.localRotation = RotationFromDirection( snake[ 0 ].direction );

			snakeTail.transform.localPosition = ToRenderSpace( snake[ snake.Count - 1 ].position );
			snakeTail.transform.localRotation = RotationFromDirection( snake[ snake.Count - 2 ].direction );

			for ( int i = 1; i < snake.Count - 1; i++ )
			{
				snakeBody[ i - 1 ].transform.localPosition = ToRenderSpace( snake[ i ].position );
				snakeBody[ i - 1 ].transform.localRotation = RotationFromDirection( snake[ i ].direction );

				if ( snake[ i ].direction != snake[ i - 1 ].direction )
				{
					UpdateSpriteRotation( snakeBody[ i - 1 ], snake[ i ].direction, snake[ i - 1 ].direction );

					snakeBody[ i - 1 ].GetComponent<SpriteRenderer>().sprite = GameSprites.SnakeCorner;

					continue;
				}

				snakeBody[ i - 1 ].GetComponent<SpriteRenderer>().sprite = GameSprites.SnakeBody;
			}
		}

		private Vector2 ToRenderSpace( Vector2 pos )
			=> gridPos + ( Vector3 ) ( pos * cellOffest );

		private Quaternion RotationFromDirection( Direction dir ) => dir switch
		{
			Direction.Up => Quaternion.AngleAxis( 0f, Vector3.forward ),
			Direction.Down => Quaternion.AngleAxis( 180f, Vector3.forward ),
			Direction.Right => Quaternion.AngleAxis( -90f, Vector3.forward ),
			Direction.Left => Quaternion.AngleAxis( 90f, Vector3.forward ),
			_ => Quaternion.identity
		};

		private void UpdateSpriteRotation( GameObject obj, Direction fromDir, Direction toDir )
		{
			bool upAndLeft = fromDir == Direction.Up && toDir   == Direction.Left;

			bool leftAndDown = fromDir == Direction.Left && toDir == Direction.Down;

			bool rightAndUp = fromDir == Direction.Right && toDir   == Direction.Up;

			bool downAndRight = fromDir == Direction.Down && toDir == Direction.Right;

			if ( upAndLeft || leftAndDown || rightAndUp || downAndRight )
			{
				obj.transform.localRotation *= Quaternion.AngleAxis( -90f, Vector3.forward );
			}
		}
	}

	internal static class GameSprites
	{
		private static readonly Rect CellRect = new( 0, 0, 10, 10 );

		public static readonly Sprite Rules = Plugin.LoadSprite( Properties.Resources.Rules, "Rules", CardUtils.DefaultCardArtRect );

		public static readonly Sprite Grid = Plugin.LoadSprite( Properties.Resources.Grid, "Grid", CardUtils.DefaultCardArtRect );

		public static readonly Sprite Apple = Plugin.LoadSprite( Properties.Resources.Apple, "Apple", CellRect );

		public static readonly Sprite SnakeHead = Plugin.LoadSprite( Properties.Resources.SnakeHead, "SnakeHead", CellRect );

		public static readonly Sprite SnakeBody = Plugin.LoadSprite( Properties.Resources.SnakeBody, "SnakeBody", CellRect );

		public static readonly Sprite SnakeTail = Plugin.LoadSprite( Properties.Resources.SnakeTail, "SnakeTail", CellRect );

		public static readonly Sprite SnakeCorner = Plugin.LoadSprite( Properties.Resources.SnakeCorner, "SnakeCorner", CellRect );
	}

}
