using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    CharacterController characterController;
    [SerializeField] SO_PlayerData playerPosition;
    [SerializeField] float speed;
    [SerializeField] AudioSource audio;
    Vector3 dir = Vector3.zero;

    MaterialPropertyBlock block;
    Renderer rend;
    public bool canTakeDamage = true;
    private void Awake()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        playerPosition.SetPosition(transform.position);
        block = new MaterialPropertyBlock();
        rend = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        characterController.Move((dir + Physics.gravity) * Time.deltaTime * speed);
        playerPosition.SetPosition(transform.position);
    }

    public void SetPosition()
    {
        playerPosition.SetPosition(transform.position);
    }

    public void OnDamaged()
    {
        if (canTakeDamage)
        {
            playerPosition.TakeDamage(1);
            AudioHandler.instance.PlaySound("Player_Hurt", audio);
            // Update UI
            GameHandler.instance.uiManager.UpdateHP();
            StartCoroutine(FlashDamage());
        }
    }

    IEnumerator FlashDamage()
    {
        float amount = 0.75f;

        while (amount > 0)
        {
            Vector3 current = Vector3.Lerp(Vector3.right, Vector3.one, 1 - amount);

            block.SetColor("_BaseColor", new Color(current.x, current.y, current.z));
            rend.SetPropertyBlock(block);
            amount -= Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }
        block.SetColor("_BaseColor", Color.white);
        rend.SetPropertyBlock(block);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        dir = new Vector3(ctx.ReadValue<Vector2>().x, 0, ctx.ReadValue<Vector2>().y).normalized;
    }
}
