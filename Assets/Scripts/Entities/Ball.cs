    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

public class Ball : GridObject
{
    public Animator animator;
    public float rollSpeed = 12f;

    bool _isRolling;
    public bool isRolling
    {
        get => _isRolling;
        private set
        {
            _isRolling = value;
            if (animator != null) animator.SetBool("IsRolling", value);
        }
    }

    Renderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    // Đặt hướng cho Blend Tree
    void SetFace(Vector2 dir)
    {
        if (animator == null || dir == Vector2.zero) return;
        animator.SetFloat("FaceX", dir.x);
        animator.SetFloat("FaceY", dir.y);
    }

    public IEnumerator Roll(List<Vector2Int> path, System.Action<int> onCellReached = null)
    {
        isRolling = true;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int cell = path[i];

            GridObject other = GridManager.Instance.GetAt(cell);
            if (other != null && other != this)
            {
                break;
            }

            GridManager.Instance.Move(this, cell);

            Vector3 target = WorldPosFor(cell);

            // Hướng lăn = từ vị trí hiện tại tới ô đích
            Vector3 delta = target - transform.position;
            SetFace(new Vector2(delta.x, delta.y).normalized);

            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, rollSpeed * Time.deltaTime);
                yield return null;
            }

            onCellReached?.Invoke(i);
        }

        Stop();
    }

    public IEnumerator RollOffScreen(Direction dir)
    {
        isRolling = true;
        Vector3 target = GridManager.Instance.OuterLimit(transform.position, dir);

        Vector3 delta = target - transform.position;
        SetFace(new Vector2(delta.x, delta.y).normalized);

        while ((transform.position - target).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, rollSpeed * Time.deltaTime);
            yield return null;
        }

        SetVisible(false);
    }

        /// Bóng xuất hiện lại ở ô cell
        public void Respawn(Vector2Int cell)
        {
            GridManager.Instance.Move(this, cell);   // ô logic về ô cũ
            Show();                                  // đặt hình vào giữa ô
            SetVisible(true);
            isRolling = false;
        }
        
        public void Stop()
        {
            isRolling = false;
            Show();
        }

        public void ResetVisual()
        {
            SetVisible(true);
            isRolling = false;
        }
        
        void SetVisible(bool visible)
        {
            if (renderers == null) return;
            foreach (Renderer r in renderers)
            {
                if (r != null) r.enabled = visible;
            }
        }
    }