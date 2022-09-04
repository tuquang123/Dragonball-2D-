﻿using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
    public static CameraFollow Instance;
    [Tooltip("Litmited the camera moving within this box collider")]
    //public Collider2D Bounds;
    public float limitLeft = -6;
    public float limitRight = 1000;
    public float limitDown = -3;
    public float limitUp = 1000;


	public float verticalOffset;
	public float lookAheadDstX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;
	public Vector2 focusAreaSize;

	[HideInInspector]
	public Vector2 _min, _max;
	public bool isFollowing{ get; set; }
    
	FocusArea focusArea;

	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirX;
	float smoothLookVelocityX;
	float smoothVelocityY;

	bool lookAheadStopped;
    [Header("ZOOM IN ZOOM OUT")]
    [Tooltip("Zoom Speed")]
    public bool allowAutoZoomIn = false;
    [Tooltip("How long player don't move to active zoom action")]
    public float timeDelay = 3f;
    public float speed = 10f;
    [Range(50, 100)]
    public float minPercent = 80;
   [ReadOnly] public float maxSize;
    [ReadOnly] public float minSize;
    float timeCounting = 0;

    public float zoomSpeed = 1;
    private bool isZooming = false;
    float originalSize, ZoomSize;
    Camera camera;
    
  [ReadOnly]  public bool manualControl = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start() {
        camera = GetComponent<Camera>();
        maxSize = camera.orthographicSize;
        minSize = maxSize * (minPercent / 100f);

        originalSize = camera.orthographicSize;

        focusArea = new FocusArea (GameManager.Instance.Player.controller.boxcollider.bounds, focusAreaSize);
        
        _min = new Vector2(limitLeft, limitDown);
        _max = new Vector2(limitRight, limitUp);
        isFollowing = true;
	}

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        timeCounting += Time.deltaTime;


        if (Input.anyKey || GameManager.Instance.Player.input != Vector2.zero)
        {
            timeCounting = 0;
        }

        //Debug.LogError(timeCounting);
    }

    void LateUpdate() {
        if (!manualControl)
            DoFollowPlayer();
	}

    public void DoFollowPlayer()
    {
        if (!isFollowing)
            return;

        focusArea.Update(GameManager.Instance.Player.controller.boxcollider.bounds);

        Vector2 focusPosition = focusArea.centre + Vector2.up * verticalOffset;

        if (focusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
            if (Mathf.Sign(GameManager.Instance.Player.controller.playerInput.x) == Mathf.Sign(focusArea.velocity.x) && GameManager.Instance.Player.controller.playerInput.x != 0)
            //if (Mathf.Sign(target) == Mathf.Sign(focusArea.velocity.x))
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDstX;
            }
            else
            {
                if (!lookAheadStopped)
                {
                    //lookAheadStopped = true;
                    targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
                }
            }
        }
        else if((GameManager.Instance.Player.isFacingRight && Mathf.Sign(currentLookAheadX) > 0) || (!GameManager.Instance.Player.isFacingRight && Mathf.Sign(currentLookAheadX) < 0))
        {
            targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
        }

        //ZOOM ZONE
        if (isZooming)
        {
            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, ZoomSize, zoomSpeed * Time.deltaTime);
        }
        else
        {
            if (timeCounting >= timeDelay && allowAutoZoomIn)
            {
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, minSize, speed * Time.deltaTime);
            }
            else
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, maxSize, speed * Time.deltaTime * 3);
        }

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);

        
            focusPosition += Vector2.right * currentLookAheadX;

        //var CameraHalfWidth = Camera.main.orthographicSize * ((float)Screen.width / Screen.height);
        focusPosition.x = Mathf.Clamp(focusPosition.x, _min.x + CameraHalfWidth, _max.x - CameraHalfWidth);
        focusPosition.y = Mathf.Clamp(focusPosition.y, _min.y + Camera.main.orthographicSize, _max.y - Camera.main.orthographicSize);

        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    public float CameraHalfWidth
    {
        get { return (Camera.main.orthographicSize * ((float)Screen.width / Screen.height)); }
    }


    void OnDrawGizmos() {
		Gizmos.color = new Color (1, 0, 0, .5f);
		Gizmos.DrawCube (focusArea.centre, focusAreaSize);
        Gizmos.color = Color.yellow;
        Vector2 boxSize = new Vector2(limitRight - limitLeft, limitUp - limitDown);
        Vector2 center = (new Vector2(limitRight + limitLeft, limitUp + limitDown)) * 0.5f;
        Gizmos.DrawWireCube(center, boxSize);

    }

	struct FocusArea {
		public Vector2 centre;
		public Vector2 velocity;
		float left,right;
		float top,bottom;


		public FocusArea(Bounds targetBounds, Vector2 size) {
			left = targetBounds.center.x - size.x/2;
			right = targetBounds.center.x + size.x/2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			velocity = Vector2.zero;
			centre = new Vector2((left+right)/2,(top +bottom)/2);
		}

		public void Update(Bounds targetBounds) {
			float shiftX = 0;
			if (targetBounds.min.x < left) {
				shiftX = targetBounds.min.x - left;
			} else if (targetBounds.max.x > right) {
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom) {
				shiftY = targetBounds.min.y - bottom;
			} else if (targetBounds.max.y > top) {
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;
			centre = new Vector2((left+right)/2,(top +bottom)/2);
			velocity = new Vector2 (shiftX, shiftY);
		}
	}

    public void ZoomIn(float value)
    {
        isZooming = true;
        ZoomSize = value;
    }

    public void ZoomOut()
    {
        isZooming = false;
    }
}
