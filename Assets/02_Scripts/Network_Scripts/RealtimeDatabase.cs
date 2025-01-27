using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class RealtimeDatabase : MonoBehaviour
{
    private DatabaseReference databaseReference;

    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    [Header("User Data")]
    private string username;
    private string password;
    private float bestClearTime = 600f;  // �⺻�� 10�� (600��)
    private int highestScore = 0;        // �⺻�� 0


    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Exception);
                return;
            }

            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("Firebase Initialized");
        });
    }

    // ȸ������ ó��
    public void RegisterUser()
    {
        string userId = usernameInput.text;
        string userPass = passwordInput.text;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userPass))
        {
            Debug.LogError("Username or password cannot be empty!");
            return;
        }

        UserData newUser = new UserData(userId, userPass);
        string json = JsonUtility.ToJson(newUser);

        databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error registering user: " + task.Exception);
            }
            else
            {
                Debug.Log("User registered successfully");
            }
        });
    }

    // �α��� ó��
    public void LoginUser()
    {
        string userId = usernameInput.text;
        string userPass = passwordInput.text;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userPass))
        {
            Debug.LogError("Username or password cannot be empty!");
            return;
        }

        databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error loading user data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    UserData userData = JsonUtility.FromJson<UserData>(snapshot.GetRawJsonValue());
                    if (userData.password == userPass)
                    {
                        username = userData.username;
                        password = userData.password;
                        bestClearTime = userData.bestClearTime;
                        highestScore = userData.highestScore;
                        PlayerPrefs.SetString("username", userData.username);
                        PlayerPrefs.Save();
                        Debug.Log($"Login successful: {username}");
                        SceneManager.LoadScene("LobbyTest");
                        
                    }
                    else
                    {
                        Debug.Log("Invalid password");
                    }
                }
                else
                {
                    Debug.Log("User not found");
                }
            }
        });
    }

    // ���� ������ ����
    public void SaveUserData()
    {
        string userId = usernameInput.text;

        databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error loading data: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    UserData existingData = JsonUtility.FromJson<UserData>(snapshot.GetRawJsonValue());

                    // ���� �ְ� ������ Ŭ���� Ÿ�� �� �� ������Ʈ
                    if (highestScore > existingData.highestScore)
                    {
                        existingData.highestScore = highestScore;
                    }

                    if (bestClearTime < existingData.bestClearTime)
                    {
                        existingData.bestClearTime = bestClearTime;
                    }

                    string json = JsonUtility.ToJson(existingData);

                    databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(saveTask => {
                        if (saveTask.IsFaulted)
                        {
                            Debug.LogError("Error saving data: " + saveTask.Exception);
                        }
                        else
                        {
                            Debug.Log("User data updated successfully!");
                        }
                    });
                }
            }
        });
    }



    // ���� ������ ����ü
    [System.Serializable]
    public class UserData
    {
        public string username;
        public string password;
        public float bestClearTime = 600f;  // �⺻�� 10��
        public int highestScore = 0;        // �⺻�� 0

        public UserData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
}
