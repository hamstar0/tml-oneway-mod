using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;


namespace OneWay {
	class OneWay : Mod {
		public OneWay() { }
	}




	class OneWayPlayer : ModPlayer {
		private Vector2 PrevScrPos = default( Vector2 );
		private Vector2 PrevPos1 = default( Vector2 );
		private Vector2 PrevPos2 = default( Vector2 );

		
		////////////////

		public override bool CloneNewInstances => false;



		////////////////

		public override void ModifyScreenPosition() {
			var plrPos = this.player.position;

			if( this.PrevPos2 == default(Vector2) ) {
				if( this.PrevPos1 != plrPos ) {
					this.PrevPos2 = this.PrevPos1;
					this.PrevPos1 = plrPos;
				}
				return;
			}

			float prev1Dist = Vector2.Distance( this.PrevPos1, plrPos );
			float prev2Dist = Vector2.Distance( this.PrevPos2, plrPos );

			if( prev1Dist >= prev2Dist ) {
				Main.screenPosition = this.PrevScrPos;
			} else if( this.PrevPos1 != plrPos ) {
				this.PrevPos2 = this.PrevPos1;
				this.PrevPos1 = plrPos;
				this.PrevScrPos = Main.screenPosition;
			}
		}
	}
}
