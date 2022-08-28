#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
using UnityEditor;
using UnityEditor.Events;
#endif
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NexPlayerAPI;
using NexUtility;


namespace NexPlayerSample
{
    public class NexUIController : MonoBehaviour
    {
        [SerializeField] NexPlayer nexPlayer;
        [SerializeField] NexUISprites sprites;
        private Vector3 lastMousePosition;

        public GameObject nexControlBar;//gameObjectToTogle
        [SerializeField] Transform nexButtons;
        [Header("Buttons")]
        [SerializeField] GameObject back;
        [SerializeField] GameObject languageCaptionSettings;
        [SerializeField] GameObject captionToggle;
        [SerializeField] GameObject maximizeScreen;
        [SerializeField] GameObject resizeScreen;
        [SerializeField] GameObject playPause;
        [SerializeField] GameObject stop;
        [SerializeField] GameObject offlineDownloadToggle;
        [SerializeField] GameObject offlineList;
        [SerializeField] GameObject loop;
        [SerializeField] GameObject volumeOptions;
        [Header("Panels")]
        [SerializeField] GameObject languageListPanel;
        [SerializeField] GameObject screenSizePanel;
        [SerializeField] GameObject downloadingPanel;
        [SerializeField] GameObject offlineListPanel;
        [SerializeField] GameObject volumeOptionPanel;
        [Header("Special Objects")]
        [SerializeField] GameObject seekBar;
        [SerializeField] NexSeekBar nexSeekBar;
        [SerializeField] GameObject offlineButtons;
        [SerializeField] OfflineDownloadButtons offlineDownloadButtons;
        [SerializeField] GameObject offlineDownloadContent;
        [SerializeField] OfflineStreaming offlineStreaming;
        [SerializeField] GameObject audioList;
        [SerializeField] AudioStreamListManager audioListManager;
        [SerializeField] GameObject captionList;
        [SerializeField] CloseCaptionStreamListManager captionListManager;
        [SerializeField] GameObject expiredMessage;

        [HideInInspector]
        public bool ReferencesFound = false;
        [HideInInspector]
        public bool ComponentsFound = false;
        [HideInInspector]
        public bool EventsAssigned = false;

        #region Methods Called From Editor
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX

