using Comfort.Common;
using EFT;
using EFT.CameraControl;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using static hazelify.RaccoonCam.Plugin;

namespace hazelify.RaccoonCam.Scripts
{
    public class CameraController : MonoBehaviour
    {
        // main required variables
        private GameObject _mainCamera;
        private Bodycam _bodycamScript;
        private Helmetcam _helmetcamScript;
        private GamePlayerOwner _gamePlayerOwner;
        private Player _player;

        // activation booleans
        public bool IsCameraActive => _bodycamScript.IsActive;
        public bool IsHelmetActive => _helmetcamScript.IsActive;


        // crosshair draws
        private Camera _camComponent;
        private GameObject _crosshairCanvas;
        private RectTransform _crosshairFoundation;
        private readonly RaycastHit[] _fireportHits = new RaycastHit[16];

        // duplicates for our referencing
        private Quaternion _smoothingMotion;
        private Quaternion _rotationOffset;
        private Transform _duplicateBone;
        private Transform _camDuplicate;
        private Transform _headDuplicate;
        private Quaternion _headOffset;

        // crosshair and references
        private float sharpness = 10f;
        private Transform _originalParent;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private static Sprite _ringSprite;

        // primary Unity related
        [UsedImplicitly]
        public void Start()
        {
            _mainCamera = GetLocalPlayerFromWorld().GetComponent<PlayerCameraController>().Camera.gameObject;
            if (_mainCamera == null)
            {
                Plugin.Logger.LogError("Failed to locate the main camera");
            }

            _bodycamScript = _mainCamera.AddComponent<Bodycam>();
            if (_bodycamScript == null)
            {
                Plugin.Logger.LogError("Failed to add Bodycam script to the main camera");
            }

            _helmetcamScript = _mainCamera.AddComponent<Helmetcam>();
            if (_helmetcamScript == null)
            {
                Plugin.Logger.LogError("Failed to add Helmetcam script to the main camera");
            }

            _gamePlayerOwner = GetLocalPlayerFromWorld().GetComponentInChildren<GamePlayerOwner>();
            if (_gamePlayerOwner == null)
            {
                Plugin.Logger.LogError("Failed to locate GamePlayerOwner");
            }

            var localPlayer = GetLocalPlayerFromWorld();
            if (localPlayer == null) return;

            _player = localPlayer;

            if (Plugin.bodyCam.Value)
                ToggleBodycamera();
            else if (Plugin.helmetCam.Value)
                ToggleHelmetcamera();
        }

        public void Update()
        {
            if (Plugin.bodyCamSwitch.Value.IsDown())
            {
                ToggleBodycamera();
            }

            if (Plugin.helmetCamSwitch.Value.IsDown())
            {
                ToggleHelmetcamera();
            }
        }

        void LateUpdate()
        {
            updateSmoothing();
            updateCrosshair();
        }
        // primary Unity related

        // smoothing
        private void updateSmoothing()
        {
            bool body   = _bodycamScript.IsActive   && Plugin.useBodycamSmoothen.Value;
            bool helmet = _helmetcamScript.IsActive && Plugin.useHelmetcamSmoothing.Value;

            // neither script is active or
            // neither smoothing option is enabled
            if (!body && !helmet) return;

            if (_duplicateBone == null   ||
                _headDuplicate == null   ||
                _camDuplicate == null    ||
                _player == null  ) return;

            // check via variable if the player is sprinting
            // to ensure the smoothing does most of its work then
            bool sprinting = _player.MovementContext.IsSprintEnabled;
            var target = sprinting ? _headDuplicate.rotation * _headOffset
                                   : _duplicateBone.rotation * _rotationOffset;

            // apply the smoothing through frames and assign the smoothing variable
            float timeExponent = 1f - Mathf.Exp(-sharpness * Time.deltaTime);
            _smoothingMotion = Quaternion.Slerp(_smoothingMotion, target, timeExponent);
            _camDuplicate.rotation = _smoothingMotion;
        }
        // smoothing

