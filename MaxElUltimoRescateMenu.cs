using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Menu principal interactivo para Max: El ultimo rescate.
/// Requiere MaxElUltimoRescatePrototype en la escena y las dos imagenes en Resources.
/// </summary>
public class MaxElUltimoRescateMenu : MonoBehaviour
{
    [Header("Imagenes")]
    [SerializeField] private Texture2D menuBackground;
    [SerializeField] private Texture2D maxCharacterPhoto;

    [Header("Audio opcional")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;

    private readonly string[] missionNames =
    {
        "Una segunda oportunidad", "El primer lugar seguro", "El corazon de un rescatista",
        "El nino que llamaba a su madre", "Cuando la tierra tiembla", "La montana no perdona",
        "Los hombres bajo la tierra", "El agua se los lleva todo", "El puente roto", "No te rindas",
        "La ultima llamada", "Doce vidas", "No voy a dejarte", "La decision de Max",
        "La promesa rota", "El silencio despues del fuego", "La busqueda que no termina",
        "La casa esta demasiado vacia", "El nombre que nunca olvidaran", "Algunos heroes nunca se van"
    };

    private readonly string[] missionSummaries =
    {
        "Max conoce a Alex y aprende a buscar agua en una noche lluviosa de Potosi.",
        "En su nuevo hogar, Max encuentra comida, una cama y paciencia.",
        "Max practica rastreo, obediencia y senales para el equipo de rescate.",
        "Max sigue el rastro de un nino perdido y guia a los rescatistas hasta el.",
        "En La Paz, Max busca sobrevivientes entre los edificios danados por el terremoto.",
        "Max rastrea a cuatro excursionistas durante una tormenta de nieve.",
        "Max entra a una mina y localiza a los trabajadores atrapados.",
        "Max busca familias en una inundacion y ayuda a rescatar a Alex.",
        "Un puente colapsa. Max busca una ruta alternativa para encontrar a Alex.",
        "Max descansa y se recupera con juegos tranquilos junto a Alex.",
        "Alex y Max preparan el equipo para un incendio industrial.",
        "Max busca a doce trabajadores en un edificio lleno de humo.",
        "Max y Alex intentan rescatar al ultimo trabajador mientras el edificio cede.",
        "Atrapado, Max pide a Alex que complete la evacuacion.",
        "Alex lleva al trabajador a la salida y se despide de Max.",
        "El edificio se derrumba despues de la evacuacion.",
        "Alex busca entre los escombros y encuentra el arnes y la bufanda de Max.",
        "En casa, Alex recorre los recuerdos que compartio con Max.",
        "El equipo y los sobrevivientes rinden homenaje a Max.",
        "Alex da una oportunidad a otro perro y mantiene vivo el legado de Max."
    };

    private MaxElUltimoRescatePrototype prototype;
    private Canvas menuCanvas;
    private Canvas gameHudCanvas;
    private GameObject modalRoot;
    private AudioSource musicSource;
    private AudioSource effectsSource;
    private int selectedMission;
    private bool hasStarted;
    private bool menuOpen = true;
    private float masterVolume = 0.8f;
    private Font uiFont;

    private void Awake()
    {
        prototype = GetComponent<MaxElUltimoRescatePrototype>();
        if (prototype == null)
            prototype = FindObjectOfType<MaxElUltimoRescatePrototype>();
        if (prototype == null)
            prototype = gameObject.AddComponent<MaxElUltimoRescatePrototype>();

        // El juego espera hasta que el jugador pulse JUGAR o CONTINUAR.
        prototype.enabled = false;

        if (menuBackground == null)
            menuBackground = Resources.Load<Texture2D>("MaxMenuBackground");
        if (maxCharacterPhoto == null)
            maxCharacterPhoto = Resources.Load<Texture2D>("MaxCharacterPhoto");

        uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        SetupAudio();
        EnsureEventSystem();
    }

    private void Start()
    {
        BuildMenuCanvas();
        BuildGameHud();
        gameHudCanvas.gameObject.SetActive(false);
        PlayMusic(menuMusic);
    }

    private void SetupAudio()
    {
        AudioListener.volume = masterVolume;
        if (FindObjectOfType<AudioListener>() == null)
            gameObject.AddComponent<AudioListener>();

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 1f;

        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.loop = false;
        effectsSource.spatialBlend = 0f;
    }

    private void EnsureEventSystem()
    {
        EventSystem existing = FindObjectOfType<EventSystem>();
        if (existing != null)
        {
            if (existing.GetComponent<BaseInputModule>() == null)
                existing.gameObject.AddComponent<StandaloneInputModule>();
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private Canvas CreateCanvas(string canvasName, int sortOrder)
    {
        GameObject canvasObject = new GameObject(canvasName, typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private void BuildMenuCanvas()
    {
        menuCanvas = CreateCanvas("Max - Menu interactivo", 20);
        RawImage background = CreateRawImage("Fondo del menu", menuCanvas.transform, menuBackground);
        StretchToParent(background.rectTransform);
        background.color = menuBackground == null ? new Color(0.055f, 0.07f, 0.09f, 1f) : Color.white;

        // La primera imagen ya muestra las opciones; estas zonas transparentes las vuelven clicables.
        AddMenuHotspot("Jugar", 24f, 135f, 201f, 175f, StartNewGame);
        AddMenuHotspot("Continuar", 24f, 176f, 184f, 211f, ContinueGame);
        AddMenuHotspot("Misiones", 24f, 212f, 183f, 245f, ShowMissions);
        AddMenuHotspot("Coleccionables", 24f, 246f, 194f, 279f, ShowCollectibles);
        AddMenuHotspot("Configuracion", 24f, 281f, 198f, 315f, ShowSettings);

        // Si la imagen de fondo falta, agrega un boton visible para que el menu siga siendo utilizable.
        if (menuBackground == null)
        {
            CreateText(menuCanvas.transform, "Fondo no asignado", "Agrega MaxMenuBackground.jpg a Assets/Resources.",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 42f), 22,
                TextAnchor.MiddleCenter, Color.white);
            CreateActionButton(menuCanvas.transform, "JUGAR", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -90f), new Vector2(260f, 54f), StartNewGame, true);
        }
    }

    private void BuildGameHud()
    {
        gameHudCanvas = CreateCanvas("Max - Retrato y pausa", 10);
        GameObject card = CreateImage("Retrato de Max", gameHudCanvas.transform,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-118f, -132f), new Vector2(196f, 226f),
            new Color(0.035f, 0.045f, 0.055f, 0.9f));

        RawImage portrait = CreateRawImage("Foto del personaje", card.transform, maxCharacterPhoto);
        SetRect(portrait.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 30f), new Vector2(148f, 166f));
        if (maxCharacterPhoto != null)
            portrait.uvRect = new Rect(0.14f, 0f, 0.86f, 0.72f);
        portrait.color = maxCharacterPhoto == null ? new Color(0.75f, 0.3f, 0.1f, 1f) : Color.white;

        CreateText(card.transform, "Nombre de Max", "MAX", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -62f), new Vector2(176f, 28f), 18, TextAnchor.MiddleCenter, new Color(1f, 0.74f, 0.28f));
        CreateActionButton(card.transform, "Volver al menu", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -94f), new Vector2(142f, 30f), ReturnToMenu, true);
    }

    private void AddMenuHotspot(string name, float xMin, float yMin, float xMax, float yMax, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(name + " - area interactiva", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(menuCanvas.transform, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(xMin / 1280f, 1f - yMax / 720f);
        rect.anchorMax = new Vector2(xMax / 1280f, 1f - yMin / 720f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = buttonObject.GetComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = true;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        SetButtonColors(button, 0.015f, 0.25f, 0.42f);
        button.onClick.AddListener(() =>
        {
            PlayEffect(buttonClickSound);
            action();
        });
        AddHoverSound(buttonObject);
    }

    private GameObject CreateActionButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action, bool visible)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = visible ? new Color(0.12f, 0.105f, 0.08f, 0.94f) : new Color(1f, 1f, 1f, 0.015f);
        image.raycastTarget = true;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        SetButtonColors(button, visible ? 0.94f : 0.015f, visible ? 0.98f : 0.25f, visible ? 1f : 0.42f);
        button.onClick.AddListener(() =>
        {
            PlayEffect(buttonClickSound);
            action();
        });

        if (visible)
            CreateText(buttonObject.transform, "Texto", label, Vector2.zero, Vector2.one, Vector2.zero,
                Vector2.zero, 17, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.45f));
        AddHoverSound(buttonObject);
        return buttonObject;
    }

    private void SetButtonColors(Button button, float normalAlpha, float hoverAlpha, float pressedAlpha)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 0.72f, 0.2f, normalAlpha);
        colors.highlightedColor = new Color(1f, 0.72f, 0.2f, hoverAlpha);
        colors.pressedColor = new Color(1f, 0.48f, 0.08f, pressedAlpha);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.transition = Selectable.Transition.ColorTint;
    }

    private void AddHoverSound(GameObject target)
    {
        EventTrigger trigger = target.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((BaseEventData eventData) => PlayEffect(buttonHoverSound));
        trigger.triggers = new List<EventTrigger.Entry> { entry };
    }

    private GameObject CreateImage(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        imageObject.GetComponent<Image>().color = color;
        return imageObject;
    }

    private RawImage CreateRawImage(string objectName, Transform parent, Texture texture)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(RawImage));
        imageObject.transform.SetParent(parent, false);
        RawImage image = imageObject.GetComponent<RawImage>();
        image.texture = texture;
        image.raycastTarget = false;
        return image;
    }

    private Text CreateText(Transform parent, string objectName, string value, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = uiFont;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void ShowMissions()
    {
        selectedMission = Mathf.Clamp(selectedMission, 0, missionNames.Length - 1);
        ShowModal("MISIONES", "Escena " + (selectedMission + 1) + " de " + missionNames.Length + "\n\n" +
            missionNames[selectedMission] + "\n" + missionSummaries[selectedMission],
            () =>
            {
                selectedMission = (selectedMission + missionNames.Length - 1) % missionNames.Length;
                ShowMissions();
            },
            () =>
            {
                selectedMission = (selectedMission + 1) % missionNames.Length;
                ShowMissions();
            });
    }

    private void ShowCollectibles()
    {
        ShowModal("COLECCIONABLES",
            "FOTOS\nRecuerdos visuales de las misiones.\n\nJUGUETES\nObjetos de los juegos y entrenamientos de Max.\n\nMEDALLAS\nReconocimientos del equipo de rescate.\n\nRECUERDOS\nMomentos de la historia que se desbloquean al avanzar.",
            null, null);
    }

    private void ShowSettings()
    {
        ShowModal("CONFIGURACION",
            "Ajusta el volumen general. Asigna clips de musica y sonidos en el componente MaxElUltimoRescateMenu.",
            null, null);

        GameObject sliderObject = new GameObject("Volumen general", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(modalRoot.transform, false);
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0f, -155f);
        sliderRect.sizeDelta = new Vector2(440f, 24f);

        GameObject track = CreateImage("Fondo del volumen", sliderObject.transform, new Vector2(0f, 0.35f),
            new Vector2(1f, 0.65f), Vector2.zero, Vector2.zero, new Color(0.2f, 0.2f, 0.2f, 1f));
        GameObject fill = CreateImage("Nivel del volumen", sliderObject.transform, new Vector2(0f, 0.35f),
            new Vector2(masterVolume, 0.65f), Vector2.zero, Vector2.zero, new Color(0.93f, 0.58f, 0.15f, 1f));
        GameObject handle = CreateImage("Control del volumen", sliderObject.transform,
            new Vector2(masterVolume, 0.5f), new Vector2(masterVolume, 0.5f), Vector2.zero,
            new Vector2(24f, 32f), new Color(1f, 0.82f, 0.43f, 1f));
        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.direction = Slider.Direction.LeftToRight;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.value = masterVolume;
        slider.onValueChanged.AddListener(SetMasterVolume);
        track.transform.SetAsFirstSibling();

        CreateText(modalRoot.transform, "Etiqueta de volumen", "VOLUMEN", new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -119f), new Vector2(440f, 32f), 18,
            TextAnchor.MiddleCenter, Color.white);
    }

    private void ShowModal(string title, string body, UnityEngine.Events.UnityAction previous,
        UnityEngine.Events.UnityAction next)
    {
        if (modalRoot != null)
            Destroy(modalRoot);

        modalRoot = new GameObject("Ventana " + title, typeof(RectTransform), typeof(Image));
        modalRoot.transform.SetParent(menuCanvas.transform, false);
        RectTransform overlayRect = modalRoot.GetComponent<RectTransform>();
        StretchToParent(overlayRect);
        modalRoot.GetComponent<Image>().color = new Color(0.015f, 0.02f, 0.035f, 0.78f);

        GameObject panel = CreateImage("Panel", modalRoot.transform, new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 470f), new Color(0.035f, 0.045f, 0.055f, 0.98f));
        CreateText(panel.transform, "Titulo", title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -58f), new Vector2(660f, 45f), 27, TextAnchor.MiddleCenter, new Color(1f, 0.74f, 0.28f));
        CreateText(panel.transform, "Contenido", body, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 12f), new Vector2(650f, 250f), 18, TextAnchor.MiddleCenter, Color.white);

        if (previous != null)
            CreateActionButton(panel.transform, "◀", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-230f, -153f), new Vector2(70f, 42f), previous, true);
        if (next != null)
            CreateActionButton(panel.transform, "▶", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(230f, -153f), new Vector2(70f, 42f), next, true);

        CreateActionButton(panel.transform, "VOLVER", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -190f), new Vector2(190f, 42f), CloseModal, true);
    }

    private void CloseModal()
    {
        if (modalRoot != null)
            Destroy(modalRoot);
        modalRoot = null;
    }

    private void SetMasterVolume(float value)
    {
        masterVolume = value;
        AudioListener.volume = value;
    }

    private void StartNewGame()
    {
        if (hasStarted)
            prototype.RestartCampaignFromMenu();
        hasStarted = true;
        EnterGame();
    }

    private void ContinueGame()
    {
        if (!hasStarted)
        {
            ShowModal("SIN PARTIDA GUARDADA", "Todavia no hay una partida para continuar. Pulsa JUGAR para comenzar la historia.",
                null, null);
            return;
        }
        EnterGame();
    }

    private void EnterGame()
    {
        CloseModal();
        menuOpen = false;
        menuCanvas.gameObject.SetActive(false);
        gameHudCanvas.gameObject.SetActive(true);
        prototype.enabled = true;
        PlayMusic(gameplayMusic);
    }

    private void ReturnToMenu()
    {
        menuOpen = true;
        prototype.enabled = false;
        gameHudCanvas.gameObject.SetActive(false);
        menuCanvas.gameObject.SetActive(true);
        CloseModal();
        PlayMusic(menuMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null)
            return;
        if (clip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }

        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
        musicSource.volume = 1f;
            musicSource.Play();
        }
        else if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    private void PlayEffect(AudioClip clip)
    {
        if (clip != null && effectsSource != null)
            effectsSource.PlayOneShot(clip);
    }

    private void Update()
    {
        if (menuOpen && Input.GetKeyDown(KeyCode.Escape) && modalRoot != null)
            CloseModal();
    }
}
