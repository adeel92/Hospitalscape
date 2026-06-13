using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Isometric.Sound;

namespace Arc
{
    [CreateAssetMenu(fileName = "ButtonAnimationDoTweenSettings", menuName = "Arc/ButtonAnimationDoTweenSettings")]
    public class ButtonAnimationDoTweenSettings : ScriptableObject
    {
        public bool IsTimeIndpendent;
        public float OnClickDownDuration = 0.2f;
        public float OnClickDownScaleMultiplier = 0.8f;
        public SoundType OnClickDownSound;
        public Ease OnClickDownEase = Ease.Linear;
        public float OnClickUpDuration = 0.2f;
        public SoundType OnClickUpSound;
        public Ease OnClickUpEase = Ease.Linear;
        public bool UseCurve;
        public AnimationCurve OnClickUpEaseCurve;
    }
}
