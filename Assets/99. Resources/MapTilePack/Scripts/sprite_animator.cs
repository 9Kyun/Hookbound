using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sprite_animator : MonoBehaviour
{

    private SpriteRenderer spr;
    public List<Sprite> sprites;
    public float time_between_frames =0.1f;
    private int current_sprite;
    public bool loop =true;
    private Image image;

    void Start()
    {
        if (gameObject.GetComponent<Image>() != null)
        {
            image = gameObject.GetComponent<Image>();
        }
        else if (gameObject.GetComponent<SpriteRenderer>() != null)
        {
            spr = gameObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.Log("Missing Spriterenderer or Image component");
        }
    }

    float elapsed = 0f;
    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        if (elapsed >= time_between_frames)
        {
            elapsed = elapsed % time_between_frames;
            OutputTime();
        }
    }



    void OutputTime()
    {
        if(current_sprite < sprites.Count -1)
        {
            current_sprite = current_sprite + 1;
        }
        else if(loop)
        {
            current_sprite = 0;
        }
        if (gameObject.GetComponent<Image>() != null)
        {
            image.sprite = sprites[current_sprite];
            image.SetNativeSize();
        }
        else
        {
            spr.sprite = sprites[current_sprite];
        }

    }
}
