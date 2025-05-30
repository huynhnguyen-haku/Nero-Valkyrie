using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MissionSelection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI missionDescription;
    [SerializeField] private TextMeshProUGUI missionObjective;
    [SerializeField] private TextMeshProUGUI missionReward;
    [SerializeField] private Image missionPreviewImage; // Mission preview image

    #region Mission Info Setters

    public void SetMissionDescription(string description)
    {
        missionDescription.text = description;
    }

    public void SetMissionObjective(string objective)
    {
        missionObjective.text = objective;
    }

    public void SetMissionReward(int reward)
    {
        missionReward.text = $"Reward: {reward} golds";
    }

    public void SetMissionPreview(Sprite preview)
    {
        if (preview != null)
        {
            missionPreviewImage.sprite = preview;
            missionPreviewImage.color = Color.white;
        }
        else
        {
            missionPreviewImage.sprite = null;
            missionPreviewImage.color = Color.clear;
        }
    }

    #endregion
}

