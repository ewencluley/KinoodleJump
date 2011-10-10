namespace DoodleJump
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open System
open Characters 
open LevelObjects 
open Kinect
  
    module Game=

        type XnaGame() as this =
            inherit Game()
    
            let screenWidth = 800
            let screenHeight = 600
            do this.Content.RootDirectory <- "XnaGameContent"
            let graphicsDeviceManager = new GraphicsDeviceManager(this)
            let random = new Random()
            
            
            let mutable spriteBatch : SpriteBatch = null

            let mutable score =0

            let mutable movingUp = false

            let mutable player:Player = new Player(this)

            let kinectInput = new KinectSensor(this, player)
            let mutable playerImg:Texture2D = null
            let mutable jointTex = null
            let mutable p = new Vector2(kinectInput.PSjointsP, 200.0f)
            let mutable s = new Vector2(kinectInput.PSjointsS, 150.0f)

            override game.Initialize() =
                graphicsDeviceManager.GraphicsProfile <- GraphicsProfile.HiDef
                graphicsDeviceManager.PreferredBackBufferWidth <- screenWidth
                graphicsDeviceManager.PreferredBackBufferHeight <- screenHeight
                //graphicsDeviceManager.IsFullScreen <- true
                graphicsDeviceManager.ApplyChanges() 
                spriteBatch <- new SpriteBatch(game.GraphicsDevice)
                
                this.Components.Add(player)
                this.makeSomePlatforms 5
                base.Initialize()

            override game.LoadContent() =
                jointTex <- this.Content.Load<Texture2D>("joint")
                base.LoadContent()
            
            override game.Update gameTime = 
                base.Update gameTime 

                if Keyboard.GetState().IsKeyDown(Keys.Left) then
                    player.Move -6.0f
                elif Keyboard.GetState().IsKeyDown(Keys.Right) then
                    player.Move 6.0f
                if player.NearTopOfScreen > 0 then
                    this.moveCameraUp player.NearTopOfScreen
                    movingUp <- true
                else
                    movingUp <-false
                if Keyboard.GetState().IsKeyDown(Keys.R) then
                    game.Reset
                p.X <- kinectInput.PSjointsP
                s.X <- kinectInput.PSjointsS

            override game.Draw gameTime = 
                game.GraphicsDevice.Clear(Color.CornflowerBlue)
                spriteBatch.Begin ()
                spriteBatch.Draw(jointTex, p, Color.White)
                spriteBatch.Draw(jointTex, s, Color.White)
                spriteBatch.End ()
                base.Draw gameTime

            override game.UnloadContent () =
                kinectInput.Uninitalize
                kinectInput.Dispose()

            member game.Reset=
                player.Dispose()
                game.Components.Clear()
                game.Initialize()

            member game.makeSomePlatforms amount=
                this.Components.Add(new Platform(this, player, 100, 300))
                this.Components.Add(new Platform(this, player, 200, 250))
                this.Components.Add(new Platform(this, player, 300, 200))
                this.Components.Add(new Platform(this, player, 200, 150))
             
            member game.addPlatforms amount=
                    if amount > 1 then
                        game.addPlatforms (amount-1)
                    let newPlat = new Platform(this, player, random.Next(0, screenWidth), 0)
                    this.Components.Add(newPlat)
                    score <- score + 10

            member game.moveCameraUp amount=
                if not movingUp then
                    match score with
                    | score when score >100 -> game.addPlatforms 5
                    | score when score >200 -> game.addPlatforms 4
                    | score when score >300 -> game.addPlatforms 3
                    | score when score >400 -> game.addPlatforms 2
                    | score when score >500 -> game.addPlatforms 1
                    | _ -> game.addPlatforms 5
                for current in this.Components do
                    (current :?> IOnscreenObject).MoveUp(amount)
            
            member game.SendDepthImg img=
                playerImg <-img