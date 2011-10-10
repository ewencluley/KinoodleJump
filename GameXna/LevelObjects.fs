namespace DoodleJump

    open Microsoft.Xna.Framework
    open Microsoft.Xna.Framework.Graphics
    open Characters

    module LevelObjects=
        
        type Platform (game:Game, player:Player, x:int, y:int) as this =
            inherit DrawableGameComponent(game)

            do this.DrawOrder <- 9 // sets the player to be on top of platforms
            let mutable sprite:Texture2D =null
            let mutable position = new Vector2(float32(x), float32(y))

            interface IOnscreenObject with
                 member this.MoveUp amount= position.Y <- position.Y + float32(amount)
                 member this.GetBounds with get() = let mutable b = sprite.Bounds
                                                    b.Offset(int(position.X), int(position.Y))
                                                    b
            

            override this.LoadContent() =
                 sprite <- game.Content.Load<Texture2D>("Platform")
        
            override this.Update gt= 
                this.BouncePlayer
                this.Onscreen
                base.Update(gt)

            override this.Draw gt=
                if sprite <> null then
                    let spriteBatch = new SpriteBatch(game.GraphicsDevice)
                    spriteBatch.Begin()
                    spriteBatch.Draw(sprite, position, Color.White)
                    spriteBatch.End()
                base.Draw(gt)

            member this.BouncePlayer=
               if player.Direction = -1 then
                   let mutable bounds = sprite.Bounds
                   bounds.Offset(int(position.X), int(position.Y))
                   let mutable playerBounds = player.Bounds
                   playerBounds.Offset(int(player.Position.X), int(player.Position.Y))
                   if bounds.Intersects(playerBounds) then
                        player.Bounce
           

            member this.Onscreen=
                if position.Y > float32(600) then
                    if game.Components.Remove(this) then
                        System.Diagnostics.Debug.WriteLine("removed")

