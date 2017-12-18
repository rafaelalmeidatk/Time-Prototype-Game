using Microsoft.Xna.Framework;
using Nez;
using Nez.BitmapFonts;
using TimePrototype.Managers;
using TimePrototype.Scenes;

namespace TimePrototype
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameMain : Core
    {
        public static BitmapFont bigBitmapFont;
        public static BitmapFont smallBitmapFont;
        
        public GameMain() : base(width: 854, height: 480, isFullScreen: false, enableEntitySystems: true, windowTitle: "Prototype The Game")
        {
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            debugRenderEnabled = false;

            IsFixedTimeStep = true;

            // Register Global Managers
            registerGlobalManager(new InputManager());
            registerGlobalManager(new SystemManager());
        }

        protected override void LoadContent()
        {
            bigBitmapFont = content.Load<BitmapFont>(Nez.Content.Fonts.titleFont);
            smallBitmapFont = content.Load<BitmapFont>(Nez.Content.Fonts.smallFont);
        }

        protected override void Initialize()
        {
            base.Initialize();
            Scene.setDefaultDesignResolution(427, 240, Scene.SceneResolutionPolicy.FixedHeight);

            // PP Fix
            scene = Scene.createWithDefaultRenderer();
            base.Update(new GameTime());
            base.Draw(new GameTime());

            Core.getGlobalManager<SystemManager>().setMapId(13);

            // Set first scene
            scene = new SceneMap();
        }
    }
}
