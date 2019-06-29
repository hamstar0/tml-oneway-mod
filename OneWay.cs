using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;


namespace OneWay {
	class OneWay : Mod {
		public OneWay() { }


		////////////////

		public override void PostDrawInterface( SpriteBatch sb ) {
			var myplayer = Main.LocalPlayer.GetModPlayer<OneWayPlayer>();

			var rect = new Rectangle( (int)myplayer.TrackingPosition.X, (int)myplayer.TrackingPosition.Y, 4, 4 );
			rect.X -= (int)Main.screenPosition.X + 2;
			rect.Y -= (int)Main.screenPosition.Y + 2;

			sb.Draw( Main.magicPixel, rect, Color.Red );
			sb.DrawString( Main.fontMouseText, "Backtracking? "+myplayer.IsBacktracking+", Rect: "+rect.ToString(), new Vector2(16, 368), Color.White );
		}
	}




	class OneWayPlayer : ModPlayer {
		public const int OffscreenMargin = 6 * 16;
		public const float FollowDistance = 192f;



		////////////////

		public static bool IsOffscreen( Rectangle box, Rectangle screen, out Vector2 offscreenAmount ) {
			screen.X += OneWayPlayer.OffscreenMargin;
			screen.Y += OneWayPlayer.OffscreenMargin;
			screen.Width -= OneWayPlayer.OffscreenMargin * 2;
			screen.Height -= OneWayPlayer.OffscreenMargin * 2;

			if( screen.Intersects(box) ) {
				offscreenAmount = default( Vector2 );
				return false;
			}

			offscreenAmount = default( Vector2 );

			if( box.X < screen.X ) {
				offscreenAmount.X = box.X - screen.X;
			} else if( (box.X + box.Width) > (screen.X + screen.Width) ) {
				offscreenAmount.X = (box.X + box.Width) - (screen.X + screen.Width );
			}

			if( box.Y < screen.Y ) {
				offscreenAmount.Y = box.Y - screen.Y;
			} else if( (box.Y + box.Height) > (screen.Y + screen.Height) ) {
				offscreenAmount.Y = (box.Y + box.Height) - (screen.Y + screen.Height);
			}

			return true;
		}


		public static Vector2 GetTrackingPosition( Vector2 oldPos, Vector2 targetPos ) {
			Vector2 pos;
			Vector2 vector = targetPos - oldPos;
			Vector2 dir = Vector2.Normalize( vector );
			float targetDist = vector.Length();
			float to = targetDist - OneWayPlayer.FollowDistance;

			pos = dir * to;
			pos += oldPos;
			return pos;
		}



		////////////////

		internal Vector2 TrackingPosition = default( Vector2 );
		internal bool IsBacktracking = false;


		////////////////

		public override bool CloneNewInstances => false;



		////////////////

		public override void ModifyScreenPosition() {
			var plrPos = this.player.Center;

			if( this.TrackingPosition == default(Vector2) ) {
				this.TrackingPosition = plrPos;
				return;
			}

			float dist = Vector2.Distance( this.TrackingPosition, plrPos );
			float backtrackAmt = OneWayPlayer.FollowDistance - dist;
			this.IsBacktracking = backtrackAmt > 0;

			if( backtrackAmt < 0 || backtrackAmt > 8f ) {
				this.TrackingPosition = OneWayPlayer.GetTrackingPosition( this.TrackingPosition, this.player.Center );
			}

			if( this.IsBacktracking ) {
				var scrRect = new Rectangle( (int)scrPos.X, (int)scrPos.Y, Main.screenWidth, Main.screenHeight );
				Vector2 offset;

				if( OneWayPlayer.IsOffscreen(this.player.getRect(), scrRect, out offset) ) {
					Main.screenPosition = scrPos + offset;
				} else {
					Main.screenPosition = scrPos;
				}
			}
		}
	}
}
