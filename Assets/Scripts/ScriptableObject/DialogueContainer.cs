using UnityEngine;

[CreateAssetMenu(fileName = "DialogueContainer", menuName = "Scriptable Objects/DialogueContainer")]
public class DialogueContainer : ScriptableObject
{
	public int AmountOfIntroSlides => _introSlides.Length;
	public string NarratorLine => _narratorLine;
	
    [SerializeField]
    [Multiline(3)]
    private string[] _introSlides;
    
    [SerializeField]
    [Multiline(3)]
    private string _kingRevealAllCorrectReply;
    
    [SerializeField]
    [Multiline(3)]
    private string _kingRevealReply;
    
    [SerializeField]
    private string _narratorLine;
    
    [Header(("Judgement"))]
    [SerializeField]
    [Multiline]
    private string[] _topScoreMessages;
    [SerializeField]
    [Multiline]
    private string[] _mediocreScoreMessages;
    [SerializeField]
    private int _topScoreValue;
    
    [SerializeField]
    [Multiline(3)]
    private string _finalQuestionAllCorrect;
    [SerializeField]
    [Multiline(3)]
    private string _finalQuestion;
    
    [SerializeField]
    private string _flatterTextAllCorrect;

    [SerializeField]
    private string _flatterText;

    public string GetIntroSlideText(int index)
    {
	    if (index < 0 || index >= _introSlides.Length)
	    {
		    return string.Empty;
	    }
	    
	    return _introSlides[index];
    }

    public string GetKingRevealText(bool allCorrect)
    {
	    return (allCorrect)  ? _kingRevealAllCorrectReply : _kingRevealReply;
    }

    public string GetScoreMessage(int score)
    {
	    var message = GetMessageBasedOnScore(score);
	    return string.Format(message, score);
    }

    private string GetMessageBasedOnScore(int score)
    {
	    if (score >= _topScoreValue)
	    {
		    return GetRandomMessage(_topScoreMessages);
	    }
	    
	    return GetRandomMessage(_mediocreScoreMessages);
    }

    public string GetFinalQuestion(bool allCorrect)
    {
	    return (allCorrect) ? _finalQuestionAllCorrect :  _finalQuestion;
    }

    public string GetFlatterLabelText(bool allCorrect)
    {
	    return (allCorrect) ? _flatterTextAllCorrect : _flatterText;
    }

    private string GetRandomMessage(string[] message)
    {
	    var randomindex = Random.Range(0, message.Length);
	    return message[randomindex];
    }
    
    
}
