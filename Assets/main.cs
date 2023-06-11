using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class main : MonoBehaviour
{

    public Button takeButton, passButton, exitButton;

    public Button[] playerSelectButtons, firstPlayerButtons, cardButtons;

    public TextMeshProUGUI cardsRemainingText, flippedCardText, playedTokensText, aiValueText;

    public TextMeshProUGUI[] playerTokensTexts, playerHandValueTexts;

    public GameObject miscGameObjects, singleTemplate, stackTemplate;

    public GameObject[] activePlayerIndicators, playerHandParents;

    public List<GameObject>[] renderedPlayerHands;

    public int cardsRemovedFromPlay;

    int playerCount, cardsRemaining, flippedCard, playedTokenCount, activePlayer, takeI = 0, passI = 1;

    int[] playerTokenCounts, playerHandValues;

    List<int>[] playerOwnedCards;

    void Start()
    {
        init();
    }

    void init()
    {
        hideTexts();
        miscGameObjects.SetActive(false);
        setupButtons();
        cardsRemaining = 33 - cardsRemovedFromPlay;
    }

    void hideTexts()
    {
        cardsRemainingText.text = "";
        flippedCardText.text = "";
        playedTokensText.text = "";
        aiValueText.text = "";
        foreach (var t in playerTokensTexts)
        {
            t.text = "";
        }
        foreach (var t in playerHandValueTexts)
        {
            t.text = "";
        }
        foreach(var s in activePlayerIndicators)
        {
            s.gameObject.SetActive(false);
        }
    }

    void setupButtons()
    {
        for (var i = 0; i < 4; i++)
        {
            var j = i;
            playerSelectButtons[j].gameObject.SetActive(true);
            playerSelectButtons[j].onClick.RemoveAllListeners();
            playerSelectButtons[j].onClick.AddListener(delegate { playerCountSelect(j); });
        }

        for (var i = 0; i < 4; i++)
        {
            var j = i;
            firstPlayerButtons[j].gameObject.SetActive(false);
            firstPlayerButtons[j].onClick.RemoveAllListeners();
            firstPlayerButtons[j].onClick.AddListener(delegate { firstPlayerSelect(j); });
        }

        for (var i = 0; i < 33; i++)
        {
            var j = i;
            cardButtons[j].gameObject.SetActive(false);
            cardButtons[j].onClick.RemoveAllListeners();
            cardButtons[j].onClick.AddListener(delegate { cardButtonPressed(j); });
        }

        takeButton.gameObject.SetActive(false);
        takeButton.onClick.RemoveAllListeners();
        takeButton.onClick.AddListener(take);

        passButton.gameObject.SetActive(false);
        passButton.onClick.RemoveAllListeners();
        passButton.onClick.AddListener(pass);

        exitButton.gameObject.SetActive(true);
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(exit);
    }

    void exit()
    {
        Application.Quit();
    }

    void playerCountSelect(int p)
    {
        playerCount = p+1;
        foreach (var button in playerSelectButtons)
        {
            button.gameObject.SetActive(false);
        }
        playerHandValues = new int[playerCount];
        playerOwnedCards = new List<int>[playerCount];
        playerTokenCounts = new int[playerCount];
        renderedPlayerHands = new List<GameObject>[playerCount];
        for(int i = 0; i < playerCount; i++)
        {
            playerTokenCounts[i] = 11;
            playerOwnedCards[i] = new List<int>();
            renderedPlayerHands[i] = new List<GameObject>();
        }
        Debug.Log(playerCount + " players selected.");
        promptFirstPlayer();
    }

    void promptFirstPlayer()
    {
        for (var i = 0; i < playerCount; i++)
        {
            firstPlayerButtons[i].gameObject.SetActive(true);
        }
    }

    void firstPlayerSelect(int p)
    {
        activePlayer = p;
        foreach (var button in firstPlayerButtons)
        {
            button.gameObject.SetActive(false);
        }
        Debug.Log("Player " + (activePlayer + 1) + " is first. Showing and starting game.");
        showGame();
        startGame();
    }

    void showGame()
    {
        miscGameObjects.SetActive(true);
    }

    void hideGame()
    {
        miscGameObjects.SetActive(false);
    }

    void take()
    {
        playerOwnedCards[activePlayer].Add(flippedCard);
        playerTokenCounts[activePlayer] += playedTokenCount;
        playedTokenCount = 0;
        drawHand(activePlayer);
        if (cardsRemaining == 0)
        {
            aiValueText.text = "";
            flippedCard = 0;
            updateHandValues();
            updateText();
            foreach (var s in activePlayerIndicators)
            {
                s.gameObject.SetActive(false);
            }
            takeButton.gameObject.SetActive(false);
            passButton.gameObject.SetActive(false);
        }
        else
        {
            promptNewFlippedCard();
        }
    }

    void drawHand(int p)
    {
        var cards = playerOwnedCards[p];
        cards.Sort();

        foreach(var v in renderedPlayerHands[p])
        {
            Destroy(v);
        }

        renderedPlayerHands[p].Clear();

        List<(int f, int l)> stacks = new List<(int f, int l)>();
        List<int> singles = new List<int>();

        int tempMin = 0;
        foreach (var card in cards)
        {
            if (!cards.Contains(card + 1))
            {
                if(tempMin == 0)
                {
                    singles.Add(card);
                } else
                {
                    stacks.Add((tempMin, card));
                    tempMin = 0;
                }
            } else if (tempMin == 0)
            {
                tempMin = card;
            }
        }

        foreach(var s in stacks)
        {
            Debug.Log(cardsRemaining + ": Player " + (activePlayer + 1) + " has the run " + s.f + " to " + s.l + ".");

            GameObject newCard = Instantiate(stackTemplate, new Vector3(0, 0, 0), Quaternion.identity);
            newCard.GetComponent<stack_scr>().setNumbers(s.f, s.l);
            newCard.GetComponent<stack_scr>().updateNumbers();
            newCard.transform.SetParent(playerHandParents[p].transform, false);

            renderedPlayerHands[p].Add(newCard);
        }
        foreach (var s in singles)
        {
            Debug.Log(cardsRemaining + ": Player " + (activePlayer + 1) + " has the stand-alone card " + s + ".");

            GameObject newCard = Instantiate(singleTemplate, new Vector3(0,0,0), Quaternion.identity);
            newCard.GetComponent<single_card_scr>().setNumber(s);
            newCard.GetComponent<single_card_scr>().updateNumber();
            newCard.transform.SetParent(playerHandParents[p].transform, false);
            
            renderedPlayerHands[p].Add(newCard);

        }

        Debug.Log("There should be "+renderedPlayerHands[p].Count+" prefabs.");

        for(var i = 0; i < renderedPlayerHands[p].Count; i++)
        {
            var card = renderedPlayerHands[p][i];
            if (p % 2 == 0)
            {
                if (i < 9) {
                    card.transform.localPosition = new Vector3(i*75,0,0);
                } else
                {
                    card.transform.localPosition = new Vector3((i-9) * 75, p==0?135:-135, 0);
                }
            } else
            {
                if (i < 5)
                {
                    card.transform.localPosition = new Vector3(0, i*-135, 0);
                }
                else
                {
                    card.transform.localPosition = new Vector3(p == 1 ? 75 : -75, (i-5)*-135, 0);
                }
            }
            card.SetActive(true);
        }
    }

    void pass()
    {
        playerTokenCounts[activePlayer]--;
        playedTokenCount++;
        activePlayer++;
        if (activePlayer >= playerCount) { activePlayer = 0; }
        turn();
    }

    void promptNewFlippedCard()
    {
        aiValueText.text = "";
        hideGame();
        cardsRemaining--;
        if (cardsRemaining != 32 - cardsRemovedFromPlay)
        {
            for (var i = 0; i < 33; i++)
            {
                if (!playerOwnedCards.Any(p => p.Contains(i + 3)))
                {
                    cardButtons[i].gameObject.SetActive(true);
                }
            }
        } else
        {
            foreach(var c in cardButtons)
            {
                c.gameObject.SetActive(true);
            }
        }
    }

    void cardButtonPressed(int c)
    {
        foreach (var card in cardButtons)
        {
            card.gameObject.SetActive(false);
        }
        flippedCard = c + 3;
        showGame();
        turn();
    }

    void startGame()
    {
        promptNewFlippedCard();
    }

    void updateText()
    {
        aiValueText.text = "";
        flippedCardText.text = flippedCard.ToString();
        playedTokensText.text = playedTokenCount.ToString() + "●";
        cardsRemainingText.text = cardsRemaining.ToString();
        for (var i = 0; i < playerCount; i++)
        {
            playerTokensTexts[i].text = playerTokenCounts[i].ToString() + "●";
            playerHandValueTexts[i].text = playerHandValues[i].ToString() + " pts";
        }

        foreach (var s in activePlayerIndicators)
        {
            s.SetActive(false);
        }
        activePlayerIndicators[activePlayer].SetActive(true);

    }

    void turn()
    {
        updateHandValues();
        updateText();
        if(activePlayer == 0) { aiTurn(); } else
        {
            takeButton.gameObject.SetActive(true);
            passButton.gameObject.SetActive(playerTokenCounts[activePlayer] != 0);
        }
    }

    void aiTurn()
    {
        takeButton.gameObject.SetActive(false);
        passButton.gameObject.SetActive(false);

        var valueOfTaking = getValueOfTaking(flippedCard);
        var valueOfPassing = getValueOfPassing(flippedCard, valueOfTaking);

        if (playerTokenCounts[0] == 0)
        {
            aiValueText.text = "Must take. Take: " + valueOfTaking.ToString("0.000") + ". Pass: " + valueOfPassing.ToString("0.000") + ".";
            instruct(takeI);
            return;
        }

        aiValueText.text = "Take: " + valueOfTaking.ToString("0.000") + ". Pass: " + valueOfPassing.ToString("0.000") + ".";

        if(valueOfPassing < valueOfTaking)
        {
            instruct(passI);
        } else {
            instruct(takeI);
        }

    }

    void instruct(int i)
    {
        if (i == takeI)
        {
            takeButton.gameObject.SetActive(true);
        } else
        {
            passButton.gameObject.SetActive(true);
        }
    }

    float getValueOfTaking(int card)
    {
        float value = card;
        var hand = playerOwnedCards[0];
        var tokenCount = playerTokenCounts[0];
        var currentValue = playerHandValues[0];
        var potentialHand = new List<int>();
        potentialHand.AddRange(hand);
        potentialHand.Add(card);

        var valueWithNewCard = getValueOfHand(potentialHand, tokenCount + playedTokenCount);
        var risk = false;
        var riskyValue = 0;
        float chanceOfSingleCard = (float)(cardsRemaining)/((float)cardsRemaining+(float)cardsRemovedFromPlay);

        Debug.Log("Cards remaining: " + cardsRemaining + ". Chance of single card: " + (chanceOfSingleCard * 100f).ToString("0.00"));


        if (hand.Contains(card + 2) && !playerOwnedCards.Any(p=>p.Contains(card+1)))
        {
            potentialHand.Add(card+1);
            risk = true;
            riskyValue = getValueOfHand(potentialHand, tokenCount + playedTokenCount);
            Debug.Log("Hand contains card two above. The risky value is " + riskyValue.ToString("0.00") + " and the chance of that card being in the deck is " + (chanceOfSingleCard * 100f).ToString("0.00") + "%."); ;
        } else if (hand.Contains(card - 2) && !playerOwnedCards.Any(p => p.Contains(card - 1))) {
            potentialHand.Add(card - 1);
            risk = true;
            riskyValue = getValueOfHand(potentialHand, tokenCount + playedTokenCount)+2;
            Debug.Log("Hand contains card two below. The risky value is " + riskyValue.ToString("0.00") + " and the chance of that card being in the deck is " + (chanceOfSingleCard * 100f).ToString("0.00") + "%.");

        }

        var cardValues = (valueWithNewCard - currentValue) * (risk ? 1 - chanceOfSingleCard : 1f) + (riskyValue - currentValue) * (risk ? chanceOfSingleCard : 0f);
        var tokenValues = (tokenCount < 6 && cardsRemaining>5 ? 0.75f : (tokenCount < 16 && cardsRemaining > 5 ? 0.5f : 0.25f)) * playedTokenCount;

        value = cardValues - tokenValues;

        Debug.Log("Value from card: " + cardValues.ToString("0.00"));
        Debug.Log("Value from tokens: " + (-1*tokenValues).ToString("0.00"));

        return value;
    }
    float getValueOfPassing(int card, float taking)
    {
        float returnChance = chanceOfReturn(card);

        Debug.Log("Return chance: " + (returnChance * 100f).ToString("0.00") + " %.");

        float value = (-1*playerCount+1+ taking) * returnChance;

        float risk = 1 - returnChance + (returnChance-1)* taking;


        return value+risk;
    }

    float chanceOfReturn(int card)
    {
        float value = card - 1.25f*playedTokenCount;

        if (playerTokenCounts.Any(p => p == 0))
        {
            return 0;
        }

        if (playerOwnedCards.Any(p => p.Contains(card + 1))){
            if (!playerOwnedCards[0].Contains(card + 1)) return 0;
        }

        if (playerOwnedCards.Any(p => p.Contains(card + 2)))
        {
            if (!playerOwnedCards[0].Contains(card + 2)) return 1-1/(1+value);
        }
        
        if (playerOwnedCards.Any(p => p.Contains(card - 1))){
            if (!playerOwnedCards[0].Contains(card - 1)) return 1-1/(1+value);
        }

        int minima = 0;
        int mindex = 0;

        for (int i = 1; i < playerCount; i++)
        {
            if (playerTokenCounts[i] <= minima)
            { minima = playerTokenCounts[i]; mindex = i; }
        }

        var tempValue = value - 1.25f*playerCount;

        if (tempValue <= 1) return 0;

        return 1-1/tempValue;

    }

    void updateHandValues()
    {
        for(var i=0; i<playerCount; i++)
        {
            playerHandValues[i] = getValueOfHand(playerOwnedCards[i], playerTokenCounts[i]);
        }
    }

    int getValueOfHand(List<int> cards, int tokens)
    {
        int value = -1 * tokens;
        
        if (cards.Count == 0) { return value; }

        foreach (var card in cards)
        {
            if (cards.Contains(card - 1)) continue;
            value += card;
        }

        return value;
    }
}
