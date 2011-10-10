namespace DoodleJump
    open Microsoft.Xna.Framework
    open Microsoft.Xna.Framework.Graphics


    module Characters=

        type IOnscreenObject=
            abstract member MoveUp: int->unit
            abstract member GetBounds: unit->Rectangle with get

        type Player (game:Game) as this=
            inherit DrawableGameComponent(game)
            
        
            let game = game
            let mutable position = new Vector2( float32(100), float32(0) )
            let mutable direction = -1
            let mutable speed = 1
            let terminalVelocity = 20
            let mutable sprite:Texture2D = null

            do this.DrawOrder <- 10 // sets the player to be on top of platforms
            interface IOnscreenObject with
                 member this.MoveUp amount= position.Y <- position.Y + float32(amount)
                 member this.GetBounds with get() = sprite.Bounds//new Rectangle()
            
            override this.LoadContent() =
                 sprite <- game.Content.Load<Texture2D>("Sprite")
        
            override this.Update gt= 
                this.Jumping
                base.Update(gt)

            override this.Draw gt=
                if sprite <> null then
                    let spriteBatch = new SpriteBatch(game.GraphicsDevice)
                    spriteBatch.Begin()
                    spriteBatch.Draw(sprite, position, Color.White)
                    spriteBatch.End()
                base.Draw(gt)

            member this.Move (distance)=
                position.X <- position.X + distance

            member this.Bounce=
                direction <- direction * -1
                speed <- terminalVelocity

            member this.Jumping=
                if speed <= 0 && direction > 0 then //if the player has reached the zenith of their jump
                    this.Bounce //invert direction, i.e. start to fall
                    speed <- 0 // make sure speed is 0

                if direction = 1 then //if going up,
                    position.Y <- position.Y - float32(direction*speed)
                    speed <- speed - 1 //reduce speed
                else //if going down
                    position.Y <- position.Y - float32(direction*speed)
                    if speed < terminalVelocity then speed <- speed + 1 //increase speed till terminal velocity reached

            member this.NearTopOfScreen=
                if position.Y <= float32(50) then
                    50 - int(position.Y)
                else
                    0

            member this.Direction
                with get() = direction

            member this.Bounds
                with get() = sprite.Bounds

            member this.Position
                with get() = position

            