using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextRevealer : MonoBehaviour
{
	[UnityEngine.Header("Configuration")]
	public int numCharactersFade = 8;
	public float charsPerSecond = 8;
	public float smoothSeconds = 0.8f;
	[UnityEngine.Header("References")]
	public UnityEvent allRevealed = new UnityEvent();

	TMPro.TextMeshProUGUI text;


	private string originalString;
	private int nRevealedCharacters;
	private bool isRevealing = false;


	public bool IsRevealing { get { return isRevealing; } }


	public void RestartWithText(string strText)
	{
		print(strText);
		nRevealedCharacters = 0;
		originalString = strText;
		text.text = BuildPartiallyRevealedString(originalString, keyCharIndex: -1, minIndex: 0, maxIndex: 0, fadeLength: 1);
	}

	public void ShowEverythingWithoutAnimation()
	{
		StopAllCoroutines();

		text.text = originalString;
		nRevealedCharacters = originalString.Length;
		isRevealing = false;

		allRevealed.Invoke();
	}

	public void ShowNextParagraphWithoutAnimation()
	{
		if (IsAllRevealed()) return;

		StopAllCoroutines();

		var paragraphEnd = GetNextParagraphEnd(nRevealedCharacters);
		text.text = BuildPartiallyRevealedString(original: originalString,
										keyCharIndex: paragraphEnd,
										minIndex: nRevealedCharacters,
										maxIndex: paragraphEnd,
										fadeLength: 0);

		nRevealedCharacters = paragraphEnd + 1;
		while (nRevealedCharacters < originalString.Length && originalString[nRevealedCharacters] == '\n')
			nRevealedCharacters += 1;

		if (IsAllRevealed())
			allRevealed.Invoke();

		isRevealing = false;
	}

	public void RevealNextParagraphAsync()//GameObject my_name, GameObject by)
	{
		StartCoroutine(RevealNextParagraph());//my_name, by));
	}
	public IEnumerator RevealNextParagraph()//GameObject my_name, GameObject by)
	{
		if (IsAllRevealed() || isRevealing) yield break;

		var paragraphEnd = GetNextParagraphEnd(nRevealedCharacters);
		if (paragraphEnd < 0) yield break;

		isRevealing = true;

		var keyChar = (float)(nRevealedCharacters - numCharactersFade);
		var keyCharEnd = paragraphEnd;
		var speed = 0f;
		var secondsElapsed = 0f;

		while (keyChar < keyCharEnd)
		{
			secondsElapsed += Time.deltaTime;
			if (secondsElapsed <= smoothSeconds)
				speed = Mathf.Lerp(0f, charsPerSecond, secondsElapsed / smoothSeconds);
			else
			{
				var secondsLeft = (keyCharEnd - keyChar) / charsPerSecond;
				if (secondsLeft < smoothSeconds)
					speed = Mathf.Lerp(charsPerSecond, 0.1f * charsPerSecond, 1f - secondsLeft / smoothSeconds);
			}

			keyChar = Mathf.MoveTowards(keyChar, keyCharEnd, speed * Time.deltaTime);
			text.text = BuildPartiallyRevealedString(original: originalString,
											keyCharIndex: keyChar,
											minIndex: nRevealedCharacters,
											maxIndex: paragraphEnd,
											fadeLength: numCharactersFade);

			yield return null;
		}

		nRevealedCharacters = paragraphEnd + 1;

		while (nRevealedCharacters < originalString.Length && originalString[nRevealedCharacters] == '\n')
			nRevealedCharacters += 1;

		if (IsAllRevealed())
			allRevealed.Invoke();

		isRevealing = false;
		//my_name.SetActive(true);
		//by.SetActive(true);
	}

	public bool IsAllRevealed()
	{
		return nRevealedCharacters >= originalString.Length;
	}


	private int GetNextParagraphEnd(int startingFrom)
	{
		var paragraphEnd = originalString.IndexOf('\n', startingFrom);
		if (paragraphEnd < 0 && startingFrom < originalString.Length) paragraphEnd = originalString.Length - 1;
		return paragraphEnd;
	}

	private string BuildPartiallyRevealedString(string original, float keyCharIndex, int minIndex, int maxIndex, int fadeLength)
	{
		var lastFullyVisibleChar = Mathf.Max(Mathf.CeilToInt(keyCharIndex), minIndex - 1);
		var firstFullyInvisibleChar = (int)Mathf.Min(keyCharIndex + fadeLength, maxIndex) + 1;

		var revealed = original.Substring(0, lastFullyVisibleChar + 1);
		var unrevealed = original.Substring(firstFullyInvisibleChar);

		var sb = new StringBuilder();
		sb.Append(revealed);

		for (var i = lastFullyVisibleChar + 1; i < firstFullyInvisibleChar; ++i)
		{
			var c = original[i];
			var originalColorRGB = ColorUtility.ToHtmlStringRGB(text.color);
			var alpha = Mathf.RoundToInt(255 * (keyCharIndex - i) / (float)fadeLength);
			sb.AppendFormat("<color=#{0}{1:X2}>{2}</color>", originalColorRGB, (byte)alpha, c);
			print(i);
		}


		sb.AppendFormat("<color=#00000000>{0}</color>", unrevealed);
		return sb.ToString();
	}


	public void RevealText(GameObject text_object)//, GameObject my_name, GameObject by)
	{
		text = text_object.GetComponent<TMPro.TextMeshProUGUI>();
		//if (string.IsNullOrEmpty(originalString))
		RestartWithText(text.text);

		RevealNextParagraphAsync();//my_name, by);
		
	}

}
