using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] PlayerMovement movements;
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;

    private int index;

    public Story currentStory;
    public bool dialogueIsPlaying { get; private set; }

    private static Dialogue instance;

    [Header("Speaker UI")]
    [SerializeField] private GameObject speakerNamePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Timer UI")]
    [SerializeField] private GameObject timerContainer;
    [SerializeField] private TextMeshProUGUI timerText;
    private Coroutine timerCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        dialogueIsPlaying = false;
        dialogueBox.SetActive(false);
        timerContainer.SetActive(false);
        textComponent.text = string.Empty;
        choicesText = new TextMeshProUGUI[choices.Length];
        int index2 = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index2] = choice.GetComponentInChildren<TextMeshProUGUI>();
            choice.SetActive(false);
            index2++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // return right away if dialogue isn't playing
        if (!dialogueIsPlaying)
        {

            return;
        }

        // handle continuing to the next line in the dialogue when Enter is pressed
        if (currentStory.currentChoices.Count == 0 && (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
        {
            if (textComponent.text == lines[index])
            {
                textComponent.text = string.Empty;
                ContinueStory();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
        }
        instance = this;
    }
    public static Dialogue GetInstance()
    {
        return instance;
    }
    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialogueBox.SetActive(true);
        index = 0;
        ContinueStory();
    }
    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            textComponent.text = string.Empty;
            lines = new[] { currentStory.Continue() };
            index = 0;
            StartCoroutine(TypeLine());
            HandleTags();
            DisplayChoices();
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
        }
    }
    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);
        movements.isAbleToMove = true;
        dialogueIsPlaying = false;
        dialogueBox.SetActive(false);
        textComponent.text = string.Empty;
    }
    IEnumerator TypeLine()
    {
        // Type each character 1 by 1
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void HandleTags()
    {
        List<string> tags = currentStory.currentTags;

        speakerNamePanel.SetActive(false);

        foreach (string tag in tags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                Debug.LogWarning("Tag could not be parsed: " + tag);
                continue;
            }

            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            if (tagKey == "speaker")
            {
                speakerNameText.text = tagValue;
                speakerNamePanel.SetActive(true);
            }
        }
    }
    public void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;
        if (currentChoices.Count > choices.Length) { Debug.LogError("More choices were given than the UI can support."); }

        int choiceButtonIndex = 0;
        foreach (Choice choice in currentChoices)
        {
            if (string.IsNullOrWhiteSpace(choice.text))
            {
                continue;
            }

            choices[choiceButtonIndex].gameObject.SetActive(true);
            choicesText[choiceButtonIndex].text = choice.text;

            Button button = choices[choiceButtonIndex].GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => MakeChoice(choice));

            choiceButtonIndex++;
        }

        for (int i = choiceButtonIndex; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        HandleTimerForChoices(currentChoices);

        StartCoroutine(SelectFirstChoice());
    }

    private void HandleTimerForChoices(List<Choice> currentChoices)
    {
        // Cari tag timer dan tag timeout_index dari baris dialog utama
        string timerTag = currentStory.currentTags.Find(tag => tag.StartsWith("timer:"));
        string timeoutIndexTag = currentStory.currentTags.Find(tag => tag.StartsWith("timeout_index:"));

        // Jika salah satu dari tag penting ini tidak ada, jangan jalankan timer
        if (timerTag == null || timeoutIndexTag == null)
        {
            return;
        }

        try
        {
            // Ambil durasi dari tag timer
            string durationString = timerTag.Split(':')[1].Trim();
            float duration = float.Parse(durationString);

            // Ambil index pilihan timeout dari tag timeout_index
            string indexString = timeoutIndexTag.Split(':')[1].Trim();
            int timeoutChoiceIndex = int.Parse(indexString);

            // Hentikan timer lama (jika ada) dan mulai timer baru dengan data yang sudah pasti benar
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(RunTimer(duration, timeoutChoiceIndex));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing timer tags: " + e.Message);
        }
    }   


    private IEnumerator RunTimer(float duration, int timeoutChoiceIndex) // Ubah nama parameter agar lebih jelas
    {
        if (timerText == null)
        {
            // Jika tidak ada timer text, jalankan timer tanpa visual
            yield return new WaitForSeconds(duration);
        }
        else
        {
            timerContainer.SetActive(true);
            float currentTime = duration;
            while (currentTime > 0f)
            {
                currentTime -= Time.deltaTime;
                timerText.text = Mathf.CeilToInt(currentTime).ToString();
                yield return null;
            }
            timerContainer.SetActive(false);
        }

        // Jika waktu habis dan ada pilihan timeout, pilih secara otomatis
        if (timeoutChoiceIndex != -1)
        {
            Debug.Log("Time's up! Selecting the timeout choice.");
            
            // Hentikan timer coroutine untuk mencegah pemanggilan ganda
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            
            // BERITAHU STORY UNTUK MEMILIH INDEX PILIHAN SECARA LANGSUNG
            currentStory.ChooseChoiceIndex(timeoutChoiceIndex);
            
            // Lanjutkan cerita setelah pilihan dibuat
            ContinueStory();
        }
    }

    private void MakeChoice(Choice choice)
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
            timerContainer.SetActive(false);
        }

        HideAllChoiceButtons();

        currentStory.ChooseChoiceIndex(choice.index);
        ContinueStory();
    }
    private IEnumerator SelectFirstChoice()
    {
        // Event System requires we clear it first, then wait 
        // for at least one frame before we set the current selected object.
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }
    private void HideAllChoiceButtons()
    {
        foreach (GameObject choiceButton in choices)
        {
            choiceButton.SetActive(false);
        }
    }
}