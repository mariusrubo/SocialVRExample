using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

/* Noting down data which are preprocessed for direct analysis. In particular, gaze data are preprocessed with regards to a partner's eyes: For each eye, we note the degrees
 * in the horizontal and vertical direction by which its looking direction misses each of the partner's eyes. This information can be inferred from the data stored by
 * SaveRawData but is efficiently computed in C#, so may likewise be noted down directly. Depending on the research questions, researchers may note down both raw data and
 * preprocessed gaze data or only one of the two. 
 * by Marius Rubo, 2023
 * */

public class SavePreprocessedData : MonoBehaviour
{
    /// <summary>
    /// The CharacterBehaviorController of the own avatar which should always be active.
    /// </summary>
    [SerializeField]
    CharacterBehaviorController characterBehaviorControllerSelf;

    /// <summary>
    /// Remote players, if any, are listed in a dictionary which is accessed to retreive their movement data.
    /// </summary>
    [SerializeField]
    ClientHandleRemotePlayers clientHandleRemotePlayers;

    /// <summary>
    /// A StreamWriter object to store data to disk and a StringBuilder object for each loop in the concatenation of strings. See SaveRawData for more details. 
    /// </summary>
    StreamWriter sw;
    StringBuilder sb1;
    StringBuilder sb2;
    StringBuilder sb3;

    void Start()
    {
        CreateFile();

        sb1 = new StringBuilder();
        sb2 = new StringBuilder();
        sb3 = new StringBuilder();
    }

    /// <summary>
    /// A new .csv file is created at start. It is stored in "Documents/SocialVR/Data". The current date, including the current second, defines the file name as this
    /// will distinguish all written files with no risk of overwriting existing files. A header with all variable names is also inserted into the file at start. 
    /// </summary>
    void CreateFile()
    {
        string time = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/SocialVR/Data/PreprocessedData-" + time + ".csv";
        sw = new StreamWriter(path);
        string header = "time;sender_id;receiver_id;SelfOrOther;" +
            "lEye_lEye.x;lEye_lEye.y;" +
            "lEye_rEye.x;lEye_rEye.y;" +
            "rEye_lEye.x;rEye_lEye.y;" +
            "rEye_rEye.x;rEye_rEye.y;";
        sw.WriteLine(header);
    }

    void OnApplicationQuit()
    {
        if (sw != null) sw.Close();
    }


    void Update()
    {
        SavePreprocessedDataToDisk();
    }

    /// <summary>
    /// Preprocessed data, i.e., a character's gaze with regards to another character, are noted down. This format does not refer to an individual's gaze viewed in isolation
    /// but to an interaction between a gaze 'sender' and a gaze 'receiver'. Therefore, when no remote players are active, there is no data to store. In this example we
    /// note down gaze data from self to each of the other characters as well as for each of these other characters and the self. If needed, gaze between remote players
    /// can be added as well (but note that data processing then scales exponentially rather than linearly with increasing number of remote players). 
    /// </summary>
    void SavePreprocessedDataToDisk()
    {
        float currentTime = Time.realtimeSinceStartup;
        int ownID = 0;
        if (FishNet.InstanceFinder.ClientManager != null) ownID = FishNet.InstanceFinder.ClientManager.Connection.ClientId;

        Dictionary<int, CharacterBehaviorController> OtherPlayers = clientHandleRemotePlayers.GetOtherPlayers();
        foreach (var otherPlayer in OtherPlayers)
        {
            // for each remote player, note how the self gazes with respect to that player...
            int other_id = otherPlayer.Key;
            CharacterBehaviorController characterBehaviorControllerOther = otherPlayer.Value;
            sb1.Clear();
            sb1.Append(currentTime).Append(";").Append(ownID).Append(";").Append(other_id).Append(";").Append("SelfAtOther").Append(";").
                Append(ProcessWhoLooksAtWhom(characterBehaviorControllerSelf, characterBehaviorControllerOther));
            sw.WriteLine(sb1);

            // ... and how that player gazes with respect to the self
            sb1.Clear();
            sb1.Append(currentTime).Append(";").Append(other_id).Append(";").Append(ownID).Append(";").Append("OtherAtSelf").Append(";").
                Append(ProcessWhoLooksAtWhom(characterBehaviorControllerOther, characterBehaviorControllerSelf));
            sw.WriteLine(sb1);
        }
    }

    /// <summary>
    /// A sender's gaze is transposed to represent horizontal and vertical deviation from a receiver's eyes. The procecure is done for each of the sender's eyes 
    /// towards each of the receiver's eyes and resulting values are represented as strings. 
    /// </summary>
    /// <param name="sender">The CharacterBehaviorController component of the character who's gaze is to be analyzed.</param>
    /// <param name="receiver">The CharacterBehaviorController component of the character with regards to whom the sender's gaze is to be analyzed.</param>
    string ProcessWhoLooksAtWhom(CharacterBehaviorController sender, CharacterBehaviorController receiver)
    {
        sb2.Clear();
        sb2.
            Append(Vector2ToString(GeometryHelpers.ProjectRayOnTarget(new Ray(sender.lEyePosition, sender.lEyeForward), receiver.lEyePosition))).
            Append(Vector2ToString(GeometryHelpers.ProjectRayOnTarget(new Ray(sender.lEyePosition, sender.lEyeForward), receiver.rEyePosition))).
            Append(Vector2ToString(GeometryHelpers.ProjectRayOnTarget(new Ray(sender.rEyePosition, sender.rEyeForward), receiver.lEyePosition))).
            Append(Vector2ToString(GeometryHelpers.ProjectRayOnTarget(new Ray(sender.rEyePosition, sender.rEyeForward), receiver.rEyePosition)));
        return sb2.ToString();
    }

    /// <summary>
    /// Create a string from a Vector2. 
    /// This function could be placed in GeometryHelpers, but then it would require to instantiate its own StringBuilder 
    /// every time. Instead, let's leave it here in order to be able to just reuse the same StringBuilder object.
    /// </summary>
    /// <param name="vec">The Vector2 to be noted as string.</param>
    string Vector2ToString(Vector2 vec)
    {
        sb3.Clear();
        sb3.Append(vec.y).Append(";").Append(vec.x).Append(";");
        return sb3.ToString();
    }


}
