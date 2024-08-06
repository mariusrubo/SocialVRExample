using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Dissonance;

/* Noting down who is speaking at each moment in time and at what amplitude. All data are received from DissonanceManager which started dissonance and keeps track of all
 * players. Importantly, DissonanceManager uses Fish-Nets ids rather than Dissonance's own ids, so that data can be linked with gaze and other behavioral data stored in
 * other scripts (SaveRawData, SavePrepreprocessedData). Similarly as in other logfile writers, data from all users are stored in the same csv file, with one line in the same
 * format for each user at each point in time. 
 * by Marius Rubo, 2023
 * */
public class SaveSpeechData : MonoBehaviour
{
    /// <summary>
    /// A reference to the DissonanceManager which allows to receive information on PlayersVoiceData
    /// </summary>
    [SerializeField] DissonanceManager dissonanceManager;
    Dictionary<string, VoicePlayerState> PlayersVoiceData = new Dictionary<string, VoicePlayerState>();

    /// <summary>
    /// A StreamWriter notes data to disk; a StringBuilder is used again to combine strings without too much garbage production.
    /// </summary>
    StreamWriter sw;
    StringBuilder sb1;

    void Start()
    {
        CreateFile();
        sb1 = new StringBuilder();
    }

    /// <summary>
    /// The logfile is written to the same folder as the other logfiles, "Documents/SocialVR/Data.
    /// </summary>
    void CreateFile()
    {
        string time = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/SocialVR/Data/SpeechData-" + time + ".csv";
        sw = new StreamWriter(path);
        string header = "time;id;SelfOrOther;isSpeaking;Amplitude";
        sw.WriteLine(header);
    }

    void Update()
    {
        SaveData();
    }

    /// <summary>
    /// As in the other logfile writers, we go through the dictionary of players in each moment in time and note down the values of interest.
    /// DissonanceManger keeps references of each player's VoicePlayerState which is being controlled by Dissonance itself.
    /// </summary>
    void SaveData()
    {
        PlayersVoiceData = dissonanceManager.GetPlayersVoiceData();

        foreach (var player in PlayersVoiceData)
        {
            float currentTime = Time.realtimeSinceStartup;
            string id = player.Key;

            int ownID = 0;
            if (FishNet.InstanceFinder.ClientManager != null) ownID = FishNet.InstanceFinder.ClientManager.Connection.ClientId;
            bool isSelf = string.Equals(id, ownID.ToString());
            string SelfOrOther = isSelf ? "Self" : "Other";

            sb1.Clear();
            sb1.Append(currentTime).Append(";").Append(id).Append(";").Append(SelfOrOther).Append(";").
                Append(BoolToInt(player.Value.IsSpeaking).ToString()).Append(";").
                Append(player.Value.Amplitude.ToString());
            sw.WriteLine(sb1);
        }
    }

    /// <summary>
    /// Turn a bool into an int with 0 for false and 1 for true.
    /// Simple function that could be placed in a general utils file but since it is only needed once in the whole project, have it right here.
    /// </summary>
    /// <param name="mybool">the bool to be transferred to string.</param>
    int BoolToInt(bool mybool)
    {
        int i = 0;
        if (mybool) i = 1;
        return (i);
    }
}
