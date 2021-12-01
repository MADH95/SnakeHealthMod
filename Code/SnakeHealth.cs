
using System.Collections;

using UnityEngine;

using Pixelplacement;

using DiskCardGame;

using APIPlugin;

using SnakeHealthMod.Game;

namespace SnakeHealthMod
{
	public partial class Plugin
	{
		private NewSpecialAbility AddSnakeHealth()
		{
			StatIconInfo info = ScriptableObject.CreateInstance<StatIconInfo>();

			info.appliesToAttack = false;

			info.appliesToHealth = true;

			info.rulebookName = "Snake Minigame";

			info.rulebookDescription =
				"The health of this card is increased by the number of apples collected in snake.";

			info.iconGraphic = LoadTexture( Properties.Resources.SnakeMinigameIcon, "SnakeMinigameIcon" );

			NewSpecialAbility ability = new( typeof( SnakeHealth ), SnakeHealth.id, info );

			SnakeHealth.specialStatIcon = ability.statIconInfo.iconType;

			return ability;
		}
	}


	internal class SnakeHealth : CardMinigameBehaviour
	{
		// Static variables
		public static SpecialAbilityIdentifier id = SpecialAbilityIdentifier.GetID( Plugin.PluginGuid, nameof( SnakeHealth ) );
		public static SpecialStatIcon specialStatIcon;

		private static readonly float tickThreshold = .15f; //Not tested, but .2 felt a little too slow
		private static readonly Vector3 cardPos = new( 0f, 6.85f, -3.65f ); //Magic number for positioning cards for minigames

		// Instance Variables
		private CardModificationInfo mod;

		private SnakeGame game;
		private float tickTime = 0;
		private bool isEnding = false;
		private Direction currentDir;

		public override bool RespondsToDrawn()
		{
			return true;
		}

		public override void SetupMinigame()
		{
			if ( base.PlayableCard is not null )
			{
				mod = new()
				{
					nonCopyable = true,
					singletonId = "SNAKE_HEALTH",
					healthAdjustment = 0
				};

				base.PlayableCard.AddTemporaryMod( mod );

			}

			Tween.Position( base.Card.transform, cardPos, .2f, 0f, Tween.EaseOut );

			base.Card.renderInfo.hidePortrait = true;
			base.Card.renderInfo.showSpecialStats = true;
			base.Card.RenderCard();


			game = new();
			base.StartCoroutine( game.Setup( base.Card.transform ) );
		}

		public override void ManagedUpdate()
		{
			if ( game is null || isEnding )
				return;

			if ( !game.HasStarted && Input.GetKeyDown( KeyCode.Space ) )
				game.Start();

			if ( !game.HasStarted )
				return;

			if ( mod.healthAdjustment != game.Score )
			{
				mod.healthAdjustment = game.Score;
				base.PlayableCard.OnStatsChanged();
			}

			if ( game.HasEnded || Input.GetKeyDown( KeyCode.Q ) )
			{
				isEnding = true;

				this.Exit();

				return;
			}

			this.UpdateDirection();
			
			tickTime += Time.deltaTime;
			if ( tickTime >= tickThreshold )
			{
				tickTime = 0;
				game.Step();
				currentDir = game.MoveDir;
			}
		}

		private IEnumerator Exit()
		{
			yield return game.End();

			game = null;

			if ( mod.healthAdjustment == 0 )
			{
				mod.healthAdjustment = 1;
			}

			base.Card.renderInfo.hidePortrait = false;
			base.Card.RenderCard();

			this.gameCompleted = true;

			yield break;
		}


		private void UpdateDirection()
		{

			if ( Input.GetKeyDown( KeyCode.W ) && currentDir != Direction.Down )
				game.MoveDir = Direction.Up;

			if ( Input.GetKeyDown( KeyCode.S ) && currentDir != Direction.Up )
				game.MoveDir = Direction.Down;

			if ( Input.GetKeyDown( KeyCode.A ) && currentDir != Direction.Right )
				game.MoveDir = Direction.Left;

			if ( Input.GetKeyDown( KeyCode.D ) && currentDir != Direction.Left )
				game.MoveDir = Direction.Right;
		}
	}
}
