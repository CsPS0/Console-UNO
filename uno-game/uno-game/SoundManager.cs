using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleUno
{
    public class SoundManager
    {
        private bool _enabled;
        private int _effectsVolume;
        private int _musicVolume;
        private CancellationTokenSource? _musicCts;
        private Task? _musicTask;

        public SoundManager(bool enabled, int effectsVolume, int musicVolume)
        {
            _enabled = enabled;
            _effectsVolume = Math.Clamp(effectsVolume, 0, 100);
            _musicVolume = Math.Clamp(musicVolume, 0, 100);
        }

        public void PlaySound(int frequency, int duration, bool isMusic = false)
        {
            if (!_enabled || _effectsVolume == 0) return;

            Task.Run(() =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        Console.Beep(Math.Clamp(frequency, 37, 32767), Math.Max(20, duration));
                    }
                }
                catch
                {

                }
            });
        }

        public void PlayMenuBeep()
        {
            PlaySound(880, 50);
        }

        public void PlayCardPlay()
        {
            if (!_enabled || _effectsVolume == 0) return;

            Task.Run(() =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        Console.Beep(700, 40);
                        Console.Beep(1100, 60);
                    }
                }
                catch { }
            });
        }

        public void PlayCardDraw()
        {
            if (!_enabled || _effectsVolume == 0) return;

            Task.Run(() =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        Console.Beep(450, 40);
                        Console.Beep(650, 50);
                    }
                }
                catch { }
            });
        }

        public void PlaySpecialAction()
        {
            if (!_enabled || _effectsVolume == 0) return;

            Task.Run(() =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        Console.Beep(523, 70);
                        Console.Beep(659, 70);
                        Console.Beep(784, 90);
                        Console.Beep(1046, 120);
                    }
                }
                catch { }
            });
        }

        public void PlayUnoAlert()
        {
            if (!_enabled || _effectsVolume == 0) return;

            Task.Run(() =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Console.Beep(880, 100);
                            Console.Beep(1320, 120);
                            Thread.Sleep(50);
                        }
                    }
                }
                catch { }
            });
        }

        public void PlayInvalid()
        {
            if (!_enabled || _effectsVolume == 0) return;

            Task.Run(() =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        Console.Beep(220, 100);
                        Console.Beep(180, 150);
                    }
                }
                catch { }
            });
        }

        public void PlayVictory()
        {
            if (!_enabled || _effectsVolume == 0) return;

            Task.Run(() =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {

                        int[] notes = { 523, 523, 523, 659, 784, 1046 };
                        int[] times = { 100, 100, 100, 180, 180, 400 };
                        for (int i = 0; i < notes.Length; i++)
                        {
                            Console.Beep(notes[i], times[i]);
                            Thread.Sleep(25);
                        }
                    }
                }
                catch { }
            });
        }

        public void PlayMainMenuMusic(bool loop = false)
        {
            if (!_enabled || _musicVolume == 0) return;

            StopMusic();

            _musicCts = new CancellationTokenSource();
            var token = _musicCts.Token;

            _musicTask = Task.Run(() =>
            {
                try
                {

                    (int Note, int Duration, int Pause)[] score =
                    {

                        (523, 100, 30),
                        (659, 100, 30),
                        (784, 130, 40),
                        (659, 90, 30),
                        (880, 150, 40),
                        (784, 180, 60),

                        (698, 100, 30),
                        (659, 100, 30),
                        (587, 100, 30),
                        (523, 100, 30),
                        (587, 130, 40),
                        (392, 160, 60),

                        (659, 90, 25),
                        (784, 90, 25),
                        (1046, 140, 40),
                        (988, 100, 30),
                        (880, 100, 30),
                        (784, 120, 30),
                        (880, 150, 40),

                        (698, 100, 30),
                        (784, 100, 30),
                        (659, 120, 30),
                        (587, 100, 30),
                        (523, 200, 80),
                        (392, 120, 30),
                        (523, 160, 100),

                        (659, 85, 25),
                        (659, 85, 25),
                        (784, 100, 30),
                        (880, 100, 30),
                        (784, 90, 30),
                        (659, 90, 30),
                        (523, 140, 40),

                        (587, 90, 25),
                        (659, 90, 25),
                        (698, 90, 25),
                        (659, 90, 25),
                        (587, 100, 30),
                        (494, 110, 30),
                        (523, 240, 250)
                    };

                    do
                    {
                        foreach (var item in score)
                        {
                            if (token.IsCancellationRequested || !_enabled || _musicVolume == 0) return;

                            if (item.Note > 0 && OperatingSystem.IsWindows())
                            {
                                Console.Beep(item.Note, item.Duration);
                            }
                            else
                            {
                                Thread.Sleep(item.Duration);
                            }

                            if (token.IsCancellationRequested) return;
                            Thread.Sleep(item.Pause);
                        }

                        if (loop)
                        {
                            Thread.Sleep(600);
                        }
                    } while (loop && !token.IsCancellationRequested);
                }
                catch
                {

                }
            }, token);
        }

        public void UpdateEffectsVolume(int volume)
        {
            _effectsVolume = Math.Clamp(volume, 0, 100);
        }

        public void UpdateMusicVolume(int volume)
        {
            _musicVolume = Math.Clamp(volume, 0, 100);
            if (_musicVolume == 0)
            {
                StopMusic();
            }
        }

        public void StopMusic()
        {
            try
            {
                _musicCts?.Cancel();
                _musicCts?.Dispose();
                _musicCts = null;
                _musicTask = null;
            }
            catch { }
        }
    }
}
