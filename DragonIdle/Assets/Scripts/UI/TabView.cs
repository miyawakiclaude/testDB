using UnityEngine;

namespace DragonIdle
{
    /// <summary>下部タブ1枚ぶんの画面。Build で階層をつくり、Refresh で数値を更新する。</summary>
    public abstract class TabView
    {
        public GameObject Root { get; protected set; }

        protected GameManager Game { get { return GameManager.Instance; } }

        public abstract string Title { get; }

        public abstract void Build(Transform parent);

        public abstract void Refresh();

        /// <summary>ドラゴンの増減や転生など、並びそのものが変わったときに呼ばれる。</summary>
        public virtual void Rebuild() { }

        public void SetVisible(bool visible)
        {
            if (Root != null) Root.SetActive(visible);
        }
    }
}