        // crosshair
        private static Color fetchCrosshairColor()
        {
            // fetching which crosshair color is used and
            // then giving the corresponding RGB value
            switch (Plugin.GlobalCrosshairColor.Value)
            {
                case Plugin.CrosshairColors.Green: return Color.green;
                case Plugin.CrosshairColors.Blue: return new Color(0.2f, 0.4f, 1f);
                case Plugin.CrosshairColors.Purple: return new Color(0.65f, 0.25f, 0.95f);
                case Plugin.CrosshairColors.Sky: return new Color(0.4f, 0.8f, 1f);
                case Plugin.CrosshairColors.White: return Color.white;
                case Plugin.CrosshairColors.Black: return Color.black;
                default: return Color.red; // red is default fallback should anything brokie (like me)
            }
        }

        private void updateCrosshair()
        {
            // checking for nulls or if either script is currently active;
            // draw no crosshair if neither case
            if ((!_bodycamScript.IsActive && !_helmetcamScript.IsActive) ||
                _player == null ||
                _camComponent == null ||
                _crosshairFoundation == null)
            {
                if (_crosshairFoundation != null) _crosshairFoundation.gameObject.SetActive(false);
                return;
            }

            // fetching the player's current firearmcontroller's hands
            // to find the weapon's muzzle, and if we do then
            // set the crosshair container's GameObject to active
            var fc = _player.HandsController as Player.FirearmController;
            var fireport = fc?.CurrentFireport;
            if (fireport == null)
            {
                _crosshairFoundation.gameObject.SetActive(false);
                return;
            }

            var origin = fireport.position;
            var dir = -fireport.up;

            // var dir = fireport.forward;
            var point = origin + dir * 500f;

            // sending raycasts from the fireport (muzzle of the weapon)
            // to determine where we are aiming (default is 16)
            int raycastFrequency = Physics.RaycastNonAlloc(origin, dir, _fireportHits, 500f, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            for (int i = 0; i < raycastFrequency; i++)
            {
                var hit = _fireportHits[i];
                if (hit.transform.IsChildOf(_player.Transform.Original)) continue;
                if (hit.distance < best) { best = hit.distance; point = hit.point; }
            }

            // ascertain two points between the fireport (muzzle of the weapon)
            // and the duplicated camera component to draw our crosshair
            // in 3D space
            var wtsp = _camComponent.WorldToScreenPoint(point);
            if (wtsp.z <= 0f)
            {
                // if we can't find any fireport point, fallback and disable
                _crosshairFoundation.gameObject.SetActive(false);
                return;
            }

            _crosshairFoundation.gameObject.SetActive(true);
            _crosshairFoundation.position = new Vector3(wtsp.x, wtsp.y, 0f);
        }

        private void createCrosshair()
        {
            // if no canvas was found, do nothing
            if (_crosshairCanvas != null) return;

            // creating a canvas for the crosshair
            _crosshairCanvas = new GameObject("RaccoonCam_CrosshairCanvas");
            var ccCanvas = _crosshairCanvas.AddComponent<Canvas>();
            ccCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            ccCanvas.sortingOrder = 1000;

            // creating a container for the crosshair to be put in (think cat in a box)
            var _crosshairContainer = new GameObject("RaccoonCam_CrosshairContainer", typeof(RectTransform));
            _crosshairContainer.transform.SetParent(_crosshairCanvas.transform, false);
            _crosshairFoundation = (RectTransform)_crosshairContainer.transform;

            // define crosshair variables
            const float chSize = 16f;           // stands for crosshair size
            const float lineThickness = 2f;     // well, line thickness lol
            const float centerSpace = 3f;       // traditional crosshair; define the empty space around the center
            const float chLineSize = 5f;        // traditional crosshair; length of each of the 4 crosshair points

            _crosshairFoundation.sizeDelta = new Vector2(16f, 16f);

            //float t = 2f;

            switch (Plugin.GlobalCrosshairStyle.Value)
            {
                case Plugin.Crosshairs.Square:
                    float e = chSize / 2f - lineThickness / 2f;

                    DrawCrosshairBounds(_crosshairFoundation, new Vector2(0, e), new Vector2(chSize, lineThickness));
                    DrawCrosshairBounds(_crosshairFoundation, new Vector2(0, -e), new Vector2(chSize, lineThickness));
                    DrawCrosshairBounds(_crosshairFoundation, new Vector2(-e, 0), new Vector2(lineThickness, chSize));
                    DrawCrosshairBounds(_crosshairFoundation, new Vector2(e, 0), new Vector2(lineThickness, chSize));

                    break;

                case Plugin.Crosshairs.Plus:
                    DrawCrosshairBounds(_crosshairFoundation, Vector2.zero, new Vector2(chSize, lineThickness));
                    DrawCrosshairBounds(_crosshairFoundation, Vector2.zero, new Vector2(lineThickness, chSize));

                    break;

                case Plugin.Crosshairs.Circle:
                    var crosshairCircle = new GameObject("RaccoonCam_CrosshairCircle", typeof(RectTransform));
                    crosshairCircle.transform.SetParent(_crosshairFoundation, false);

                    var ccImage = crosshairCircle.AddComponent<Image>();
                    ccImage.sprite = DrawRingSprite();
                    ccImage.color = fetchCrosshairColor();
                    ccImage.raycastTarget = false;

                    var ccTransform = (RectTransform)crosshairCircle.transform;
                    ccTransform.anchorMin = ccTransform.anchorMax = ccTransform.pivot = new Vector2(0.5f, 0.5f);
                    ccTransform.sizeDelta = new Vector2(chSize, chSize);

                    break;
                case Plugin.Crosshairs.Traditional:
                    float o = centerSpace + chLineSize / 2f;

                    DrawCrosshairBounds(_crosshairFoundation, new Vector2(0, o), new Vector2(lineThickness, chLineSize));
                    DrawCrosshairBounds(_crosshairFoundation, new Vector2(0, -o), new Vector2(lineThickness, chLineSize));
                    DrawCrosshairBounds(_crosshairFoundation, new Vector2(-o, 0), new Vector2(chLineSize, lineThickness));
                    DrawCrosshairBounds(_crosshairFoundation, new Vector2(o, 0), new Vector2(chLineSize, lineThickness));

                    break;
            }
        }

        private void DrawCrosshairBounds(Transform parent, Vector2 pos, Vector2 size)
        {
            // the 4 bounds for the crosshair;
            // creating an image first for color
            // and then then assign the crosshair's
            // transform
            var crosshairBound = new GameObject("RaccoonCam_CrosshairBound", typeof(RectTransform));
            crosshairBound.transform.SetParent(parent, false);
            var cbImage = crosshairBound.AddComponent<Image>();

            cbImage.color = fetchCrosshairColor();
            cbImage.raycastTarget = false;

            var cbTransform = (RectTransform)crosshairBound.transform;
            cbTransform.anchorMin = cbTransform.anchorMax = cbTransform.pivot = new Vector2(0.5f, 0.5f);
            cbTransform.anchoredPosition = pos;
            cbTransform.sizeDelta = size;
        }

        private static Sprite DrawRingSprite()
        {
            // if no ring sprite then we brokie (like me)
            if (_ringSprite != null) return _ringSprite;

            // define the properties of the circle's sprite
            const int size = 128;
            float ringThickness = size / 2f, inner = ringThickness - 10f;   // 5 = ring thickness in texture pixels
            var texture2D = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pxSize = new Color[size * size];

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - ringThickness, dy = y + 0.5f - ringThickness;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(ringThickness - d) * Mathf.Clamp01(d - inner); // soft edges
                    pxSize[y * size + x] = new Color(1f, 1f, 1f, a);
                }

            // assign and apply, then send
            texture2D.SetPixels(pxSize);
            texture2D.Apply();
            texture2D.filterMode = FilterMode.Bilinear;

            _ringSprite = Sprite.Create(texture2D, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _ringSprite;
        }

        private void destroyCrosshair()
        {
            // destroy crosshair to dispose
            if (_crosshairCanvas == null) return;

            Destroy(_crosshairCanvas);
            _crosshairCanvas = null;
            _crosshairFoundation = null;
        }
        // crosshair

        // primary plugin functions
        private void ToggleHelmetcamera()
        {
            var localPlayer = GetLocalPlayerFromWorld();
            if (localPlayer == null) return;

            if (!_helmetcamScript.IsActive)
            {
                SetPlayerToHelmetcamMode(localPlayer);
            }
            else
            {
                SetPlayerToFirstPersonMode(localPlayer);
            }
        }

        private void ToggleBodycamera()
        {
            var localPlayer = GetLocalPlayerFromWorld();
            if (localPlayer == null) return;

            if (!_bodycamScript.IsActive)
            {
                SetPlayerToBodycamMode(localPlayer);
            }
            else
            {
                SetPlayerToFirstPersonMode(localPlayer);
            }
        }

        private void SetPlayerToHelmetcamMode(Player localPlayer)
        {
            // big function, but we'll get there
            // first off we assign the camera to our
            // camera component for the duplicate;
            // after that we get the forward projection
            // and position of the main player cam's transform
            var cam = _mainCamera.transform;
            _camComponent = _mainCamera.GetComponent<Camera>();

            var fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            var position = new Vector3(_mainCamera.transform.position.x, _mainCamera.transform.position.y - 1f, _mainCamera.transform.position.z + 1f);

            // attempt to find the player's body;
            // necessary for property manipulation
            var playerBody = Traverse.Create(localPlayer).Field<PlayerBody>("_playerBody").Value;
            if (playerBody != null)
            {
                // assign pov then assigning all our duplicate variables
                // as well as assigning the originals for referencing later
                localPlayer.PointOfView = EPointOfView.ThirdPerson;

                var bone = localPlayer.PlayerBones.Head.Original;
                _headDuplicate = localPlayer.PlayerBones.Head.Original;
                _headOffset = Quaternion.Inverse(_headDuplicate.rotation) * cam.rotation;

                _rotationOffset = _headOffset;

                _duplicateBone = bone;
                _smoothingMotion = cam.rotation;
                _camDuplicate = _mainCamera.transform;

                _originalParent = cam.parent;
                _originalPosition = cam.localPosition;
                _originalRotation = cam.localRotation;

                var right = Vector3.Cross(Vector3.up, fwd);

                // setting XYZ coordinate placement
                // values for the helmet cam presets;
                // we start with 0 to have a base
                float X = 0f;
                float Y = 0f;
                float Z = 0f;

                if (Plugin.UseHelmetcamValues.Value)
                {
                    // if the option for using custom values is enabled,
                    // go here
                    switch (Plugin.helmetCamType.Value)
                    {
                        case HelmetCamTypes.Top:
                            X = Plugin.helmetCamTopX.Value;
                            Y = Plugin.helmetCamTopY.Value;
                            Z = Plugin.helmetCamTopZ.Value;
                            break;
                        case HelmetCamTypes.Left:
                            X = Plugin.helmetCamLeftX.Value;
                            Y = Plugin.helmetCamLeftY.Value;
                            Z = Plugin.helmetCamLeftZ.Value;
                            break;
                        case HelmetCamTypes.Right:
                            X = Plugin.helmetCamRightX.Value;
                            Y = Plugin.helmetCamRightY.Value;
                            Z = Plugin.helmetCamRightZ.Value;
                            break;
                        case HelmetCamTypes.AboveEyes:
                            X = Plugin.helmetCamAboveEyesX.Value;
                            Y = Plugin.helmetCamAboveEyesY.Value;
                            Z = Plugin.helmetCamAboveEyesZ.Value;
                            break;
                        default:
                            X = 0.04f; Y = 0.22f; Z = 0.0f;
                            break;
                    }
                }
                else
                {
                    // if the option for using custom values is disabled,
                    // go here; hardcoded values that I thought fit well
                    switch (Plugin.helmetCamType.Value)
                    {
                        case HelmetCamTypes.Top:
                            X = 0.04f; Y = 0.22f; Z = 0.0f;
                            break;
                        case HelmetCamTypes.Left:
                            X = 0.06f; Y = 0.1f; Z = -0.16f;
                            break;
                        case HelmetCamTypes.Right:
                            X = 0.06f; Y = 0.1f; Z = 0.17f;
                            break;
                        case HelmetCamTypes.AboveEyes:
                            X = 0.18f; Y = 0.08f; Z = 0.0f;
                            break;
                        default:
                            X = 0.04f; Y = 0.22f; Z = 0.0f;
                            break;
                    }
                }

                // setting position for the main player camera
                // and assigning its parent to the bone we want
                //             bone            X         Y                Z
                cam.position = bone.position + fwd * X + Vector3.up * Y + right * Z;
                cam.SetParent(bone, true);
            }
            else
            {
                Plugin.Logger.LogError("Failed to get the PlayerBody field");
            }

            // activate script and do the
            // crosshair if option is enabled
            _helmetcamScript.IsActive = true;

            if (Plugin.useCrosshair.Value)
                createCrosshair();
        }

        private void SetPlayerToBodycamMode(Player localPlayer)
        {
            // big function, but we'll get there
            // first off we assign the camera to our
            // camera component for the duplicate;
            // after that we get the forward projection
            // and position of the main player cam's transform
            var cam = _mainCamera.transform;
            _camComponent = _mainCamera.GetComponent<Camera>();

            var fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
            var position = new Vector3(cam.position.x, cam.position.y - 1f, cam.position.z + 1f);

            // attempt to find the player's body;
            // necessary for property manipulation
            var playerBody = Traverse.Create(localPlayer).Field<PlayerBody>("_playerBody").Value;
            if (playerBody != null)
            {
                // assign pov then assigning all our duplicate variables
                // as well as assigning the originals for referencing later
                localPlayer.PointOfView = EPointOfView.ThirdPerson;

                var bone = localPlayer.PlayerBones.Spine3.Original;
                _headDuplicate = localPlayer.PlayerBones.Head.Original;
                _headOffset = Quaternion.Inverse(_headDuplicate.rotation) * cam.rotation;

                // here we assign all the original variables to the
                // duplicates for use later; this was the easiest way
                // I found of dealing with finding a frame of
                // reference with the Player's body

                _duplicateBone = bone;
                _rotationOffset = Quaternion.Inverse(bone.rotation) * cam.rotation;
                _smoothingMotion = cam.rotation;
                _camDuplicate = _mainCamera.transform;

                _originalParent = cam.parent;
                _originalPosition = cam.localPosition;
                _originalRotation = cam.localRotation;

                var right = Vector3.Cross(Vector3.up, fwd);

                // setting XYZ coordinate placement
                // values for the body-worn cam presets;
                // we start with 0 to have a base
                float X = 0f;
                float Y = 0f;
                float Z = 0f;

                if (Plugin.UseBodycamValues.Value)
                {
                    // if the option for using custom values is enabled,
                    // go here
                    X = Plugin.bodyCamX.Value;
                    Y = Plugin.bodyCamY.Value;
                    Z = Plugin.bodyCamZ.Value;
                }
                else
                {
                    // if the option for using custom values is disabled,
                    // go here; I found these hardcoded values to work the best
                    if (Plugin.isSOInstalled)
                    {
                        X = 0.23f; // forward
                        Y = 0.06f; // up
                        Z = 0.04f; // right
                    }
                    else
                    {
                        X = 0.23f; // forward
                        Y = -0.02f; // up
                        Z = 0.11f; // right
                    }
                }

                // setting position for the main player camera
                // and assigning its parent to the bone we want
                //             bone            X         Y                Z
                cam.position = bone.position + fwd * X + Vector3.up * Y + right * Z;
                cam.SetParent(bone, true);
                cam.rotation = Quaternion.LookRotation(localPlayer.Transform.forward, Vector3.up);
            }
            else
            {
                Plugin.Logger.LogError("Failed to get the PlayerBody field");
            }

            // activate script and do the
            // crosshair if option is enabled
            _bodycamScript.IsActive = true;

            if (Plugin.useCrosshair.Value)
                createCrosshair();
        }

        private void SetPlayerToFirstPersonMode(Player localPlayer)
        {
            // here we do all things required to clean up,
            // dispose, and go back to first person pov
            _bodycamScript.IsActive = false;
            _helmetcamScript.IsActive = false;

            destroyCrosshair();

            var cam = _mainCamera.transform;
            cam.SetParent(_originalParent, false);
            cam.localPosition = _originalPosition;
            cam.localRotation = _originalRotation;

            localPlayer.PointOfView = EPointOfView.FirstPerson;
        }
        // primary plugin functions

        // helper function for player fetch
        private Player GetLocalPlayerFromWorld()
        {
            // simple helper function for fetching
            // the main player character from the
            // game world. thanks Terkoiz:
            // https://github.com/TerkoizLT/SPT-Freecam/blob/1a476d3f2c4e1d80992092e25c51b4282e00cfd3/project/Terkoiz.Freecam/FreecamController.cs#L227

            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null) return null;

            return gameWorld.MainPlayer;
        }
        // helper function for player fetch

        [UsedImplicitly]
        public void OnDestroy()
        {
            // destroying all relevant scriptery
            Destroy(_bodycamScript);
            Destroy(_helmetcamScript);
            destroyCrosshair();
        }
    }
}
