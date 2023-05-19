using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class main : MonoBehaviour
{

    public Button takeButton, passButton, exitButton;

    public Button[] playerSelectButtons, firstPlayerButtons;

    public TextMesh cardsRemainingText, flippedCardText, playedTokensText, aiValueText;

    public TextMesh[] playerTokensTexts, playerHandValueTexts;

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

        setupButtons();
        cardsRemaining = 33 - 9;
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
    }

    void setupButtons()
    {
        for (var i = 0; i < 4; i++)
        {
            playerSelectButtons[i].gameObject.SetActive(true);
            playerSelectButtons[i].onClick.RemoveAllListeners();
            playerSelectButtons[i].onClick.AddListener(delegate { playerCountSelect(i); });
        }

        for (var i = 0; i < 4; i++)
        {
            firstPlayerButtons[i].gameObject.SetActive(false);
            firstPlayerButtons[i].onClick.RemoveAllListeners();
            firstPlayerButtons[i].onClick.AddListener(delegate { firstPlayerSelect(i); });
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
        playerCount = p;
        foreach (var button in playerSelectButtons)
        {
            button.gameObject.SetActive(false);
        }
        playerTokenCounts = new int[playerCount];
        for(int i = 0; i < playerCount; i++)
        {
            playerTokenCounts[i] = p == 4 ? 11 : p == 3 ? 11 : p == 2 ? 11 : 11; //TODO: Set intial player token counts
        }
        promptFirstPlayer();
    }

    void promptFirstPlayer()
    {
        for (var i = 0; i < playerCount; i++)
        {
            playerSelectButtons[i].gameObject.SetActive(true);
        }
    }

    void firstPlayerSelect(int p)
    {
        activePlayer = p;
        foreach (var button in firstPlayerButtons)
        {
            button.gameObject.SetActive(false);
        }
        showGame();
        startGame();
    }

    void showGame()
    {

    }

    void take()
    {
        playerOwnedCards[activePlayer].Add(flippedCard);
        playerTokenCounts[activePlayer] += playedTokenCount;
        playedTokenCount = 0;
        promptNewFlippedCard();
        turn();
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
        cardsRemaining--;
    }

    void startGame()
    {
        promptNewFlippedCard();
        turn();
    }

    void updateText()
    {
        aiValueText.text = "";
        flippedCardText.text = flippedCard.ToString();
        playedTokensText.text = playedTokenCount.ToString();
        for(var i = 0; i < playerCount; i++) {
            playerTokensTexts[i].text = playerTokenCounts[i].ToString();
            playerHandValueTexts[i].text = playerHandValues[i].ToString();
        }

    }

    void turn()
    {
        updateHandValues();
        updateText();
        if(activePlayer == 0) { aiTurn(); } else
        {
            takeButton.gameObject.SetActive(true);
            passButton.gameObject.SetActive(true);
        }
    }

    void aiTurn()
    {
        takeButton.gameObject.SetActive(false);
        passButton.gameObject.SetActive(false);

        if (playerTokenCounts[0] == 0)
        {
            instruct(takeI);
            return;
        }

        var valueOfTaking = getValueOfTaking(flippedCard);
        var valueOfPassing = getValueOfPassing(flippedCard);

        aiValueText.text = "Take: " + valueOfTaking.ToString() + ". Pass: " + valueOfPassing.ToString() + ".";

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
        var valueWithNewCard = getValueOfHand(potentialHand, tokenCount);
        
        value = valueWithNewCard-currentValue;
        

        return value;
    }
    float getValueOfPassing(int card)
    {
        float value = 1;

        return value;
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

        foreach (var card in cards)
        {
            if (cards.Contains(card - 1)) continue;
            value += card;
        }

        return value;
    }

}
