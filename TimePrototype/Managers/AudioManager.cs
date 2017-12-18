using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Nez;

namespace TimePrototype.Managers
{
    public static class AudioManager
    {
        public static SoundEffect arrow;
        public static SoundEffect footstep;
        public static SoundEffect jump;
        public static SoundEffect rewind;
        public static SoundEffect slowmotion;
        public static SoundEffect hit;
        public static SoundEffect bush;
        public static SoundEffect key;
        public static SoundEffect door;

        public static Song malicious;

        public static void loadAllSounds()
        {
            arrow = load(Content.Audios.arrow);
            footstep = load(Content.Audios.footstep);
            jump = load(Content.Audios.jump);
            rewind = load(Content.Audios.rewind);
            slowmotion = load(Content.Audios.slowmotion);
            hit = load(Content.Audios.hit);
            bush = load(Content.Audios.bush);
            key = load(Content.Audios.key);
            door = load(Content.Audios.door);

            malicious = loadBgm(Content.Audios.malicious);
        }

        private static SoundEffect load(string name)
        {
            return Core.content.Load<SoundEffect>(name);
        }

        private static Song loadBgm(string name)
        {
            return Core.content.Load<Song>(name);
        }
    }
}
