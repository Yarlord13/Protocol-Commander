using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGameEngine
{
    public enum AnimationState { Stopped, Playing, Paused }

    public class Keyframe<T>
    {
        public float Time { get; set; }
        public T Value { get; set; }
        public Keyframe(float time, T value) { Time = time; Value = value; }
    }

    public class AnimationTrack<T>
    {
        private readonly Action<T> _setter;
        private readonly Func<T> _getter;
        private List<Keyframe<T>> _keyframes = new List<Keyframe<T>>();

        public IReadOnlyList<Keyframe<T>> Keyframes => _keyframes;

        public AnimationTrack(Action<T> setter, Func<T> getter = null)
        {
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
            _getter = getter;
        }

        public void AddKeyframe(Keyframe<T> keyframe)
        {
            _keyframes.Add(keyframe);
            _keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        public void SetKeyframes(IEnumerable<Keyframe<T>> keyframes)
        {
            _keyframes = keyframes.ToList();
            _keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        public void Apply(T value) => _setter(value);

        public T GetInitialValue() => _getter != null ? _getter() : default;

        public T Evaluate(float time)
        {
            if (_keyframes.Count == 0) return default;
            if (time <= _keyframes[0].Time) return _keyframes[0].Value;
            if (time >= _keyframes.Last().Time) return _keyframes.Last().Value;

            Keyframe<T> prev = _keyframes[0], next = _keyframes[0];
            for (int i = 0; i < _keyframes.Count - 1; i++)
            {
                if (time >= _keyframes[i].Time && time <= _keyframes[i + 1].Time)
                {
                    prev = _keyframes[i];
                    next = _keyframes[i + 1];
                    break;
                }
            }

            float t = (time - prev.Time) / (next.Time - prev.Time);
            return Lerp(prev.Value, next.Value, t);
        }

        private static T Lerp(T a, T b, float t)
        {
            Type type = typeof(T);
            if (type == typeof(float)) return (T)(object)MathHelper.Lerp((float)(object)a, (float)(object)b, t);
            if (type == typeof(Vector2)) return (T)(object)Vector2.Lerp((Vector2)(object)a, (Vector2)(object)b, t);
            if (type == typeof(Vector3)) return (T)(object)Vector3.Lerp((Vector3)(object)a, (Vector3)(object)b, t);
            if (type == typeof(Color)) return (T)(object)Color.Lerp((Color)(object)a, (Color)(object)b, t);
            if (type == typeof(int)) return (T)(object)(int)MathHelper.Lerp((int)(object)a, (int)(object)b, t);
            throw new NotSupportedException($"Type {typeof(T)} not supported for lerp.");
        }
    }

    public class AnimationClip
    {
        public string Name { get; set; }
        public float Duration { get; private set; }
        public bool Loop { get; set; }

        private readonly List<object> _tracks = new List<object>();

        public AnimationClip(string name, bool loop = false)
        {
            Name = name;
            Loop = loop;
        }

        public void AddTrack<T>(AnimationTrack<T> track)
        {
            _tracks.Add(track);
            RecalculateDuration();
        }

        private void RecalculateDuration()
        {
            Duration = 0f;
            foreach (var t in _tracks)
            {
                var type = t.GetType();
                var keyframesProp = type.GetProperty("Keyframes");
                var keyframes = keyframesProp.GetValue(t) as System.Collections.IList;
                if (keyframes != null && keyframes.Count > 0)
                {
                    var lastKf = keyframes[keyframes.Count - 1];
                    float time = (float)lastKf.GetType().GetProperty("Time").GetValue(lastKf);
                    if (time > Duration) Duration = time;
                }
            }
        }

        public void Sample(float time)
        {
            foreach (var trackObj in _tracks)
            {
                var type = trackObj.GetType();
                var evaluateMethod = type.GetMethod("Evaluate");
                var applyMethod = type.GetMethod("Apply");
                var value = evaluateMethod.Invoke(trackObj, new object[] { time });
                applyMethod.Invoke(trackObj, new object[] { value });
            }
        }
    }

    public static class AnimationClipExtensions
    {
        public static AnimationTrack<T> AddTrack<T>(this AnimationClip clip, Action<T> setter, Func<T> getter = null)
        {
            var track = new AnimationTrack<T>(setter, getter);
            clip.AddTrack(track);
            return track;
        }
    }

    public class Animation : Behaviour
    {
        private Dictionary<string, AnimationClip> _clips = new Dictionary<string, AnimationClip>();
        private AnimationClip _currentClip;
        private float _currentTime;
        private AnimationState _state = AnimationState.Stopped;

        public AnimationState State => _state;
        public event Action<string> OnFinished;

        public void AddClip(AnimationClip clip) => _clips[clip.Name] = clip;
        public bool RemoveClip(string name) => _clips.Remove(name);
        public bool HasClip(string name) => _clips.ContainsKey(name);

        public void Play(string clipName)
        {
            if (!_clips.TryGetValue(clipName, out _currentClip))
            {
                System.Diagnostics.Debug.WriteLine($"Animation: clip '{clipName}' not found.");
                return;
            }

            _currentTime = 0f;
            _state = AnimationState.Playing;
            _currentClip.Sample(0f);
        }

        public void Pause() { _state = AnimationState.Paused; }
        public void Resume() { if (_state == AnimationState.Paused) _state = AnimationState.Playing; }
        public void Stop()
        {
            _state = AnimationState.Stopped;
            _currentTime = 0f;
            _currentClip = null;
        }

        public override void Update(GameTime gameTime)
        {
            if (_state != AnimationState.Playing || _currentClip == null) return;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _currentTime += delta;

            if (_currentTime >= _currentClip.Duration)
            {
                if (_currentClip.Loop)
                    _currentTime %= _currentClip.Duration;
                else
                {
                    _currentTime = _currentClip.Duration;
                    _state = AnimationState.Stopped;
                    OnFinished?.Invoke(_currentClip.Name);
                }
            }

            _currentClip.Sample(_currentTime);
        }
    }
}