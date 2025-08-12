using System;
using System.Threading;
using NetCoreAudio;
using System.IO;

namespace ConsoleUno
{
    internal class SoundManager
    {
        private readonly bool _enabled;
        private readonly int _effectsVolume;
        private readonly int _musicVolume;
        private readonly Player _effectsPlayer = new Player();
        private readonly Player _musicPlayer = new Player();

        private const string BeepSoundPath = "uno-game\\uno-game\\beep.wav";
        private const string MusicSoundPath = "uno-game\\uno-game\\music.wav";

        public SoundManager(bool enabled, int effectsVolume, int musicVolume)
        {
            _enabled = enabled;
            _effectsVolume = Math.Clamp(effectsVolume, 10, 100);
            _musicVolume = Math.Clamp(musicVolume, 10, 100);

            _effectsPlayer.SetVolume((byte)_effectsVolume);
            _musicPlayer.SetVolume((byte)_musicVolume);
        }

        public async void PlaySound(int frequency, int duration, bool isMusic = false)
        {
            if (!_enabled) return;

            try
            {
                if (File.Exists(BeepSoundPath))
                {
                    await _effectsPlayer.Play(BeepSoundPath);
                }
                else
                {
                    Console.WriteLine($"Warning: {BeepSoundPath} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }

        public async void PlayMainMenuMusic(bool loop = false)
        {
            if (!_enabled) return;

            try
            {
                if (File.Exists(MusicSoundPath))
                {
                    _musicPlayer.PlaybackFinished += (sender, args) =>
                    {
                        if (loop && _enabled)
                        {
                            _musicPlayer.Play(MusicSoundPath);
                        }
                    };
                    await _musicPlayer.Play(MusicSoundPath);
                }
                else
                {
                    Console.WriteLine($"Warning: {MusicSoundPath} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing music: {ex.Message}");
            }
        }

        public void UpdateEffectsVolume(int volume)
        {
            _effectsPlayer.SetVolume((byte)Math.Clamp(volume, 0, 100));
        }

        public void UpdateMusicVolume(int volume)
        {
            _musicPlayer.SetVolume((byte)Math.Clamp(volume, 0, 100));
        }

        public void StopMusic()
        {
            _musicPlayer.Stop();
        }
    }
}