        public void FixPrefab()
        {
            FindReferences();
            FindOrAddComponents();
            SetSprites();
            SetSortingOrders();
            DisableObjects();
        }
        public void Bind()
        {
            if (nexPlayer == null)
            {
                nexPlayer = FindObjectOfType<NexPlayer>();
                if (nexPlayer == null)
                    Debug.Log("There is not a NexPlayer instance in the Scene!");
            }
            else
                AddPersistantListeners();
        }
        void FindReferences()
        {
            try
            {
                nexPlayer = FindObjectOfType<NexPlayer>();
                if (nexPlayer == null)
                    Debug.Log("There is not a NexPlayer instance in the Scene!");

                nexControlBar = transform.GetChild(1).gameObject;
                // Buttons
                var controlCanvas = nexControlBar.transform.GetChild(0);
                nexButtons = controlCanvas.GetChild(2);
                back = controlCanvas.GetChild(1).gameObject;
                languageCaptionSettings = nexButtons.GetChild(0).gameObject;
                captionToggle = nexButtons.GetChild(1).gameObject;
                maximizeScreen = nexButtons.GetChild(2).gameObject;
                resizeScreen = nexButtons.GetChild(3).gameObject;
                playPause = nexButtons.GetChild(5).gameObject;
                stop = nexButtons.GetChild(6).gameObject;
                offlineButtons = nexButtons.GetChild(8).gameObject;
                offlineDownloadToggle = offlineButtons.transform.GetChild(0).gameObject;
                offlineList = offlineButtons.transform.GetChild(1).gameObject;
                loop = nexButtons.GetChild(9).gameObject;
                volumeOptions = nexButtons.GetChild(10).gameObject;
                // Panels
                var nexPanels = controlCanvas.GetChild(3);
                languageListPanel = nexPanels.GetChild(0).gameObject;
                audioList = languageListPanel.transform.GetChild(0).gameObject;
                captionList = languageListPanel.transform.GetChild(1).gameObject;
                screenSizePanel = nexPanels.GetChild(1).gameObject;
                downloadingPanel = nexPanels.GetChild(2).gameObject;
                offlineListPanel = nexPanels.GetChild(3).gameObject;
                offlineDownloadContent = offlineListPanel.transform.GetChild(0).GetChild(0).gameObject;
                volumeOptionPanel = nexPanels.GetChild(4).gameObject;
                var slider = volumeOptionPanel.transform.GetChild(0);
                slider.GetComponent<Slider>().fillRect = (RectTransform)slider.transform.GetChild(1).GetChild(0);
                slider.GetComponent<Slider>().handleRect = (RectTransform)slider.transform.GetChild(2).GetChild(0);
                // SeekBar
                seekBar = nexControlBar.transform.GetChild(1).GetChild(0).gameObject;
                seekBar.GetComponent<Slider>().fillRect = (RectTransform)seekBar.transform.GetChild(3).GetChild(0);
                seekBar.GetComponent<Slider>().handleRect = (RectTransform)seekBar.transform.GetChild(4).GetChild(0);
                expiredMessage = nexControlBar.transform.GetChild(3).gameObject;

                ReferencesFound = true;
            }
            catch (Exception e)
            {
                ReferencesFound = false;
                Debug.LogError("Find references failed, exception: " + e);
            }
        }
        void FindOrAddComponents()
        {
            try
            {
                // NexSeekBar
                nexSeekBar = AddComponentIfMissing<NexSeekBar>(seekBar);
                nexSeekBar.FindReferences(nexPlayer);
                // Offline
                offlineStreaming = AddComponentIfMissing<OfflineStreaming>(offlineDownloadContent);
                offlineDownloadButtons = AddComponentIfMissing<OfflineDownloadButtons>(offlineButtons);
                GameObject go = offlineStreaming.FindReferences(nexPlayer, offlineDownloadButtons);
                NexHolder nh = AddComponentIfMissing<NexHolder>(go);
                UnityEventTools.RemovePersistentListener(AddComponentIfMissing<Button>(go).onClick, nh.RunThisVideo);
                UnityEventTools.AddPersistentListener(AddComponentIfMissing<Button>(go).onClick, nh.RunThisVideo);
                GameObject progressBar = downloadingPanel.transform.GetChild(0).gameObject;
                DownloadProgressBar downloadProgressBar = AddComponentIfMissing<DownloadProgressBar>(progressBar);
                downloadProgressBar.FindReferences(nexPlayer);
                UnityEventTools.RemovePersistentListener(downloadProgressBar.downloadDone, offlineDownloadButtons.HandleDownloadDone);
                UnityEventTools.AddPersistentListener(downloadProgressBar.downloadDone, offlineDownloadButtons.HandleDownloadDone);
                downloadProgressBar.GetComponent<Slider>().fillRect = downloadProgressBar.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                offlineDownloadButtons.FindReferences(nexPlayer, downloadingPanel, offlineListPanel, progressBar, offlineStreaming, offlineDownloadToggle, this);
                // Audio & caption
                audioListManager = AddComponentIfMissing<AudioStreamListManager>(audioList.transform.GetChild(1).gameObject);
                audioListManager.FindReferences(nexPlayer);
                AddComponentIfMissing<NexAudioInfo>(audioListManager.transform.GetChild(0).gameObject);
                captionListManager = AddComponentIfMissing<CloseCaptionStreamListManager>(captionList.transform.GetChild(1).gameObject);
                captionListManager.FindReferences(nexPlayer);
                AddComponentIfMissing<NexCaptionInfo>(captionListManager.transform.GetChild(0).gameObject);

                ComponentsFound = true;
            }
            catch (Exception e)
            {
                ComponentsFound = false;
                Debug.LogError("Adding Components failed, exception" + e);
            }
        }
        void SetSprites()
        {
            if (sprites == null)
            {
                sprites = (NexUISprites)AssetDatabase.LoadAssetAtPath(NexPlayerFullFeatSampleFolderRoot.GetRelativePath() + "/NexPlayer/Resources/NexUISprites.asset", typeof(NexUISprites));
            }
            sprites.ResetReferences();
            //Buttons
            back.GetComponent<Image>().sprite = sprites.arrow;
            languageCaptionSettings.GetComponentInChildren<Image>().sprite = sprites.languageCaptionSettings;
            captionToggle.GetComponentInChildren<Image>().sprite = sprites.caption;
            captionToggle.GetComponentInChildren<Image>().color = Color.gray;
            maximizeScreen.GetComponent<Image>().sprite = sprites.maximizeScreen;
            maximizeScreen.GetComponent<Image>().material = sprites.whiteMat;
            resizeScreen.GetComponent<Image>().sprite = sprites.resizeScreen;
            screenSizePanel.transform.GetChild(0).GetComponent<Image>().sprite = sprites.originSize;
            screenSizePanel.transform.GetChild(1).GetComponent<Image>().sprite = sprites.maximizeScreen;
            screenSizePanel.transform.GetChild(2).GetComponent<Image>().sprite = sprites.fullScreen;
            screenSizePanel.transform.GetChild(3).GetComponent<Image>().sprite = sprites.fitVertically;
            screenSizePanel.transform.GetChild(4).GetComponent<Image>().sprite = sprites.fitHorizontal;
            playPause.GetComponent<Image>().sprite = sprites.play;
            stop.GetComponent<Image>().sprite = sprites.stop;
            offlineDownloadToggle.GetComponent<Image>().sprite = sprites.download;
            offlineList.GetComponent<Image>().sprite = sprites.offlineList;
            loop.GetComponent<Image>().sprite = sprites.loop;
            volumeOptions.GetComponent<Image>().sprite = sprites.audio;
            volumeOptionPanel.transform.GetChild(1).GetComponent<Image>().sprite = sprites.audio;
            // Panels
            Sprite background = sprites.Background;
            languageListPanel.transform.GetChild(0).GetComponent<Image>().sprite = background;
            languageListPanel.transform.GetChild(1).GetComponent<Image>().sprite = background;
            screenSizePanel.GetComponent<Image>().sprite = background;
            downloadingPanel.GetComponent<Image>().sprite = background;
            offlineListPanel.GetComponent<Image>().sprite = background;
            offlineListPanel.transform.GetChild(0).GetComponent<Image>().sprite = sprites.UiMask;
            offlineDownloadContent.transform.GetChild(0).GetComponent<Image>().sprite = sprites.uiButtonDefault;
            volumeOptionPanel.GetComponent<Image>().sprite = background;
            volumeOptionPanel.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = background;
            volumeOptionPanel.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().sprite = sprites.UiSprite;
            volumeOptionPanel.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Image>().sprite = sprites.Knob;
            // NexSeekBar
            seekBar.transform.GetChild(1).GetComponent<Image>().sprite = background;
            seekBar.transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = sprites.UiSprite;
            seekBar.transform.GetChild(3).GetChild(0).GetComponent<Image>().sprite = sprites.UiSprite;
            seekBar.transform.GetChild(4).GetChild(0).GetComponent<Image>().sprite = sprites.Knob;
        }
        void SetSortingOrders()
        {
            transform.GetChild(0).GetComponent<Canvas>().sortingOrder = 1;
            nexControlBar.transform.GetChild(0).GetComponent<Canvas>().sortingOrder = 2;
            nexControlBar.transform.GetChild(1).GetComponent<Canvas>().sortingOrder = 3;
            nexControlBar.transform.GetChild(2).GetComponent<Canvas>().sortingOrder = 4;
        }
        void DisableObjects()
        {
            nexButtons.GetChild(4).gameObject.SetActive(false);
            nexButtons.GetChild(7).gameObject.SetActive(false);
            languageListPanel.SetActive(false);
            screenSizePanel.SetActive(false);
            downloadingPanel.SetActive(false);
            offlineListPanel.SetActive(false);
            volumeOptionPanel.SetActive(false);
            expiredMessage.SetActive(false);
        }
        void AddPersistantListeners()
        {
            try
            {
                ClearPersistentListeners();
                UnityEventTools.AddPersistentListener(back.GetComponent<Button>().onClick, nexPlayer.ToggleQuit);
                // Main Buttons
                UnityEventTools.AddPersistentListener(languageCaptionSettings.GetComponent<Button>().onClick, ToggleLanguageList);
                UnityEventTools.AddPersistentListener(captionToggle.GetComponent<Toggle>().onValueChanged, nexPlayer.ToggleCaption);
                UnityEventTools.AddPersistentListener(maximizeScreen.GetComponent<Button>().onClick, nexPlayer.MaximizeScreen);
                UnityEventTools.AddPersistentListener(resizeScreen.GetComponent<Button>().onClick, ToggleScreenSize);
                UnityEventTools.AddPersistentListener(playPause.GetComponent<Button>().onClick, nexPlayer.TogglePlayPause);
                UnityEventTools.AddPersistentListener(stop.GetComponent<Button>().onClick, nexPlayer.Stop);
                UnityEventTools.AddPersistentListener(offlineDownloadToggle.GetComponent<Button>().onClick, offlineDownloadButtons.DownloadVideo);
                UnityEventTools.AddPersistentListener(offlineList.GetComponent<Button>().onClick, offlineDownloadButtons.EnableOfflineList);
                UnityEventTools.AddPersistentListener(loop.GetComponent<Button>().onClick, nexPlayer.ToggleLoop);
                UnityEventTools.AddPersistentListener(volumeOptions.GetComponent<Button>().onClick, ToggleVolumeOption);
                // SizePanel
                UnityEventTools.AddPersistentListener(screenSizePanel.transform.GetChild(0).GetComponent<Button>().onClick, OriginSize);
                UnityEventTools.AddPersistentListener(screenSizePanel.transform.GetChild(1).GetComponent<Button>().onClick, FitToScreen);
                UnityEventTools.AddPersistentListener(screenSizePanel.transform.GetChild(2).GetComponent<Button>().onClick, StretchToFullScreen);
                UnityEventTools.AddPersistentListener(screenSizePanel.transform.GetChild(3).GetComponent<Button>().onClick, FitVertically);
                UnityEventTools.AddPersistentListener(screenSizePanel.transform.GetChild(4).GetComponent<Button>().onClick, FitHorizontally);
                // VolumePanel
                UnityEventTools.AddPersistentListener(volumeOptionPanel.transform.GetChild(0).GetComponent<Slider>().onValueChanged, nexPlayer.SetVolume01);
                UnityEventTools.AddPersistentListener(volumeOptionPanel.transform.GetChild(1).GetComponent<Button>().onClick, nexPlayer.ToggleMute);
                // SeekBar
                //UnityEventTools.AddPersistentListener(seekBar.GetComponent<Slider>().onValueChanged, Seek);

                EditorUtility.SetDirty(languageCaptionSettings.GetComponent<Button>());
                nexPlayer.expiredMessage = expiredMessage;
                EventsAssigned = true;
            }
            catch (Exception e)
            {
                EventsAssigned = false;
                Debug.LogError("Adding Listeners failed, exception" + e);
            }

        }
        void ClearPersistentListeners()
        {
            UnityEventTools.RemovePersistentListener(back.GetComponent<Button>().onClick, nexPlayer.ToggleQuit);
            // Main Buttons
            UnityEventTools.RemovePersistentListener(languageCaptionSettings.GetComponent<Button>().onClick, ToggleLanguageList);
            UnityEventTools.RemovePersistentListener<bool>(captionToggle.GetComponent<Toggle>().onValueChanged, nexPlayer.ToggleCaption);
            UnityEventTools.RemovePersistentListener(maximizeScreen.GetComponent<Button>().onClick, nexPlayer.MaximizeScreen);
            UnityEventTools.RemovePersistentListener(resizeScreen.GetComponent<Button>().onClick, ToggleScreenSize);
            UnityEventTools.RemovePersistentListener(playPause.GetComponent<Button>().onClick, nexPlayer.TogglePlayPause);
            UnityEventTools.RemovePersistentListener(stop.GetComponent<Button>().onClick, nexPlayer.Stop);
            UnityEventTools.RemovePersistentListener(offlineDownloadToggle.GetComponent<Button>().onClick, offlineDownloadButtons.DownloadVideo);
            UnityEventTools.RemovePersistentListener(offlineList.GetComponent<Button>().onClick, offlineDownloadButtons.EnableOfflineList);
            UnityEventTools.RemovePersistentListener(loop.GetComponent<Button>().onClick, nexPlayer.ToggleLoop);
            UnityEventTools.RemovePersistentListener(volumeOptions.GetComponent<Button>().onClick, ToggleVolumeOption);
            // SizePanel
            UnityEventTools.RemovePersistentListener(screenSizePanel.transform.GetChild(0).GetComponent<Button>().onClick, OriginSize);
            UnityEventTools.RemovePersistentListener(screenSizePanel.transform.GetChild(1).GetComponent<Button>().onClick, FitToScreen);
            UnityEventTools.RemovePersistentListener(screenSizePanel.transform.GetChild(2).GetComponent<Button>().onClick, StretchToFullScreen);
            UnityEventTools.RemovePersistentListener(screenSizePanel.transform.GetChild(3).GetComponent<Button>().onClick, FitVertically);
            UnityEventTools.RemovePersistentListener(screenSizePanel.transform.GetChild(4).GetComponent<Button>().onClick, FitHorizontally);
            // VolumePanel
            UnityEventTools.RemovePersistentListener<float>(volumeOptionPanel.transform.GetChild(0).GetComponent<Slider>().onValueChanged, nexPlayer.SetVolume01);
            UnityEventTools.RemovePersistentListener(volumeOptionPanel.transform.GetChild(1).GetComponent<Button>().onClick, nexPlayer.ToggleMute);
            // SeekBar
            //UnityEventTools.RemovePersistentListener<float>(seekBar.GetComponent<Slider>().onValueChanged, Seek);

            EditorUtility.SetDirty(languageCaptionSettings.GetComponent<Button>());
        }

