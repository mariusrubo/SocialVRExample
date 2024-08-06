using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

/* Noting down state data of the own as well as other's avatars continuously. Data from all characters are saved to the same file in the same format, with one line per 
 * character per save time point. Therefore, if others connect or disconnect, we do not obtain new files but there are merely new lines added to each time point. Data storage
 * is carried out on the client here, implying that every client saves all data. This procedure allows to additionally assess concordance between simulations on different client
 * computers. Data storage may be switched to the server software if this feature is not required. 
 * 
 * Recorded data incorporate all relevant information but are not yet preprocessed in any way: for instance, while we record where each character is positioned at each moment 
 * and where each character is looking, this does not yet tell us directly who is looking at whom. Additional geometric operations are required for such analyses which can be 
 * conducted after data storage (e.g., using Matlab, R or Python). Since they are often computationally efficient, they can also be conducted on the fly in this program 
 * (see SaveProcessedData) so that stored data can be more directly analysed.
 * 
 * Note that most operations in data storage are comparatively lightweight to modern computers (e.g., collecting data from the CharacterBehaviorController component, communicating
 * with the disk itself). A potentially problematic process is string concatenation which produces garbage, which is why the script makes heavy use of StringBuilders.
 * by Marius Rubo, 2023
 * */
public class SaveRawData : MonoBehaviour
{
    /// <summary>
    /// We always store data from the own character's CharacterBehaviorController.
    /// </summary>
    [SerializeField]
    CharacterBehaviorController characterBehaviorControllerSelf;

    /// <summary>
    /// Remote players, if any, are listed in a dictionary which is accessed to retreive their movement data.
    /// </summary>
    [SerializeField]
    ClientHandleRemotePlayers clientHandleRemotePlayers;

    /// <summary>
    /// StreamWriter can handle data storage so efficiently that data throughput in this scenario should never be a problem on modern computers.
    /// By contrast, string concatenation should not be conducted using the "+"-operator when repeated many times per frame as this produces garbage, but StringBuilders
    /// can concatenate strings with little overhead. Note that we need several StringBuilder objects here since each object is cleared for each new operation, and string
    /// concatenation occurs in nested loops (thus we need one StringBuilder per loop layer).
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
        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/SocialVR/Data/RawData-" + time + ".csv";
        sw = new StreamWriter(path);
        string header = "time;id;SelfOrOther;" +
        "character.position.x;character.position.y;character.position.z;" +
        "character.rotation.w;character.rotation.x;character.rotation.y;character.rotation.z;" +
        "headIK.position.x;headIK.position.y;headIK.position.z;" +
        "headIK.rotation.w;headIK.rotation.x;headIK.rotation.y;headIK.rotation.z;" +
        "lEye.position.x;lEye.position.y;lEye.position.z;" +
        "lEye.rotation.w;lEye.rotation.x;lEye.rotation.y;lEye.rotation.z;" +
        "rEye.position.x;rEye.position.y;rEye.position.z;" +
        "rEye.rotation.w;rEye.rotation.x;rEye.rotation.y;rEye.rotation.z;" +
        "smiling"; // since string concatenation only occurs once, there is only minimal garbage production and no need to use a StringBuilder
        sw.WriteLine(header);
    }


    void OnApplicationQuit()
    {
        if (sw != null) sw.Close();
    }


    void Update()
    {
        SaveRawDataToDisk();
    }

    /// <summary>
    /// All relevant data from each character (self and remote players) are stored, with one line per frame for each. 
    /// </summary>
    void SaveRawDataToDisk()
    {
        float currentTime = Time.realtimeSinceStartup;

        // data of the self are always recorded
        int ownID = 0;
        if (FishNet.InstanceFinder.ClientManager != null) ownID = FishNet.InstanceFinder.ClientManager.Connection.ClientId;
        sb1.Clear();
        sb1.Append(currentTime).Append(";").Append(ownID).Append(";").Append("Self").Append(";").Append(CharacterMovementDataAsString(characterBehaviorControllerSelf));
        sw.WriteLine(sb1);

        // data of others are recorded if there are others
        Dictionary<int, CharacterBehaviorController> OtherPlayers = clientHandleRemotePlayers.GetOtherPlayers();
        foreach (var otherPlayer in OtherPlayers)
        {
            int id = otherPlayer.Key;
            CharacterBehaviorController characterBehaviorControllerOther = otherPlayer.Value;
            sb1.Clear();
            sb1.Append(currentTime).Append(";").Append(id).Append(";").Append("Other").Append(";").Append(CharacterMovementDataAsString(characterBehaviorControllerOther));
            sw.WriteLine(sb1);
        }
    }

    /// <summary>
    /// Create a string from all relevant data in a CharacterBehaviorController. Just reuse the same StringBuilder each time the function is called. As there is no 
    /// multi-threading here, there is no risk of different processes interferring with sb2 at the same time. If data storage should ever be moved to a multi-threaded
    /// design, consider instantiating a new StringBuilder each time the function is called (which has its own overhead of course). 
    /// </summary>
    /// <param name="cbc">A character's CharacterBehaviorController component, regardless of whether the character represents the self or a remote player.</param>
    private string CharacterMovementDataAsString(CharacterBehaviorController cbc)
    {
        sb2.Clear();
        sb2.
            Append(Vector3ToString(cbc.CharacterPosition)).
            Append(QuaternionToString(cbc.CharacterRotation)).
            Append(Vector3ToString(cbc.headIKPosition)).
            Append(QuaternionToString(cbc.headIKRotation)).
            Append(Vector3ToString(cbc.lEyePosition)).
            Append(QuaternionToString(cbc.lEyeRotation)).
            Append(Vector3ToString(cbc.rEyePosition)).
            Append(QuaternionToString(cbc.rEyeRotation)).
            Append(cbc.Smiling.ToString());

        return sb2.ToString();
    }

    /// <summary>
    /// Create a string from a Vector3. This function could be placed in GeometryHelpers, but then it would require to instantiate its own StringBuilder 
    /// every time. Instead, let's leave it here in order to be able to just reuse the same StringBuilder object.
    /// </summary>
    /// <param name="vec">The Vector3 to be noted as string.</param>
    string Vector3ToString(Vector3 vec)
    {
        sb3.Clear();
        sb3.Append(vec.x).Append(";").Append(vec.y).Append(";").Append(vec.z).Append(";");
        return sb3.ToString();
    }

    /// <summary>
    /// Create a string from a Quaternion.
    /// </summary>
    /// <param name="quaternion">The Quaternion to be noted as string.</param>
    string QuaternionToString(Quaternion quaternion)
    {
        sb3.Clear();
        sb3.Append(quaternion.w).Append(";").Append(quaternion.x).Append(";").Append(quaternion.y).Append(";").Append(quaternion.z).Append(";");
        return sb3.ToString();
    }
}
