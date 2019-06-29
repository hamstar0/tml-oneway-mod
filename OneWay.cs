using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace OneWay {
	class OneWay : Mod {
		public OneWay() { }


		////////////////

		public override void PostDrawInterface( SpriteBatch sb ) {
			var myplayer = Main.LocalPlayer.GetModPlayer<OneWayPlayer>();
			float colorShift = 1f;

			lock( OneWayPlayer.MyLock ) {
				int i = 0;
				foreach( Vector2 pos in myplayer.TrackingPositions.Values ) {
					var rect = new Rectangle( (int)pos.X, (int)pos.Y, 8, 8 );
					rect.X -= (int)Main.screenPosition.X + 4;
					rect.Y -= (int)Main.screenPosition.Y + 4;

					Color color = Color.Lerp( Color.Black, Color.Red, colorShift );

					sb.Draw( Main.magicPixel, rect, color );

					colorShift = 1f - ( i++ / myplayer.TrackingPositions.Count );
				}

				sb.DrawString( Main.fontMouseText, "Backtracking? " + myplayer.IsBacktracking + ", tracks: " + myplayer.TrackingPositions.Count, new Vector2( 16, 420 ), Color.White );
			}
		}
	}




	class OneWayPlayer : ModPlayer {
		public const int OffscreenMargin = 6 * 16;
		public const float FollowDistance = 192f;

		internal readonly static object MyLock = new object();



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

		internal IDictionary<string, Vector2> TrackingPositions = new Dictionary<string, Vector2>();
		private Vector2 CurrTrackingPos = default( Vector2 );

		internal bool IsBacktracking = false;
		private Vector2 LastTrackedScreenPos = default( Vector2 );


		////////////////

		public override bool CloneNewInstances => false;



		////////////////

		public override void ModifyScreenPosition() {
			var plrPos = this.player.Center;

			lock( OneWayPlayer.MyLock ) {
				if( this.TrackingPositions.Count == 0 ) {
					this.TrackingPositions[(int)( plrPos.X / 16f ) + "," + (int)( plrPos.Y / 16f )] = plrPos;
					this.LastTrackedScreenPos = Main.screenPosition;
					return;
				}

				float shortestDist = Int32.MaxValue;
				foreach( Vector2 pos in this.TrackingPositions.Values ) {
					float dist = Vector2.Distance( pos, plrPos );
					if( dist < shortestDist ) {
						shortestDist = dist;
					}
				}

				float backtrackAmt = OneWayPlayer.FollowDistance - shortestDist;
				this.IsBacktracking = backtrackAmt > 0;

				this.CurrTrackingPos = OneWayPlayer.GetTrackingPosition( this.CurrTrackingPos, this.player.Center );
				string posStr = (int)( this.CurrTrackingPos.X / 16f ) + "," + (int)( this.CurrTrackingPos.Y / 16f );

				if( backtrackAmt < 0 || backtrackAmt > 8f ) {
					this.TrackingPositions[ posStr ] = this.CurrTrackingPos;
				}

				if( this.IsBacktracking ) {
					var scrPos = this.LastTrackedScreenPos;
					var scrRect = new Rectangle( (int)scrPos.X, (int)scrPos.Y, Main.screenWidth, Main.screenHeight );
					Vector2 offset;

					if( OneWayPlayer.IsOffscreen( this.player.getRect(), scrRect, out offset ) ) {
						Main.screenPosition = scrPos + offset;
					} else {
						Main.screenPosition = scrPos;
					}
				} else {
					this.TrackingPositions.Clear();
					this.TrackingPositions[posStr] = this.CurrTrackingPos;
					this.LastTrackedScreenPos = Main.screenPosition;
				}
			}
		}
	}
}