        public void FillNexPlayerUIReferences()
        {
            nexPlayer.FillUI();
        }
#endif
        #endregion

        #region NexUIFunctionality
        void Start()
        {
            DisableForWinMacWeb();
        }

        void Update()
        {
            if (PointerIsNotOverUI() && HasThePointerBeingClicked())
            {
                ToogleUI();
            }
        }

        /// <summary>
        /// Toggles the UI visibility taking into account the VR mode
        /// </summary>
        private void ToogleUI()
        {
            nexControlBar.SetActive(!nexControlBar.activeSelf);
        }
        private bool IsUIVisible()
        {
            return nexControlBar.activeSelf;
        }
        /// <summary>
        /// Informs if cardboard is present in the build
        /// </summary>
        private bool DoesTheBuildSupportCardboard()
        {
#if UNITY_5_6_OR_NEWER
            return System.Array.Exists(UnityEngine.XR.XRSettings.supportedDevices, s => s.ToLower().Contains("cardboard"));
#else
            return false;
#endif
        }
        /// <summary>
        /// Informs whether the mouse has been clicked or not
        /// </summary>
        private bool HasThePointerBeingClicked()
        {
            bool hasThePointerBeingClicked = false;
            if (Input.GetButtonDown("Fire1"))
            {
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                hasThePointerBeingClicked = Vector3.Distance(lastMousePosition, Input.mousePosition) < 10;
            }

            return hasThePointerBeingClicked;
        }
        /// <summary>
        /// Provides information to know if the pointer or touch input is over the game objects without being blocked by any UI element
        /// </summary>
        /// <returns>true if no UI element is blocking the pointer our touch</returns>
        private bool PointerIsNotOverUI()
        {
            bool isOverGameObject = true;
            if (!UnityEngine.XR.XRSettings.enabled)
            {
                if (Input.touchCount > 0)
                {
                    Touch[] touches = Input.touches;
                    int i = 0;
                    while (isOverGameObject && i < touches.Length)
                    {
                        if (EventSystem.current.IsPointerOverGameObject(touches[i].fingerId))
                        {
                            isOverGameObject = false;
                        }
                        i++;
                    }
                }
                else
                {
                    isOverGameObject = !EventSystem.current.IsPointerOverGameObject();
                }
            }
            return isOverGameObject;
        }

