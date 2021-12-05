
using System.Collections.Generic;

using DiskCardGame;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using APIPlugin;
using UnityEngine;

namespace SnakeHealthMod
{
	[BepInPlugin( PluginGuid, PluginName, PluginVersion )]
	[BepInDependency( "cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency )]
	public partial class Plugin : BaseUnityPlugin
	{
		public const string PluginGuid = "MADH.inscryption.SnakeHealthMod";
		private const string PluginName = "SnakeHealthMod";
		private const string PluginVersion = "1.3.0.0";

		internal static ManualLogSource Log;

		private void Awake ()
		{
			Logger.LogInfo( $"Loaded {PluginName}!" );
			Log = base.Logger;

			Harmony harmony = new(PluginGuid);
			harmony.PatchAll();

			AddSnakeHealth();
			AddMonty();
		}

		private void AddMonty ()
		{
			List<CardMetaCategory> metaCategories = new() { CardMetaCategory.Rare };

			string name = "MontyPython";
			string displayedName = "Monty";

			Texture2D tex = new( 2, 2 );
			tex.LoadImage( Properties.Resources.Monty );

			List<SpecialAbilityIdentifier> sAbIds = new() { SnakeHealth.id };

			List<CardAppearanceBehaviour.Appearance> appearances = CardUtils.getRareAppearance;

			NewCard.Add( name, displayedName, 1, 0, metaCategories, CardComplexity.Simple, CardTemple.Nature,
				bloodCost: 1, specialStatIcon: SnakeHealth.specialStatIcon, specialAbilitiesIdsParam: sAbIds,
				defaultTex: tex, appearanceBehaviour: appearances );
		}

		internal static Sprite LoadSprite ( byte[] imgBytes, string name, Rect rect )
		{
			Sprite sprite = Sprite.Create( LoadTexture( imgBytes, name ), rect, CardUtils.DefaultVector2 );
			sprite.name = string.Concat( "portrait_", name );

			return sprite;
		}

		internal static Texture2D LoadTexture ( byte[] imgBytes, string name )
		{
			Texture2D tex = new( 2, 2 );
			tex.LoadImage( imgBytes );

			tex.name = string.Concat( "portrait_", name );
			tex.filterMode = FilterMode.Point;

			return tex;
		}
	}
}
