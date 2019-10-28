using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowSprite : MonoBehaviour
{
    public SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        if (sprite == null)
            sprite = gameObject.GetComponent<SpriteRenderer>();
        sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        sprite.receiveShadows = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPreRender()
    {
        if(sprite == null)
            sprite = gameObject.GetComponent<SpriteRenderer>();
        sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        sprite.receiveShadows = true;
    }
}
