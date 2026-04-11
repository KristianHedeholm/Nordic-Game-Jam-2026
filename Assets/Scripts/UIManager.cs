using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls all UI panels. Attach to a persistent canvas object in the scene.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject introPanel;
    public GameObject loadingPanel;
    public GameObject guessPanel;
    public GameObject revealPanel;
    public GameObject finalJudgmentPanel;
    public GameObject winPanel;
    public GameObject deathPanel;

    [Header("Intro Panel")]
    public TMP_Text introText;
    public Button introStartButton;

    [Header("Guess Panel")]
    public TMP_Text riddleText;
    public TMP_Text categoryLabel;
    public Transform optionsContainer;   // Horizontal layout with buttons
    public Button optionButtonPrefab;

    [Header("Reveal Panel")]
    public TMP_Text revealText;
    public Button revealContinueButton;

    [Header("Final Judgment Panel")]
    public TMP_Text finalText;
    public Button flatterButton;
    public Button truthButton;

    [Header("Win Panel")]
    public TMP_Text winText;
    public Button winPlayAgainButton;

    [Header("Death Panel")]
    public TMP_Text deathText;
    public Button deathPlayAgainButton;

    void Start()
    {
        HideAll();
    }

    void HideAll()
    {
        introPanel?.SetActive(false);
        loadingPanel?.SetActive(false);
        guessPanel?.SetActive(false);
        revealPanel?.SetActive(false);
        finalJudgmentPanel?.SetActive(false);
        winPanel?.SetActive(false);
        deathPanel?.SetActive(false);
    }

    public void ShowIntro()
    {
        HideAll();
        introPanel.SetActive(true);
        introText.text = "The King demands your presence!\n\n" +
                         "He has dressed in the finest garments in all the land...\n" +
                         "or so he believes.\n\n" +
                         "Guess what he is wearing. Answer wisely.\n" +
                         "<i>Your head depends on it.</i>";

        introStartButton.onClick.RemoveAllListeners();
        introStartButton.onClick.AddListener(() => GameManager.Instance.GoToPhase(GamePhase.GuessClothing));
    }

    public void ShowLoading()
    {
        HideAll();
        loadingPanel.SetActive(true);
    }

    public void ShowGuessPanel(string category, string riddle, List<string> options, Action<string> onChosen)
    {
        HideAll();
        guessPanel.SetActive(true);

        categoryLabel.text = $"What is the King's <b>{category}</b>?";
        riddleText.text = $"\"{riddle}\"";

        // Clear old buttons
        foreach (Transform child in optionsContainer)
            Destroy(child.gameObject);

        // Spawn one button per option
        foreach (string option in options)
        {
            var btn = Instantiate(optionButtonPrefab, optionsContainer);
            btn.GetComponentInChildren<TMP_Text>().text = option;
            string captured = option;
            btn.onClick.AddListener(() => onChosen(captured));
        }
    }

    public void ShowReveal(string color, string clothing, string material)
    {
        HideAll();
        revealPanel.SetActive(true);

        revealText.text = $"The King steps forward...\n\n" +
                          $"You guessed: a <b>{color} {material} {clothing}</b>.\n\n" +
                          $"<i>The curtain falls.</i>\n\n" +
                          $"The King stands before you.\n\n" +
                          $"He is... <b>wearing nothing at all.</b>";

        revealContinueButton.onClick.RemoveAllListeners();
        revealContinueButton.onClick.AddListener(() => GameManager.Instance.GoToPhase(GamePhase.FinalJudgment));
    }

    public void ShowFinalJudgment()
    {
        HideAll();
        finalJudgmentPanel.SetActive(true);

        finalText.text = "The King beams at you with absolute confidence.\n\n" +
                         "\"Well?\" he says. \"What do you think of my magnificent outfit?\"\n\n" +
                         "<i>Choose your words carefully.</i>";

        flatterButton.GetComponentInChildren<TMP_Text>().text = "\"Your Majesty, you look absolutely divine!\"";
        truthButton.GetComponentInChildren<TMP_Text>().text = "\"...You're naked.\"";

        flatterButton.onClick.RemoveAllListeners();
        truthButton.onClick.RemoveAllListeners();

        flatterButton.onClick.AddListener(() => GameManager.Instance.OnPlayerFlatters());
        truthButton.onClick.AddListener(() => GameManager.Instance.OnPlayerTruth());
    }

    public void ShowWin()
    {
        HideAll();
        winPanel.SetActive(true);

        winText.text = "The King claps with delight!\n\n" +
                       "\"Yes! YES! You truly have the finest eyes in the kingdom!\"\n\n" +
                       "You survive. The King is happy.\n" +
                       "The kingdom is safe.\n\n" +
                       "<i>(He is still naked.)</i>";

        winPlayAgainButton.onClick.RemoveAllListeners();
        winPlayAgainButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }

    public void ShowDeath()
    {
        HideAll();
        deathPanel.SetActive(true);

        deathText.text = "The King's eyes narrow.\n\n" +
                         "\"NAKED?!\"\n\n" +
                         "\"GUARDS! OFF WITH THEIR HEAD!\"\n\n" +
                         "You told the truth.\n" +
                         "It didn't matter.\n\n" +
                         "<i>It never does.</i>";

        deathPlayAgainButton.onClick.RemoveAllListeners();
        deathPlayAgainButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }
}
