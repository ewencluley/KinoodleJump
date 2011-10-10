namespace DoodleJump

    open Microsoft.Research.Kinect.Nui

    open Microsoft.Xna.Framework
    open Microsoft.Xna.Framework.Graphics

    open System.Linq
    open System

    open Characters

    module Kinect=
        type KinectSensor(game:Game, player:Player)=
            inherit DrawableGameComponent(game)
            let mutable img:Texture2D = null
            let mutable pelvisPos = float32(0.0)
            let mutable shoulderPos = float32(0.0)
            let SkeletonReady (sender : obj) (args: SkeletonFrameReadyEventArgs)=
                let mutable skeleton = null
                let mutable i=0
                while skeleton = null && i< args.SkeletonFrame.Skeletons.Length do
                    if args.SkeletonFrame.Skeletons.ElementAt(i).TrackingState = SkeletonTrackingState.Tracked then
                        skeleton <- args.SkeletonFrame.Skeletons.ElementAt(i)
                        let shoulderCenter = skeleton.Joints.[JointID.Head] //.ScaleTo(640, 480) //in f# .[i] is notation for accessing array element, not simply []
                        let pelvis = skeleton.Joints.[JointID.HipCenter ]//.ScaleTo(640, 480)
                        let toMove = (pelvis.Position.X - shoulderCenter.Position.X) * float32(-40)
                        pelvisPos <- pelvis.Position.X * 60.0f
                        shoulderPos <- shoulderCenter.Position.X * 60.0f
                        player.Move(toMove)
                    i<-i+1
                
                
            let DepthReady (sender : obj) (args:ImageFrameReadyEventArgs)=

                let maxDist = 4000
                let minDist = 850
                let distOffset = maxDist - minDist
                
                img <- new Texture2D(game.GraphicsDevice,320, 240)
                let pImg = args.ImageFrame.Image
                let DepthColor = Array.create (pImg.Width*pImg.Height) (new Color(255,255,255))

                for y = 0 to pImg.Height-1 do
                    for x = 0 to pImg.Width-1 do
                        let n = (y * pImg.Width + x) * 2

                        let distance = int pImg.Bits.[n] ||| (int pImg.Bits.[n+1] <<< 8)
                        //printfn "%08X" distance
                        //let bytes : byte [] = [|1uy; 1uy|]
                        //let distance : int = 257

                        
                        
                        //let distance:int = int((pImg.Bits.[n + 0]) ||| (pImg.Bits.[n + 1] <<< 8))
                        //Diagnostics.Debug.WriteLine("dist:" + distance.ToString())
                        let intensity = (255-(255*Math.Max(distance-minDist,0)/distOffset))
                        //Diagnostics.Debug.WriteLine(intensity)
                        //let intensity = 0
                        //let x = (intensity)
                        //Diagnostics.Debug.WriteLine("int:"+ intensity.ToString())
                        DepthColor.[y * pImg.Width + x] <- new Color(intensity, intensity, intensity)
                        ()
                img.SetData(DepthColor)


            let nui = new Runtime()
            do nui.Initialize(RuntimeOptions.UseSkeletalTracking ||| RuntimeOptions.UseDepth)
            //do nui.SkeletonEngine.TransformSmooth <- true;
            do nui.SkeletonFrameReady.AddHandler(new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonReady))
            //do nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.Depth)
            //do nui.DepthFrameReady.AddHandler(new EventHandler<ImageFrameReadyEventArgs>(DepthReady))

            override this.Draw gameTime=
                base.Draw gameTime

            override this.Update gameTime=
                //Diagnostics.Debug.WriteLine(tracked)
                base.Update gameTime
            
            member this.Uninitalize =
                nui.Uninitialize ()

            member this.DepthImg with get()=img
            
            member this.PSjointsP with get() = pelvisPos

            member this.PSjointsS with get() = shoulderPos