        public void DisableForWinMacWeb()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                offlineDownloadToggle.SetActive(false);
                offlineList.SetActive(false);
                resizeScreen.SetActive(false);
            }

            else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                offlineDownloadToggle.SetActive(false);
                offlineList.SetActive(false);
                resizeScreen.SetActive(false);
            }

            else if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                offlineDownloadToggle.SetActive(false);
                offlineList.SetActive(false);
                languageCaptionSettings.SetActive(false);
                resizeScreen.SetActive(false);
                captionToggle.SetActive(false);
                resizeScreen.SetActive(false);
                loop.SetActive(false);
            }
        }
        #endregion

        #region Event Callbacks
        private void TogglePanel(GameObject panel)
        {
            if (panel == null)
                return;

            if (panel.activeSelf)
            {
                panel.SetActive(false);
            }
            else
            {
                HideAllPanels();
                panel.SetActive(true);
            }
        }
        public void HideAllPanels()
        {
            languageListPanel.SetActive(false);
            screenSizePanel.SetActive(false);
            downloadingPanel.SetActive(false);
            offlineListPanel.SetActive(false);
            volumeOptionPanel.SetActive(false);
        }
        public void ToggleLanguageList()
        {
            TogglePanel(languageListPanel);
        }
        public void ToggleScreenSize()
        {
            TogglePanel(screenSizePanel);
        }
        public void ToggleVolumeOption()
        {
            TogglePanel(volumeOptionPanel);
        }
        public void OriginSize()
        {
            nexPlayer.ChangeAspectRatio(VideoAspectRatio.OriginSize);
        }
        public void FitToScreen()
        {
            nexPlayer.ChangeAspectRatio(VideoAspectRatio.FitToScreen);
        }
        public void StretchToFullScreen()
        {
            nexPlayer.ChangeAspectRatio(VideoAspectRatio.StretchToFullScreen);
        }
        public void FitVertically()
        {
            nexPlayer.ChangeAspectRatio(VideoAspectRatio.FitVertically);
        }
        public void FitHorizontally()
        {
            nexPlayer.ChangeAspectRatio(VideoAspectRatio.FitHorizontally);
        }
        public void Seek(float value)
        {
            nexPlayer.Seek();
        }

        public void AllowSeek(bool value)
        {
            nexPlayer.allowSeek = value;

            if (nexPlayer.allowSeek)
            {
                nexPlayer.Seek();
            }
        }
        #endregion

        #region Helpers
        private T AddComponentIfMissing<T>(GameObject go) where T : Component
        {
            T result = go.GetComponent<T>();
            if (result == null)
            {
                result = go.AddComponent<T>() as T;
            }
            return result;
        }
        #endregion
    }
}