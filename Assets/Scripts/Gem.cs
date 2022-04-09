using System;
using UnityEngine;

public class Gem : MonoBehaviour {

    public event Action<int, int> OnGemClick;

    public int GemType { get; set; }
    public int X => x;
    public int Y => y;
    
    private Animator animator;

    private int x;
    private int y;
    
    private void Start() {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }
    
    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(0)) {
            // OnGemClick(x, y);
            SetSpinning(true); //todo remove
        }
    }
    
    public void SetBoardPosition(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public void SetSpinning(bool spinning) {
        animator.enabled = spinning;
        animator.Rebind();
        animator.Update(0f);
    }
}
